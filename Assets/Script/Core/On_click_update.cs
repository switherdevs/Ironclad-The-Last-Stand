using UnityEngine;

public class ChonLinhClick : MonoBehaviour
{
    [Header("--- ĐIỀN INDEX CỦA CHỦNG LÍNH NÀY ---")]
    [Tooltip("KhoGrak = 0, IronStorm = 1, Terminator = 2...")]
    [SerializeField] private int indexChungLinh;

    [Header("--- KẾT NỐI ĐỐI TƯỢNG VIỀN SÁNG ---")]
    [Tooltip("Kéo GameObject hiệu ứng viền sáng (Highlight) nằm trong con lính vào đây")]
    [SerializeField] private GameObject objectVienSang;

    private void Start()
    {
        // Đầu trận đảm bảo tất cả lính đều tắt viền sáng
        BatTatVienSang(false);
    }

    // Hàm tự động kích hoạt của Unity khi chuột click vào con lính
    private void OnMouseDown()
    {
        if (UpgradeUIManager.Instance != null)
        {
            // Truyền chính số Index và bản thân Script này (this) sang bộ quản lý UI
            UpgradeUIManager.Instance.HienUIChungLinh(indexChungLinh, this);
        }
    }

    // Hàm điều khiển bật/tắt viền sáng được gọi trực tiếp từ UpgradeUIManager
    public void BatTatVienSang(bool trangThai)
    {
        if (objectVienSang != null)
        {
            objectVienSang.SetActive(trangThai);
        }
    }
}