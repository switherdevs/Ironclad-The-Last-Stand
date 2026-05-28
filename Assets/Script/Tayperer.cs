using UnityEngine;
using UnityEngine.UI;

public class Tayperer : MonoBehaviour
{
    [Header("--- KẾT NỐI UI THANH MÁU ---")]
    public Slider ThanhMau;

    [Header("--- CHỈ SỐ MÁU ---")]
    [SerializeField]
    private int maxHP = 20;
    private int currentHp;

    public static Tayperer skibidi { get; private set; }

    public bool GameOver = false;
    private void Awake()
    {
        skibidi = this;
    }

    void Start()
    {
        currentHp = maxHP;
        ThanhMau.gameObject.SetActive(false);

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
        GameOver = true;
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
}