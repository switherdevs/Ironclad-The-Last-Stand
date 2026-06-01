using UnityEngine;

public class NhanVat4 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu (Pháo Hạng Nặng)")]
    public float TamBan = 12f;
    public int satThuong = 150;
    public float tocDoBan = 0.4f;
    public Transform DiemBan;
    public GameObject prefabDanMobi;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;
    public float doLechHangY = 0.2f; // Độ lệch nhỏ để đảm bảo thẳng hàng mới bắn

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f;

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
    }

    void Update()
    {
        // 1. KIỂM TRA VÀ QUÉT TÌM QUÁI THÔNG MINH CHỐNG DỒN ĐAM
        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null; // Reset nếu quái cũ đã chết hoặc ẩn
            TimMucTieuThongMinh();
        }

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            // Nếu quái nằm trong tầm quét radar (Tầm bắn + thêm khoảng cách chuẩn bị)
            if (khoangCachXThucTe <= (TamBan + 5f))
            {
                dangDungBan = true; // Ưu tiên bám đuổi quái, dừng việc đi lùi về box thủ

                // NẾU BỊ LỆCH LÀN Y: Tự động trượt Y bám theo quái luôn
                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                }

                // ĐIỀU KIỆN CHÍ MẠNG: CHỈ bắn khi ĐÃ THẲNG HÀNG (Y) và TRONG TẦM BẮN (X)
                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x); // Đã sửa hoàn toàn thành XoayMat theo yêu cầu

                    if (Time.time >= thoiGianBanTiepTheo)
                    {
                        BanThienThachPooling();
                        thoiGianBanTiepTheo = Time.time + (1f / tocDoBan);
                    }
                }
            }
        }

        // Nếu không có quái hoặc quái ở quá xa, quay trở về vị trí phòng thủ ban đầu
        if (!daDenViTriThu && !dangDungBan)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        XoayMat(viTriCoDinh.x); // Đã sửa thành XoayMat
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null) return;
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    // LOGIC CHỌN ĐỊCH THÔNG MINH CHỐNG TRÙNG MỤC TIÊU (ƯU TIÊN QUÁI GẦN NHẤT CHƯA KHÓA)
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

                // Ưu tiên 1: Gần nhất và CHƯA bị ai khóa mục tiêu
                if (!marker.daBiKhoaMucTieu)
                {
                    if (kc < khoangCachNganNhat)
                    {
                        khoangCachNganNhat = kc;
                        quaiUuTien = quai;
                    }
                }
                // Dự phòng: Gần nhất nhưng đã có đồng đội khóa trước
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
            if (marker != null) marker.daBiKhoaMucTieu = true; // Thực hiện khóa quái
        }
        else if (quaiDuPhong != null)
        {
            mucTieuQuai = quaiDuPhong.transform; // Nếu map hết quái rảnh thì chấp nhận bắn chung
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
            vienDan.transform.SetParent(null); // Đưa đạn ra ngoài cha để bay tự do
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

    // Đã chuyển thành tên hàm XoayMat viết hoa chuẩn xác
    void XoayMat(float xMucTieu)
    {
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