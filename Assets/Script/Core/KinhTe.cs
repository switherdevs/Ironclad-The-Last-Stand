using UnityEngine;
using TMPro;

public class HeThongKinhTe : MonoBehaviour
{
    public static HeThongKinhTe Instance { get; private set; }

    [Header("--- KẾT NỐI DATA THƯỢNG TIỀN (SCRIPTABLE OBJECT) ---")]
    [SerializeField] private ThuongTienQuaiData dataThuongTienQuai;

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
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null)
        {
            tienNangCapLinh = boQuanLySave.DocThongTinGame();
        }
        else
        {
            tienNangCapLinh = 0;
        }

        CapNhatGiaoDienTien();
    }

    public void BamNutSaveGame()
    {
        if (boQuanLySave != null)
        {
            boQuanLySave.LuuThongTinGame(tienNangCapLinh);
        }
    }

    // ĐÃ CHỈNH SỬA: Tiền diệt quái CHỈ cộng vào quỹ Nâng Cấp ngoài trận
    public void NhanTienKhiQuaiChet(string tenQuai)
    {
        int tienThuongThucTe = 30; // Mặc định nếu không tìm thấy cấu hình phù hợp

        if (dataThuongTienQuai != null)
        {
            tienThuongThucTe = dataThuongTienQuai.LayTienThuongTuTenQuai(tenQuai);
        }

        // CHỈ cộng vào tiền nâng cấp lính (.txt) và cập nhật UI text hiển thị
        ThayDoiTienNangCapLinh(tienThuongThucTe);

        Debug.Log($"[KINH TẾ] Quái '{tenQuai}' chết -> Nhận {tienThuongThucTe} Tiền Nâng Cấp tích lũy!");
    }

    public void CapNhatGiaoDienTien()
    {
        if (textTienNangCap != null)
        {
            textTienNangCap.text = tienNangCapLinh.ToString();
        }
    }

    public void ThayDoiTienNangCapLinh(int soLuongThayDoi)
    {
        tienNangCapLinh += soLuongThayDoi;
        CapNhatGiaoDienTien();
    }
}