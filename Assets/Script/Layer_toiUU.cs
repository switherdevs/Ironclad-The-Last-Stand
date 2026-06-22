using UnityEngine;
using UnityEngine.Rendering; // Bắt buộc phải có để gọi SortingGroup

/// <summary>
/// Tự động cập nhật sortingOrder của Sorting Group dựa theo tọa độ Y thời gian thực.
/// Thích hợp cho lính lắp ghép từ PSB/Khung xương để chống chồng chéo hình hoàn toàn.
/// </summary>
[RequireComponent(typeof(SortingGroup))]
public class DynamicSortingGroup : MonoBehaviour
{
    [Header("--- CẤU HÌNH LAYER TỰ ĐỘNG ---")]
    [Tooltip("Hệ số nhân độ chính xác. Số càng cao tính toán phân lớp càng chi tiết (Khuyên dùng: 100 hoặc 1000)")]
    public int heSoChinhXac = 100;

    [Tooltip("Số bù trừ gốc để không bị âm Order (Nếu Layer của bạn cần nằm trong khoảng số cụ thể)")]
    public int diemBuTruGoc = 5000;

    [Tooltip("Bật nếu lính di chuyển liên tục. Tắt nếu lính đứng yên một chỗ để tiết kiệm hiệu năng")]
    public bool capNhatLienTuc = true;

    private SortingGroup _sortingGroup;
    private Transform _transform;
    private float _viTriYCu;

    void Awake()
    {
        _sortingGroup = GetComponent<SortingGroup>();
        _transform = transform;
    }

    void OnEnable()
    {
        CapNhatThuTuHienThi();
    }

    void LateUpdate()
    {
        if (!capNhatLienTuc) return;

        // Chỉ tính toán lại khi con lính thực sự có sự dịch chuyển trục Y để tối ưu CPU
        if (!Mathf.Approximately(_transform.position.y, _viTriYCu))
        {
            CapNhatThuTuHienThi();
        }
    }

    /// <summary>
    /// Thuật toán ép Sorting Group đổi Order dựa theo tọa độ chân lính
    /// </summary>
    public void CapNhatThuTuHienThi()
    {
        if (_sortingGroup == null) return;

        _viTriYCu = _transform.position.y;

        // TOÁN HỌC: Trục Y càng nhỏ (càng dịch xuống dưới màn hình) thì số Order càng to 
        int orderMoi = diemBuTruGoc - Mathf.RoundToInt(_viTriYCu * heSoChinhXac);

        // ĐÃ SỬA: Thay đổi từ orderInLayer sang sortingOrder chuẩn cú pháp Unity cho SortingGroup
        _sortingGroup.sortingOrder = orderMoi;
    }
}