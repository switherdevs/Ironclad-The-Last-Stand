using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    // THIẾT KẾ MÔ HÌNH SINGLETON: Giúp các Script khác gọi sang đây tương tác dễ dàng
    public static ResourceManager Instance { get; private set; }

    [Header("--- KẾT NỐI UI TEXT MESH PRO ---")]
    public TextMeshProUGUI textHienThiTien;   // Ô kéo chữ hiển thị Tiền (Ví dụ: "Vàng: 500")
    public TextMeshProUGUI textHienThiLinh;  // Ô kéo chữ hiển thị Lính (Ví dụ: "Lính: 10/100")

    [Header("--- CẤU HÌNH TÀI NGUYÊN GAME ---")]
    public int soTienHienTai = 0;
    public int soTienToiDa = 9999;

    public int soLinhHienTai = 0;

    public int soLinhToiDa = 100;

    void Awake()
    {
        // Khởi tạo Singleton: Tự dán chính nó vào ô Instance để mở cổng kết nối cho toàn game
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Vừa vào game, lập tức vẽ số tiền và số lính ban đầu lên màn hình
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

    public void NutBamCongTienTestGame()
    {
        Debug.Log("Nút Test Game được nhấn! Thực hiện cộng 500 vàng.");
        TangTien(500); // Gọi hàm gốc ở trên và truyền tham số 500 vào
    }

    // --- ĐOẠN SỬA ĐỔI: CẬP NHẬT LOGIC CHIẾM SLOT LÍNH LINH HOẠT ---
    // Hàm nhận vào 'soSlotChiem' để kiểm tra xem quỹ quân số còn đủ chỗ chứa hay không
    public bool KiemTraVaThemLinh(int soSlotChiem)
    {
        // Kiểm tra xem số lượng lính hiện tại cộng thêm số slot dự kiến có vượt quá giới hạn tối đa không
        if (soLinhHienTai + soSlotChiem > soLinhToiDa)
        {
            Debug.LogWarning($"KHÔNG ĐỦ SLOT TRỐNG! Cần thêm {soSlotChiem} slot nhưng hiện tại chỉ còn trống {soLinhToiDa - soLinhHienTai} slot.");
            return false; // Trả về false để Script sinh lính biết mà hủy lệnh sinh
        }

        // Nếu đủ chỗ trống, tiến hành cộng số slot chiếm dụng vào tổng số lính hiện tại
        soLinhHienTai += soSlotChiem;
        CapNhatGiaoDienUI(); // Cập nhật lại UI hiển thị số lính mới
        return true; // Trả về true báo hiệu chiếm slot thành công, cho phép sinh lính
    }
    // -------------------------------------------------------------

    // HÀM VẼ CHỮ LÊN MÀN HÌNH UI
    void CapNhatGiaoDienUI()
    {
        if (textHienThiTien != null)
        {
            textHienThiTien.text = "VÀNG: " + soTienHienTai + " / " + soTienToiDa;
        }

        if (textHienThiLinh != null)
        {
            textHienThiLinh.text = "LÍNH: " + soLinhHienTai + " / " + soLinhToiDa;
        }
    }
}
