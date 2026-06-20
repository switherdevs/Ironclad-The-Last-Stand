using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class Titans_new : MonoBehaviour
{
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu;
    public LoaiLinh loaiHinhDonVi = LoaiLinh.Titan;

    [Header("Chỉ số chiến đấu")]
    public float TamBan = 50f;
    public int satThuong = 40;
    public Transform DiemBan;
    public GameObject prefabDanLon;

    [Header("Hệ thống bắn loạt (Burst)")]
    public int soVienMoiLoat = 10;
    public float thoiGianGiuaCacVien = 0.1f;
    public float thoiGianHoiChieu = 3f;
    [Range(0.1f, 5f)] public float heSoTangToc = 1f;

    [Header("Di chuyển & Độ mượt")]
    public float tocDoHanhQuan = 2f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;

    private Rigidbody2D rb;
    private Transform mucTieuQuai;
    private bool _registered = false;
    private bool dangTrongLoat = false;
    private float thoiGianBanTiepTheo = 0f;

    public Health_phechinh phechinh;
    private Animator Titan_animator;
    private AudioSource Amthanh;
    [SerializeField] private AudioClip shoot;
    private float HoiChieuThucTe => thoiGianHoiChieu / Mathf.Max(heSoTangToc, 0.01f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 10f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        Amthanh = GetComponent<AudioSource>();
    }

    void Start()
    {
        phechinh = GetComponent<Health_phechinh>();
        Titan_animator = GetComponentInChildren<Animator>();

        // 🎯 Tự động kích hoạt đăng ký xếp hàng ma trận khi xuất hiện
        KichHoatXepHang();
    }

    void Update()
    {
        if ((phechinh != null && phechinh.Dear) || (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuThongMinh();
        else ResetTarget();

        // Xoay mặt nhân vật về hướng quái khi đang ngắm/bắn
        if (mucTieuQuai != null && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            XoayMat(mucTieuQuai.position.x);
        }

        // CẬP NHẬT ANIMATION DI CHUYỂN
        if (Titan_animator != null)
        {
            Titan_animator.SetBool("Titan_isMoving", !dangTrongLoat && rb.linearVelocity.magnitude > 0.4f);
        }
    }

    void FixedUpdate()
    {
        if (phechinh != null && phechinh.Dear) return;

        Vector2 vanTocMongMuon = Vector2.zero;

        // Nếu có mục tiêu hợp lệ
        if (mucTieuQuai != null && mucTieuQuai.gameObject.activeInHierarchy && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            float distToTarget = Vector2.Distance(transform.position, mucTieuQuai.position);

            // Nếu quái lọt vào tầm bắn lớn và sẵn sàng bắn (Hoặc đang xả loạt đạn)
            if (distToTarget <= TamBan && (dangTrongLoat || Time.time >= thoiGianBanTiepTheo))
            {
                vanTocMongMuon = Vector2.zero; // Đứng yên xả Burst súng máy

                if (!dangTrongLoat && Time.time >= thoiGianBanTiepTheo)
                {
                    StartCoroutine(ThucHienLoatBan());
                }
            }
            else
            {
                // Nếu quái ở quá xa tầm bắn hoặc đang trong thời gian chờ hồi chiêu Burst -> Tiếp tục di chuyển theo hàng
                vanTocMongMuon = TinhVanTocDoiHinh();
            }
        }
        else
        {
            // Không có quái -> Đi theo đội hình ma trận
            vanTocMongMuon = TinhVanTocDoiHinh();
        }

        // Áp dụng lực di chuyển Lerp mượt mà tránh giật lag vật lý
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, vanTocMongMuon, doMuotDiChuyen);
    }

    IEnumerator ThucHienLoatBan()
    {
        dangTrongLoat = true;
        if (Titan_animator != null) Titan_animator.SetBool("Titan_isShooting", true);

        for (int i = 0; i < soVienMoiLoat; i++)
        {
            if (mucTieuQuai == null || KiemTraDichDaChet(mucTieuQuai.gameObject)) break;

            Vector2 dir = ((Vector2)mucTieuQuai.position - (Vector2)DiemBan.position).normalized;
            TitanBanDanXeoPooling(dir);

            if (Amthanh != null && shoot != null) Amthanh.PlayOneShot(shoot);
            yield return new WaitForSeconds(thoiGianGiuaCacVien);
        }

        dangTrongLoat = false;
        thoiGianBanTiepTheo = Time.time + HoiChieuThucTe;
        if (Titan_animator != null) Titan_animator.SetBool("Titan_isShooting", false);
    }

    void TitanBanDanXeoPooling(Vector2 dir)
    {
        if (DiemBan == null || prefabDanLon == null) return;
        GameObject vienDan = QuanLyDan.Instance != null ? QuanLyDan.Instance.LayDanTuKho(prefabDanLon) : Instantiate(prefabDanLon);
        if (vienDan != null)
        {
            float gocXoay = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            vienDan.transform.SetPositionAndRotation(DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
            vienDan.SetActive(true);
            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null) { scriptDan.satThuong = satThuong; scriptDan.KichHoatVienDan(); }
        }
    }

    public void KichHoatXepHang()
    {
        int rank = GetRank();
        if (rank >= 0 && FormationManager.Instance != null)
        {
            _registered = FormationManager.Instance.Register(gameObject, rank);
        }
    }

    Vector2 TinhVanTocDoiHinh()
    {
        if (FormationManager.Instance == null) return Vector2.zero;
        Vector2 slotVelocity = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan);

        if (mucTieuQuai == null) // Xoay hướng mặt theo lệnh di chuyển khi không có quái
        {
            if (lenhHienTai == LenhChienThuat.PhongThu || lenhHienTai == LenhChienThuat.TanCong) XoayMat(transform.position.x + 1f);
            else if (lenhHienTai == LenhChienThuat.RutLui) XoayMat(transform.position.x - 1f);
        }
        return slotVelocity;
    }

    bool KiemTraDichDaChet(GameObject go) => go.GetComponent<Health_chaos>()?.Deadre ?? go.CompareTag("Untagged");

    void XoayMat(float x)
    {
        if (dangTrongLoat || Mathf.Abs(x - transform.position.x) < 0.05f) return;
        float dir = Mathf.Sign(x - transform.position.x);
        transform.localScale = new Vector3(dir * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void ResetTarget() { if (mucTieuQuai != null) { KhoaMucTieu k = mucTieuQuai.GetComponent<KhoaMucTieu>(); if (k) k.daBiKhoaMucTieu = false; } mucTieuQuai = null; }

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject best = null; float maxDist = -Mathf.Infinity;
        foreach (var q in mangQuai)
        {
            if (!q.activeInHierarchy || KiemTraDichDaChet(q)) continue;
            KhoaMucTieu k = q.GetComponent<KhoaMucTieu>() ?? q.AddComponent<KhoaMucTieu>();
            if (!k.daBiKhoaMucTieu && q.transform.position.x > maxDist) { maxDist = q.transform.position.x; best = q; }
        }
        if (best != null) { mucTieuQuai = best.transform; best.GetComponent<KhoaMucTieu>().daBiKhoaMucTieu = true; }
    }

    // 🎯 ĐỒNG BỘ: Chỉnh rank Titan về đúng 4 (Trùng khớp với cấu hình mảng trong FormationManager)
    int GetRank() => loaiHinhDonVi switch { LoaiLinh.KhoGrak => 0, LoaiLinh.IronStorm => 1, LoaiLinh.Terminator => 2, LoaiLinh.DeadIron => 3, LoaiLinh.Titan => 4, _ => -1 };

    private void OnDisable()
    {
        ResetTarget();
        if (_registered && FormationManager.Instance != null) FormationManager.Instance.Unregister(gameObject);
        if (Titan_animator) Titan_animator.SetBool("Titan_isShooting", false);
    }
}