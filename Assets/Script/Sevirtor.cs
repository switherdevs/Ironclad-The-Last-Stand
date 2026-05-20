using UnityEngine;

public class Sevirtor : MonoBehaviour
{
    // Các trạng thái của thợ đào vàng
    public enum MinerState { DiToiBaiVang, DangDaoVang, VeNhaChinh, CatVang }

    [Header("Trạng thái hiện tại")]
    public MinerState trangThaiHienTai = MinerState.DiToiBaiVang;

    [Header("Cấu hình vị trí")]
    public Transform DiemBaiVang;   // Kéo GameObject Mỏ Vàng vào đây ngoài Unity
    public Transform DiemNhaChinh;  // Kéo GameObject Nhà Chính vào đây ngoài Unity

    [Header("Chỉ số đào vàng")]
    public float tocDoDiChuyen = 3f;
    public float thoiGianDaoVang = 3f;
    public int luongVangMangTheo = 10;

    private float demThoiGianDao;
    private float doLechToiThieu = 0.5f; // Tăng nhẹ sai số để chống kẹt trục Y lẻ

    void Start()
    {
        // Tự động tìm kiếm theo đúng Tag bạn đã đặt ngoài Editor
        if (DiemBaiVang == null) DiemBaiVang = GameObject.FindWithTag("gold")?.transform;
        if (DiemNhaChinh == null) DiemNhaChinh = GameObject.FindWithTag("Home")?.transform;
    }

    void Update()
    {
        switch (trangThaiHienTai)
        {
            case MinerState.DiToiBaiVang:
                HanhDong_DiToiBaiVang();
                break;

            case MinerState.DangDaoVang:
                HanhDong_DangDaoVang();
                break;

            case MinerState.VeNhaChinh:
                HanhDong_VeNhaChinh();
                break;

            case MinerState.CatVang: // ĐÃ MỞ KHÓA: Giúp lính thực hiện cất tiền và quay đầu
                HanhDong_CatVang();
                break;
        }
    }

    void HanhDong_DiToiBaiVang()
    {
        if (DiemBaiVang == null) return;

        XoayMat(DiemBaiVang.position.x);
        transform.position = Vector3.MoveTowards(transform.position, DiemBaiVang.position, tocDoDiChuyen * Time.deltaTime);

        // Lớp bảo vệ 1: Đo khoảng cách gần sát thì đào luôn
        if (Vector3.Distance(transform.position, DiemBaiVang.position) <= doLechToiThieu)
        {
            BatDauDaoVang();
        }
    }

    void HanhDong_DangDaoVang()
    {
        demThoiGianDao -= Time.deltaTime;
        if (demThoiGianDao <= 0f)
        {
            trangThaiHienTai = MinerState.VeNhaChinh;
            Debug.Log("Đào xong! Đang vác " + luongVangMangTheo + " vàng về nhà chính.");
        }
    }

    void HanhDong_VeNhaChinh()
    {
        if (DiemNhaChinh == null) return;

        XoayMat(DiemNhaChinh.position.x);
        transform.position = Vector3.MoveTowards(transform.position, DiemNhaChinh.position, tocDoDiChuyen * Time.deltaTime);

        // Lớp bảo vệ 1: Đo khoảng cách gần sát nhà thì cất vàng luôn
        if (Vector3.Distance(transform.position, DiemNhaChinh.position) <= doLechToiThieu)
        {
            trangThaiHienTai = MinerState.CatVang;
        }
    }

    void HanhDong_CatVang()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.TangTien(luongVangMangTheo);
            Debug.Log("Thợ mỏ đã cất vàng vào kho thành công! Tổng tiền tăng.");
        }
        else
        {
            Debug.LogError("Không tìm thấy ResourceManager trên bản đồ!");
        }
        // Cất vàng xong lập tức đổi trạng thái để đi bộ ra bãi đào tiếp
        trangThaiHienTai = MinerState.DiToiBaiVang;
    }

    void BatDauDaoVang()
    {
        if (trangThaiHienTai == MinerState.DiToiBaiVang)
        {
            trangThaiHienTai = MinerState.DangDaoVang;
            demThoiGianDao = thoiGianDaoVang;
            Debug.Log("Bắt đầu đào vàng!");
        }
    }

    // Lớp bảo vệ 2: Chạm trực tiếp bằng Collider Trigger vật lý
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (trangThaiHienTai == MinerState.DiToiBaiVang && collision.CompareTag("gold"))
        {
            BatDauDaoVang();
        }

        if (trangThaiHienTai == MinerState.VeNhaChinh && collision.CompareTag("Home"))
        {
            trangThaiHienTai = MinerState.CatVang;
        }
    }

    void XoayMat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

}
