using UnityEngine;

public class TitanPhe9 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu")]
    public float TamBan = 50f;
    public int satThuong = 40;
    public Transform DiemBan;
    public GameObject prefabDanLon;

    [Header("Hệ thống bắn loạt (Burst)")]
    public int soVienMoiLoat = 3;
    public float thoiGianGiuaCacVien = 0.2f;
    public float thoiGianHoiChieu = 3f;

    [Range(0.1f, 5f)]
    public float heSoTangToc = 1f;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;
    public float doLechHangY = 0.3f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 2f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;

    private int soVienDaBan = 0;
    private bool dangTrongLoat = false;
    private float thoiGianBanTiepTheo = 0f;
    public Health_phechinh phechinh;

    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip shoot;

    private float HoiChieuThucTe => thoiGianHoiChieu / Mathf.Max(heSoTangToc, 0.01f);

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator Titan_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────

    void Start()
    {
        Amthanh = GetComponent<AudioSource>();
        phechinh = GetComponent<Health_phechinh>();
        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        Titan_animator = GetComponentInChildren<Animator>();
        // ────────────────────────────────────────────────────────────────────

        if (vungBoxPhongThu != null)
            viTriCoDinh = LayViTriNgauNhienTrongBox(vungBoxPhongThu);
        else
        {
            viTriCoDinh = transform.position;
            daDenViTriThu = true;
        }
    }

    void Update()
    {
        if (phechinh.Dear) return;

        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null;
            dangTrongLoat = false;
            soVienDaBan = 0;
            // ── [ANIMATION] Reset isShooting khi mất target giữa loạt ───────
            if (Titan_animator != null)
                Titan_animator.SetBool("Titan_isShooting", false);
            // ──────────────────────────────────────────────────────────────
            TimMucTieuThongMinh();
        }

        // ── [ANIMATION] Cập nhật isMoving theo vị trí thực tế ───────────────
        if (Titan_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            Titan_animator.SetBool("Titan_isMoving", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            if (khoangCachXThucTe <= (TamBan + 2f))
            {
                dangDungBan = true;

                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                    return;
                }

                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x);
                    XuLyBanLoat();
                    return;
                }
            }
        }

        if (!daDenViTriThu && !dangDungBan)
            HanhQuanVaoViTri();
    }

    void XuLyBanLoat()
    {
        if (Time.time < thoiGianBanTiepTheo) return;

        if (dangTrongLoat)
        {
            // ── [ANIMATION] Bật isShooting khi bắn viên trong loạt ──────────
            if (Titan_animator != null)
                Titan_animator.SetBool("Titan_isShooting", true);
            // ──────────────────────────────────────────────────────────────

            TitanBanDanPooling();
            soVienDaBan++;

            if (soVienDaBan >= soVienMoiLoat)
            {
                dangTrongLoat = false;
                soVienDaBan = 0;
                thoiGianBanTiepTheo = Time.time + HoiChieuThucTe;

                // ── [ANIMATION] Tắt isShooting khi hết loạt, vào hồi chiêu ──
                if (Titan_animator != null)
                    Titan_animator.SetBool("Titan_isShooting", false);
                // ──────────────────────────────────────────────────────────
            }
            else
            {
                thoiGianBanTiepTheo = Time.time + thoiGianGiuaCacVien;
            }
        }
        else
        {
            dangTrongLoat = true;
            soVienDaBan = 0;

            // ── [ANIMATION] Bật isShooting khi bắt đầu loạt mới ────────────
            if (Titan_animator != null)
                Titan_animator.SetBool("Titan_isShooting", true);
            // ──────────────────────────────────────────────────────────────

            TitanBanDanPooling();
            soVienDaBan++;

            thoiGianBanTiepTheo = Time.time + thoiGianGiuaCacVien;
        }
    }

    public void DatHeSoTangToc(float heSo) => heSoTangToc = Mathf.Max(0.1f, heSo);
    public void DatSoVienMoiLoat(int soVien) => soVienMoiLoat = Mathf.Max(1, soVien);

    void HanhQuanVaoViTri()
    {
        XoayMat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    public void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null) return;
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            transform.position.z
        );
    }

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float xLonNhat = -Mathf.Infinity;
        GameObject quaiUuTien = null;
        GameObject quaiDuPhong = null;
        float xDuPhongLonNhat = -Mathf.Infinity;

        foreach (GameObject quai in mangQuai)
        {
            if (!quai.activeInHierarchy) continue;

            float viTriX = quai.transform.position.x;
            KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>() ?? quai.AddComponent<KhoaMucTieu>();

            if (!marker.daBiKhoaMucTieu)
            {
                if (viTriX > xLonNhat) { xLonNhat = viTriX; quaiUuTien = quai; }
            }
            else
            {
                if (viTriX > xDuPhongLonNhat) { xDuPhongLonNhat = viTriX; quaiDuPhong = quai; }
            }
        }

        XacDinhVaKhoaMucTieu(quaiUuTien, quaiDuPhong);
    }

    void XacDinhVaKhoaMucTieu(GameObject quaiUuTien, GameObject quaiDuPhong)
    {
        if (quaiUuTien != null)
        {
            mucTieuQuai = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
            mucTieuQuai = quaiDuPhong.transform;
        else
            mucTieuQuai = null;
    }

    void XoayMat(float xMucTieu)
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(
            xMucTieu < transform.position.x ? -scaleX : scaleX,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    void TitanBanDanPooling()
    {
        if (DiemBan == null || QuanLyDan.Instance == null || prefabDanLon == null) return;

        float huongBanX = (mucTieuQuai.position.x < transform.position.x) ? 180f : 0f;
        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanLon);

        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = Quaternion.Euler(0, 0, huongBanX);
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);
            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
                Amthanh.PlayOneShot(shoot);
            }
        }
    }

    private void OnDisable()
    {
        if (mucTieuQuai != null)
        {
            KhoaMucTieu marker = mucTieuQuai.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }
        dangTrongLoat = false;
        soVienDaBan = 0;

        // ── [ANIMATION] Reset animation khi bị disable ──────────────────────
        if (Titan_animator != null)
        {
            Titan_animator.SetBool("Titan_isMoving", false);
            Titan_animator.SetBool("Titan_isShooting", false);
        }
        // ────────────────────────────────────────────────────────────────────
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}