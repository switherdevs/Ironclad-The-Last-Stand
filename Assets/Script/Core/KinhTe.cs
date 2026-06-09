using UnityEngine;
using TMPro;

public class HeThongKinhTe : MonoBehaviour
{
    public static HeThongKinhTe Instance { get; private set; }

    [Header("--- CÁC LOẠI TIỀN ---")]
    public int tongTienHienTai; // Tiền vàng trong trận đấu 
    public int tienNangCapLinh; // Tiền dùng để nâng cấp lính ngoài Menu (Được lưu bằng file .txt)

    [Header("--- KẾT NỐI UI TEXTMESH PRO ---")]
    [SerializeField] private TextMeshProUGUI textTienNangCap;

    [Header("--- KẾT NỐI HỆ THỐNG SAVE ---")]
    public SaveSystem boQuanLySave;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Tự động tìm hệ thống save nếu quên kéo thả
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        // Đọc tiền từ file .txt lúc mới vào game
        if (boQuanLySave != null)
        {
            tienNangCapLinh = boQuanLySave.DocThongTinGame();
        }
        else
        {
            tienNangCapLinh = 0;
        }

        // Cập nhật giao diện chữ ngay khi vừa vào game để hiện số tiền đọc từ file save
        CapNhatGiaoDienTien();
    }

    public void BamNutSaveGame()
    {
        if (boQuanLySave != null)
        {
            // Lấy chính xác biến tienNangCapLinh đem đi lưu vào file
            boQuanLySave.LuuThongTinGame(tienNangCapLinh);
        }
    }

    // ĐÃ SỬA TẠI ĐÂY: Quái chết thì tiền lập tức được cộng vào quỹ nâng cấp
    public void NhanTienKhiQuaiChet(string tenQuai)
    {
        tongTienHienTai += 30; // Vẫn giữ nguyên logic cũ của bạn

        // Dòng này giúp tiền chảy vào quỹ nâng cấp và cập nhật lên UI ngay lập tức
        ThayDoiTienNangCapLinh(30);
    }

    // Hàm cập nhật chữ lên màn hình UI
    public void CapNhatGiaoDienTien()
    {
        if (textTienNangCap != null)
        {
            textTienNangCap.text = tienNangCapLinh.ToString();
        }
    }

    // HÀM CHÍNH ĐỂ BẠN GỌI KHI MUA/NÂNG CẤP LÍNH:
    public void ThayDoiTienNangCapLinh(int soLuongThayDoi)
    {
        tienNangCapLinh += soLuongThayDoi;
        CapNhatGiaoDienTien(); // Ép UI vẽ lại số mới ngay lập tức
    }
}