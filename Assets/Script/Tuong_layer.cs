using UnityEngine;
using UnityEngine.Rendering; // Quản lý SortingGroup
using System.Collections.Generic; // Quản lý danh sách lính trong vùng

[RequireComponent(typeof(SpriteRenderer))]
public class StatueTriggerSorter : MonoBehaviour
{
    [Header("--- CẤU HÌNH NGƯỠNG Y ---")]
    [Tooltip("Ngưỡng cắt ranh giới bệ đá. Hãy để bằng đúng vị trí chân tượng trên Scene")]
    public float thresholdY = -17.7f;

    [Header("--- TINH CHỈNH ORDER ---")]
    [Tooltip("Độ lệch lớp (Mặc định: 10). Tượng là 70 -> Trước là 80, Sau là 60")]
    public int sortingOffset = 10;

    // Danh sách lưu trữ những con lính đang nằm trong vùng của tượng
    private List<SortingGroup> _danhSachLinhTrongVung = new List<SortingGroup>();
    private SpriteRenderer _mySpriteRenderer;
    private int _orderGocCuaTuong;

    void Awake()
    {
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Lấy Order gốc của tượng làm mốc (Ví dụ: 70)
        if (_mySpriteRenderer != null)
        {
            _orderGocCuaTuong = _mySpriteRenderer.sortingOrder;
        }
    }

    void Update()
    {
        // Nếu có lính trong vùng, liên tục quét tọa độ Y để ép lớp
        for (int i = _danhSachLinhTrongVung.Count - 1; i >= 0; i--)
        {
            SortingGroup sgLinh = _danhSachLinhTrongVung[i];

            // Phòng trường hợp lính bị chết hoặc bị xóa (Destroy) khi đang đứng trong vùng
            if (sgLinh == null || !sgLinh.gameObject.activeInHierarchy)
            {
                _danhSachLinhTrongVung.RemoveAt(i);
                continue;
            }

            // SO SÁNH TỌA ĐỘ Y CỦA CHÂN LÍNH VỚI NGƯỠNG
            if (sgLinh.transform.position.y > thresholdY)
            {
                // Lính đứng cao hơn ngưỡng -> Ở PHÍA SAU TƯỢNG
                sgLinh.sortingOrder = _orderGocCuaTuong - sortingOffset;
            }
            else
            {
                // Lính đứng thấp hơn ngưỡng -> Ở PHÍA TRƯỚC TƯỢNG
                sgLinh.sortingOrder = _orderGocCuaTuong + sortingOffset;
            }
        }
    }

    // Khi bất kỳ vật thể nào đi vào vùng Trigger của Tượng
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Tìm component SortingGroup ở đối tượng va chạm (hoặc con/cha của nó)
        SortingGroup sg = collision.GetComponentInParent<SortingGroup>();
        if (sg == null) sg = collision.GetComponentInChildren<SortingGroup>();

        // Nếu đối tượng là lính có khung xương (có SortingGroup) và chưa có trong danh sách
        if (sg != null && !_danhSachLinhTrongVung.Contains(sg))
        {
            _danhSachLinhTrongVung.Add(sg);
        }
    }

    // Khi lính đi ra khỏi vùng Trigger của Tượng
    void OnTriggerExit2D(Collider2D collision)
    {
        SortingGroup sg = collision.GetComponentInParent<SortingGroup>();
        if (sg == null) sg = collision.GetComponentInChildren<SortingGroup>();

        if (sg != null && _danhSachLinhTrongVung.Contains(sg))
        {
            // Trả lại Order mặc định cho lính trước khi thả ra (Ví dụ lính Chaos Zelot gốc là 12)
            // Ở đây ta tạm trả về một số mặc định thích hợp hoặc giữ nguyên số hiện tại tùy bạn
            _danhSachLinhTrongVung.Remove(sg);
        }
    }

    // Vẽ đường ranh giới hiển thị trực quan trong Scene
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = new Vector3(transform.position.x - 5f, thresholdY, transform.position.z);
        Vector3 end = new Vector3(transform.position.x + 5f, thresholdY, transform.position.z);
        Gizmos.DrawLine(start, end);
    }
}