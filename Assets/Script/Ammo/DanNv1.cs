using UnityEngine;

public class DanNV1 : MonoBehaviour
{
    public khodanan cauHinhDan; // Kéo file ScriptableObject khodanan vào đây trên Prefab
    public int satThuong = 10;
    private float demThoiGian;
    private bool daKichHoat = false;
    private TrailRenderer Line;

    void Awake()
    {
        Line = GetComponent<TrailRenderer>();
    }

    // ĐÃ CHUYỂN ĐỔI: Tận dụng OnEnable để tự động làm mới viên đạn mỗi khi lấy từ Kho ra
    private void OnEnable()
    {
        // 1. Bật lại vệt đuôi đạn TrailRenderer
        if (Line != null)
        {
            Line.enabled = true;
        }

        // 2. TỰ ĐỘNG KÍCH HOẠT: Không lo súng quên gọi hàm kích hoạt nữa
        if (cauHinhDan != null)
        {
            demThoiGian = cauHinhDan.Duytri;
        }
        else
        {
            demThoiGian = 3f; // Phòng hờ nếu quên kéo file cấu hình ngoài Unity
        }

        daKichHoat = true; // Cho phép đạn bay ngay khi vừa OnEnable
    }

    void Update()
    {
        if (!daKichHoat) return;

        // Đã sửa: Nếu quên gán cấu hình thì vẫn cho đạn bay theo một vận tốc mặc định (Vd: 10f) thay vì return đứng im
        float tocDo = (cauHinhDan != null) ? cauHinhDan.tocDobay : 10f;

        // Đạn bay thẳng theo trục phải (X) của chính nó
        transform.Translate(Vector2.right * tocDo * Time.deltaTime, Space.Self);

        // Đếm ngược thời gian biến mất
        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            AnVienDanVeKho();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Xử lý trừ máu quái tại đây (Ví dụ: collision.GetComponent<BaseEnemy>().TakeDamage(satThuong);)

            // Lập tức thu đạn về kho Pooling
            AnVienDanVeKho();
        }
    }

    // ĐÃ THÊM: Hàm gom gọn logic ẩn đạn để tránh viết lặp đi lặp lại nhiều lần
    private void AnVienDanVeKho()
    {
        if (Line != null) Line.enabled = false; // Tắt vệt đuôi để không bị kéo vệt loang lổ khi đổi vị trí
        daKichHoat = false;
        gameObject.SetActive(false); // Ẩn đi để trả về kho cho QuanLyDan
    }

    // Bạn vẫn có thể giữ hàm này nếu các script Súng khác đang gọi nó, không bị ảnh hưởng gì cả
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