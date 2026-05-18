using UnityEngine;
using TMPro; // BẮT BUỘC PHẢI CÓ: Để điều khiển các ô chữ TextMeshPro hiển thị UI

public class ResourceManager : MonoBehaviour
{
    // THIẾT KẾ MÔ HÌNH SINGLETON: Giúp các Script khác gọi sang đây tương tác dễ dàng
    public static ResourceManager Instance { get; private set; }

    [Header("--- KẾT NỐI UI TEXT MESH PRO ---")]
    public TextMeshProUGUI textHienThiTien;   // Ô kéo chữ hiển thị Tiền (Ví dụ: "Vàng: 500")
    public TextMeshProUGUI textHienThiLinh;  // Ô kéo chữ hiển thị Lính (Ví dụ: "Lính: 10/100")

    [Header("--- CẤU HÌNH TÀI NGUYÊN GAME ---")]
    private int soTienHienTai = 0;           // Số tiền người chơi đang sở hữu, ban đầu vào game bằng 0
    private int soTienToiDa = 9999;          // Giới hạn tiền tối đa theo yêu cầu là 9999

    private int soLinhHienTai = 0;           // Số lượng lính hiện có trên map, ban đầu bằng 0
    private int soLinhToiDa = 100;           // Giới hạn quân số tối đa theo yêu cầu là 100

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

    // ĐÃ XÓA: Hàm Update() chứa sự kiện lắng nghe Input chuột trái cũ đã bị gạt bỏ hoàn toàn

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

    // HÀM MỚI: DÀNH RIÊNG CHO SỰ KIỆN NÚT BẤM (BUTTON CLICK) TRÊN GIAO DIỆN TEST GAME
    // BẮT BUỘC có chữ "public" thì ô thiết lập sự kiện của nút bấm Unity mới nhìn thấy hàm này
    public void NutBamCongTienTestGame()
    {
        Debug.Log("Nút Test Game được nhấn! Thực hiện cộng 500 vàng.");
        TangTien(500); // Gọi hàm gốc ở trên và truyền tham số 500 vào
    }

    // HÀM XỬ LÝ THÊM LÍNH (Có kiểm tra chốt chặn tối đa 100 con)
    public bool KiemTraVaThemLinh()
    {
        if (soLinhHienTai >= soLinhToiDa)
        {
            Debug.LogWarning("QUÂN SỐ ĐÃ ĐẠT GIỚI HẠN TỐI ĐA (100/100)! Không thể sinh thêm lính.");
            return false;
        }

        soLinhHienTai++;
        CapNhatGiaoDienUI();
        return true;
    }

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

    // ĐÃ XÓA: Toàn bộ hàm KiemTraClickVaoMoVang() cũ bắn tia ray phức tạp đã bị gỡ bỏ sạch sẽ
}