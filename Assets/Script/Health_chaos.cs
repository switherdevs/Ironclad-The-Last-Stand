using UnityEngine;
using UnityEngine.UI;

public class Health_chaos : MonoBehaviour
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
         if(collision.CompareTag("SatthuongI"))
        {
            TakeDamage(5);
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