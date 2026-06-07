using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class Health_chaos : MonoBehaviour
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
    private float Animationdead = 2f;

    private Animator animator;
    public bool Deadre = false;

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
        Deadre = true;
        if (animator != null)
        {
            animator.SetBool("die", true);
        }

        // SỬA TẠI ĐÂY: Ẩn thanh máu ngay lập tức khi chết để không bị hiện đè lên xác quái Chaos
        if (ThanhMau != null)
        {
            ThanhMau.gameObject.SetActive(false);
        }

        StartCoroutine(Out());
    }

    IEnumerator Out()
    {
        yield return new WaitForSeconds(Animationdead);
        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // SỬA TẠI ĐÂY: Chặn không cho quái nhận thêm sát thương hoặc hiện lại thanh máu nếu đã chết
        if (currentHp <= 0) return;

        if (collision.CompareTag("SatthuongI"))
        {
            DamageSource nguonDan = collision.GetComponent<DamageSource>();

            if (nguonDan != null && bangSatThuongChung != null)
            {
                string chungLoaiDan = nguonDan.tenChungLinhBan;
                int damSauCung = bangSatThuongChung.LaySatThuongTuChung(chungLoaiDan);
                Debug.Log($"🎯 Trúng đạn từ chủng: {chungLoaiDan} | Tự động lọc ra Đam: {damSauCung}");
                TakeDamage(damSauCung);
            }

            if (ThanhMau != null && currentHp > 0)
            {
                ThanhMau.gameObject.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        // SỬA TẠI ĐÂY: Thêm kiểm tra an toàn tránh lỗi NullReferenceException khi tái sử dụng quái từ Object Pool
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

        // SỬA TẠI ĐÂY: Đảm bảo reset lại giá trị thanh máu đầy và ẩn đi khi quái Chaos hồi sinh
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