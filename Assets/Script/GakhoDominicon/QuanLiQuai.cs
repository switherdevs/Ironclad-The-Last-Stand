using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    // Danh sách lưu trữ kẻ địch
    public List<GameObject> danhSachDich = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy(GameObject Enemy)
    {
        if (!danhSachDich.Contains(Enemy)) danhSachDich.Add(Enemy);
    }

    public void UnregisterEnemy(GameObject Enemy)
    {
        if (danhSachDich.Contains(Enemy)) danhSachDich.Remove(Enemy);
    }
}