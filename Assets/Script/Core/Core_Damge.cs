using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HeThongSatThuongData", menuName = "Game/He Thong Sat Thuong")]
public class HeThongSatThuongData : ScriptableObject
{
    // Cấu trúc thông tin của 1 chủng lính
    [System.Serializable]
    public struct ThongTinChungLinh
    {
        public string tenChungLinh;  // Ghi đúng tên Prefab lính (Ví dụ: Titan, IronStorm...)
        public int satThuongGoc;     // Lượng đam quy định
        public float heSoBoTro;      // Biến tăng sát thương khi dùng đồ (để dành sau này)
    }

    [Header("--- BẢNG TRA CỨU SÁT THƯƠNG TOÀN GAME ---")]
    public List<ThongTinChungLinh> danhSachSatThuong = new List<ThongTinChungLinh>();

    // Hàm tự động lọc và lấy ra đúng sát thương dựa vào Tên chủng lính
    public int LaySatThuongTuChung(string tenChung)
    {
        foreach (var chung in danhSachSatThuong)
        {
            // Nếu tên vật va chạm chứa tên chủng lính trong bảng
            if (tenChung.Contains(chung.tenChungLinh))
            {
                // Tính toán sát thương kèm hệ số bổ trợ đồ đạc nếu có
                return Mathf.RoundToInt(chung.satThuongGoc * chung.heSoBoTro);
            }
        }
        return 5; 
    }
}