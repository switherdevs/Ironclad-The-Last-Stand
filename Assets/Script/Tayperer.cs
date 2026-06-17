using UnityEngine;
using UnityEngine.UI;

public class Tayperer : MonoBehaviour
{
    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField]
    private int maxHP = 1000; // Nâng cấp lên máu mặc định 1000
    private int currentHp;

    [Header("--- THIẾT LẬP SPRITE ĐỔI GIAI ĐOẠN ---")]
    [SerializeField] private SpriteRenderer spriteRenderer; // Thành phần hiển thị hình ảnh
    [SerializeField] private Sprite spriteGiaiDoan1_750;     // Máu <= 750 (Nứt nẻ nhẹ)
    [SerializeField] private Sprite spriteGiaiDoan2_500;     // Máu <= 500 (Nứt nẻ vừa)
    [SerializeField] private Sprite spriteGiaiDoan3_250;     // Máu <= 250 (Nứt nẻ nặng)

    [Header("--- PREFAB POPUP SÁT THƯƠNG (TEXT MESH PRO) ---")]
    [Tooltip("Kéo Prefab chữ nhảy sát thương vào đây giống hệt bên Phe Chính")]
    public GameObject prefabPopupSatThuong;

    private Animator animator; // Dùng để gọi animation sụp đổ khi chết
    private bool daChayAnimChet = false; // Khóa chống trùng phát animation nhiều lần

    public static Tayperer skibidi { get; private set; }

    public bool GameOver = false;

    private void Awake()
    {
        skibidi = this;
        animator = GetComponentInChildren<Animator>(); // Tự động lấy Animator trên cùng Object hoặc con của nó

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Tự động lấy SpriteRenderer nếu quên kéo
        }
    }

    void Start()
    {
        currentHp = maxHP;

        if (ThanhMau != null)
        {
            ThanhMau.minValue = 0f;
            ThanhMau.maxValue = maxHP;
            ThanhMau.value = currentHp;
            ThanhMau.gameObject.SetActive(false); // Đồng bộ ẩn thanh máu ban đầu
        }
    }

    public void TakeDamage(int damage)
    {
        if (GameOver) return; // Nếu đã thua rồi thì không nhận thêm sát thương

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHP);

        // ⭐ KÍCH HOẠT POPUP CHỮ NHẢY SÁT THƯƠNG GIỐNG PHE CHÍNH:
        if (prefabPopupSatThuong != null)
        {
            GameObject popup = Instantiate(prefabPopupSatThuong);

            // Tạo độ lệch ngẫu nhiên trục X để chữ nổi liên tiếp không bị đè lên nhau
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

        // KIỂM TRA ĐỔI SPRITE THEO 4 GIAI ĐOẠN MÁU
        CapNhatHinhAnhTheoMucMau();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void CapNhatHinhAnhTheoMucMau()
    {
        if (spriteRenderer == null) return;

        // Ưu tiên kiểm tra mốc máu thấp nhất trước để không bị nhảy sai giai đoạn
        if (currentHp <= 250)
        {
            if (spriteGiaiDoan3_250 != null) spriteRenderer.sprite = spriteGiaiDoan3_250;
        }
        else if (currentHp <= 500)
        {
            if (spriteGiaiDoan2_500 != null) spriteRenderer.sprite = spriteGiaiDoan2_500;
        }
        else if (currentHp <= 750)
        {
            if (spriteGiaiDoan1_750 != null) spriteRenderer.sprite = spriteGiaiDoan1_750;
        }
    }

    void Die()
    {
        GameOver = true;

        // KÍCH HOẠT ANIMATION SỤP ĐỔ 1 LẦN DUY NHẤT
        if (animator != null && !daChayAnimChet)
        {
            daChayAnimChet = true;
            animator.SetTrigger("Collapse"); // Tạo Trigger tên "Collapse" trong Animator Controller của bạn
            Debug.Log("[TAYPERER] Đã kích hoạt hiệu ứng sụp đổ trận đấu!");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameOver) return;

        if (collision.CompareTag("SatthuongQ"))
        {
            TakeDamage(10);
            if (ThanhMau != null)
            {
                ThanhMau.gameObject.SetActive(true);
            }
        }
    }
}