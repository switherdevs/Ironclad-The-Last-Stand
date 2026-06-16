using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("--- KẾT NỐI UI TEXT MESH PRO ---")]
    public TextMeshProUGUI textHienThiTien;
    public TextMeshProUGUI textHienThiLinh;
    public TextMeshProUGUI textHienThiServitor;

    [Header("--- CẤU HÌNH TÀI NGUYÊN GAME ---")]
    [SerializeField] public int soTienHienTai = 50;
    public int soTienToiDa = 9999;
    public int soLinhHienTai = 0;
    public int soLinhToiDa = 100;
    public int soSevitorHienTai = 0;
    public int soSevitorToiDa = 6;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        soSevitorHienTai = 2;
        CapNhatGiaoDienUI();
    }

    public void TangTien(int soTienCongThem)
    {
        soTienHienTai += soTienCongThem;
        if (soTienHienTai > soTienToiDa)
        {
            soTienHienTai = soTienToiDa;
        }
        CapNhatGiaoDienUI();
    }

    public bool KiemTraVaTruTien(int soTienCanTra)
    {
        if (soTienHienTai >= soTienCanTra)
        {
            soTienHienTai -= soTienCanTra;
            CapNhatGiaoDienUI();
            return true;
        }
        else
        {
            Debug.Log("KHÔNG ĐỦ VÀNG!");
            return false;
        }
    }

    public void NutBamCongTienTestGame()
    {
        TangTien(500);
    }

    public bool KiemTraVaThemLinh(int soSlotChiem, bool isSevitor = false)
    {
        if (isSevitor)
        {
            if (soSevitorHienTai >= soSevitorToiDa)
            {
                Debug.LogWarning("ĐẠT GIỚI HẠN SERVITOR!");
                return false;
            }
        }

        if (soLinhHienTai + soSlotChiem > soLinhToiDa)
        {
            Debug.LogWarning($"KHÔNG ĐỦ SLOT TRỐNG!");
            return false;
        }

        if (isSevitor) soSevitorHienTai++;
        soLinhHienTai += soSlotChiem;
        CapNhatGiaoDienUI();
        return true;
    }

    public void TruLinh(int soSlotGiaiPhong, bool isSevitor = false)
    {
        soLinhHienTai -= soSlotGiaiPhong;
        if (isSevitor && soSevitorHienTai > 0)
        {
            soSevitorHienTai--;
        }
        if (soLinhHienTai < 0) soLinhHienTai = 0;

        CapNhatGiaoDienUI();
    }

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

        if (textHienThiServitor != null)
        {
            textHienThiServitor.text = "SERVITOR: " + soSevitorHienTai + " / " + soSevitorToiDa;
        }

        // ĐÃ ĐỒNG BỘ: Gọi sang hệ thống Spawn lính làm mờ nút ngay khi tiền thay đổi trạng thái số dư
        if (SpawnLinh.Instance != null)
        {
            SpawnLinh.Instance.CapNhatTrangThaiNut();
        }
    }
}