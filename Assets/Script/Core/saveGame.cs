using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveSystem : MonoBehaviour
{
    private string duongDanFile;

    private void Awake()
    {
        duongDanFile = Path.Combine(Application.persistentDataPath, "savegame.txt");
    }

    // Hàm kiểm tra xem đã từng có dữ liệu save cũ chưa để quản lý nút Continue
    public bool KiemTraCoFileSave()
    {
        return File.Exists(duongDanFile) && DocToanBoFile().Count > 0;
    }

    // ================= KHU VỰC LƯU / ĐỌC TIỀN NÂNG CẤP LÍNH =================
    public void LuuThongTinGame(int soTien)
    {
        List<string> lines = DocToanBoFile();
        lines.RemoveAll(l => l.StartsWith("TienNangCap:"));
        lines.Add($"TienNangCap:{soTien}");
        GhiToanBoFile(lines);
        Debug.Log($"[SAVE] Đã lưu tiền nâng cấp mới vào file txt: {soTien}");
    }

    public int DocThongTinGame()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("TienNangCap:"));
        if (dongTimThay != null)
        {
            return int.Parse(dongTimThay.Split(':').Last());
        }
        return 0;
    }

    // ================= KHU VỰC LƯU / ĐỌC TIẾN TRÌNH MAP (MỚI) =================
    public void LuuTienTrinhMap(int mapIndex)
    {
        List<string> lines = DocToanBoFile();

        // Chỉ nâng cấp tiến trình nếu Map mới vượt qua Map cũ đã lưu
        int mapCu = DocTienTrinhMap();
        if (mapIndex > mapCu)
        {
            lines.RemoveAll(l => l.StartsWith("TienTrinhMap:"));
            lines.Add($"TienTrinhMap:{mapIndex}");
            GhiToanBoFile(lines);
            Debug.Log($"[SAVE PROGRESS] Đã mở khóa tới Map: {mapIndex}");
        }
    }

    public int DocTienTrinhMap()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("TienTrinhMap:"));
        if (dongTimThay != null)
        {
            return int.Parse(dongTimThay.Split(':').Last());
        }
        return 1; // Mặc định mới chơi thì chỉ được chọn Map 1
    }

    // ================= KHU VỰC LƯU / ĐỌC ĐỘ KHÓ =================
    public void LuuDoKhoGame(int doKhoIndex)
    {
        List<string> lines = DocToanBoFile();
        lines.RemoveAll(l => l.StartsWith("DoKho:"));
        lines.Add($"DoKho:{doKhoIndex}");
        GhiToanBoFile(lines);
    }

    public int DocDoKhoGame()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("DoKho:"));
        if (dongTimThay != null)
        {
            return int.Parse(dongTimThay.Split(':').Last());
        }
        return 1;
    }

    // ================= KHU VỰC LƯU / ĐỌC THÔNG TIN NÂNG CẤP LÍNH =================
    public void LuuNangCapLinh(int indexLinh, int capMau, int capSt, int mauGoc, int stGoc)
    {
        List<string> lines = DocToanBoFile();

        // Tìm và xóa dòng lưu cũ của con lính này dựa theo số thứ tự (index) tránh trùng lặp dòng
        lines.RemoveAll(l => l.StartsWith($"NangCapLinh:{indexLinh}|"));

        // Thêm dòng dữ liệu mới cấu trúc mảng vào danh sách txt
        lines.Add($"NangCapLinh:{indexLinh}|{capMau}|{capSt}|{mauGoc}|{stGoc}");

        GhiToanBoFile(lines);
    }

    public string DocNangCapLinh(int indexLinh)
    {
        List<string> lines = DocToanBoFile();
        // Tìm đúng dòng bắt đầu bằng index của con lính cần tìm
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith($"NangCapLinh:{indexLinh}|"));
        if (dongTimThay != null)
        {
            // Trả về chuỗi chứa toàn bộ thông số sau dấu hai chấm để giải mã
            return dongTimThay.Split(':').Last();
        }
        return null;
    }

    // ================= HÀM ĐỌC/GHI FILE HỆ THỐNG GỐC =================
    private List<string> DocToanBoFile()
    {
        if (!File.Exists(duongDanFile)) return new List<string>();
        return File.ReadAllLines(duongDanFile).ToList();
    }

    private void GhiToanBoFile(List<string> lines)
    {
        File.WriteAllLines(duongDanFile, lines);
    }
}