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
        public float heSoBoTro;

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

    // 🎯 BIẾN TĨNH TOÀN CỤC: Quản lý hệ số buff của Chaplain (Mặc định bằng 1f tức là không buff)
    public static float heSoBuffChaplain = 1.0f;

    // ================= HÀM LẤY SÁT THƯƠNG TỔNG LỰC TRONG TRẬN =================
    public int LaySatThuongTuChung(string testChung)
    {
        for (int i = 0; i < danhSachSatThuong.Count; i++)
        {
            var chung = danhSachSatThuong[i];
            if (testChung.Contains(chung.tenChungLinh))
            {
                float heSoNangCap = 1f;

                if (chung.mangHeSoSatThuong != null && chung.mangHeSoSatThuong.Count > 0)
                {
                    if (chung.capDoSatThuong < chung.mangHeSoSatThuong.Count)
                    {
                        heSoNangCap = chung.mangHeSoSatThuong[chung.capDoSatThuong];
                    }
                    else
                    {
                        heSoNangCap = chung.mangHeSoSatThuong[chung.mangHeSoSatThuong.Count - 1];
                    }
                }

                // 🎯 CÔNG THỨC TỔNG: Tính toán sát thương gốc, nâng cấp, và nhân thêm hệ số Buff từ Chaplain tại đây
                return Mathf.RoundToInt(chung.satThuongGoc * chung.heSoBoTro * heSoNangCap * heSoBuffChaplain);
            }
        }
        return 5;
    }

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
                    if (chung.capDoMau < chung.mangHeSoMau.Count)
                    {
                        heSoNangCap = chung.mangHeSoMau[chung.capDoMau];
                    }
                    else
                    {
                        heSoNangCap = chung.mangHeSoMau[chung.mangHeSoMau.Count - 1];
                    }
                }

                return Mathf.RoundToInt(chung.mauGoc * heSoNangCap);
            }
        }
        return 20;
    }

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