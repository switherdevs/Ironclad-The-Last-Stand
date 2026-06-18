using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health_chaos : MonoBehaviour
{
    [Header("--- KẾT NỐI SCRIPTABLE OBJECT DUY NHẤT ---")]
    public HeThongSatThuongData bangSatThuongChung;

    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField] private int maxHP = 20;
    private int currentHp;
    [SerializeField] private float Animationdead = 2f;

    [Header("--- CẤU HÌNH HOẠT ẢNH CHẾT TÙY BIẾN ---")]
    [SerializeField] private string tenAnimationChet = "die_ter_chaos";

    [Header("--- PREFAB POPUP SÁT THƯƠNG (TEXT MESH PRO) ---")]
    public GameObject prefabPopupSatThuong;

    private Animator animator;
    [SerializeField]
    private GameObject spritel;
    public bool Deadre = false;

    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Dead;
    private void Awake()
    {
        Amthanh = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        animator.enabled = false;
        spritel.SetActive(false);
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
        if(Dead != null)
        {
            Amthanh.PlayOneShot(Dead);
        }
    gameObject.tag = "Untagged";
        Deadre = true;

        if (HeThongKinhTe.Instance != null)
        {
            HeThongKinhTe.Instance.NhanTienKhiQuaiChet(gameObject.name);
        }
        
        if (animator != null && !string.IsNullOrEmpty(tenAnimationChet) && animator.enabled)
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
        yield return new WaitForSeconds(Animationdead);
        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentHp <= 0) return;

        if (collision.CompareTag("Map"))
        {
            spritel.SetActive(true);
            animator.enabled = true;
        }

        if (collision.CompareTag("SatthuongI"))
        {
            DamageSource nguonDan = collision.GetComponent<DamageSource>();

            if (nguonDan != null && bangSatThuongChung != null)
            {
                string chungLoaiDan = nguonDan.tenChungLinhBan;
                int damSauCung = bangSatThuongChung.LaySatThuongTuChung(chungLoaiDan);
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
        animator.enabled = false;
        spritel.SetActive(false);
        gameObject.tag = "Enemy";
        Deadre = false;
        currentHp = maxHP;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.Rebind();
        }

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