using UnityEngine;

public class Dannv2 : MonoBehaviour
{
    public khodanan cauHinhDan;
    [HideInInspector] public int satThuong = 100; // Sẽ được nhân vật nạp đam vào khi bắn

    [Header("--- HIỆU ỨNG VA CHẠM ---")]
    [Tooltip("Prefab hiệu ứng sẽ được tạo ra tại vị trí viên đạn va chạm")]
    public GameObject prefabHieuUngNo;

    private float demThoiGian;
    private bool daKichHoat = false;
    private TrailRenderer line;

    // SỬA: Đổi từ Start sang Awake để lấy linh kiện trước khi OnEnable chạy
    private void Awake()
    {
        line = GetComponent<TrailRenderer>();
    }

    // ĐÃ CHUYỂN ĐỔI: Tự động reset và kích hoạt đạn mỗi khi lôi từ Kho ra (Chuẩn Pooling)
    private void OnEnable()
    {
        // 1. Bật lại vệt đuôi đạn, bảo đảm an toàn không bị lỗi Null
        if (line != null)
        {
            line.enabled = true;
        }

        // 2. Tự động đặt lại thời gian sống từ file cấu hình
        if (cauHinhDan != null)
        {
            demThoiGian = cauHinhDan.Duytri;
        }
        else
        {
            demThoiGian = 3f;
        }

        daKichHoat = true; // Cho phép đạn bay ngay lập tức
    }

    void Update()
    {
        if (!daKichHoat) return;

        // Lấy tốc độ từ cấu hình, nếu không có thì mặc định bằng 8f
        float tocDo = (cauHinhDan != null) ? cauHinhDan.tocDobay : 8f;

        // Xác định hướng bay dựa trên hướng mặt của viên đạn
        float huongDi = (transform.right.x >= 0) ? 1f : -1f;

        // Di chuyển đạn theo trục thế giới độc lập
        transform.Translate(Vector3.right * huongDi * tocDo * Time.deltaTime, Space.World);

        // Tính thời gian tự hủy trả về kho pooling
        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            ThanhCongTraDan();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chạm trúng mục tiêu có Tag là Enemy
        if (collision.CompareTag("Enemy"))
        {
            // 🌟 NÂNG CẤP: Tạo hiệu ứng nổ ngay khi chạm Collider của Enemy
            TaoHieuUngVaCham();

            Health_chaos mauQuai = collision.GetComponent<Health_chaos>();
            if (mauQuai == null) mauQuai = collision.GetComponentInParent<Health_chaos>();

            if (mauQuai != null)
            {
                // Gây sát thương đơn mục tiêu
                mauQuai.TakeDamage(satThuong);
                Debug.Log($"🎯 Đạn đánh trúng {collision.name}, gây {satThuong} sát thương đơn!");
            }

            // Biến mất ngay sau khi chạm mục tiêu
            ThanhCongTraDan();
        }
    }

    // Hàm tạo hiệu ứng tại vị trí va chạm độc lập
    void TaoHieuUngVaCham()
    {
        if (prefabHieuUngNo != null)
        {
            Instantiate(prefabHieuUngNo, transform.position, transform.rotation);
        }
    }

    void ThanhCongTraDan()
    {
        if (line != null) line.enabled = false; // Tắt vệt đuôi trước khi ẩn để tránh bị loang dòng chữ trên màn hình
        daKichHoat = false;
        gameObject.SetActive(false); // Trả về kho đạn ẩn
    }

    // Giữ lại hàm cũ để không bị báo lỗi ở các script Súng đang gọi nó
    public void KichHoatVienDan()
    {
        if (cauHinhDan != null)
        {
            demThoiGian = cauHinhDan.Duytri;
        }
        else
        {
            demThoiGian = 3f;
        }
        daKichHoat = true;
    }
}