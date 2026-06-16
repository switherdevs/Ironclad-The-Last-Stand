using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance;

    [Header("--- KẾT NỐI SCRIPT LIÊN QUAN ---")]
    [SerializeField] private HeThongSatThuongData dataLinh;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private HeThongKinhTe kinhTe;
    [SerializeField] private SaveSystem saveSystem; // KẾT NỐI MỚI: Dùng để tương tác đọc/ghi file

    [Header("--- PANEL CHÍNH ---")]
    [SerializeField] private GameObject panelNangCap;

    [Header("--- CÁC Ô CHỮ HIỂN THỊ UI ---")]
    [SerializeField] private TextMeshProUGUI textTenLinh;

    [Space(5)]
    [Header("== Khung Máu ==")]
    [SerializeField] private TextMeshProUGUI textCapMau;
    [SerializeField] private TextMeshProUGUI textChiSoMau;
    [SerializeField] private TextMeshProUGUI textGiaMau;

    [Space(5)]
    [Header("== Khung Sát Thương ==")]
    [SerializeField] private TextMeshProUGUI textCapST;
    [SerializeField] private TextMeshProUGUI textChiSoST;
    [SerializeField] private TextMeshProUGUI textGiaST;

    [Space(5)]
    [Header("== Khung Tiền Tệ ==")]
    [Tooltip("Ô chữ hiển thị tổng số tiền người chơi đang có")]
    [SerializeField] private TextMeshProUGUI textTienHienCo;

    [Header("--- NÚT BẤM NÂNG CẤP ---")]
    [SerializeField] private Button btnNangCapMau;
    [SerializeField] private Button btnNangCapST;

    [Header("--- HỆ THỐNG ÂM THANH (SFX) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxClickChonLinh;
    [SerializeField] private AudioClip sfxNangCapThanhCong;

    private int indexLinhHienTai = -1;
    private ChonLinhClick linhDangDuocChonHienTai = null;

    private void Awake()
    {
        Instance = this;
        if (panelNangCap != null) panelNangCap.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Tự động tìm SaveSystem trong Scene nếu người chơi quên không kéo tay
        if (saveSystem == null) saveSystem = FindFirstObjectByType<SaveSystem>();
    }

    private void Start()
    {
        // THỰC THI QUY TRÌNH KIỂM TRA SAVE GAME KHI VÀO GAME
        XuLyKhaiHaoSaveGame();

        CapNhatTienHienCoUI();
    }

    // Hàm xử lý kiểm tra và nạp file Save hoặc tạo mới
    private void XuLyKhaiHaoSaveGame()
    {
        if (saveSystem == null || dataLinh == null) return;

        // Trường hợp 1: Có file save cũ -> Nạp lại dữ liệu
        if (saveSystem.KiemTraCoFileSave())
        {
            for (int i = 0; i < dataLinh.danhSachSatThuong.Count; i++)
            {
                string duLieuLinh = saveSystem.DocNangCapLinh(i);
                if (!string.IsNullOrEmpty(duLieuLinh))
                {
                    // Giải mã chuỗi cấu trúc: index|capMau|capSt|mauGoc|stGoc
                    string[] thongTin = duLieuLinh.Split('|');

                    var cl = dataLinh.danhSachSatThuong[i];
                    cl.capDoMau = int.Parse(thongTin[1]);
                    cl.capDoSatThuong = int.Parse(thongTin[2]);
                    cl.mauGoc = int.Parse(thongTin[3]);
                    cl.satThuongGoc = int.Parse(thongTin[4]);

                    dataLinh.danhSachSatThuong[i] = cl; // Cập nhật ngược lại mảng struct
                }
            }
            Debug.Log("<color=cyan>[SaveSystem] Đã nạp thành công cấp độ lính từ file save!</color>");
        }
        // Trường hợp 2: Không tìm thấy save -> Reset về mặc định ban đầu và tạo mốc lưu mới
        else
        {
            dataLinh.ResetToanBoChiSoVeMocGoc(); // Gọi hàm Reset về mốc mặc định chúng ta đã làm

            // Tiến hành ghi file khởi tạo cấp độ 0 ban đầu cho toàn bộ danh sách lính
            for (int i = 0; i < dataLinh.danhSachSatThuong.Count; i++)
            {
                var cl = dataLinh.danhSachSatThuong[i];
                saveSystem.LuuNangCapLinh(i, cl.capDoMau, cl.capDoSatThuong, cl.mauGoc, cl.satThuongGoc);
            }
            Debug.Log("<color=yellow>[SaveSystem] Không tìm thấy file save cũ. Đã tạo file Save mặc định cấp 0!</color>");
        }
    }

    public void CapNhatTienHienCoUI()
    {
        if (kinhTe != null && textTienHienCo != null)
        {
            textTienHienCo.text = $"Vàng: {kinhTe.tienNangCapLinh}";
        }
    }

    public void HienUIChungLinh(int indexLinh, ChonLinhClick linhScript = null)
    {
        if (dataLinh == null || indexLinh < 0 || indexLinh >= dataLinh.danhSachSatThuong.Count) return;

        if (linhDangDuocChonHienTai != null)
        {
            linhDangDuocChonHienTai.BatTatVienSang(false);
        }

        if (linhScript != null)
        {
            linhDangDuocChonHienTai = linhScript;
            linhDangDuocChonHienTai.BatTatVienSang(true);
        }

        indexLinhHienTai = indexLinh;
        panelNangCap.SetActive(true);

        PhatAmThanh(sfxClickChonLinh);

        CapNhatGiaoDien();
    }

    public void CapNhatGiaoDien()
    {
        CapNhatTienHienCoUI();

        if (indexLinhHienTai == -1 || kinhTe == null || dataLinh == null) return;

        var cl = dataLinh.danhSachSatThuong[indexLinhHienTai];

        textTenLinh.text = cl.tenChungLinh;
        textCapMau.text = $"Cấp Máu: {cl.capDoMau}";
        textCapST.text = $"Cấp Đam: {cl.capDoSatThuong}";

        if (textChiSoMau != null)
        {
            int mauThucTe = dataLinh.LayMauTuChung(cl.tenChungLinh);
            textChiSoMau.text = $"Máu : {mauThucTe}";
        }

        if (textChiSoST != null)
        {
            int stThucTe = dataLinh.LaySatThuongTuChung(cl.tenChungLinh);
            textChiSoST.text = $"ST : {stThucTe}";
        }

        int tienHienCo = kinhTe.tienNangCapLinh;

        if (cl.mangGiaTienMau != null && cl.capDoMau < cl.mangGiaTienMau.Count)
        {
            int giaMau = cl.mangGiaTienMau[cl.capDoMau];
            textGiaMau.text = $"Giá: {giaMau}";
            btnNangCapMau.interactable = (tienHienCo >= giaMau);
        }
        else
        {
            textGiaMau.text = "MAX LEVEL";
            btnNangCapMau.interactable = false;
        }

        if (cl.mangGiaTienSatThuong != null && cl.capDoSatThuong < cl.mangGiaTienSatThuong.Count)
        {
            int giaST = cl.mangGiaTienSatThuong[cl.capDoSatThuong];
            textGiaST.text = $"Giá: {giaST}";
            btnNangCapST.interactable = (tienHienCo >= giaST);
        }
        else
        {
            textGiaST.text = "MAX LEVEL";
            btnNangCapST.interactable = false;
        }

        btnNangCapMau.onClick.RemoveAllListeners();
        btnNangCapMau.onClick.AddListener(() => OnClickNangCapMau());

        btnNangCapST.onClick.RemoveAllListeners();
        btnNangCapST.onClick.AddListener(() => OnClickNangCapSatThuong());
    }

    private void OnClickNangCapMau()
    {
        int capTruocKhiNang = dataLinh.danhSachSatThuong[indexLinhHienTai].capDoMau;
        upgradeManager.NangCapMauLinh(indexLinhHienTai);

        if (dataLinh.danhSachSatThuong[indexLinhHienTai].capDoMau > capTruocKhiNang)
        {
            PhatAmThanh(sfxNangCapThanhCong);

            // BỔ SUNG: Tự động lưu thông tin máu mới của con lính này xuống file txt
            var cl = dataLinh.danhSachSatThuong[indexLinhHienTai];
            saveSystem.LuuNangCapLinh(indexLinhHienTai, cl.capDoMau, cl.capDoSatThuong, cl.mauGoc, cl.satThuongGoc);
            // Lưu kèm theo cả số tiền sau khi bị trừ 
            saveSystem.LuuThongTinGame(kinhTe.tienNangCapLinh);
        }
        CapNhatGiaoDien();
    }

    private void OnClickNangCapSatThuong()
    {
        int capTruocKhiNang = dataLinh.danhSachSatThuong[indexLinhHienTai].capDoSatThuong;
        upgradeManager.NangCapSatThuongLinh(indexLinhHienTai);

        if (dataLinh.danhSachSatThuong[indexLinhHienTai].capDoSatThuong > capTruocKhiNang)
        {
            PhatAmThanh(sfxNangCapThanhCong);

            // BỔ SUNG: Tự động lưu thông tin sát thương mới của con lính này xuống file txt
            var cl = dataLinh.danhSachSatThuong[indexLinhHienTai];
            saveSystem.LuuNangCapLinh(indexLinhHienTai, cl.capDoMau, cl.capDoSatThuong, cl.mauGoc, cl.satThuongGoc);
            // Lưu kèm theo cả số tiền sau khi bị trừ 
            saveSystem.LuuThongTinGame(kinhTe.tienNangCapLinh);
        }
        CapNhatGiaoDien();
    }

    private void PhatAmThanh(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void DongUI()
    {
        if (linhDangDuocChonHienTai != null)
        {
            linhDangDuocChonHienTai.BatTatVienSang(false);
            linhDangDuocChonHienTai = null;
        }
        panelNangCap.SetActive(false);
        indexLinhHienTai = -1;
    }
}