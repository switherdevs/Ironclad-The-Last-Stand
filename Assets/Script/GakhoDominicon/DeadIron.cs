using UnityEngine;

public class NhanVat4 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu (Pháo Hạng Nặng)")]
    public float TamBan = 12f;
    public int satThuong = 150;
    public Transform DiemBan;
    public GameObject prefabDanMobi;

    [Header("Hiệu ứng nạp năng lượng")]
    public GameObject hieuUngNapNangLuong; // Kéo Prefab/GameObject ánh sáng vào đây
    public float thoiGianNapNangLuong = 5f; // Đổi thành 5 giây theo yêu cầu

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;
    public float doLechHangY = 0.2f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f;
    private bool dangTrongLuongBan = false; // Biến chặn trùng lặp Coroutine

    void Start()
    {
        if (vungBoxPhongThu != null)
        {
            viTriCoDinh = LayViTriNgauNhienTrongBox(vungBoxPhongThu);
        }
        else
        {
            viTriCoDinh = transform.position;
            daDenViTriThu = true;
        }

        // Đảm bảo ban đầu hiệu ứng nạp năng lượng luôn tắt
        if (hieuUngNapNangLuong != null) hieuUngNapNangLuong.SetActive(false);
    }

    void Update()
    {
        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null;
            TimMucTieuThongMinh();
        }

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            if (khoangCachXThucTe <= (TamBan + 5f))
            {
                dangDungBan = true;

                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                }

                // Điều kiện bắn: Thẳng hàng Y, trong tầm bắn X, hết thời gian hồi và CHƯA nằm trong luồng bắn nào
                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x);

                    if (Time.time >= thoiGianBanTiepTheo && !dangTrongLuongBan)
                    {
                        StartCoroutine(LuongBanNapNangLuong());
                    }
                }
            }
        }

        // Nếu không có quái hoặc đang bận nạp năng lượng/bắn thì không đi lùi về thủ
        if (!daDenViTriThu && !dangDungBan && !dangTrongLuongBan)
        {
            HanhQuanVaoViTri();
        }
    }

    // Luồng xử lý nạp năng lượng và bắn tuần tự
    private System.Collections.IEnumerator LuongBanNapNangLuong()
    {
        dangTrongLuongBan = true;

        // 1. Kích hoạt hiệu ứng ánh sáng nạp năng lượng (Animation tự reset chạy từ đầu)
        if (hieuUngNapNangLuong != null)
        {
            hieuUngNapNangLuong.SetActive(true);
        }

        // 2. Chờ 5 giây nạp năng lượng
        yield return new WaitForSeconds(thoiGianNapNangLuong);

        // 3. Thực hiện bắn đạn plasma ra sau khi nạp đầy
        if (mucTieuQuai != null) // Kiểm tra lại đề phòng quái chết trong 5 giây chờ
        {
            BanThienThachPooling();
        }

        // 4. Bắn xong lập tức tắt hiệu ứng ánh sáng
        if (hieuUngNapNangLuong != null)
        {
            hieuUngNapNangLuong.SetActive(false);
        }

        // 5. Đặt thời gian hồi cho phát bắn kế tiếp (Nếu muốn bắn liên tục không hồi thì bỏ dòng dưới)
        thoiGianBanTiepTheo = Time.time + 0.5f;

        dangTrongLuongBan = false;
    }

    void HanhQuanVaoViTri()
    {
        XoayMat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null || dangTrongLuongBan) return; // Không trượt Y khi đang đứng tụ năng lượng bắn
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject quaiUuTien = null;
        GameObject quaiDuPhong = null;
        float kcDuPhongNganNhat = Mathf.Infinity;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float kc = Vector2.Distance(transform.position, quai.transform.position);
                KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>();
                if (marker == null) marker = quai.AddComponent<KhoaMucTieu>();

                if (!marker.daBiKhoaMucTieu)
                {
                    if (kc < khoangCachNganNhat)
                    {
                        khoangCachNganNhat = kc;
                        quaiUuTien = quai;
                    }
                }
                else
                {
                    if (kc < kcDuPhongNganNhat)
                    {
                        kcDuPhongNganNhat = kc;
                        quaiDuPhong = quai;
                    }
                }
            }
        }
        XacDinhVaKhoaMucTieu(quaiUuTien, quaiDuPhong);
    }

    void XacDinhVaKhoaMucTieu(GameObject quaiUuTien, GameObject quaiDuPhong)
    {
        if (quaiUuTien != null)
        {
            mucTieuQuai = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
        {
            mucTieuQuai = quaiDuPhong.transform;
        }
        else
        {
            mucTieuQuai = null;
        }
    }

    void BanThienThachPooling()
    {
        if (DiemBan == null || prefabDanMobi == null) return;

        float huongBanX = (mucTieuQuai.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = null;
        if (QuanLyDan.Instance != null)
        {
            vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanMobi);
        }

        if (vienDan == null)
        {
            vienDan = Instantiate(prefabDanMobi, DiemBan.position, rotation);
        }
        else
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);
        }

        if (vienDan != null)
        {
            DanNv4 scriptDan = vienDan.GetComponent<DanNv4>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    void XoayMat(float xMucTieu)
    {
        if (dangTrongLuongBan) return; // Khóa xoay hướng khi đang tụ pháo nạp năng lượng để tăng độ logic
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    private void OnDisable()
    {
        if (mucTieuQuai != null)
        {
            KhoaMucTieu marker = mucTieuQuai.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}