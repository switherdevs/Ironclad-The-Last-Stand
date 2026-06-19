using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class Health_phechinh : MonoBehaviour
{
    [Header("--- KẾT NỐI SCRIPTABLE OBJECT DUY NHẤT ---")]
    public HeThongSatThuongData bangSatThuongChung;

    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField] private int maxHP = 20;
    private int currentHp;
    [SerializeField] private float Dead_ani;

    [Header("--- CẤU HÌNH HOẠT ẢNH CHẾT TÙY BIẾN ---")]
    [SerializeField] private string tenAnimationChet = "die_phechinh_state";

    [Header("--- PREFAB POPUP SÁT THƯƠNG (TEXT MESH PRO) ---")]
    public GameObject prefabPopupSatThuong;
    public bool Dear = false;
    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Dead;
    private Animator animator;

    private void Awake()
    {
        Amthanh = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        CapNhatMauTheoDataGoc(); // Cập nhật máu nâng cấp từ Data Gốc

        if (ThanhMau != null)
        {
            ThanhMau.minValue = 0f;
            ThanhMau.maxValue = maxHP;
            ThanhMau.value = currentHp;
            ThanhMau.gameObject.SetActive(false);
        }
    }

    private void CapNhatMauTheoDataGoc()
    {
        if (bangSatThuongChung != null)
        {
            maxHP = bangSatThuongChung.LayMauTuChung(gameObject.name);
        }
        currentHp = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHP);

        if (prefabPopupSatThuong != null)
        {
            GameObject popup = Instantiate(prefabPopupSatThuong);
            float doLechNgauNhienX = Random.Range(-0.5f, 0.5f);
            Vector3 viTriXuatHien = transform.position + new Vector3(doLechNgauNhienX, 1.5f, 0f);
            popup.transform.position = viTriXuatHien;

            DamagePopup scriptPopup = popup.GetComponent<DamagePopup>();
            if (scriptPopup != null)
            {
                scriptPopup.ThietLapThongSo(damage);
            }
        }

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
        if (Dead != null)
        {
            Amthanh.PlayOneShot(Dead);
        }
        Dear = true;
        gameObject.tag = "Untagged";

        if (ResourceManager.Instance != null)
        {
            string tenLinhQuetDuoc = gameObject.name.ToLower();

            // ⭐ ĐÃ NÂNG CẤP: Nhận diện Servitor tử trận
            if (tenLinhQuetDuoc.Contains("servitor"))
            {
                // Giả định Servitor tốn 1 slot lính chung, và truyền 'true' để trừ số lượng Servitor hiển thị
                ResourceManager.Instance.TruLinh(1, true);
                Debug.Log($"[TỰ ĐỘNG] Servitor làm việc tử trận. Thu hồi 1 slot lính và giảm 1 Servitor.");
            }
            else if (tenLinhQuetDuoc.Contains("khograk"))
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

        if (animator != null && !string.IsNullOrEmpty(tenAnimationChet))
        {
            animator.CrossFade(tenAnimationChet, 0.1f);
        }

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
        Dear = false;
        gameObject.tag = "Phechinh";

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.Rebind();
        }

        CapNhatMauTheoDataGoc();

        if (ThanhMau != null)
        {
            ThanhMau.maxValue = maxHP;
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