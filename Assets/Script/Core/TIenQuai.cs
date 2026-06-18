using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ThuongTienQuaiData", menuName = "ScriptableObjects/ThuongTienQuaiData")]
public class ThuongTienQuaiData : ScriptableObject
{
    [System.Serializable]
    public class CauHinhTienQuai
    {
        [Tooltip("Từ khóa trong tên Prefab quái (ví dụ: terminator, khograk)")]
        public string tuKhoaTenQuai;
        [Tooltip("Số tiền thưởng nhận được khi con quái này chết")]
        public int soTienThuong = 30;
    }

    [Header("--- DANH SÁCH PHẦN THƯỞNG CỦA CÁC LOẠI QUÁI ---")]
    public List<CauHinhTienQuai> danhSachThuongQuai;

    /// <summary>
    /// Hàm tự động tìm tiền thưởng dựa trên tên quái
    /// </summary>
    public int LayTienThuongTuTenQuai(string tenQuai)
    {
        if (danhSachThuongQuai == null || string.IsNullOrEmpty(tenQuai)) return 30; // Mặc định trả về 30 nếu lỗi

        string tenQuaiThongThuong = tenQuai.ToLower();

        foreach (var cauHinh in danhSachThuongQuai)
        {
            if (!string.IsNullOrEmpty(cauHinh.tuKhoaTenQuai) && tenQuaiThongThuong.Contains(cauHinh.tuKhoaTenQuai.ToLower()))
            {
                return cauHinh.soTienThuong;
            }
        }

        return 30; // Trả về 30 nếu không tìm thấy từ khóa nào khớp
    }
}