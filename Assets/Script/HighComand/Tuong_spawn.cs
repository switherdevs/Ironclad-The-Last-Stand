using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải có để làm việc với linh kiện Button

public class HeroSpawner : MonoBehaviour
{
    [Header("--- VỊ TRÍ SPAWN TƯỚNG (UI) ---")]
    [Tooltip("Kéo RectTransform hoặc Transform nơi bạn muốn con tướng xuất hiện làm con của nó.")]
    public Transform viTriDatTuong;

    [Header("--- UI CÓ SẴN TRÊN MÀN HÌNH ---")]
    [Tooltip("Kéo chiếc Button Skill có sẵn trên Canvas của Scene vào đây để hệ thống tự động gán chức năng.")]
    public Button nutBamSkillCoSan;

    [Header("--- Danh Sách Dự Phòng (Nếu chơi ngay không qua Menu) ---")]
    [Tooltip("Mảng chứa tất cả các Prefab tướng theo đúng thứ tự ID (Phòng trường hợp bạn bật thẳng cảnh game để test).")]
    public GameObject[] danhSachPrefabTuong;

    private SaveSystem quanLySave;

    private void Start()
    {
        quanLySave = Object.FindFirstObjectByType<SaveSystem>();
        SinhTuongVaoTran();
    }

    private void SinhTuongVaoTran()
    {
        // 🌟 ĐÃ KHÔI PHỤC VÀ ĐỒNG BỘ CHUẨN UI: Xác định đối tượng cha (Parent) cho con tướng
        // Nếu bạn quên kéo 'viTriDatTuong' trên Inspector, nó sẽ tự lấy chính Object chứa Script này làm cha.
        Transform chaCuaUI = (viTriDatTuong != null) ? viTriDatTuong : transform;

        if (viTriDatTuong == null)
        {
            Debug.LogWarning("[HeroSpawner] Biến 'viTriDatTuong' bị null! Tướng sẽ được gắn trực tiếp làm con của Spawner này.");
        }

        GameObject prefabCanSinh = null;

        // CÁCH 1: Lấy trực tiếp Prefab từ con tướng đang chọn trong RAM (Nếu đi từ Menu sang)
        if (HeroSelection.tuongDuocChonHienTai != null)
        {
            prefabCanSinh = HeroSelection.tuongDuocChonHienTai.prefabTuongTrongTran;
        }

        // CÁCH 2: Nếu RAM trống (Do bạn test nhanh trong Unity Editor), tiến hành đọc file Save
        if (prefabCanSinh == null && quanLySave != null)
        {
            int idTuongDaSave = quanLySave.DocTuongDaChon();
            if (danhSachPrefabTuong != null && idTuongDaSave >= 0 && idTuongDaSave < danhSachPrefabTuong.Length)
            {
                prefabCanSinh = danhSachPrefabTuong[idTuongDaSave];
            }
        }

        // THỰC THI: Sinh bản thể tướng làm UI con và liên kết vào nút bấm có sẵn
        if (prefabCanSinh != null)
        {
            // 🌟 ĐÃ SỬA LẠI: Sinh con tướng ra làm CON của vị trí quy định, ép không méo tỉ lệ (false)
            GameObject tuongTrongTran = Instantiate(prefabCanSinh, chaCuaUI, false);

            // Đảm bảo con tướng UI nằm ngay trung tâm ô chứa quy định
            tuongTrongTran.transform.localPosition = Vector3.zero;

            // =================================================================
            // TRUNG TÂM LIÊN KẾT NÚT BẤM CÓ SẴN (Nhận diện tướng ngẫu nhiên)
            // =================================================================

            // Bảo đảm an toàn: Kiểm tra xem bạn đã kéo chiếc nút bấm có sẵn vào ô biến chưa
            if (nutBamSkillCoSan != null)
            {
                // 👉 KIỂM TRA 1: Thử xem con tướng vừa gọi ra có chứa script 'SniperSkill' hay không?
                SniperSkill scriptSniper = tuongTrongTran.GetComponent<SniperSkill>();
                if (scriptSniper != null)
                {
                    // Truyền chiếc nút bấm có sẵn ngoài màn hình vào để Sniper chiếm quyền điều khiển
                    scriptSniper.GanNutBamSkillTuDong(nutBamSkillCoSan);
                    Debug.Log($"<color=green><b>[HeroSpawner]</b> Đã nhận diện Sniper! Đã gán nút bấm có sẵn chạy chiêu bắn tỉa.</color>");
                    return; // Hoàn thành việc kết nối, thoát khỏi hàm
                }

                // 👉 KIỂM TRA 2: Sau này bạn làm thêm các hệ tướng khác (Pháp sư, Chiến binh...), chỉ cần viết tiếp ở đây:
                /*
                MageSkill scriptMage = tuongTrongTran.GetComponent<MageSkill>();
                if (scriptMage != null)
                {
                    scriptMage.GanNutBamSkillMage(nutBamSkillCoSan);
                    return;
                }
                */
            }
            else
            {
                Debug.LogWarning("[HeroSpawner] Bạn chưa kéo chiếc Button có sẵn trên Canvas vào ô 'nutBamSkillCoSan' của Spawner!");
            }
        }
        else
        {
            Debug.LogError("[HeroSpawner] Không thể sinh tướng! Prefab bị null (Chưa chọn tướng hoặc danh sách dự phòng trống).");
        }
    }
}