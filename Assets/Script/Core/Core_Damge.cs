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
        public int satThuongGoc;     // Lượng đam quy định ở Cấp 0
        public int mauGoc;           // Lượng máu quy định ở Cấp 0
        public float heSoBoTro;      // Giữ nguyên biến cũ cho đồ đạc sau này

        [Header("--- CẤP ĐỘ NÂNG CẤP HIỆN TẠI ---")]
        public int capDoSatThuong;
        public int capDoMau;

        [Header("--- CẤU HÌNH MẢNG HỆ SỐ NÂNG CẤP TÙY Ý ---")]
        [Tooltip("Mảng nhân sát thương. Ví dụ: Cấp 0 điền 1, Cấp 1 điền 1.2, Cấp 2 điền 1.5...")]
        public List<float> mangHeSoSatThuong;

        [Tooltip("Mảng nhân máu. Ví dụ: Cấp 0 điền 1, Cấp 1 điền 1.3, Cấp 2 điền 1.7...")]
        public List<float> mangHeSoMau;

        [Header("--- CẤU HÌNH MẢNG GIÁ TIỀN CHO TỪNG CẤP ---")]
        [Tooltip("Số tiền tốn để nâng lên cấp tiếp theo của SÁT THƯƠNG (Cấp 0 lên 1, Cấp 1 lên 2...)")]
        public List<int> mangGiaTienSatThuong;

        [Tooltip("Số tiền tốn để nâng lên cấp tiếp theo của MÁU (Cấp 0 lên 1, Cấp 1 lên 2...)")]
        public List<int> mangGiaTienMau;
    }

    [Header("--- BẢNG TRA CỨU SÁT THƯƠNG TOÀN GAME ---")]
    public List<ThongTinChungLinh> danhSachSatThuong = new List<ThongTinChungLinh>();

    // Hàm tự động lọc và lấy ra đúng sát thương dựa vào Tên chủng lính và nhân hệ số nâng cấp
    public int LaySatThuongTuChung(string tenChung)
    {
        foreach (var chung in danhSachSatThuong)
        {
            // Nếu tên vật va chạm chứa tên chủng lính trong bảng
            if (tenChung.Contains(chung.tenChungLinh))
            {
                float heSoNangCap = 1f;
                // Kiểm tra nếu có điền mảng hệ số nâng cấp và cấp độ hiện tại nằm trong mảng
                if (chung.mangHeSoSatThuong != null && chung.capDoSatThuong < chung.mangHeSoSatThuong.Count)
                {
                    heSoNangCap = chung.mangHeSoSatThuong[chung.capDoSatThuong];
                }

                // Tính toán sát thương kèm hệ số bổ trợ đồ đạc VÀ hệ số nâng cấp mảng
                return Mathf.RoundToInt(chung.satThuongGoc * chung.heSoBoTro * heSoNangCap);
            }
        }
        return 5;
    }

    // Hàm tự động lọc và lấy ra đúng máu tối đa dựa vào Tên chủng lính và nhân hệ số nâng cấp
    public int LayMauTuChung(string tenChung)
    {
        string tenChungLower = tenChung.ToLower();
        foreach (var chung in danhSachSatThuong)
        {
            if (tenChungLower.Contains(chung.tenChungLinh.ToLower()))
            {
                float heSoNangCap = 1f;
                // Kiểm tra nếu có điền mảng hệ số nâng cấp và cấp độ hiện tại nằm trong mảng
                if (chung.mangHeSoMau != null && chung.capDoMau < chung.mangHeSoMau.Count)
                {
                    heSoNangCap = chung.mangHeSoMau[chung.capDoMau];
                }

                // Tính toán lượng máu tối đa sau khi nhân hệ số nâng cấp mảng
                return Mathf.RoundToInt(chung.mauGoc * heSoNangCap);
            }
        }
        return 20; // Máu mặc định dự phòng nếu không tìm thấy trong bảng
    }
}