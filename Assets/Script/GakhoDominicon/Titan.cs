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
        // Nếu bạn gán vùng Box, Titan sẽ tự tìm 1 điểm ngẫu nhiên trong vùng đó để đi ra
        if (vungBoxPhongThu != null)
        {
            viTriCoDinh = LayViTriNgauNhienTrongBox(vungBoxPhongThu);
        }
        else
        {
            // Nếu quên gán Box, đứng im tại chỗ cũ
            viTriCoDinh = transform.position;
            daDenViTriThu = true;
        }
    }

    void Update()
    {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        // 1. LUÔN LUÔN quét tìm quái liên tục trên bản đồ
        TimQuaiGanNhat();

        // BƯỚC 1: XỬ LÝ CHIẾN ĐẤU VÀ TỰ CĂN LÀN Y KHI THẤY QUÁI
        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            // Nếu quái lọt vào phạm vi kích hoạt (Tầm bắn + 2 ô)
            if (khoangCachXThucTe <= (TamBan + 2f))
            {
                // NẾU BỊ LỆCH LÀN Y: Tự động trượt Y bám theo quái luôn (kể cả khi đang hành quân)
                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                    return; // Ngăn không cho chạy hàm hành quân phía dưới, tập trung bám làn quái
                }

                // NẾU ĐÃ THẲNG LÀN Y & LỌT TẦM BẮN X: Đứng im xả đạn Plasma!
                if (khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x);

                    // Kiểm tra hồi chiêu bắn đạn
                    if (Time.time >= thoiGianBanTiepTheo)
                    {
                        TitanBanDanPooling();
                        thoiGianBanTiepTheo = Time.time + thoiGianHoiChieu;
                    }

                    return; // Chặn đứng mọi di chuyển, đứng im bắn quái
                }
            }
        }
        // BƯỚC 2: NẾU KHÔNG CÓ QUÁI (HOẶC QUÁI Ở QUÁ XA) -> ĐI RA BOX THỦ
        if (!daDenViTriThu)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        XoayMat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.05f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true; // Đến nơi an toàn, chuyển sang trạng thái thủ chốt!
        }
    }

    public void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null) return;

        // Giữ nguyên tọa độ X cố định trong Box, chỉ thay đổi vị trí Y bám theo quái
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

    void TimQuaiGanNhat()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject quaiGanNhat = null;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float kc = Vector2.Distance(transform.position, quai.transform.position);
                if (kc < khoangCachNganNhat)
                {
                    khoangCachNganNhat = kc;
                    quaiGanNhat = quai;
                }
            }
        }

        if (quaiGanNhat != null) mucTieuQuai = quaiGanNhat.transform;
        else mucTieuQuai = null;
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
            vienDan.SetActive(true);

            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}