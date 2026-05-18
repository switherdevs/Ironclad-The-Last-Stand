using System.Collections.Generic;
using UnityEngine;

public class QuanLyDan : MonoBehaviour
{
    public static QuanLyDan Instance;

    public khodanan KhoDan;
    private List<GameObject> DanTrongKho = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject); // Xóa cái thừa đi nếu bị trùng
        }
    }
    void Start()
    {
        if (KhoDan == null || KhoDan.DanPrefab == null)
        {
            Debug.LogError("Chưa gán file cấu hình khodanan hoặc thiếu DanPrefab trong file!");
            return;
        }

        // Đọc số lượng từ file ScriptableObject để tạo sẵn đạn ẩn
        for (int i = 0; i < KhoDan.SoLuongDanBanDau; i++)
        {
            GameObject obj = Instantiate(KhoDan.DanPrefab);
            obj.SetActive(false); // Ẩn đạn đi, cất vào kho
            DanTrongKho.Add(obj);
        }
    }
    // Hàm lấy đạn rảnh từ kho ra để bắn
    public GameObject LayDanTuKho()
    {
        // 1. Tìm xem viên đạn nào đang ẩn (đang rảnh) thì lôi ra dùng
        for (int i = 0; i < DanTrongKho.Count; i++)
        {
            if (DanTrongKho[i] != null && !DanTrongKho[i].activeInHierarchy)
            {
                return DanTrongKho[i];
            }
        }

        // 2. Nếu lính bắn quá nhanh làm kho bị hết đạn, tự sinh thêm 1 viên mới để bù vào kho
        GameObject obj = Instantiate(KhoDan.DanPrefab);
        //obj.SetActive(false);
        DanTrongKho.Add(obj);
        return obj;
    }
    // Update is called once per frame
    void Update()
    {

    }
}