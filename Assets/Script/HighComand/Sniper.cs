using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SniperSkill : MonoBehaviour
{
    [Header("--- CURSOR HỒNG TÂM ---")]
    public Texture2D sniperCursor;

    [Header("--- HIỆU ỨNG & SÁT THƯƠNG ---")]
    public GameObject explosionPrefab;
    public int damage = 20;
    public float BanKinh = 5f;

    [Header("--- CẤU HÌNH SỐ LƯỢNG ĐẠN ---")]
    public int maxShots = 5;
    private int currentShots;
    private bool skillActive = false;

    [Header("--- CẤU HÌNH THỜI GIAN HỒI CHIÊU (MỚI) ---")]
    [Tooltip("Thời gian hồi chiêu của kỹ năng (giây) sau khi bắn hết đạn")]
    [SerializeField] private float thoiGianHoiChieu = 15f;
    private float dongHoHoiChieu = 0f;
    private bool isCooldown = false;

    [Header("--- GIAO DIỆN UI ĐIỀU KHIỂN (MỚI) ---")]
    [Tooltip("Kéo Button kích hoạt kỹ năng này vào đây")]
    [SerializeField] private Button nutBamSkillSniper;
    [Tooltip("Gắn thành phần CanvasGroup của nút bấm (hoặc của bảng skill) để làm mờ")]
    [SerializeField] private CanvasGroup canvasGroupNutBam;
    [Tooltip("Kéo TextMeshPro hiển thị số đạn / thời gian hồi vào đây")]
    [SerializeField] private TextMeshProUGUI textHienThiTrangThai;

    [Header("--- ÂM THANH ---")]
    private AudioSource Amthanh;
    [SerializeField] private AudioClip Shooot;

    private void Start()
    {
        Amthanh = GetComponent<AudioSource>();

        // Thiết lập trạng thái UI ban đầu
        CapNhatGiaoDienUI();
    }

    void Update()
    {
        // 1. Xử lý logic đếm ngược hồi chiêu
        if (isCooldown)
        {
            DongHoDemNguocHoiChieu();
            return; // Nếu đang hồi chiêu thì không chạy logic bắn bên dưới
        }

        // 2. Logic click chuột bắn khi đang active skill
        if (!skillActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void ActivateSkill()
    {
        // Chặn kích hoạt nếu đang hồi chiêu hoặc đang bật sẵn rồi
        if (isCooldown || skillActive) return;

        skillActive = true;
        currentShots = maxShots;

        // Thay đổi con trỏ chuột thành hồng tâm bắn tỉa
        Vector2 hotspot = new Vector2(sniperCursor.width / 2, sniperCursor.height / 2);
        Cursor.SetCursor(sniperCursor, hotspot, CursorMode.Auto);

        // Vô hiệu hóa nút bấm tạm thời khi đang trong chế độ ngắm bắn để tránh người chơi bấm lại
        if (nutBamSkillSniper != null) nutBamSkillSniper.interactable = false;

        CapNhatTextTrangThai();
        Debug.Log("Đã kích hoạt chế độ bắn tỉa!");
    }

    void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Tạo hiệu ứng nổ và phát âm thanh
        if (explosionPrefab != null) Instantiate(explosionPrefab, mousePos, Quaternion.identity);
        if (Amthanh != null && Shooot != null) Amthanh.PlayOneShot(Shooot);

        // Quét và gây sát thương cho quái trong bán kính quy định
        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePos, BanKinh);
        foreach (Collider2D hit in hits)
        {
            // Tìm component máu của quái (bạn có thể đổi thành kiểu dữ liệu máu của bạn nếu cần)
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                // enemy.TakeDamage(damage); // Gọi hàm trừ máu quái của bạn ở đây
            }
        }

        currentShots--;
        CapNhatTextTrangThai();
        Debug.Log("Đã bắn! Còn lại: " + currentShots);

        if (currentShots <= 0)
        {
            BatDauHoiChieuKieuMoi();
        }
    }

    void BatDauHoiChieuKieuMoi()
    {
        skillActive = false;
        isCooldown = true;
        dongHoHoiChieu = thoiGianHoiChieu;

        // Trả con trỏ chuột về mặc định
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Làm mờ nút bấm và khóa không cho click
        if (nutBamSkillSniper != null) nutBamSkillSniper.interactable = false;
        if (canvasGroupNutBam != null) canvasGroupNutBam.alpha = 0.4f; // Mờ còn 40%

        Debug.Log("Hết đạn! Bắt đầu chuyển sang trạng thái hồi chiêu.");
    }

    void DongHoDemNguocHoiChieu()
    {
        dongHoHoiChieu -= Time.deltaTime;

        if (textHienThiTrangThai != null)
        {
            // Hiển thị số giây còn lại (lấy 1 chữ số thập phân cho đẹp, ví dụ: 12.4s)
            textHienThiTrangThai.text = string.Format("HỒI: {0:0.0}s", dongHoHoiChieu);
        }

        if (dongHoHoiChieu <= 0f)
        {
            // Hồi chiêu xong hoàn toàn
            isCooldown = false;
            dongHoHoiChieu = 0f;

            // Khôi phục nút bấm về trạng thái sẵn sàng ban đầu
            if (nutBamSkillSniper != null) nutBamSkillSniper.interactable = true;
            if (canvasGroupNutBam != null) canvasGroupNutBam.alpha = 1.0f; // Sáng rõ 100%

            CapNhatTextTrangThai();
            Debug.Log("Kỹ năng Bắn tỉa đã hồi xong! Sẵn sàng sử dụng.");
        }
    }

    void CapNhatGiaoDienUI()
    {
        if (nutBamSkillSniper != null) nutBamSkillSniper.interactable = true;
        if (canvasGroupNutBam != null) canvasGroupNutBam.alpha = 1.0f;
        CapNhatTextTrangThai();
    }

    void CapNhatTextTrangThai()
    {
        if (textHienThiTrangThai == null) return;

        if (skillActive)
        {
            textHienThiTrangThai.text = $"ĐẠN: {currentShots}/{maxShots}";
        }
        else if (!isCooldown)
        {
            textHienThiTrangThai.text = "SẴN SÀNG";
        }
    }
}