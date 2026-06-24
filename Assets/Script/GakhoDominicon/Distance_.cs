using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý đội hình ma trận động:
/// Tự động xếp lính theo các hàng dọc dựa vào cấu hình từng lớp nhân vật,
/// Có cơ chế tự động dồn hàng lên phía trước khi hàng trước chết sạch (Stick War style).
/// Đã được tối ưu hóa hiển thị trục Y chống chồng chéo hình ảnh khi lính xuất hiện thời gian thực.
/// </summary>
public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance { get; private set; }

    [System.Serializable]
    public class CauHinhLopLinh
    {
        public string tenLoaiLinh;         // Tên chủng lính để hiển thị (Titan, KhoGrak...)
        public int capBacRank;             // Chỉ số phân lớp (0, 1, 2, 3...)

        [Header("Tùy chỉnh Vị Trí & Khoảng Cách Riêng")]
        public float toaDoXBatDau;         // Vị trí X (ngang) xuất phát riêng của loại lính này
        public float toaDoYBatDau;         // Vị trí Y (dọc) xuất phát riêng của loại lính này ở đỉnh hàng
        public float gianCachDocY = 1.2f;  // Khoảng cách giữa các lính trong cùng một hàng dọc của loại này
        public float gianCachNgangX = 2.5f;// Khoảng cách giữa hàng trước và hàng sau của loại này

        public int soLuongHangToiDa = 6;   // Số lính tối đa trên 1 hàng dọc của lớp này
    }

    [Header("--- CẤU HÌNH TỪNG LOẠI LÍNH ---")]
    public CauHinhLopLinh[] danhSachCauHinh = new CauHinhLopLinh[]
    {
        // Bạn có thể tùy chỉnh X, Y, khoảng cách dọc/ngang riêng biệt ngay trên Inspector
        new CauHinhLopLinh { tenLoaiLinh = "Titan",      capBacRank = 4, toaDoXBatDau = -2f, toaDoYBatDau = -1.5f, gianCachDocY = 3.0f, gianCachNgangX = 4.0f, soLuongHangToiDa = 1 },
        new CauHinhLopLinh { tenLoaiLinh = "KhoGrak",    capBacRank = 0, toaDoXBatDau = -4f, toaDoYBatDau = -1.5f, gianCachDocY = 1.2f, gianCachNgangX = 2.5f, soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "IronStorm",  capBacRank = 1, toaDoXBatDau = -8f, toaDoYBatDau = -1.5f, gianCachDocY = 1.2f, gianCachNgangX = 2.5f, soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "Terminator", capBacRank = 2, toaDoXBatDau = -12f,toaDoYBatDau = -1.5f, gianCachDocY = 1.5f, gianCachNgangX = 3.0f, soLuongHangToiDa = 4 },
        new CauHinhLopLinh { tenLoaiLinh = "DeadIron",   capBacRank = 3, toaDoXBatDau = -16f,toaDoYBatDau = -1.5f, gianCachDocY = 1.5f, gianCachNgangX = 3.0f, soLuongHangToiDa = 4 },
    };

    [Header("--- TỐC ĐỘ ĐỘI HÌNH ---")]
    public float tocDoDiChuyenVeSlot = 5.0f;   // Tốc độ di chuyển mượt của lính về ô
    public float saiSoDichDen = 0.15f;         // Ngưỡng dừng để triệt tiêu dao động giật giật

    // Lớp nội bộ biểu diễn trạng thái của một ô quân trong ma trận dọc
    private class ODoQuan
    {
        public int hangNgangThuMay;     // Chỉ số hàng hiện tại sau khi dồn (0 = hàng đầu, 1 = hàng sau...)
        public int viTriTrongHang;      // Vị trí đứng trong hàng đó (0 -> soLuongHangToiDa - 1)
        public int instanceID;          // ID của Instance lính đang giữ ô này (0 nếu ô trống)
        public Vector2 toaDoGocKhaiSinh;// Vị trí tính toán gốc ban đầu
    }

    // Bộ nhớ quản lý dữ liệu linh hoạt bằng Dictionary để tăng tốc độ truy xuất
    private readonly Dictionary<int, List<ODoQuan>> _khoOQuanTheoRank = new Dictionary<int, List<ODoQuan>>();
    private readonly Dictionary<int, ODoQuan> _linhNaoOAnDo = new Dictionary<int, ODoQuan>();
    private readonly Dictionary<int, CauHinhLopLinh> _soDoCauHinh = new Dictionary<int, CauHinhLopLinh>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Khởi tạo sơ đồ cấu hình dữ liệu tĩnh ban đầu
        foreach (var cauHinh in danhSachCauHinh)
        {
            _soDoCauHinh[cauHinh.capBacRank] = cauHinh;
            _khoOQuanTheoRank[cauHinh.capBacRank] = new List<ODoQuan>();
        }
    }

    // ── PUBLIC API ĐIỀU KHIỂN XẾP HÀNG ───────────────────────────────────────────────────

    public bool Register(GameObject go, int rank)
    {
        if (go == null) return false;
        int idLinh = go.GetInstanceID();

        // Nếu lính đã đăng ký rồi thì không xử lý trùng lặp
        if (_linhNaoOAnDo.ContainsKey(idLinh)) return true;

        if (!_soDoCauHinh.TryGetValue(rank, out CauHinhLopLinh ch)) return false;

        List<ODoQuan> danhSachOQuan = _khoOQuanTheoRank[ch.capBacRank];
        ODoQuan oTrongTimThay = null;

        // Tìm một ô trống cũ (lính trước đó đã chết giải phóng) để tái sử dụng
        foreach (var oQuan in danhSachOQuan)
        {
            if (oQuan.instanceID == 0)
            {
                oTrongTimThay = oQuan;
                break;
            }
        }

        // Nếu không tìm thấy ô trống cũ, sinh ra ô mới ở hàng sau
        if (oTrongTimThay == null)
        {
            int tongSoOAnhEm = danhSachOQuan.Count;
            int hangDuKien = tongSoOAnhEm / ch.soLuongHangToiDa;
            int viTriDuKien = tongSoOAnhEm % ch.soLuongHangToiDa;

            oTrongTimThay = new ODoQuan
            {
                hangNgangThuMay = hangDuKien,
                viTriTrongHang = viTriDuKien,
                toaDoGocKhaiSinh = TinhToaDoGoc(ch, hangDuKien, viTriDuKien)
            };
            danhSachOQuan.Add(oTrongTimThay);
        }

        // Gán ID của lính vào ô dữ liệu này
        oTrongTimThay.instanceID = idLinh;
        _linhNaoOAnDo[idLinh] = oTrongTimThay;

        // Gọi cập nhật dồn hàng lập tức để phân bổ lại vị trí
        CapNhatThuatToanDonHang(ch.capBacRank);
        return true;
    }

    public void Unregister(GameObject go)
    {
        if (go == null) return;
        int idLinh = go.GetInstanceID();

        if (!_linhNaoOAnDo.TryGetValue(idLinh, out ODoQuan oQuan)) return;

        oQuan.instanceID = 0; // Đánh dấu giải phóng ô đứng ngay lập tức
        _linhNaoOAnDo.Remove(idLinh); // Bẻ gãy liên kết để lính chết không bao giờ tìm thấy ô nữa

        // Kích hoạt thuật toán dồn hàng cho những con còn sống ở phía sau tiến lên
        foreach (var cap in _khoOQuanTheoRank)
        {
            if (cap.Value.Contains(oQuan))
            {
                CapNhatThuatToanDonHang(cap.Key);
                break;
            }
        }
    }

    public bool TryGetSlot(GameObject go, out Vector2 pos)
    {
        pos = Vector2.zero;
        if (go == null) return false;

        // Kiểm tra xem con lính này có còn trong từ điển quản lý không
        if (_linhNaoOAnDo.TryGetValue(go.GetInstanceID(), out ODoQuan oQuan))
        {
            // Nếu ô này đã bị gán về 0 (tức là lính đã gọi Unregister hoặc chết), từ chối trả về vị trí
            if (oQuan.instanceID == 0) return false;

            if (_soDoCauHinh.TryGetValue(LayRankCuaOQuan(oQuan), out CauHinhLopLinh ch))
            {
                pos = TinhToaDoThucTeHienTai(ch, oQuan);
                return true;
            }
        }
        return false;
    }

    public Vector2 GetSlotVelocity(GameObject go, float overrideSpeed = -1f)
    {
        if (go == null) return Vector2.zero;

        // Nếu không lấy được Slot hợp lệ (do lính đã chết), trả về vận tốc bằng 0 lập tức
        Vector2 viTriDich;
        if (!TryGetSlot(go, out viTriDich))
        {
            return Vector2.zero;
        }

        Vector2 huongDi = viTriDich - (Vector2)go.transform.position;
        if (huongDi.magnitude <= saiSoDichDen) return Vector2.zero;

        float tocDo = overrideSpeed > 0f ? overrideSpeed : tocDoHienTaiKhac(go, huongDi.magnitude);
        return huongDi.normalized * tocDo;
    }

    // ── THUẬT TOÁN ĐẨY HÀNG STICK WAR ──────────────────────────────────────────────

    private void CapNhatThuatToanDonHang(int rank)
    {
        List<ODoQuan> danhSachO = _khoOQuanTheoRank[rank];
        if (danhSachO.Count == 0) return;

        int cấuHìnhMaxHàng = _soDoCauHinh[rank].soLuongHangToiDa;
        int soLuongHangHienTai = 0;
        foreach (var o in danhSachO)
        {
            int hangGoc = danhSachO.IndexOf(o) / cấuHìnhMaxHàng;
            if (hangGoc > soLuongHangHienTai) soLuongHangHienTai = hangGoc;
        }

        // Tạo bảng trạng thái kiểm tra xem hàng đó hiện tại có lính sống hay không
        Dictionary<int, bool> hangNayConLinhSong = new Dictionary<int, bool>();
        for (int h = 0; h <= soLuongHangHienTai; h++) hangNayConLinhSong[h] = false;

        foreach (var o in danhSachO)
        {
            int hangGoc = danhSachO.IndexOf(o) / cấuHìnhMaxHàng;
            if (o.instanceID != 0)
            {
                hangNayConLinhSong[hangGoc] = true; // Hàng này có ít nhất một con lính còn sống
            }
        }

        // Đẩy lùi các chỉ số hàng thực tế dựa trên số lượng các hàng trống phía trước nó
        foreach (var o in danhSachO)
        {
            int hangGocBanDau = danhSachO.IndexOf(o) / cấuHìnhMaxHàng;
            int soHangTrongPhiaTruoc = 0;

            for (int h = 0; h < hangGocBanDau; h++)
            {
                if (!hangNayConLinhSong[h])
                {
                    soHangTrongPhiaTruoc++; // Đếm số hàng phía trước bị chết sạch hoàn toàn
                }
            }

            // Gán lại vị trí hàng hiển thị thực tế cho ô
            o.hangNgangThuMay = hangGocBanDau - soHangTrongPhiaTruoc;
        }
    }

    // ── TOÁN HỌC ĐỘI HÌNH (XẾP TỪ TRÊN XUỐNG + MICRO-OFFSET CHỐNG TRÙNG LAYER) ──────

    private Vector2 TinhToaDoGoc(CauHinhLopLinh ch, int hang, int viTri)
    {
        // Đã sử dụng biến tọa độ độc lập của cấu hình (ch) thay vì biến toàn cục
        float xGoc = ch.toaDoXBatDau - (hang * ch.gianCachNgangX);
        float yGoc = ch.toaDoYBatDau - (viTri * ch.gianCachDocY) - (hang * 0.001f);
        return new Vector2(xGoc, yGoc);
    }

    private Vector2 TinhToaDoThucTeHienTai(CauHinhLopLinh ch, ODoQuan o)
    {
        // Tính toán vị trí X và Y linh hoạt theo từng cấu hình của chủng lính
        float xThucTe = ch.toaDoXBatDau - (o.hangNgangThuMay * ch.gianCachNgangX);
        float yThucTe = ch.toaDoYBatDau - (o.viTriTrongHang * ch.gianCachDocY) - (o.hangNgangThuMay * 0.001f);

        // Tạo hiệu ứng so le nhẹ giữa các hàng đối với các đội hình đông
        if (ch.soLuongHangToiDa > 2 && o.hangNgangThuMay % 2 != 0)
        {
            yThucTe -= ch.gianCachDocY * 0.3f;
        }

        return new Vector2(transform.position.x + xThucTe, transform.position.y + yThucTe);
    }

    private int LayRankCuaOQuan(ODoQuan oTarget)
    {
        foreach (var cap in _khoOQuanTheoRank)
        {
            if (cap.Value.Contains(oTarget)) return cap.Key;
        }
        return 0;
    }

    private float tocDoHienTaiKhac(GameObject go, float kc)
    {
        if (kc > 5f) return tocDoDiChuyenVeSlot * 1.5f;
        return tocDoDiChuyenVeSlot;
    }
}