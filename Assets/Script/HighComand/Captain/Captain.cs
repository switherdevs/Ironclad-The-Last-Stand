using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CaptainSkill : MonoBehaviour
{
    [Header("--- CẤU HÌNH THỜI GIAN HỒI CHIÊU ---")]
    [SerializeField] private float thoiGianHoiChieu = 20f;
    private float dongHoHoiChieu = 0f;
    private bool isCooldown = false;

    [Header("--- GIAO DIỆN UI ĐIỀU KHIỂN ---")]
    [Tooltip("Kéo trực tiếp cấu kiện TextMeshPro hiển thị thời gian hồi chiêu của Captain vào đây (Nếu nó nằm ngoài Button).")]
    [SerializeField] private TextMeshProUGUI textHienThiTuChon;

    // Các biến tham chiếu nội bộ nhận từ Spawner
    private Button nutBamSkillCaptain;
    private CanvasGroup canvasGroupNutBam;
    private TextMeshProUGUI textHienThiTrangThai;

    // Tham chiếu tới bộ điều khiển bom dưới Map (Sẽ tự tìm khi vào trận)
    private MapAirStrikeController boDieuKhienBomDuoiMap;
    private Animator Animations;
    private void Start()
    {
        // Tự động đi tìm bộ cấu hình b
        // om của Map hiện tại
        boDieuKhienBomDuoiMap = Object.FindFirstObjectByType<MapAirStrikeController>();
        if (boDieuKhienBomDuoiMap == null)
        {
            Debug.LogError("[CaptainSkill] Không tìm thấy MapAirStrikeController nào trên Map này!");
        }
        Animations = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isCooldown)
        {
            DongHoDemNguocHoiChieu();
        }
    }

    // Hàm này sẽ được HeroSpawner gọi tự động để gán nút bấm tương tự như Sniper
    public void GanNutBamSkillTuDong(Button nutTuSpawner)
    {
        if (nutTuSpawner == null) return;

        nutBamSkillCaptain = nutTuSpawner;
        canvasGroupNutBam = nutBamSkillCaptain.GetComponent<CanvasGroup>();

        // 🌟 THUẬT TOÁN KIỂM TRA ĐA NĂNG:
        // Nếu bạn đã kéo tay Text vào ô 'textHienThiTuChon', sử dụng nó. 
        // Nếu ô đó trống (null), script sẽ tự lùng sục tìm TextMeshPro con bên trong Button.
        if (textHienThiTuChon != null)
        {
            textHienThiTrangThai = textHienThiTuChon;
        }
        else
        {
            textHienThiTrangThai = nutBamSkillCaptain.GetComponentInChildren<TextMeshProUGUI>();
        }

        nutBamSkillCaptain.onClick.RemoveAllListeners();
        nutBamSkillCaptain.onClick.AddListener(ActivateSkill);

        CapNhatGiaoDienUI();
    }

    public void ActivateSkill()
    {
        if (isCooldown) return;
        if (boDieuKhienBomDuoiMap == null) return;

        // RA LỆNH: Gọi Map kích hoạt chuỗi máy bay và thả bom theo thứ tự
        boDieuKhienBomDuoiMap.KichHoatKhongKich();

        // Bắt đầu hồi chiêu bản thân
        BatDauHoiChieu();
    }

    void BatDauHoiChieu()
    {
        isCooldown = true;
        dongHoHoiChieu = thoiGianHoiChieu;

        if (nutBamSkillCaptain != null) nutBamSkillCaptain.interactable = false;
        if (canvasGroupNutBam != null) canvasGroupNutBam.alpha = 0.4f;
    }

    void DongHoDemNguocHoiChieu()
    {
        dongHoHoiChieu -= Time.deltaTime;

        if (textHienThiTrangThai != null)
        {
            textHienThiTrangThai.text = string.Format("HỒI: {0:0.0}s", dongHoHoiChieu);
        }

        if (dongHoHoiChieu <= 0f)
        {
            isCooldown = false;
            CapNhatGiaoDienUI();
        }
    }

    void CapNhatGiaoDienUI()
    {
        if (nutBamSkillCaptain != null) nutBamSkillCaptain.interactable = true;
        if (canvasGroupNutBam != null) canvasGroupNutBam.alpha = 1.0f;

        if (textHienThiTrangThai != null)
        {
            textHienThiTrangThai.text = "SẴN SÀNG";
        }
    }
}