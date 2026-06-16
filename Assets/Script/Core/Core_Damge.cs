using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeThongSatThuongData", menuName = "Scriptable Objects/HeThongSatThuongData")]
public class HeThongSatThuongData : ScriptableObject
{
    [System.Serializable]
    public struct ChiSoSatThuongLinh
    {
        public string tenChungLinh;

        [Header("--- THÔNG SỐ GỐC ---")]
        public int mauGoc;
        public int satThuongGoc;
        public float heSoBoTro; // Hệ số bổ trợ gốc của bạn

        [Header("--- CẤP ĐỘ HIỆN TẠI ---")]
        public int capDoMau;
        public int capDoSatThuong;

        [Header("--- MẢNG CẤU HÌNH TỶ LỆ NÂNG CẤP ---")]
        public List<float> mangHeSoMau;
        public List<float> mangHeSoSatThuong;

        [Header("--- MẢNG CẤU HÌNH GIÁ TIỀN ---")]
        public List<int> mangGiaTienMau;
        public List<int> mangGiaTienSatThuong;
    }

    [Header("--- DANH SÁCH DỮ LIỆU TẤT CẢ CHỦNG LÍNH ---")]
    public List<ChiSoSatThuongLinh> danhSachSatThuong = new List<ChiSoSatThuongLinh>();

    // ================= HÀM LẤY SÁT THƯƠNG THỰC TẾ (ĐÃ SỬA LỖI MAX LEVEL) =================
    public int LaySatThuongTuChung(string tenChung)
    {
        for (int i = 0; i < danhSachSatThuong.Count; i++)
        {
            var chung = danhSachSatThuong[i];
            if (tenChung.Contains(chung.tenChungLinh))
            {
                float heSoNangCap = 1f;

                if (chung.mangHeSoSatThuong != null && chung.mangHeSoSatThuong.Count > 0)
                {
                    // Nếu cấp độ hiện tại nằm trong phạm vi mảng, lấy bình thường
                    if (chung.capDoSatThuong < chung.mangHeSoSatThuong.Count)
                    {
                        heSoNangCap = chung.mangHeSoSatThuong[chung.capDoSatThuong];
                    }
                    // CHỐT CHẶN: Nếu cấp độ bằng hoặc vượt quá độ dài mảng (Max Level), ép lấy phần tử cuối cùng
                    else
                    {
                        heSoNangCap = chung.mangHeSoSatThuong[chung.mangHeSoSatThuong.Count - 1];
                    }
                }

                return Mathf.RoundToInt(chung.satThuongGoc * chung.heSoBoTro * heSoNangCap);
            }
        }
        return 5; // Giá trị trả về mặc định nếu không tìm thấy chủng lính
    }

    // ================= HÀM LẤY MÁU THỰC TẾ (ĐÃ SỬA LỖI MAX LEVEL) =================
    public int LayMauTuChung(string tenChung)
    {
        string tenChungLower = tenChung.ToLower();
        for (int i = 0; i < danhSachSatThuong.Count; i++)
        {
            var chung = danhSachSatThuong[i];
            if (tenChungLower.Contains(chung.tenChungLinh.ToLower()))
            {
                float heSoNangCap = 1f;

                if (chung.mangHeSoMau != null && chung.mangHeSoMau.Count > 0)
                {
                    // Nếu cấp độ hiện tại nằm trong phạm vi mảng, lấy bình thường
                    if (chung.capDoMau < chung.mangHeSoMau.Count)
                    {
                        heSoNangCap = chung.mangHeSoMau[chung.capDoMau];
                    }
                    // CHỐT CHẶN: Nếu cấp độ bằng hoặc vượt quá độ dài mảng (Max Level), ép lấy phần tử cuối cùng
                    else
                    {
                        heSoNangCap = chung.mangHeSoMau[chung.mangHeSoMau.Count - 1];
                    }
                }

                return Mathf.RoundToInt(chung.mauGoc * heSoNangCap);
            }
        }
        return 20; // Giá trị trả về mặc định nếu không tìm thấy chủng lính
    }

    // ================= HÀM RESET CHỈ SỐ VỀ MẶC ĐỊNH (KHI KHÔNG CÓ FILE SAVE) =================
    public void ResetToanBoChiSoVeMocGoc()
    {
        for (int i = 0; i < danhSachSatThuong.Count; i++)
        {
            var cl = danhSachSatThuong[i];
            cl.capDoMau = 0;
            cl.capDoSatThuong = 0;
            danhSachSatThuong[i] = cl;
        }
        Debug.Log("[Data] Đã reset toàn bộ cấp độ nâng cấp lính về cấp 0.");
    }
}