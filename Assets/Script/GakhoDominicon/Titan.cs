using UnityEngine;

public class TitanPhe9 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu")]
    public float TamBan = 50f;          // Tầm bắn quét kẻ địch của Titan
    public float thoiGianHoiChieu = 3f; // Khoảng cách thời gian bắn giữa các viên đạn là 3 giây
    public int satThuong = 40;
    public Transform DiemBan;
    public GameObject prefabDanLon; // Kéo thả Prefab viên đạn to màu xanh (DanLon) vào đây

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;  // Tốc độ di chuyển tịnh tiến lên/xuống để bắt quái
    public float doLechHangY = 0.3f;   // Sai số hàng Y

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu; // Kéo thả ô Box Collider đại diện vùng thủ của Titan vào đây
    public float tocDoHanhQuan = 2f;   // Tốc độ đi từ nhà ra điểm thủ của Titan

    private Vector3 viTriCoDinh;        // Vị trí ngẫu nhiên tính toán được trong Box
    private bool daDenViTriThu = false; // Kiểm tra xem đã đến nơi chưa
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f; // Biến lưu mốc thời gian được phép bắn tiếp

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
        // 1. KIỂM TRA VÀ QUÉT TÌM QUÁI ĐI ĐẦU HÀNG CHỐNG DỒN ĐAM
        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null;
            TimMucTieuThongMinh();
        }

        bool dangDungBan = false;

        // BƯỚC 1: XỬ LÝ CHIẾN ĐẤU VÀ TỰ CĂN LÀN Y KHI THẤY QUÁI
        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            if (khoangCachXThucTe <= (TamBan + 2f))
            {
                dangDungBan = true;

                // NẾU BỊ LỆCH LÀN Y: Tự động trượt Y bám theo quái luôn
                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                    return;
                }

                // NẾU ĐÃ THẲNG LÀN Y & LỌT TẦM BẮN X: Đứng im xả đạn!
                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x); // Đã sửa từ XoMat thành XoayMat chuẩn xác

                    if (Time.time >= thoiGianBanTiepTheo)
                    {
                        TitanBanDanPooling();
                        thoiGianBanTiepTheo = Time.time + thoiGianHoiChieu;
                    }

                    return;
                }
            }
        }
        // BƯỚC 2: NẾU KHÔNG CÓ QUÁI -> ĐI RA BOX THỦ
        if (!daDenViTriThu && !dangDungBan)
        {
            HanhQuanVaoViTri();
        }
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

    public void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null) return;

        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float xLonNhat = -Mathf.Infinity;
        GameObject quaiUuTien = null;
        GameObject quaiDuPhong = null;
        float xDuPhongLonNhat = -Mathf.Infinity;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float viTriX = quai.transform.position.x;

                KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>();
                if (marker == null) marker = quai.AddComponent<KhoaMucTieu>();

                if (!marker.daBiKhoaMucTieu)
                {
                    if (viTriX > xLonNhat)
                    {
                        xLonNhat = viTriX;
                        quaiUuTien = quai;
                    }
                }
                else
                {
                    if (viTriX > xDuPhongLonNhat)
                    {
                        xDuPhongLonNhat = viTriX;
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

    void XoayMat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void TitanBanDanPooling()
    {
        if (DiemBan == null || QuanLyDan.Instance == null || prefabDanLon == null) return;

        float huongBanX = (mucTieuQuai.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanLon);
        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);

            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}