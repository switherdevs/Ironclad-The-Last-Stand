using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveSystem : MonoBehaviour
{
    private string duongDanFile;

    private void Awake()
    {
        // Đường dẫn lưu file .txt an toàn trên cả Máy tính và Điện thoại
        duongDanFile = Path.Combine(Application.persistentDataPath, "savegame.txt");
    }

    // ================= KHU VỰC LƯU / ĐỌC TIỀN NÂNG CẤP LÍNH =================

    public void LuuThongTinGame(int soTien)
    {
        List<string> lines = DocToanBoFile();

        // Xóa dòng lưu tiền cũ nếu đã tồn tại để ghi đè dòng mới
        lines.RemoveAll(l => l.StartsWith("TienNangCap:"));
        lines.Add($"TienNangCap:{soTien}");

        GhiToanBoFile(lines);
        Debug.Log($"[SAVE] Đã lưu tiền nâng cấp: {soTien}");
    }

    public int DocThongTinGame()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("TienNangCap:"));

        if (dongTimThay != null)
        {
            string giaTri = dongTimThay.Split(':').Last();
            return int.Parse(giaTri);
        }

        return 0; // Mặc định trả về 0 nếu chưa có dữ liệu tiền
    }

    // ================= KHU VỰC LƯU / ĐỌC ĐỘ KHÓ (KẾT NỐI CHAOS DIRECTOR) =================

    public void LuuDoKhoGame(int doKhoIndex)
    {
        List<string> lines = DocToanBoFile();

        // Xóa dòng lưu độ khó cũ nếu đã tồn tại để ghi đè dòng mới
        lines.RemoveAll(l => l.StartsWith("DoKho:"));
        lines.Add($"DoKho:{doKhoIndex}");

        GhiToanBoFile(lines);
        Debug.Log($"[SAVE] Đã lưu chỉ số độ khó: {doKhoIndex}");
    }

    public int DocDoKhoGame()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("DoKho:"));

        if (dongTimThay != null)
        {
            string giaTri = dongTimThay.Split(':').Last();
            return int.Parse(giaTri);
        }

        return 1; // Mặc định trả về 1 (tương ứng với DoKho.Normal) nếu file trống
    }

    // ================= CÁC HÀM BỔ TRỢ ĐỌC/GHI FILE .TXT DẠNG LIST ĐỂ GIỮ NGUYÊN DỮ LIỆU ĐÃ CÓ =================

    private List<string> DocToanBoFile()
    {
        if (!File.Exists(duongDanFile))
        {
            return new List<string>();
        }
        return File.ReadAllLines(duongDanFile).ToList();
    }

    private void GhiToanBoFile(List<string> lines)
    {
        File.WriteAllLines(duongDanFile, lines);
    }
}