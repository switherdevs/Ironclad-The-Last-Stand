using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để tải lại Scene

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

    // ================= KHU VỰC XÓA FILE SAVE GAME & RESET RAM TUYỆT ĐỐI =================
    public void XoaFileSaveGame()
    {
        // 1. Xóa file vật lý trên ổ cứng
        if (File.Exists(duongDanFile))
        {
            File.Delete(duongDanFile);
            Debug.Log("<color=red><b>[SaveSystem]</b> Đã xóa file savegame.txt thành công!</color>");
        }
        else
        {
            Debug.LogWarning("[SaveSystem] Không tìm thấy file save để xóa.");
        }

        // 2. 🔥 ÉP GAME TẢI LẠI MENU: Xóa sạch các biến cũ đang chạy ngầm trong RAM
        // Điều này đảm bảo toàn bộ thông số lính, tiền, tướng quay về mặc định 100% trước khi vào trận
        string sceneHienTai = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneHienTai);
        Debug.Log($"<color=yellow><b>[SaveSystem]</b> Đã reload lại Scene {sceneHienTai} để đồng bộ RAM!</color>");
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

    // ================= KHU VỰC LƯU / ĐỌC TIẾN TRÌNH MAP =================
    public void LuuTienTrinhMap(int mapIndex)
    {
        List<string> lines = DocToanBoFile();

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
        return 1;
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

        lines.RemoveAll(l => l.StartsWith($"NangCapLinh:{indexLinh}|"));
        lines.Add($"NangCapLinh:{indexLinh}|{capMau}|{capSt}|{mauGoc}|{stGoc}");

        GhiToanBoFile(lines);
    }

    public string DocNangCapLinh(int indexLinh)
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith($"NangCapLinh:{indexLinh}|"));
        if (dongTimThay != null)
        {
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

    // ================= KHU VỰC LƯU / ĐỌC TƯỚNG CHỌN =================
    public void LuuTuongDaChon(int idTuong)
    {
        List<string> lines = DocToanBoFile();
        lines.RemoveAll(l => l.StartsWith("TuongDaChon:"));
        lines.Add($"TuongDaChon:{idTuong}");
        GhiToanBoFile(lines);
        Debug.Log($"[SAVE HERO] Đã lưu ID tướng đã chọn vào file: {idTuong}");
    }

    public int DocTuongDaChon()
    {
        List<string> lines = DocToanBoFile();
        string dongTimThay = lines.FirstOrDefault(l => l.StartsWith("TuongDaChon:"));
        if (dongTimThay != null)
        {
            return int.Parse(dongTimThay.Split(':').Last());
        }
        return 0;
    }
}