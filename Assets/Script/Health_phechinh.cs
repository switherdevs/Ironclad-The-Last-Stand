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
    [SerializeField] private int maxHP = 20;
    private int currentHp;
    [SerializeField] private float Dead_ani;

    [Header("--- CẤU HÌNH HOẠT ẢNH CHẾT TÙY BIẾN ---")]
    [SerializeField] private string tenAnimationChet = "die_phechinh_state";

    [Header("--- PREFAB POPUP SÁT THƯƠNG (TEXT MESH PRO) ---")]
    public GameObject prefabPopupSatThuong;
    public bool Dear = false;

    private Animator animator;

    private void Awake()
    {
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

    // Hàm bổ trợ tự động lấy máu đã nâng cấp dựa theo tên của lính (Mới cập nhật)
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

        // KÍCH HOẠT POPUP CHỮ CHO PHE CHÍNH:
        if (prefabPopupSatThuong != null)
        {
            GameObject popup = Instantiate(prefabPopupSatThuong);

            // Tạo độ lệch ngẫu nhiên trục X để chữ nổi liên tiếp đẹp mắt
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
        Dear = true;
        gameObject.tag = "Untagged";

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

        // Thay vì SetBool, ép chạy trực tiếp bằng CrossFade dựa trên tên nhập ở Inspector
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
            animator.Rebind(); // Trả toàn bộ tư thế và hoạt ảnh về mặc định từ Object Pool
        }

        CapNhatMauTheoDataGoc(); // Đảm bảo lấy lính từ Pool ra vẫn nhận đúng máu nâng cấp mới nhất

        if (ThanhMau != null)
        {
            ThanhMau.maxValue = maxHP; // Đồng bộ lại độ dài thanh máu theo HP mới
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