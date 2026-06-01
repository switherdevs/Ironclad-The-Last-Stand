using UnityEngine;

public class SpawnLinh : MonoBehaviour
{
    [Header("--- VỊ TRÍ XUẤT HIỆN ---")]
    public Transform spawnPoint;

    [Header("--- GIÁ VÀNG CHO TỪNG LOẠI LÍNH ---")]
    public int giaSevitor = 10;
    public int giaKhograkGuard = 50;
    public int giaIronStormMarine = 100;
    public int giaStormTerminator = 200;
    public int giaIronDreadWalker = 500;
    public int giaDominiconTitan = 1000;

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

    // --- CÁC HÀM SPAWN ---

    public void Sevitor()
    {
        // TRUYỀN THAM SỐ TRUE XÁC ĐỊNH ĐÂY LÀ SERVITOR
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

    // --- HÀM XỬ LÝ CHUNG (Đã cập nhật tham số isSevitor) ---
    private void XuLyMuaLinh(int gia, int slot, GameObject prefab, bool isSevitor = false)
    {
        // 1. Kiểm tra và trừ tiền trước
        if (ResourceManager.Instance.KiemTraVaTruTien(gia))
        {
            // 2. Kiểm tra slot (kèm định danh isSevitor)
            if (ResourceManager.Instance.KiemTraVaThemLinh(slot, isSevitor))
            {
                // Nếu cả hai đều thỏa mãn thì mới spawn
                SpawnUnit(prefab);
            }
            else
            {
                // Nếu hết slot thì hoàn lại tiền đã trừ
                ResourceManager.Instance.TangTien(gia);
            }
        }
    }

    private void SpawnUnit(GameObject unitPrefab)
    {
        if (unitPrefab != null && spawnPoint != null)
        {
            Instantiate(unitPrefab, spawnPoint.position, unitPrefab.transform.rotation);
        }
    }

    public void Timeskip()
    {
        speedup = !speedup;
        Time.timeScale = speedup ? 6 : 1;
    }
}