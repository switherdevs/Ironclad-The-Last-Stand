using UnityEngine;
using UnityEngine.UI;

public class Health_chaos : MonoBehaviour
{
    [Header("--- KẾT NỐI SCRIPTABLE OBJECT DUY NHẤT ---")]
    public HeThongSatThuongData bangSatThuongChung; // Kéo file ScriptableObject duy nhất vào đây

    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField]
    private int maxHP = 20;
    private int currentHp;

    void Start()
    {
        currentHp = maxHP;

        ThanhMau.gameObject.SetActive(false);
        if (ThanhMau != null)
        {
            ThanhMau.minValue = 0f;
            ThanhMau.maxValue = maxHP;
            ThanhMau.value = currentHp;
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
        gameObject.SetActive(false);
    }

    // LUỒNG TỰ ĐỘNG CHỌN LỌC SÁT THƯƠNG
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra đúng Tag phe địch
        if (collision.CompareTag("SatthuongI"))
        {
            DamageSource nguonDan = collision.GetComponent<DamageSource>();

            if (nguonDan != null && bangSatThuongChung != null)
            {
                // 1. Đạn báo tên chủng lính bắn ra nó (Ví dụ: "Titan")
                string chungLoaiDan = nguonDan.tenChungLinhBan;

                // 2. ScriptableObject tự động chọn lọc và trả về đúng số dam tương ứng
                int damSauCung = bangSatThuongChung.LaySatThuongTuChung(chungLoaiDan);

                Debug.Log($"🎯 Trúng đạn từ chủng: {chungLoaiDan} | Tự động lọc ra Đam: {damSauCung}");

                // 3. Trừ máu
                TakeDamage(damSauCung);
            }

            ThanhMau.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        currentHp = maxHP;
    }
    private void OnDisable()
    {
        ThanhMau.gameObject.SetActive(false);
    }
}