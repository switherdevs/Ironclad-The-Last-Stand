using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC có để quản lý Button

public class SpawnLinh : MonoBehaviour
{
    // THIẾT KẾ SINGLETON để ResourceManager có thể gọi ngược lại làm mờ nút khi tiền thay đổi
    public static SpawnLinh Instance { get; private set; }

    [Header("--- VỊ TRÍ XUẤT HIỆN ---")]
    public Transform spawnPoint;

    [Header("--- GIÁ VÀNG CHO TỪNG LOẠI LÍNH ---")]
    public int giaSevitor = 75;
    public int giaKhograkGuard = 75;
    public int giaIronStormMarine = 200;
    public int giaStormTerminator = 400;
    public int giaIronDreadWalker = 750;
    public int giaDominiconTitan = 1500;

    [Header("--- NÚT BẤM UI MUA LÍNH ---")]
    [Tooltip("Kéo thả các thành phần Button tương ứng ngoài Canvas vào đây")]
    public Button btnSevitor;
    public Button btnKhograkGuard;
    public Button btnIronStormMarine;
    public Button btnStormTerminator;
    public Button btnIronDreadWalker;
    public Button btnDominiconTitan;

    [Header("--- DANH SÁCH PREFABS & SLOT LÍNH ---")]
    public GameObject SevitorPrefab;
    public int slotSevitor = 0;

    public GameObject khograkGuardPrefab;
    public int slotKhograkGuard = 1;

    public GameObject ironStormMarinePrefab;
    public int slotIronStormMarine = 2;

    public GameObject stormTerminatorPrefab;
    public int slotStormTerminator = 5;

    public GameObject ironDreadWalkerPrefab;
    public int slotIronDreadWalker = 10;

    public GameObject dominiconTitanPrefab;
    public int slotDominiconTitan = 20;

    [Header("--- CẤU HÌNH KHÁC ---")]
    public bool speedup = false;
    [SerializeField] private AudioSource Amthanh;
    [SerializeField] private AudioClip ser;
    [SerializeField] private AudioClip KhoGrak;
    [SerializeField] private AudioClip IronStom;
    [SerializeField] private AudioClip Ter;
    [SerializeField] private AudioClip DeadIron;
    [SerializeField] private AudioClip Titan;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        Amthanh = GetComponent<AudioSource>();

        // Vừa vào game, tự kiểm tra trạng thái mờ/sáng nút dựa theo số tiền ban đầu
        CapNhatTrangThaiNut();
    }

    // HÀM CHÍNH KHÓA/LÀM MỜ NÚT BẤM THEO SỐ TIỀN HIỆN CÓ
    public void CapNhatTrangThaiNut()
    {
        if (ResourceManager.Instance == null) return;

        int tienHienTai = ResourceManager.Instance.soTienHienTai;

        // Nếu ĐỦ tiền thì nút sáng (true), THIẾU tiền thì nút tự mờ và khóa click (false)
        if (btnSevitor != null) btnSevitor.interactable = (tienHienTai >= giaSevitor);
        if (btnKhograkGuard != null) btnKhograkGuard.interactable = (tienHienTai >= giaKhograkGuard);
        if (btnIronStormMarine != null) btnIronStormMarine.interactable = (tienHienTai >= giaIronStormMarine);
        if (btnStormTerminator != null) btnStormTerminator.interactable = (tienHienTai >= giaStormTerminator);
        if (btnIronDreadWalker != null) btnIronDreadWalker.interactable = (tienHienTai >= giaIronDreadWalker);
        if (btnDominiconTitan != null) btnDominiconTitan.interactable = (tienHienTai >= giaDominiconTitan);
    }

    // --- CÁC HÀM SPAWN ---
    public void Sevitor()
    {
        XuLyMuaLinh(giaSevitor, slotSevitor, SevitorPrefab, true);
    }

    public void SpawnKhograkGuard()
    {
        XuLyMuaLinh(giaKhograkGuard, slotKhograkGuard, khograkGuardPrefab);
    }

    public void SpawnIronStormMarine()
    {
        XuLyMuaLinh(giaIronStormMarine, slotIronStormMarine, ironStormMarinePrefab);
    }

    public void SpawnStormTerminator()
    {
        XuLyMuaLinh(giaStormTerminator, slotStormTerminator, stormTerminatorPrefab);
    }

    public void SpawnIronDreadWalker()
    {
        XuLyMuaLinh(giaIronDreadWalker, slotIronDreadWalker, ironDreadWalkerPrefab);
    }

    public void SpawnDominiconTitan()
    {
        XuLyMuaLinh(giaDominiconTitan, slotDominiconTitan, dominiconTitanPrefab);
    }

    private void XuLyMuaLinh(int gia, int slot, GameObject prefab, bool isSevitor = false)
    {
        if (ResourceManager.Instance.KiemTraVaTruTien(gia))
        {
            if (ResourceManager.Instance.KiemTraVaThemLinh(slot, isSevitor))
            {
                SpawnUnit(prefab);
                PhatAmThanhMuaLinh(prefab); // Phát âm thanh tương ứng khi mua thành công
            }
            else
            {
                // Hoàn tiền nếu không đủ slot chứa lính
                ResourceManager.Instance.TangTien(gia);
            }
        }

        // ĐÃ CẬP NHẬT: Sau mỗi lượt tính toán mua/hoàn tiền, bắt buộc kiểm tra lại ví tiền để cập nhật độ mờ nút bấm
        CapNhatTrangThaiNut();
    }

    // Hàm phụ xử lý phát âm thanh gọn gàng hơn
    private void PhatAmThanhMuaLinh(GameObject prefab)
    {
        if (Amthanh == null) return;

        if (prefab == SevitorPrefab) Amthanh.PlayOneShot(ser);
        else if (prefab == khograkGuardPrefab) Amthanh.PlayOneShot(KhoGrak);
        else if (prefab == ironStormMarinePrefab) Amthanh.PlayOneShot(IronStom);
        else if (prefab == stormTerminatorPrefab) Amthanh.PlayOneShot(Ter);
        else if (prefab == ironDreadWalkerPrefab) Amthanh.PlayOneShot(DeadIron);
        else if (prefab == dominiconTitanPrefab) Amthanh.PlayOneShot(Titan);
    }

    private void SpawnUnit(GameObject unitPrefab)
    {
        if (unitPrefab != null && spawnPoint != null)
        {
            Instantiate(unitPrefab, spawnPoint.position, unitPrefab.transform.rotation);
        }
    }

}