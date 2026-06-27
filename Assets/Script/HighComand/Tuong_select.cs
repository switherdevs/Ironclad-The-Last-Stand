using UnityEngine;

public class ChonTuongClick : MonoBehaviour
{
    [Header("--- CẤU HÌNH ID TƯỚNG ---")]
    [Tooltip("Số thứ tự tương ứng trong mảng của HeroSelectionManager (Con đầu tiên là 0, con tiếp theo là 1...)")]
    [SerializeField] private int idTuong;

    private HeroSelectionManager managerTong;

    private void Start()
    {
        // Tự động tìm kiếm ông quản lý tổng trong Scene Menu
        managerTong = Object.FindFirstObjectByType<HeroSelectionManager>();
    }

    // Hàm tự động chạy khi click chuột vào Collider 2D của tướng
    private void OnMouseDown()
    {
        if (managerTong != null)
        {
            // Báo cho ông tổng biết con tướng ID này vừa được click!
            managerTong.ChonTuongBằngClick(idTuong);
        }
    }
}