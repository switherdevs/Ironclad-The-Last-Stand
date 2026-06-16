using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("--- KẾT NỐI DỮ LIỆU ---")]
    [SerializeField] private HeThongSatThuongData dataLinh;
    [SerializeField] private HeThongKinhTe kinhTe;
    [SerializeField] private SaveSystem bộSaveGame; // Kết nối hệ thống Save txt gốc của bạn

    private void Start()
    {
        LoadUpgradeData();
    }

    // Hàm gắn vào Button để nâng cấp MÁU cho lính dựa trên INDEX trong mảng (0 đến 5)
    public void NangCapMauLinh(int indexLinh)
    {
        if (dataLinh == null || indexLinh < 0 || indexLinh >= dataLinh.danhSachSatThuong.Count) return;

        var chungLinh = dataLinh.danhSachSatThuong[indexLinh];
        int capHienTai = chungLinh.capDoMau;

        // Kiểm tra xem đã đạt cấp độ tối đa theo cấu hình mảng giá tiền chưa
        if (chungLinh.mangGiaTienMau == null || capHienTai >= chungLinh.mangGiaTienMau.Count)
        {
            Debug.LogWarning($"[MAX] Chủng lính {chungLinh.tenChungLinh} đã đạt cấp MÁU tối đa!");
            return;
        }

        // Lấy giá tiền tương ứng với cấp độ hiện tại trong mảng cấu hình công thức
        int giaHienTai = chungLinh.mangGiaTienMau[capHienTai];

        // Kiểm tra tiền nâng cấp từ HeThongKinhTe bằng chính xác biến từ nay về sau của bạn
        if (kinhTe != null && kinhTe.tienNangCapLinh >= giaHienTai)
        {
            kinhTe.tienNangCapLinh -= giaHienTai; // Trừ tiền nâng cấp lính

            chungLinh.capDoMau++; // Tăng cấp độ máu lên 1 bậc
            dataLinh.danhSachSatThuong[indexLinh] = chungLinh; // Cập nhật lại vào mảng struct dữ liệu gốc

            // ĐỒNG BỘ LƯU GAME NGAY LẬP TỨC KHI ĐỔI CHỈ SỐ VÀ TRỪ TIỀN
            if (bộSaveGame != null)
            {
                bộSaveGame.LuuNangCapLinh(indexLinh, chungLinh.capDoMau, chungLinh.capDoSatThuong, chungLinh.mauGoc, chungLinh.satThuongGoc);
                bộSaveGame.LuuThongTinGame(kinhTe.tienNangCapLinh); // Đồng bộ lưu luôn số tiền mới sau trừ
            }

            Debug.Log($"[NÂNG CẤP THÀNH CÔNG] MÁU {chungLinh.tenChungLinh} lên Cấp {chungLinh.capDoMau}. Tiền còn lại: {kinhTe.tienNangCapLinh}");
        }
        else
        {
            Debug.LogWarning($"[THẤT BẠI] Không đủ tiền để nâng cấp Máu cho {chungLinh.tenChungLinh}! Cần: {giaHienTai}");
        }
    }

    // Hàm gắn vào Button để nâng cấp SÁT THƯƠNG cho lính dựa trên INDEX trong mảng (0 đến 5)
    public void NangCapSatThuongLinh(int indexLinh)
    {
        if (dataLinh == null || indexLinh < 0 || indexLinh >= dataLinh.danhSachSatThuong.Count) return;

        var chungLinh = dataLinh.danhSachSatThuong[indexLinh];
        int capHienTai = chungLinh.capDoSatThuong;

        // Kiểm tra xem đã đạt cấp độ tối đa theo cấu hình mảng giá tiền sát thương chưa
        if (chungLinh.mangGiaTienSatThuong == null || capHienTai >= chungLinh.mangGiaTienSatThuong.Count)
        {
            Debug.LogWarning($"[MAX] Chủng lính {chungLinh.tenChungLinh} đã đạt cấp SÁT THƯƠNG tối đa!");
            return;
        }

        int giaHienTai = chungLinh.mangGiaTienSatThuong[capHienTai];

        if (kinhTe != null && kinhTe.tienNangCapLinh >= giaHienTai)
        {
            kinhTe.tienNangCapLinh -= giaHienTai; // Trừ tiền

            chungLinh.capDoSatThuong++; // Tăng cấp độ sát thương lên 1 bậc
            dataLinh.danhSachSatThuong[indexLinh] = chungLinh;

            // ĐỒNG BỘ LƯU GAME NGAY LẬP TỨC KHI ĐỔI CHỈ SỐ VÀ TRỪ TIỀN
            if (bộSaveGame != null)
            {
                bộSaveGame.LuuNangCapLinh(indexLinh, chungLinh.capDoMau, chungLinh.capDoSatThuong, chungLinh.mauGoc, chungLinh.satThuongGoc);
                bộSaveGame.LuuThongTinGame(kinhTe.tienNangCapLinh); // Đồng bộ lưu luôn số tiền mới sau trừ
            }

            Debug.Log($"[NÂNG CẤP THÀNH CÔNG] SÁT THƯƠNG {chungLinh.tenChungLinh} lên Cấp {chungLinh.capDoSatThuong}. Tiền còn lại: {kinhTe.tienNangCapLinh}");
        }
        else
        {
            Debug.LogWarning($"[THẤT BẠI] Không đủ tiền để nâng cấp Sát thương cho {chungLinh.tenChungLinh}! Cần: {giaHienTai}");
        }
    }

    // Tự động Load lại dữ liệu từ file txt gốc khi bắt đầu màn chơi
    public void LoadUpgradeData()
    {
        if (bộSaveGame == null || dataLinh == null) return;

        for (int i = 0; i < dataLinh.danhSachSatThuong.Count; i++)
        {
            string chuoiDuaVe = bộSaveGame.DocNangCapLinh(i);

            if (!string.IsNullOrEmpty(chuoiDuaVe))
            {
                string[] dataParts = chuoiDuaVe.Split('|');

                if (dataParts.Length >= 5)
                {
                    var chungLinh = dataLinh.danhSachSatThuong[i];

                    // dataParts[0] là index cũ trong dòng txt, bóc tách nạp từ phần tử số 1 trở đi
                    chungLinh.capDoMau = int.Parse(dataParts[1]);
                    chungLinh.capDoSatThuong = int.Parse(dataParts[2]);
                    chungLinh.mauGoc = int.Parse(dataParts[3]);
                    chungLinh.satThuongGoc = int.Parse(dataParts[4]);

                    dataLinh.danhSachSatThuong[i] = chungLinh;
                }
            }
        }
        Debug.Log("Đã đồng bộ tải toàn bộ dữ liệu nâng cấp lính từ file gốc savegame.txt!");
    }
}