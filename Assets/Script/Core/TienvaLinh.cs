using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    // THIẾT KẾ MÔ HÌNH SINGLETON: Giúp các Script khác gọi sang đây tương tác dễ dàng
    public static ResourceManager Instance { get; private set; }

    [Header("--- KẾT NỐI UI TEXT MESH PRO ---")]
    public TextMeshProUGUI textHienThiTien;   // Ô kéo chữ hiển thị Tiền (Ví dụ: "Vàng: 500")
    public TextMeshProUGUI textHienThiLinh;  // Ô kéo chữ hiển thị Lính (Ví dụ: "Lính: 10/100")

    // BỔ SUNG: Tham chiếu UI cho Servitor
    public TextMeshProUGUI textHienThiServitor;

    [Header("--- CẤU HÌNH TÀI NGUYÊN GAME ---")]
    [SerializeField]
    public int soTienHienTai = 50;
    public int soTienToiDa = 9999;

    public int soLinhHienTai = 0;
    public int soLinhToiDa = 100;

    // BỔ SUNG: Biến đếm và cấu hình Servitor
    public int soSevitorHienTai = 0;
    public int soSevitorToiDa = 6; // Có thể tùy chỉnh trong Inspector

    void Awake()
    {
        // Khởi tạo Singleton: Tự dán chính nó vào ô Instance để mở cổng kết nối cho toàn game
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Vừa vào game, lập tức vẽ số tiền và số lính ban đầu lên màn hình
        soSevitorHienTai = 2;
        CapNhatGiaoDienUI();
    }

    // HÀM XỬ LÝ TĂNG TIỀN (Có kiểm tra chốt chặn tối đa 9999)
    public void TangTien(int soTienCongThem)
    {
        soTienHienTai += soTienCongThem; // Cộng dồn số tiền mới vào quỹ

        // CHỐT CHẶN MAX TIỀN: Nếu sau khi cộng mà vượt quá 9999, ép nó về đúng 9999
        if (soTienHienTai > soTienToiDa)
        {
            soTienHienTai = soTienToiDa;
        }

        CapNhatGiaoDienUI(); // Vẽ lại con số mới lên màn hình
    }

    public bool KiemTraVaTruTien(int soTienCanTra)
    {
        if (soTienHienTai >= soTienCanTra)
        {
            soTienHienTai -= soTienCanTra;
            CapNhatGiaoDienUI();
            return true; // Đủ tiền, cho phép mua
        }
        else
        {
            Debug.Log("KHÔNG ĐỦ VÀNG!");
            return false; // Không đủ tiền
        }
    }

    public void NutBamCongTienTestGame()
    {
        TangTien(500);
    }

    public bool KiemTraVaThemLinh(int soSlotChiem, bool isSevitor = false)
    {
        // KIỂM TRA ĐIỀU KIỆN RIÊNG CHO SERVITOR
        if (isSevitor)
        {
            if (soSevitorHienTai >= soSevitorToiDa)
            {
                Debug.LogWarning("ĐẠT GIỚI HẠN SERVITOR!");
                return false;
            }
        }

        // KIỂM TRA ĐIỀU KIỆN TỔNG SLOT
        if (soLinhHienTai + soSlotChiem > soLinhToiDa)
        {
            Debug.LogWarning($"KHÔNG ĐỦ SLOT TRỐNG! Cần thêm {soSlotChiem} slot nhưng hiện tại chỉ còn trống {soLinhToiDa - soLinhHienTai} slot.");
            return false;
        }

        // THỰC THI CỘNG DỒN
        if (isSevitor) soSevitorHienTai++;
        soLinhHienTai += soSlotChiem;
        CapNhatGiaoDienUI();
        return true;
    }

    public void TruLinh(int soSlotGiaiPhong, bool isSevitor = false)
    {
        soLinhHienTai -= soSlotGiaiPhong;

        // GIẢM BIẾN ĐẾM SERVITOR NẾU ĐƠN VỊ BỊ HỦY LÀ SERVITOR
        if (isSevitor && soSevitorHienTai > 0)
        {
            soSevitorHienTai--;
        }

        // CHỐT CHẶN AN TOÀN
        if (soLinhHienTai < 0)
        {
            soLinhHienTai = 0;
        }

        CapNhatGiaoDienUI();
    }

    // HÀM VẼ CHỮ LÊN MÀN HÌNH UI
    void CapNhatGiaoDienUI()
    {
        if (textHienThiTien != null)
        {
            textHienThiTien.text = "Gold: " + soTienHienTai + " / " + soTienToiDa;
        }

        if (textHienThiLinh != null)
        {
            textHienThiLinh.text = "Solider: " + soLinhHienTai + " / " + soLinhToiDa;
        }

        // BỔ SUNG: Hiển thị Servitor lên UI
        if (textHienThiServitor != null)
        {
            textHienThiServitor.text = "SERVITOR: " + soSevitorHienTai + " / " + soSevitorToiDa;
        }
    }
}