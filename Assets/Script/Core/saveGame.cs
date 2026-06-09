using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    private string duongDanFile;

    private void Awake()
    {
        duongDanFile = Path.Combine(Application.persistentDataPath, "savegame.txt");
    }

    public void LuuThongTinGame(int tienNangCapLinh)
    {
        List<string> duLieuLuu = new List<string>
        {
            $"TienNangCapLinh:{tienNangCapLinh}"
        };

        File.WriteAllLines(duongDanFile, duLieuLuu);
        Debug.Log("Da luu file tai: " + duongDanFile);
    }

    public int DocThongTinGame()
    {
        if (!File.Exists(duongDanFile))
        {
            Debug.LogWarning("Chua co file save nao ton tai, khoi tao mac dinh bang 0.");
            return 0;
        }

        string[] tatCaCacDong = File.ReadAllLines(duongDanFile);

        // SỬA TẠI ĐÂY: Chuyển hết dòng chữ về chữ thường (.ToLower()) trước khi so sánh 
        // để tránh hoàn toàn lỗi lệch chữ hoa / chữ thường chí mạng
        string dongChuaDuLieu = tatCaCacDong.FirstOrDefault(dong =>
            dong.ToLower().Trim().StartsWith("tiennangcaplinh:"));

        if (string.IsNullOrEmpty(dongChuaDuLieu))
        {
            Debug.LogWarning("Tim thay file save nhung khong co dong chua du lieu TienNangCapLinh.");
            return 0;
        }

        string giaTriChu = dongChuaDuLieu.Split(':').LastOrDefault();

        if (int.TryParse(giaTriChu, out int ketQua))
        {
            Debug.Log($"[THANH CÔNG] Tai file save thanh con! So tien doc duoc: {ketQua}");
            return ketQua;
        }

        return 0;
    }
}