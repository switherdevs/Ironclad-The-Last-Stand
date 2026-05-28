using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
public class SpawnLinh : MonoBehaviour
{
    [Header("--- VỊ TRÍ XUẤT HIỆN ---")]

    public Transform spawnPoint;

    [Header("--- DANH SÁCH PREFABS LÍNH ---")]
    public GameObject SevitorPrefab;
    public GameObject khograkGuardPrefab;
    public int KhoGrakGuand = 1;
    public GameObject ironStormMarinePrefab;
    public int ironStormMarine = 2;

    public GameObject stormTerminatorPrefab;
    public int stormTerminator = 5;

    public GameObject ironDreadWalkerPrefab;
    public int ironDreadWalker = 10;

    public GameObject dominiconTitanPrefab;
    public int dominiconTitan = 20;

    private float KhoangNghi;
    public bool speedup = false;
    public void Sevitor()
    {
        SpawnUnit(SevitorPrefab);
    }

    public void SpawnKhograkGuard()
    {
        if (ResourceManager.Instance.KiemTraVaThemLinh(1))
        {
            SpawnUnit(khograkGuardPrefab);
        }
    }

    public void SpawnIronStormMarine()
    {
        if (ResourceManager.Instance.KiemTraVaThemLinh(2))
        {
            SpawnUnit(ironStormMarinePrefab);
        }
    }

    public void SpawnStormTerminator()
    {
        if (ResourceManager.Instance.KiemTraVaThemLinh(5))
        {
            SpawnUnit(stormTerminatorPrefab);
        }
    }

    public void SpawnIronDreadWalker()
    {
        if (ResourceManager.Instance.KiemTraVaThemLinh(10))
        {
            SpawnUnit(ironDreadWalkerPrefab);
        }
    }

    public void SpawnDominiconTitan()
    {
        if (ResourceManager.Instance.KiemTraVaThemLinh(20))
        {
            SpawnUnit(dominiconTitanPrefab);
        }
    }
    public void Timeskip()
    {
        speedup = !speedup;

        if(speedup == true)
        {
            Time.timeScale = 6;
        }
        else
        {
            Time.timeScale = 1;
        }

    }

    private void SpawnUnit(GameObject unitPrefab)
    {
        if (unitPrefab != null && spawnPoint != null)
        {
            Instantiate(unitPrefab, spawnPoint.position, unitPrefab.transform.rotation);
        }
    }
}