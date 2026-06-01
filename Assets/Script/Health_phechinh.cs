using UnityEngine;
using UnityEngine.UI;

public class Health_phechinh : MonoBehaviour
{
    [Header("--- KẾT NỐI SCRIPTABLE OBJECT DUY NHẤT ---")]
    public HeThongSatThuongData bangSatThuongChung; // Kéo file ScriptableObject duy nhất của hệ thống vào đây

    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField]
    private int maxHP = 20;
    private int currentHp;

    void Start()
    {
        currentHp = maxHP;

        if (ThanhMau != null)
        {
            ThanhMau.minValue = 0f;
            ThanhMau.maxValue = maxHP;
            ThanhMau.value = currentHp;
            ThanhMau.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHP);

        if (ThanhMau != null)
        {
            ThanhMau.value = currentHp;
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // --- GIỮ NGUYÊN CƠ CHẾ TỰ ĐỘNG THU HỒI SLOT LÍNH CỦA NHÓM BẠN ---
        if (ResourceManager.Instance != null)
        {
            string tenLinhQuetDuoc = gameObject.name.ToLower();

            if (tenLinhQuetDuoc.Contains("khograk"))
            {
                ResourceManager.Instance.TruLinh(1);
                Debug.Log($"[TỰ ĐỘNG] Chủng KhoGrak Guard tử trận. Thu hồi 1 slot.");
            }
            else if (tenLinhQuetDuoc.Contains("ironstorm"))
            {
                ResourceManager.Instance.TruLinh(2);
                Debug.Log($"[TỰ ĐỘNG] Chủng IronStorm Marine tử trận. Thu hồi 2 slot.");
            }
            else if (tenLinhQuetDuoc.Contains("terminator"))
            {
                ResourceManager.Instance.TruLinh(5);
                Debug.Log($"[TỰ ĐỘNG] Chủng Terminator tử trận. Thu hồi 5 slot.");
            }
            else if (tenLinhQuetDuoc.Contains("dead iron walk") || tenLinhQuetDuoc.Contains("dead_iron_walk"))
            {
                ResourceManager.Instance.TruLinh(10);
                Debug.Log($"[TỰ ĐỘNG] Chủng Dead Iron Walk tử trận. Thu hồi 10 slot.");
            }
            else if (tenLinhQuetDuoc.Contains("titan"))
            {
                ResourceManager.Instance.TruLinh(20);
                Debug.Log($"[TỰ ĐỘNG] SIÊU TITAN sụp đổ! Thu hồi 20 slot.");
            }
            else
            {
                Debug.LogWarning($"[CẢNH BÁO] Không tìm thấy từ khóa nhận diện trong tên: '{gameObject.name}' để thu hồi slot!");
            }
        }
        // ---------------------------------------------------------------------

        gameObject.SetActive(false);
    }

    // LUỒNG HOẠT ĐỘNG ĐÃ ĐỒNG BỘ: Tự động lọc sát thương nhận vào từ phe Chaos
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu trúng đạn hoặc kiếm của phe dị giáo (Tag: SatthuongQ)
        if (collision.CompareTag("SatthuongQ"))
        {
            DamageSource nguonDanChaos = collision.GetComponent<DamageSource>();

            if (nguonDanChaos != null && bangSatThuongChung != null)
            {
                // 1. Lấy tên chủng lính Chaos bắn viên đạn/vung kiếm này (Ví dụ: "Chaos zelos")
                string chungLoaiDanChaos = nguonDanChaos.tenChungLinhBan;

                // 2. Tra cứu vào ScriptableObject duy nhất để lấy số đam tương ứng của con quái đó
                int damNhanVe = bangSatThuongChung.LaySatThuongTuChung(chungLoaiDanChaos);

                Debug.Log($"🛡️ Phe chính trúng đạn từ chủng Chaos: {chungLoaiDanChaos} | Hệ thống lọc ra Đam: {damNhanVe}");

                // 3. Thực hiện trừ máu
                TakeDamage(damNhanVe);
            }
            else
            {
                // Sát thương dự phòng nếu bạn chưa kịp gán tên chủng cho viên đạn/kiếm của quái
                TakeDamage(5);
            }

            if (ThanhMau != null)
            {
                ThanhMau.gameObject.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        currentHp = maxHP;
        if (ThanhMau != null)
        {
            ThanhMau.value = maxHP;
            ThanhMau.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (ThanhMau != null)
        {
            ThanhMau.gameObject.SetActive(false);
        }
    }
}