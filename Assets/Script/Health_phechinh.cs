using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health_phechinh : MonoBehaviour
{
    [Header("--- KẾT NỐI SCRIPTABLE OBJECT DUY NHẤT ---")]
    public HeThongSatThuongData bangSatThuongChung;

    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField]
    private int maxHP = 20;
    private int currentHp;
    [SerializeField]
    private float Dead_ani;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

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

        // SỬA TẠI ĐÂY: Đẩy lệnh set animation chết ra khỏi ngoặc if để luôn luôn chạy khi Die()
        if (animator != null)
        {
            animator.SetBool("die", true);
        }

        // SỬA TẠI ĐÂY: Ẩn thanh máu ngay lập tức khi bắt đầu chết để không bị hiện đè lên xác
        if (ThanhMau != null)
        {
            ThanhMau.gameObject.SetActive(false);
        }

        StartCoroutine(Out());
    }

    IEnumerator Out()
    {
        yield return new WaitForSeconds(Dead_ani);
        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // SỬA TẠI ĐÂY: Chặn không nhận thêm sát thương và không bật lại thanh máu nếu máu đã hết (đang trong trạng thái chết)
        if (currentHp <= 0) return;

        if (collision.CompareTag("SatthuongQ"))
        {
            DamageSource nguonDanChaos = collision.GetComponent<DamageSource>();

            if (nguonDanChaos != null && bangSatThuongChung != null)
            {
                string chungLoaiDanChaos = nguonDanChaos.tenChungLinhBan;
                int damNhanVe = bangSatThuongChung.LaySatThuongTuChung(chungLoaiDanChaos);
                Debug.Log($"🛡️ Phe chính trúng đạn từ chủng Chaos: {chungLoaiDanChaos} | Hệ thống lọc ra Đam: {damNhanVe}");
                TakeDamage(damNhanVe);
            }
            else
            {
                TakeDamage(5);
            }

            if (ThanhMau != null && currentHp > 0)
            {
                ThanhMau.gameObject.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        // SỬA TẠI ĐÂY: Thêm điều kiện if bảo hiểm để tránh lỗi crash NullReference khi lấy dữ liệu Animator từ Object Pool
        if (animator != null)
        {
            animator.SetBool("die", false);
        }
        else
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.SetBool("die", false);
        }

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