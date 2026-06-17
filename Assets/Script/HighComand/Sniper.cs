using UnityEngine;
using UnityEngine.Rendering;

public class SniperSkill : MonoBehaviour
{
    [Header("Cursor")]
    public Texture2D sniperCursor;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Skill")]
    public int maxShots = 5;

    [Header("Damage")]
    public int damage = 20;

    [Header("Ban Kinh")]
    public float BanKinh = 5f;

    private int currentShots;
    private bool skillActive = false;

    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Shooot;
    private void Start()
    {
         Amthanh = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!skillActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void ActivateSkill()
    {
        skillActive = true;
        currentShots = maxShots;

        Vector2 hotspot = new Vector2(
            sniperCursor.width / 2,
            sniperCursor.height / 2);

        Cursor.SetCursor(sniperCursor, hotspot, CursorMode.Auto);

        Debug.Log("Đã kích hoạt chế độ bắn tỉa!");
    }

    void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Hiệu ứng nổ
        Instantiate(explosionPrefab, mousePos, Quaternion.identity);
        Amthanh.PlayOneShot(Shooot);

        // Gây sát thương cho quái trong bán kính 1.5
        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePos, BanKinh);

        foreach (Collider2D hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
        }

        currentShots--;

        Debug.Log("Đã bắn! Còn lại: " + currentShots);

        if (currentShots <= 0)
        {
            DeactivateSkill();
        }
    }

    void DeactivateSkill()
    {
        skillActive = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        Debug.Log("Hết đạn, thoát chế độ bắn tỉa!");
    }
}