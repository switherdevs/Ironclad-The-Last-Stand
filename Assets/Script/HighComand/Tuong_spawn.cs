using UnityEngine;

public class HeroSpawner : MonoBehaviour
{
    [Header("--- Vị Trí Xuất Hiện ---")]
    [Tooltip("Kéo Transform vị trí nơi bạn muốn con tướng xuất hiện khi vào game.")]
    public Transform viTriDatTuong;

    [Header("--- Danh Sách Dự Phòng (Nếu chơi ngay không qua Menu) ---")]
    [Tooltip("Mảng chứa tất cả các Prefab tướng theo đúng thứ tự ID (Phòng trường hợp người chơi vào thẳng cảnh game không qua Menu chọn).")]
    public GameObject[] danhSachPrefabTuong;

    private SaveSystem quanLySave;

    private void Start()
    {
        quanLySave = Object.FindFirstObjectByType<SaveSystem>();
        SinhTuongVaoTran();
    }

    private void SinhTuongVaoTran()
    {
        // Kiểm tra vị trí đặt tướng, nếu null thì lấy vị trí của chính Spawner này làm mặc định
        Vector3 viTriSpawn = transform.position;
        Quaternion huongSpawn = Quaternion.identity;

        if (viTriDatTuong != null)
        {
            viTriSpawn = viTriDatTuong.position;
            huongSpawn = viTriDatTuong.rotation;
        }
        else
        {
            Debug.LogWarning("[HeroSpawner] Biến 'viTriDatTuong' bị null! Tướng sẽ được sinh ra tại vị trí của Spawner.");
        }

        GameObject prefabCanSinh = null;

        // CÁCH 1: Lấy trực tiếp Prefab từ con tướng đang chọn trong RAM (Nếu đi từ Menu sang)
        if (HeroSelection.tuongDuocChonHienTai != null)
        {
            prefabCanSinh = HeroSelection.tuongDuocChonHienTai.prefabTuongTrongTran;
        }

        // CÁCH 2: Nếu RAM trống (Do bạn bật thẳng Scene Game để test bằng Unity), tiến hành đọc file Save
        if (prefabCanSinh == null && quanLySave != null)
        {
            int idTuongDaSave = quanLySave.DocTuongDaChon();
            if (danhSachPrefabTuong != null && idTuongDaSave >= 0 && idTuongDaSave < danhSachPrefabTuong.Length)
            {
                prefabCanSinh = danhSachPrefabTuong[idTuongDaSave];
            }
        }

        // THỰC THI: Tiến hành sinh tướng ra màn chơi sau khi đã bắt hết các điều kiện an toàn
        if (prefabCanSinh != null)
        {
            GameObject tuongTrongTran = Instantiate(prefabCanSinh, viTriSpawn, huongSpawn);
            Debug.Log($"<color=green><b>[HeroSpawner]</b> Đã sinh tướng {tuongTrongTran.name} thành công vào trận đấu!</color>");
        }
        else
        {
            Debug.LogError("[HeroSpawner] Không thể sinh tướng! Prefab tướng bị null (Chưa chọn tướng hoặc danh sách dự phòng trống).");
        }
    }
}