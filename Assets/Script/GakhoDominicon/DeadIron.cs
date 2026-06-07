using UnityEngine;

public class NhanVat4 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu (Pháo Hạng Nặng)")]
    public float TamBan = 12f;
    public int satThuong = 150;
    public Transform DiemBan;
    public GameObject prefabDanMobi;

    [Header("Hiệu ứng nạp năng lượng")]
    public GameObject hieuUngNapNangLuong;
    public float thoiGianNapNangLuong = 5f;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;
    public float doLechHangY = 0.2f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f;
    private bool dangTrongLuongBan = false;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator DeadIron_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        DeadIron_animator = GetComponentInChildren<Animator>();
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

        if (hieuUngNapNangLuong != null) hieuUngNapNangLuong.SetActive(false);
    }

    void Update()
    {
        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null;
            // ── [ANIMATION] Reset isShooting khi mất target giữa luồng bắn ──
            if (!dangTrongLuongBan)
            {
                if (DeadIron_animator != null)
                    DeadIron_animator.SetBool("DeadIron_isShooting", false);
            }
            // ──────────────────────────────────────────────────────────────
            TimMucTieuThongMinh();
        }

        // ── [ANIMATION] Cập nhật isMoving theo vị trí thực tế ───────────────
        if (DeadIron_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            DeadIron_animator.SetBool("DeadIron_isMoving", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            if (khoangCachXThucTe <= (TamBan + 5f))
            {
                dangDungBan = true;

                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                }

                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x);

                    if (Time.time >= thoiGianBanTiepTheo && !dangTrongLuongBan)
                    {
                        StartCoroutine(LuongBanNapNangLuong());
                    }
                }
            }
        }

        if (!daDenViTriThu && !dangDungBan && !dangTrongLuongBan)
        {
            HanhQuanVaoViTri();
        }
    }

    private System.Collections.IEnumerator LuongBanNapNangLuong()
    {
        dangTrongLuongBan = true;

        if (hieuUngNapNangLuong != null)
            hieuUngNapNangLuong.SetActive(true);

        // ── [ANIMATION] Bật isShooting khi bắt đầu nạp năng lượng ──────────
        // (DeadIron dùng animation "charge + fire" gộp chung trong isShooting)
        if (DeadIron_animator != null)
            DeadIron_animator.SetBool("DeadIron_isShooting", true);
        // ────────────────────────────────────────────────────────────────────

        yield return new WaitForSeconds(thoiGianNapNangLuong);

        if (mucTieuQuai != null)
        {
            BanThienThachPooling();
        }

        if (hieuUngNapNangLuong != null)
            hieuUngNapNangLuong.SetActive(false);

        // ── [ANIMATION] Tắt isShooting sau khi bắn xong ─────────────────────
        if (DeadIron_animator != null)
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
        // ────────────────────────────────────────────────────────────────────

        thoiGianBanTiepTheo = Time.time + 0.5f;

        dangTrongLuongBan = false;
    }

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

    void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null || dangTrongLuongBan) return;
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
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
            mucTieuQuai = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
        {
            mucTieuQuai = quaiDuPhong.transform;
        }
        else
        {
            mucTieuQuai = null;
        }
    }

    void BanThienThachPooling()
    {
        if (DiemBan == null || prefabDanMobi == null) return;

        float huongBanX = (mucTieuQuai.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = null;
        if (QuanLyDan.Instance != null)
        {
            vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanMobi);
        }

        if (vienDan == null)
        {
            vienDan = Instantiate(prefabDanMobi, DiemBan.position, rotation);
        }
        else
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);
        }

        if (vienDan != null)
        {
            DanNv4 scriptDan = vienDan.GetComponent<DanNv4>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    void XoayMat(float xMucTieu)
    {
        if (dangTrongLuongBan) return;
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    private void OnDisable()
    {
        if (mucTieuQuai != null)
        {
            KhoaMucTieu marker = mucTieuQuai.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }

        // ── [ANIMATION] Reset animation khi bị disable ──────────────────────
        if (DeadIron_animator != null)
        {
            DeadIron_animator.SetBool("DeadIron_isMoving", false);
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
        }
        // ────────────────────────────────────────────────────────────────────
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}