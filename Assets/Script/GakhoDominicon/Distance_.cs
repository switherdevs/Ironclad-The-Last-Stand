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
        public float doLuiXMacDinh;        // Khoảng cách lùi mặc định ban đầu của lớp lính này (trục X âm)
        public int soLuongHangToiDa = 6;   // Số lính tối đa trên 1 hàng của lớp này
    }

    [Header("--- CẤU HÌNH TỪNG LOẠI LÍNH ---")]
    public CauHinhLopLinh[] danhSachCauHinh = new CauHinhLopLinh[]
    {
        new CauHinhLopLinh { tenLoaiLinh = "Titan",      capBacRank = 4, doLuiXMacDinh =  0f,   soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "KhoGrak",    capBacRank = 0, doLuiXMacDinh = -4.0f, soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "IronStorm",  capBacRank = 1, doLuiXMacDinh = -8.0f, soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "Terminator", capBacRank = 2, doLuiXMacDinh = -12.0f,soLuongHangToiDa = 6 },
        new CauHinhLopLinh { tenLoaiLinh = "DeadIron",   capBacRank = 3, doLuiXMacDinh = -16.0f,soLuongHangToiDa = 6 },
    };

    [Header("--- THIẾT LẬP KÍCH THƯỚC ĐỘI HÌNH ---")]
    public float toaDoXHangDau = -2f;         // Vị trí X của hàng đầu tiên tiên phong
    public float toaDoYTrungTam = -1.5f;       // Điểm bắt đầu của vị trí lính đầu tiên trên đỉnh hàng dọc Y
    public float gianCachDocY = 1.2f;          // Khoảng cách giữa các lính trong cùng một hàng dọc
    public float gianCachNgangX = 2.5f;        // Khoảng cách giữa hàng trước và hàng sau
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

    /// <summary>
    /// Đăng ký một con lính mới xuất hiện vào hệ thống ma trận đội hình
    /// </summary>
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

    /// <summary>
    /// Hủy đăng ký giải phóng ô khi lính chết
    /// </summary>
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

    /// <summary>
    /// Hàm lấy vị trí Slot thực tế theo Tọa độ thế giới (World Position)
    /// </summary>
    public bool TryGetSlot(GameObject go, out Vector2 pos)
    {
        pos = Vector2.zero;
        if (go == null) return false;

        // Kiểm tra xem con lính này có còn trong từ điển quản lý không
        if (_linhNaoOAnDo.TryGetValue(go.GetInstanceID(), out ODoQuan oQuan))
        {
            // Nếu ô này đã bị gán về 0 (tức là lính đã gọi Unregister hoặc chết), từ chối trả về vị trí di chuyển
            if (oQuan.instanceID == 0) return false;

            if (_soDoCauHinh.TryGetValue(LayRankCuaOQuan(oQuan), out CauHinhLopLinh ch))
            {
                pos = TinhToaDoThucTeHienTai(ch, oQuan);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Hàm tính toán vector vận tốc (Velocity) mượt mà để lính bám theo Slot của mình
    /// </summary>
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

    /// <summary>
    /// Thuật toán quét và dồn hàng tự động khi có lính chết ở hàng trước
    /// </summary>
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
        float xGoc = toaDoXHangDau + ch.doLuiXMacDinh - (hang * gianCachNgangX);

        // Vị trí đầu tiên (viTri = 0) nằm tại đỉnh trên, các lính tiếp theo trừ dần Y để xếp dịch xuống dưới.
        // Bổ sung thêm micro-offset để phân tách lớp hiển thị cơ học tuyệt đối.
        float yGoc = toaDoYTrungTam - (viTri * gianCachDocY) - (hang * 0.001f);
        return new Vector2(xGoc, yGoc);
    }

    private Vector2 TinhToaDoThucTeHienTai(CauHinhLopLinh ch, ODoQuan o)
    {
        float xThucTe = toaDoXHangDau + ch.doLuiXMacDinh - (o.hangNgangThuMay * gianCachNgangX);

        // 🎯 ĐIỀU CHỈNH QUAN TRỌNG: Trừ đi một lượng siêu nhỏ (o.hangNgangThuMay * 0.001f) dựa vào hàng ngang.
        // Điều này đảm bảo lính ở hàng sau sẽ có Y thấp hơn hàng trước một chút xíu nếu vô tình đứng ngang hàng,
        // giúp tính năng Custom Axis Y của URP nhận biết thứ tự đè hình thời gian thực chuẩn 100%.
        float yThucTe = toaDoYTrungTam - (o.viTriTrongHang * gianCachDocY) - (o.hangNgangThuMay * 0.001f);

        // Tạo hiệu ứng so le nhẹ giữa các hàng đối với các đội hình đông (6 lính) để nhìn trực quan hơn
        if (ch.soLuongHangToiDa > 2 && o.hangNgangThuMay % 2 != 0)
        {
            // Hàng lẻ sẽ thụt xuống dưới một chút để tạo độ so le đan xen đẹp mắt
            yThucTe -= gianCachDocY * 0.3f;
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
        // Nếu lính ở quá xa vị trí slot (ví dụ khi mới Spawn), tăng tốc độ để chạy nhanh về hàng ngũ
        if (kc > 5f) return tocDoDiChuyenVeSlot * 1.5f;
        return tocDoDiChuyenVeSlot;
    }
}