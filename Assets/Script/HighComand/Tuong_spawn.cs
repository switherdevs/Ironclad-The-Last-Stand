using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thư viện bắt buộc để làm việc với linh kiện chữ TextMeshPro

public class HeroSpawner : MonoBehaviour
{
    [Header("--- CHẾ ĐỘ DEBUG KIỂM TRA NHANH ---")]
    [Tooltip("Tích chọn ô này nếu bạn muốn test nhanh tướng trong Cảnh này mà không cần đi từ Menu.")]
    public bool kichHoatCheDoDebug = false;

    [Tooltip("Nhập số ID tướng bạn muốn ép buộc xuất hiện khi bật chế độ Debug (0 là Sniper, 1 là Captain...).")]
    public int idTuongMuonDebug = 0;

    [Header("--- MẢNG VỊ TRÍ SPAWN TƯỚNG UI (THEO INDEX) ---")]
    [Tooltip("Element 0 là vị trí của Sniper, Element 1 là vị trí của Captain...")]
    public Transform[] danhSachViTriSpawn;

    [Header("--- MẢNG PREFAB TƯỚNG TRONG TRẬN (THEO INDEX) ---")]
    [Tooltip("Element 0 là Prefab Sniper, Element 1 là Prefab Captain...")]
    public GameObject[] danhSachPrefabTuong;

    [Header("--- UI CÓ SẴN TRÊN MÀN HÌNH ---")]
    [Tooltip("Kéo chiếc Button Skill có sẵn trên Canvas của Scene vào đây.")]
    public Button nutBamSkillCoSan;

    [Tooltip("Kéo cấu kiện TextMeshPro hiển thị thời gian hồi chiêu (Cooldown) trên Canvas vào đây để hệ thống tự truyền vào cho Tướng.")]
    public TextMeshProUGUI textHienThiHoiChieuCoSan;

    private SaveSystem quanLySave;

    private void Start()
    {
        quanLySave = Object.FindFirstObjectByType<SaveSystem>();
        SinhTuongVaoTran();
    }

    private void SinhTuongVaoTran()
    {
        int idTuongDaChon = 0; // Mặc định ban đầu

        // 🌟 THUẬT TOÁN KIỂM TRA CHẾ ĐỘ DEBUG THÔNG MINH
        if (kichHoatCheDoDebug)
        {
            idTuongDaChon = idTuongMuonDebug;
            Debug.Log($"<color=orange><b>[Spawner Debug]</b> Đang bật chế độ TEST NHANH! Ép buộc sinh Tướng ID: {idTuongDaChon}</color>");
        }
        else
        {
            if (quanLySave != null && quanLySave.KiemTraCoFileSave())
            {
                idTuongDaChon = quanLySave.DocTuongDaChon();
                Debug.Log($"<color=yellow><b>[Spawner]</b> Đọc từ file Save thấy Người chơi chọn Tướng ID: {idTuongDaChon}</color>");
            }
            else
            {
                Debug.LogWarning("[Spawner] Không tìm thấy File Save, tự động chọn Tướng ID mặc định: 0");
            }
        }

        // --- BƯỚC 1: KIỂM TRA TÍNH HỢP LỆ CỦA CÁC MẢNG ---
        if (danhSachPrefabTuong == null || danhSachPrefabTuong.Length == 0)
        {
            Debug.LogError("[Spawner] Mảng danhSachPrefabTuong đang bị TRỐNG ngoài Inspector!");
            return;
        }

        if (danhSachViTriSpawn == null || danhSachViTriSpawn.Length == 0)
        {
            Debug.LogError("[Spawner] Mảng danhSachViTriSpawn đang bị TRỐNG ngoài Inspector!");
            return;
        }

        if (idTuongDaChon < 0 || idTuongDaChon >= danhSachPrefabTuong.Length || idTuongDaChon >= danhSachViTriSpawn.Length)
        {
            Debug.LogError($"[Spawner] ID {idTuongDaChon} bị vượt quá kích thước mảng bạn cấu hình! Ép về ID 0.");
            idTuongDaChon = 0;
        }

        // --- BƯỚC 2: XÁC ĐỊNH PREFAB VÀ VỊ TRÍ CHA THEO INDEX ---
        GameObject prefabCanSinh = danhSachPrefabTuong[idTuongDaChon];
        Transform chaCuaUI = danhSachViTriSpawn[idTuongDaChon];

        if (prefabCanSinh == null || chaCuaUI == null)
        {
            Debug.LogError($"[Spawner] Phần tử ở ô số {idTuongDaChon} trong mảng Prefab hoặc mảng Vị trí bị để trống (Null)!");
            return;
        }

        // --- BƯỚC 3: TIẾN HÀNH SINH TƯỚNG VÀO ĐÚNG VỊ TRÍ ---
        GameObject tuongTrongTran = Instantiate(prefabCanSinh, chaCuaUI, false);
        tuongTrongTran.transform.localPosition = Vector3.zero;
        Debug.Log($"<color=green><b>[Spawner]</b> Đã sinh thành công Prefab vào đúng Vị trí {chaCuaUI.name}!</color>");

        // --- BƯỚC 4: LIÊN KẾT CHỨC NĂNG NÚT BẤM SKILL & TEXT COOLDOWN ---
        if (nutBamSkillCoSan != null)
        {
            // 👉 KIỂM TRA 1: Nếu con tướng sinh ra chứa script SniperSkill
            SniperSkill scriptSniper = tuongTrongTran.GetComponent<SniperSkill>();
            if (scriptSniper != null)
            {
                // Nếu người chơi có kéo Text ngoài vào Spawner, truyền linh kiện đó cho Sniper quản lý
                if (textHienThiHoiChieuCoSan != null)
                {
                    scriptSniper.textHienThiTrangThai = textHienThiHoiChieuCoSan;
                }

                scriptSniper.GanNutBamSkillTuDong(nutBamSkillCoSan);
                Debug.Log("<color=green>[Spawner] Đã kết nối thành công nút bấm và Text Cooldown cho Sniper!</color>");
            }

            // 👉 KIỂM TRA 2: Nếu con tướng sinh ra chứa script CaptainSkill
            CaptainSkill scriptCaptain = tuongTrongTran.GetComponent<CaptainSkill>();
            if (scriptCaptain != null)
            {
                // Truyền linh kiện Text hiển thị từ Spawner trực tiếp sang cho Captain quản lý công khai
                if (textHienThiHoiChieuCoSan != null)
                {
                    scriptCaptain.textHienThiTrangThai = textHienThiHoiChieuCoSan;
                }

                scriptCaptain.GanNutBamSkillTuDong(nutBamSkillCoSan);
                Debug.Log("<color=green>[Spawner] Đã kết nối thành công nút bấm và Text Cooldown cho Captain!</color>");
            }
        }
        else
        {
            Debug.LogWarning("[Spawner] Bạn chưa kéo chiếc Button có sẵn ngoài màn hình vào ô 'nutBamSkillCoSan'!");
        }
    }
}