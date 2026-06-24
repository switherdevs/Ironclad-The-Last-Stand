using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HeroSelection : MonoBehaviour
{
    [Header("--- Cấu Hình Tướng ---")]
    [Tooltip("ID duy nhất của con tướng này (Ví dụ: Tướng A là 0, Tướng B là 1, Tướng C là 2...)")]
    public int idTuong = 0;

    [Tooltip("Kéo Prefab của con tướng này dùng để sinh ra trong trận đấu vào đây.")]
    public GameObject prefabTuongTrongTran;

    [Header("--- Hiệu Ứng Ánh Sáng ---")]
    [Tooltip("Kéo GameObject vòng sáng hoặc hiệu ứng chỉ định chọn tướng vào đây.")]
    public GameObject anhSangChonTuong;

    // Biến tĩnh (static) để quản lý con tướng đang được chọn hiện tại trên RAM
    public static HeroSelection tuongDuocChonHienTai;
    private SaveSystem quanLySave;

    private void Awake()
    {
        quanLySave = Object.FindFirstObjectByType<SaveSystem>();
        if (quanLySave == null)
        {
            Debug.LogError("[HeroSelection] Không tìm thấy script SaveSystem trong Scene! Hãy chắc chắn có một đối tượng giữ SaveSystem.");
        }
    }

    private void Start()
    {
        // Kiểm tra xem trong Save cũ, con tướng này có đang được chọn không
        if (quanLySave != null)
        {
            int idTrongSave = quanLySave.DocTuongDaChon();
            if (idTrongSave == idTuong)
            {
                KichHoatTuongNay();
            }
            else
            {
                TatAnhSang();
            }
        }
    }

    // Hàm tự động chạy khi người chơi dùng chuột Click vào Collider của Tướng
    private void OnMouseDown()
    {
        KichHoatTuongNay();
    }

    private void KichHoatTuongNay()
    {
        // Nếu có con tướng khác đang bật sáng, tắt nó đi trước
        if (tuongDuocChonHienTai != null && tuongDuocChonHienTai != this)
        {
            tuongDuocChonHienTai.TatAnhSang();
        }

        // Cập nhật con tướng này thành con tướng đang được chọn
        tuongDuocChonHienTai = this;

        // Bật hiệu ứng ánh sáng (nếu có biến)
        if (anhSangChonTuong != null)
        {
            anhSangChonTuong.SetActive(true);
            // Đảm bảo ánh sáng đi theo vị trí của tướng
            anhSangChonTuong.transform.position = transform.position;
        }

        // Lưu ngay lập tức vào Save Game
        if (quanLySave != null)
        {
            quanLySave.LuuTuongDaChon(idTuong);
        }
    }

    public void TatAnhSang()
    {
        if (anhSangChonTuong != null)
        {
            anhSangChonTuong.SetActive(false);
        }
    }
}
