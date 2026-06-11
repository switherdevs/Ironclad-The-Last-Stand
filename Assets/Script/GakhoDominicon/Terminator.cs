using System.Collections;
using UnityEngine;

public class Terminator : MonoBehaviour
{
    [Header("Chỉ số chiến đấu")]
    public float TamBan = 9f;
    public float soDanBan = 1f;
    public int satThuong = 10;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 5f;
    public float DolechHangY = 0.3f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform ThayDich;
    private float HoiChieu = 0f;

    public int soLuongDanTrongLoat = 5;
    public float thoiGianCachNhauGiuaCacVien = 0.2f;
    private bool dangTrongThoiGianNghi = false;
    public Health_phechinh phechinh;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator Terminator_animator;
    private AudioSource AmthanhTer;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────
    [SerializeField]
    private AudioClip TiengSung;
    void Start()
    {
        AmthanhTer = GetComponent<AudioSource>();
        phechinh = GetComponent<Health_phechinh>();
        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        Terminator_animator = GetComponentInChildren<Animator>();
        // ────────────────────────────────────────────────────────────────────

        if (vungBoxPhongThu != null)
        {
            viTriCoDinh = LayViTriNgauNhienTrongBox(vungBoxPhongThu);
        }
        else
        {
            viTriCoDinh = transform.position;
            daDenViTriThu = true;
        }
    }

    void Update()
    {
        if (phechinh.Dear) return;

        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        TimKiemKeDich();

        // ── [ANIMATION] Cập nhật isMoving theo vị trí thực tế ───────────────
        if (Terminator_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            Terminator_animator.SetBool("Terminator_isMoving", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────

        if (ThayDich != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

            if (khoangCachX <= (TamBan + 2f))
            {
                if (doLechYThucTe > DolechHangY)
                {
                    DiChuyenTrungHangY();
                    return;
                }

                if (khoangCachX <= TamBan)
                {
                    Xoaymat(ThayDich.position.x);

                    if (!dangTrongThoiGianNghi && Time.time >= HoiChieu)
                    {
                        StartCoroutine(ChuKyXaDanVaNguongBan());
                        HoiChieu = Time.time + 1f / soDanBan;
                    }

                    return;
                }
            }
        }

        if (!daDenViTriThu)
        {
            HanhQuanVaoViTri();
        }
    }

    IEnumerator ChuKyXaDanVaNguongBan()
    {
        dangTrongThoiGianNghi = true;

        // ── [ANIMATION] Bật trạng thái bắn ──────────────────────────────────
        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", true);
        // ────────────────────────────────────────────────────────────────────
        AmthanhTer.PlayOneShot(TiengSung);
        for (int i = 0; i < soLuongDanTrongLoat; i++)
        {
            TanCong();
            yield return new WaitForSeconds(thoiGianCachNhauGiuaCacVien);
        }

        // ── [ANIMATION] Tắt trạng thái bắn ──────────────────────────────────
        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", false);
        // ────────────────────────────────────────────────────────────────────

        yield return new WaitForSeconds(3f);

        dangTrongThoiGianNghi = false;
    }

    void HanhQuanVaoViTri()
    {
        Xoaymat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.05f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    public void DiChuyenTrungHangY()
    {
        if (ThayDich == null) return;

        Vector3 viTriMucTieu = new Vector3(transform.position.x, ThayDich.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    void TimKiemKeDich()
    {
        GameObject[] mangDich = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject dichGanNhat = null;

        foreach (GameObject dich in mangDich)
        {
            if (dich.activeInHierarchy)
            {
                float khoangCach = Vector2.Distance(transform.position, dich.transform.position);
                if (khoangCach < khoangCachNganNhat)
                {
                    khoangCachNganNhat = khoangCach;
                    dichGanNhat = dich;
                }
            }
        }

        if (dichGanNhat != null) ThayDich = dichGanNhat.transform;
        else
        {
            ThayDich = null;
            // ── [ANIMATION] Reset isShooting khi không còn địch ─────────────
            if (Terminator_animator != null)
                Terminator_animator.SetBool("Terminator_isShooting", false);
            // ──────────────────────────────────────────────────────────────
        }
    }

    void Xoaymat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    void TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) return;

        float huongBanX = (ThayDich.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.SetActive(true);

            DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }

    private void OnDisable()
    {
        dangTrongThoiGianNghi = false;
        // ── [ANIMATION] Reset animation khi bị disable ──────────────────────
        if (Terminator_animator != null)
        {
            Terminator_animator.SetBool("Terminator_isMoving", false);
            Terminator_animator.SetBool("Terminator_isShooting", false);
        }
        // ────────────────────────────────────────────────────────────────────
    }
}