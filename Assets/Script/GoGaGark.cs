using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NhanVat1Controller : MonoBehaviour
{
    public float TamBan = 9f;
    public float soDanBan = 1f;
    public int satThuong = 10;
    public Transform DiemBan;

    public float tocDoDiChuyenY = 5f;

    private Transform ThayDich;
    private float HoiChieu = 0f;

    public float DolechHangY = 0.3f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy)
        {
            TimKiemKeDich();
        }

        if (ThayDich != null)
        {
            Xoaymat();

            // Kiểm tra khoảng cách thực tế

            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            if (doLechYThucTe > DolechHangY)
            {
                DiChuyenTrungHangY();
            }
            // 3. LOGIC TẤN CÔNG: Nếu đã trùng hàng Y mới xét khoảng cách X để bắn
            else
            {
                float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

                if (khoangCachX <= TamBan && Time.time >= HoiChieu)
                {
                    TanCong();
                    HoiChieu = Time.time + 1f / soDanBan;
                }
            }
        }
    }
    void TimKiemKeDich()
    {
        GameObject[] mangDich = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject dichGanNhat = null;

        // QUAN TRỌNG: Trước khi tìm, phải reset biến này về null 
        // để tránh việc nhân vật nhớ mục tiêu cũ ở hàng khác!
        dichGanNhat = null;

        foreach (GameObject dich in mangDich)
        {
            if (dich.activeInHierarchy)
            {
                // Tính khoảng cách Vector2 tổng thể để tìm mục tiêu gần nhất
                float khoangCach = Vector2.Distance(transform.position, dich.transform.position);
                if (khoangCach < khoangCachNganNhat)
                {
                    khoangCachNganNhat = khoangCach;
                    dichGanNhat = dich;
                }
            }
        }

        if (dichGanNhat != null) ThayDich = dichGanNhat.transform;
        else ThayDich = null;
    }
    void DiChuyenTrungHangY()
    {
        // Giữ nguyên vị trí X và Z của lính, chỉ thay đổi đích đến Y bằng Y của quái
        Vector3 viTriMucTieu = new Vector3(transform.position.x, ThayDich.position.y, transform.position.z);

        // Di chuyển tịnh tiến cực mượt (Bất chấp Rigidbody2D đang ở chế độ Kinematic)
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }
    void Xoaymat()
    {
        // Đã bọc bảo vệ: Hàm này chỉ chạy khi ThayDich chắc chắn không bị Null
        if (ThayDich.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Quay trái
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);  // Quay phải
        }
    }
    void TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null) return;
        float huongBanX = (ThayDich.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho();
        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.SetActive(true); // Kích hoạt đạn

            DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
            if (scriptDan != null)
            {
                // Đồng bộ chính xác với biến trong script DanNV1 hiện tại của bạn
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
            vienDan.SetActive(true);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // Vẽ tầm xa
        Gizmos.DrawWireSphere(transform.position, TamBan);

        // Vẽ 2 đường biên giới hạn hàng Z cho bạn dễ nhìn trực quan
        Gizmos.color = Color.blue;
        Vector3 lineLeft = transform.position + Vector3.left * TamBan;
        Vector3 lineRight = transform.position + Vector3.right * TamBan;

        Gizmos.DrawLine(lineLeft + Vector3.forward * DolechHangY, lineRight + Vector3.forward * DolechHangY);
        Gizmos.DrawLine(lineLeft + Vector3.back * DolechHangY, lineRight + Vector3.back * DolechHangY);
    }
}