using UnityEngine;

public class AirStrikeBomb : MonoBehaviour
{
    [Header("--- CẤU HÌNH NGẪU NHIÊN TRONG QUẢ BOM ---")]
    [Tooltip("Độ lệch ngẫu nhiên tối đa theo trục Y khi bom nổ.")]
    public float offsetNgauNhienY = 1.5f;

    [Header("--- CẤU HÌNH THỜI GIAN & NƠI PHÁT NỔ MỚI ---")]
    [Tooltip("Bật chế độ này nếu bạn muốn bom tự nổ SAU MỘT KHOẢNG THỜI GIAN quy định (Tính từ lúc xuất hiện).")]
    public bool noTheoThoiGian = false;
    [Tooltip("Số giây quả bom sẽ bay trước khi tự phát nổ (Chỉ có tác dụng khi tích chọn ô 'noTheoThoiGian').")]
    public float thoiGianTuDongNo = 0.5f;

    [Tooltip("Bật chế độ này nếu bạn muốn bom tự nổ KHI RƠI ĐẾN MỘT CAO ĐỘ (Trục Y) nhất định.")]
    public bool noTheoCaoDoY = false;
    [Tooltip("Quả bom cứ rơi xuống đến tọa độ Y này là phát nổ ngay trên không (Chỉ có tác dụng khi tích chọn ô 'noTheoCaoDoY').")]
    public float caoDoYSePhatNo = 1.0f;

    private Vector3 viTriDichDen;
    private float tocDoRoi = 15f;
    private System.Action hanhDongKhiNo;
    private bool daDenDich = false;
    private float boDemThoiGian = 0f; // Biến dùng để đếm số giây đã trôi qua

    // Hàm khởi hành nhận tọa độ đích từ Map và thực thi di chuyển
    public void KhoiHanh(Vector3 viTriNoBanDau, float doCaoBatDau, float tocDo, System.Action callbackNo)
    {
        tocDoRoi = tocDo;
        hanhDongKhiNo = callbackNo;
        boDemThoiGian = 0f; // Reset lại bộ đếm thời gian khi bắt đầu rơi

        // 🎯 THUẬT TOÁN TẠO SỰ NỔ NGẪU NHIÊN TRÊN TRỤC Y:
        float toadoY_NgauNhien = viTriNoBanDau.y + Random.Range(-offsetNgauNhienY, offsetNgauNhienY);

        // Gán tọa độ đích đến cuối cùng sau khi đã tính độ lệch ngẫu nhiên
        viTriDichDen = new Vector3(viTriNoBanDau.x, toadoY_NgauNhien, viTriNoBanDau.z);

        // Đặt vị trí xuất phát ban đầu của quả bom ở trên cao so với điểm đích ngẫu nhiên mới
        transform.position = new Vector3(viTriDichDen.x, viTriDichDen.y + doCaoBatDau, viTriDichDen.z);

        daDenDich = false;
    }

    private void Update()
    {
        if (daDenDich) return;

        // 1. Di chuyển tịnh tiến hình học mượt mà về tọa độ đích dưới đất
        transform.position = Vector3.MoveTowards(transform.position, viTriDichDen, tocDoRoi * Time.deltaTime);

        // 2. 🎯 THUẬT TOÁN KIỂM TRA ĐIỀU KIỆN NỔ THEO THỜI GIAN
        if (noTheoThoiGian)
        {
            boDemThoiGian += Time.deltaTime; // Cộng dồn thời gian thực tế trôi qua từng khung hình
            if (boDemThoiGian >= thoiGianTuDongNo)
            {
                daDenDich = true;
                ThucThiNo();
                return; // Nổ luôn và dừng hàm Update lại
            }
        }

        // 3. 🎯 THUẬT TOÁN KIỂM TRA ĐIỀU KIỆN NỔ THEO CAO ĐỘ Y (NỔ TRÊN KHÔNG)
        if (noTheoCaoDoY)
        {
            // Nếu quả bom rơi từ trên cao xuống và vượt qua (nhỏ hơn hoặc bằng) mốc cao độ Y bạn cài
            if (transform.position.y <= caoDoYSePhatNo)
            {
                daDenDich = true;
                ThucThiNo();
                return;
            }
        }

        // 4. KIỂM TRA MẶC ĐỊNH: Nếu không bật 2 chế độ trên, bom chạm đúng đích ban đầu sẽ nổ
        if (!noTheoThoiGian && !noTheoCaoDoY)
        {
            if (Vector3.Distance(transform.position, viTriDichDen) < 0.1f)
            {
                daDenDich = true;
                ThucThiNo();
            }
        }
    }

    private void ThucThiNo()
    {
        // 🌟 CẬP NHẬT QUAN TRỌNG: Đồng bộ lại điểm nổ thực tế của Map Controller dựa theo tọa độ hiện tại của Bom
        // Giúp Vùng Nổ Sát Thương sinh ra chuẩn xác tại nơi quả bom vừa nổ (Dù là đang ở trên không)
        if (hanhDongKhiNo != null)
        {
            hanhDongKhiNo.Invoke();
        }

        // Tự hủy bản thân quả bom sau khi nổ xong
        Destroy(gameObject);
    }
}