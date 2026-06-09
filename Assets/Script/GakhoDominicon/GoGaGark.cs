using UnityEditor.Timeline;
using UnityEngine;

public class NhanVat1Controller : MonoBehaviour
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
    [SerializeField]
    private int soVienLoatNay = 1;

    private Animator Khogark_animatior;
    private Vector3 viTriKhungHinhTruoc;
    public Health_phechinh phechinh;

    void Start()
    {
        phechinh = GetComponent<Health_phechinh>();
        Khogark_animatior = GetComponentInChildren<Animator>();
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

        if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy)
        {
            ThayDich = null;
            // ── [ANIMATION] Reset isShooting khi mất target giữa loạt ───────
            if (Khogark_animatior != null)
                Khogark_animatior.SetBool("Khogark_isShooting", false);
            // ──────────────────────────────────────────────────────────────
            TimMucTieuThongMinh();
        }

        bool dangDungBan = false;

        // ── [ANIMATION] Cập nhật isMoving theo vị trí thực tế ──────────────
        if (Khogark_animatior != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            Khogark_animatior.SetBool("Khogark_isMoving", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────

        if (ThayDich != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

            if (khoangCachX <= (TamBan + 2f))
            {
                dangDungBan = true;

                if (doLechYThucTe > DolechHangY)
                {
                    DiChuyenTrungHangY();
                }

                if (doLechYThucTe <= DolechHangY && khoangCachX <= TamBan)
                {
                    Xoaymat(ThayDich.position.x);

                    if (HoiChieu <= Time.time)
                    {
                        StartCoroutine(TanCong());
                        HoiChieu = Time.time + (1f / soDanBan);
                    }
                }
            }
        }

        if (!daDenViTriThu && !dangDungBan)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        Xoaymat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
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

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject quaiUuTien = null;
        GameObject quaiDuPhong = null;
        float kcDuPhongNganNhat = Mathf.Infinity;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float kc = Vector2.Distance(transform.position, quai.transform.position);

                KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>();
                if (marker == null) marker = quai.AddComponent<KhoaMucTieu>();

                if (!marker.daBiKhoaMucTieu)
                {
                    if (kc < khoangCachNganNhat)
                    {
                        khoangCachNganNhat = kc;
                        quaiUuTien = quai;
                    }
                }
                else
                {
                    if (kc < kcDuPhongNganNhat)
                    {
                        kcDuPhongNganNhat = kc;
                        quaiDuPhong = quai;
                    }
                }
            }
        }
        XacDinhVaKhoaMucTieu(quaiUuTien, quaiDuPhong);
    }

    void XacDinhVaKhoaMucTieu(GameObject quaiUuTien, GameObject quaiDuPhong)
    {
        if (quaiUuTien != null)
        {
            ThayDich = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
        {
            ThayDich = quaiDuPhong.transform;
        }
        else
        {
            ThayDich = null;
        }
    }

    void Xoaymat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private System.Collections.IEnumerator TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) yield break;

        // ── [ANIMATION] Bật trạng thái bắn ─────────────────────────────────
        if (Khogark_animatior != null)
            Khogark_animatior.SetBool("Khogark_isShooting", true);
        // ────────────────────────────────────────────────────────────────────

        for (int i = 0; i < soVienLoatNay; i++)
        {
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy) break;

            Xoaymat(ThayDich.position.x);

            float huongBanX = (ThayDich.position.x < transform.position.x) ? 180f : 0f;
            Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

            GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
            if (vienDan != null)
            {
                vienDan.transform.position = DiemBan.position;
                vienDan.transform.rotation = rotation;
                vienDan.transform.SetParent(null);
                vienDan.SetActive(true);

                DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
                if (scriptDan != null)
                {
                    scriptDan.satThuong = satThuong;
                    scriptDan.KichHoatVienDan();
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        // ── [ANIMATION] Tắt trạng thái bắn ─────────────────────────────────
        if (Khogark_animatior != null)
            Khogark_animatior.SetBool("Khogark_isShooting", false);
        // ────────────────────────────────────────────────────────────────────

        yield return new WaitForSeconds(1f / soDanBan);
    }

    private void OnDisable()
    {
        if (ThayDich != null)
        {
            KhoaMucTieu marker = ThayDich.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }
        // ── [ANIMATION] Reset animation khi bị disable ──────────────────────
        if (Khogark_animatior != null)
        {
            Khogark_animatior.SetBool("Khogark_isMoving", false);
            Khogark_animatior.SetBool("Khogark_isShooting", false);
        }
        // ────────────────────────────────────────────────────────────────────
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}