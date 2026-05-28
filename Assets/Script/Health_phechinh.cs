using UnityEngine;
using UnityEngine.UI;

public class Health_phechinh : MonoBehaviour
{
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
        // --- CƠ CHẾ TỰ ĐỘNG QUÉT THEO TÊN PREFAB CHUẨN ĐỂ THU HỒI SLOT ---
        if (ResourceManager.Instance != null)
        {
            // Lấy tên hiển thị của Object/Prefab và viết thường toàn bộ để đối chiếu
            string tenLinhQuetDuoc = gameObject.name.ToLower();

            // 1. Chủng prefab_KhoGrak_Guand (Quy định: Thu hồi 1 slot)
            if (tenLinhQuetDuoc.Contains("khograk"))
            {
                ResourceManager.Instance.TruLinh(1);
                Debug.Log($"[TỰ ĐỘNG] Chủng KhoGrak Guard tử trận. Thu hồi 1 slot.");
            }
            // 2. Chủng IronStorm (Quy định: Thu hồi 2 slot)
            else if (tenLinhQuetDuoc.Contains("ironstorm"))
            {
                ResourceManager.Instance.TruLinh(2);
                Debug.Log($"[TỰ ĐỘNG] Chủng IronStorm Marine tử trận. Thu hồi 2 slot.");
            }
            // 3. Chủng Terminator (Quy định: Thu hồi 5 slot)
            else if (tenLinhQuetDuoc.Contains("terminator"))
            {
                ResourceManager.Instance.TruLinh(5);
                Debug.Log($"[TỰ ĐỘNG] Chủng Terminator tử trận. Thu hồi 5 slot.");
            }
            // 4. Chủng Dead Iron walk (Quy định: Thu hồi 10 slot)
            else if (tenLinhQuetDuoc.Contains("dead iron walk") || tenLinhQuetDuoc.Contains("dead_iron_walk"))
            {
                ResourceManager.Instance.TruLinh(10);
                Debug.Log($"[TỰ ĐỘNG] Chủng Dead Iron Walk tử trận. Thu hồi 10 slot.");
            }
            // 5. Chủng Titan (Quy định: Thu hồi 20 slot)
            else if (tenLinhQuetDuoc.Contains("titan"))
            {
                ResourceManager.Instance.TruLinh(20);
                Debug.Log($"[TỰ ĐỘNG] SIÊU TITAN sụp đổ! Thu hồi 20 slot.");
            }
            else
            {
                // Cảnh báo an toàn phòng khi ngài đặt tên Prefab sai chính tả ngoài cửa sổ Project
                Debug.LogWarning($"[CẢNH BÁO] Không tìm thấy từ khóa nhận diện trong tên: '{gameObject.name}' để thu hồi slot!");
            }
        }
        // ---------------------------------------------------------------------

        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SatthuongQ"))
        {
            TakeDamage(10);
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