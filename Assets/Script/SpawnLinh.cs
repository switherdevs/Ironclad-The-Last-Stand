using UnityEngine;
using UnityEngine.UI;

public class SpawnLinh : MonoBehaviour
{
    [Header("--- VỊ TRÍ XUẤT HIỆN ---")]

    public Transform spawnPoint;

    [Header("--- DANH SÁCH PREFABS LÍNH ---")]
    public GameObject SevitorPrefab;
    public GameObject khograkGuardPrefab;
    public GameObject ironStormMarinePrefab;
    public GameObject stormTerminatorPrefab;
    public GameObject ironDreadWalkerPrefab;
    public GameObject dominiconTitanPrefab;
    private float KhoangNghi;
    public void Sevitor()
    {
        SpawnUnit(SevitorPrefab);
    }

    public void SpawnKhograkGuard()
    {
        SpawnUnit(khograkGuardPrefab);
    }

    public void SpawnIronStormMarine()
    {
        SpawnUnit(ironStormMarinePrefab);
    }

    public void SpawnStormTerminator()
    {
        SpawnUnit(stormTerminatorPrefab);
    }

    public void SpawnIronDreadWalker()
    {
        SpawnUnit(ironDreadWalkerPrefab);
    }

    public void SpawnDominiconTitan()
    {
        SpawnUnit(dominiconTitanPrefab);
    }

    private void SpawnUnit(GameObject unitPrefab)
    {
        if (unitPrefab != null && spawnPoint != null)
        {
            Instantiate(unitPrefab, spawnPoint.position, unitPrefab.transform.rotation);
        }
        else
        {
            Debug.LogError("Thiếu Prefab lính hoặc chưa kéo Spawn Point vào Script!");
        }
    }
}