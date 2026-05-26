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
            obj.name = KhoDan.DanPrefab.name; // Đặt tên chuẩn để lát nữa đối chiếu loại đạn
            obj.SetActive(false); // Ẩn đạn đi, cất vào kho
            DanTrongKho.Add(obj);
        }
    }
    // Hàm lấy đạn rảnh từ kho ra để bắn
    public GameObject LayDanTuKho(GameObject prefabLoaiDan)
    {
        if (prefabLoaiDan == null)
        {
            Debug.LogError("Chưa truyền Prefab loại đạn cần lấy vào hàm LayDanTuKho!");
            return null;
        }

        // 1. Tìm xem trong danh sách có viên đạn nào ĐANG ẨN và TRÙNG TÊN với Prefab này không
        for (int i = 0; i < DanTrongKho.Count; i++)
        {
            if (DanTrongKho[i] != null && !DanTrongKho[i].activeInHierarchy)
            {
                // So sánh tên đạn trong kho có chứa tên của Prefab cần lấy hay không
                if (DanTrongKho[i].name == prefabLoaiDan.name)
                {
                    return DanTrongKho[i]; // Lôi đúng loại đạn đó ra tái sử dụng
                }
            }
        }

        // 2. Nếu tìm khắp kho mà không có viên nào thuộc loại này đang rảnh -> Tự sinh thêm 1 viên mới đúng loại đó
        GameObject obj = Instantiate(prefabLoaiDan);
        obj.name = prefabLoaiDan.name; // Đặt tên chuẩn để các lần bắn sau có thể tìm thấy
        DanTrongKho.Add(obj);

        return obj;
    }
}