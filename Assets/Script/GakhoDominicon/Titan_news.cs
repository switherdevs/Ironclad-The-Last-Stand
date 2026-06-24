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

    [Header("Chỉ số chiến đấu (Tầm Xa)")]
    public float TamBan = 50f;
    public int satThuong = 40;
    public Transform DiemBan; // Điểm bắn dùng làm mốc so sánh Y thông minh
    public GameObject prefabDanLon;

    [Header("Hệ thống bắn loạt (Burst - Súng Máy)")]
    public int soVienMoiLoat = 10;
    public float thoiGianGiuaCacVien = 0.1f;
    public float thoiGianHoiChieu = 3f;
    [Range(0.1f, 5f)] public float heSoTangToc = 1f;

    [Header("--- Hệ Thống Giẫm Đạp (Cận Combat) ---")]
    public Transform tamQuetGiam;
    public int soQuoiDeGiam = 3;
    public float banKinhQuetGiam = 3.5f;
    public float thoiGianChoDenKhiGiamTrung = 0.4f;
    public GameObject vfxChanKhiGiam;
    public GameObject objectKhiGiam;
    public float thoiGianGiam = 1.2f;

    [Header("Di chuyển & Độ mượt")]
    public float tocDoHanhQuan = 2f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;

    [Header("Cấu hình di chuyển thông minh (Trục Y)")]
    [Tooltip("Khoảng cách tối đa Titan có thể di chuyển lên/xuống theo trục Y so với vị trí gốc")]
    public float phamViDiChuyenY = 5f;
    [Tooltip("Sai số trục Y cho phép giữa họng súng và quái để Titan dừng di chuyển Y và bắn")]
    public float saiSoCanhY = 0.1f;

    private Rigidbody2D rb;
    private Transform mucTieuQuai;
    private bool _registered = false;
    private bool dangTrongLoat = false;
    private bool dangGiamDap = false;
    private float thoiGianBanTiepTheo = 0f;
    private float viTriYGoc;

    public Health_phechinh phechinh;
    private Animator Titan_animator;
    private AudioSource Amthanh;
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip amThanhGiam;

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

        if (tamQuetGiam == null) tamQuetGiam = transform;

        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(false);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(false);

        viTriYGoc = transform.position.y;

        KichHoatXepHang();
    }

    void Update()
    {
        if ((phechinh != null && phechinh.Dear) || (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)) return;

        if (dangGiamDap) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuThongMinh();
        else ResetTarget();

        if (mucTieuQuai != null && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            XoayMat(mucTieuQuai.position.x);
        }

        if (Titan_animator != null)
        {
            Titan_animator.SetBool("Titan_isMoving", !dangTrongLoat && rb.linearVelocity.magnitude > 0.4f);
        }
    }

    void FixedUpdate()
    {
        if (phechinh != null && phechinh.Dear) return;

        if (!dangGiamDap && KiemTraVaKichHoatGiam())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (dangGiamDap)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 vanTocMongMuon = Vector2.zero;

        // Tính toán vận tốc dựa trên Đội hình (X) và canh tọa độ Y theo Điểm Bắn
        vanTocMongMuon = TinhVanTocCanhGocYThongMinh();

        if (mucTieuQuai != null && mucTieuQuai.gameObject.activeInHierarchy && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            float distToTargetX = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            // Kiểm tra khoảng cách X lọt vào tầm bắn
            if (distToTargetX <= TamBan)
            {
                vanTocMongMuon.x = 0; // Đứng lại theo chiều ngang giữ khoảng cách đội hình

                // Thêm điều kiện: Chỉ thực sự xả súng khi trục Y của điểm bắn đã khớp gần như hoàn hảo với quái
                float chechLechY = mucTieuQuai.position.y - (DiemBan != null ? DiemBan.position.y : transform.position.y);

                if (Mathf.Abs(chechLechY) <= saiSoCanhY)
                {
                    if (!dangTrongLoat && Time.time >= thoiGianBanTiepTheo)
                    {
                        StartCoroutine(ThucHienLoatBan());
                    }
                }
            }
        }

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, vanTocMongMuon, doMuotDiChuyen);
    }

    bool KiemTraVaKichHoatGiam()
    {
        Collider2D[] vatTheQuetDuoc = Physics2D.OverlapCircleAll(tamQuetGiam.position, banKinhQuetGiam);
        int demSoQuoiHienTai = 0;

        foreach (var col in vatTheQuetDuoc)
        {
            if (col.CompareTag("Enemy") && !KiemTraDichDaChet(col.gameObject))
            {
                demSoQuoiHienTai++;
            }
        }

        if (demSoQuoiHienTai >= soQuoiDeGiam)
        {
            StartCoroutine(ThucHienDonGiam());
            return true;
        }
        return false;
    }

    IEnumerator ThucHienDonGiam()
    {
        dangGiamDap = true;
        if (dangTrongLoat) dangTrongLoat = false;

        if (Titan_animator != null)
        {
            Titan_animator.SetBool("Titan_isShooting", false);
            Titan_animator.SetTrigger("Titan_StompTrigger");
        }

        float thoiGianConLai = Mathf.Max(0f, thoiGianGiam - thoiGianChoDenKhiGiamTrung);
        yield return new WaitForSeconds(thoiGianChoDenKhiGiamTrung);

        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(true);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(true);
        if (Amthanh != null && amThanhGiam != null) Amthanh.PlayOneShot(amThanhGiam);

        yield return new WaitForSeconds(thoiGianConLai);

        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(false);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(false);

        dangGiamDap = false;
        thoiGianBanTiepTheo = Time.time + 0.5f;
    }

    IEnumerator ThucHienLoatBan()
    {
        dangTrongLoat = true;
        if (Titan_animator != null) Titan_animator.SetBool("Titan_isShooting", true);

        for (int i = 0; i < soVienMoiLoat; i++)
        {
            if (mucTieuQuai == null || KiemTraDichDaChet(mucTieuQuai.gameObject) || dangGiamDap) break;

            Vector2 huongBayMoiVien = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
            TitanBanDanHuongMucTieu(huongBayMoiVien);

            if (Amthanh != null && shoot != null) Amthanh.PlayOneShot(shoot);
            yield return new WaitForSeconds(thoiGianGiuaCacVien);
        }

        dangTrongLoat = false;
        thoiGianBanTiepTheo = Time.time + HoiChieuThucTe;
        if (Titan_animator != null) Titan_animator.SetBool("Titan_isShooting", false);
    }

    void TitanBanDanHuongMucTieu(Vector2 dir)
    {
        if (DiemBan == null || prefabDanLon == null) return;

        GameObject vienDan = QuanLyDan.Instance != null ? QuanLyDan.Instance.LayDanTuKho(prefabDanLon) : Instantiate(prefabDanLon);
        if (vienDan != null)
        {
            float gocXoayMặcĐịnh = dir == Vector2.left ? 180f : 0f;
            Quaternion rotationCuaDan = Quaternion.Euler(0, 0, gocXoayMặcĐịnh);

            vienDan.transform.SetPositionAndRotation(DiemBan.position, rotationCuaDan);
            vienDan.SetActive(true);

            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
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

    // 🎯 THUẬT TOÁN ĐÃ ĐƯỢC NÂNG CẤP THEO YÊU CẦU: CANH CHUẨN TRỤC Y CỦA ĐIỂM BẮN THEO QUÁI & BỎ MAP
    Vector2 TinhVanTocCanhGocYThongMinh()
    {
        Vector2 tocDoKetQua = Vector2.zero;

        // 1. Lấy tốc độ X từ FormationManager
        if (FormationManager.Instance != null)
        {
            tocDoKetQua.x = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan).x;
        }

        // 2. Tính toán trục Y để Điểm bắn (DiemBan) đuổi kịp trục Y của quái mục tiêu
        if (mucTieuQuai != null && mucTieuQuai.gameObject.activeInHierarchy && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            // Lấy vị trí Y của điểm bắn (nếu chưa gán DiemBan thì tự lấy tâm Titan)
            float hienTaiY = DiemBan != null ? DiemBan.position.y : transform.position.y;
            float mucTieuY = mucTieuQuai.position.y;

            // Tính khoảng cách lệch giữa họng súng và quái
            float chechLechY = mucTieuY - hienTaiY;

            // Nếu khoảng lệch lớn hơn sai số cho phép, tiếp tục di chuyển đón đầu
            if (Mathf.Abs(chechLechY) > saiSoCanhY)
            {
                // Kiểm tra giới hạn phạm vi di chuyển trục Y so với vị trí gốc ban đầu
                if (chechLechY > 0 && transform.position.y < viTriYGoc + phamViDiChuyenY)
                {
                    tocDoKetQua.y = tocDoHanhQuan; // Quái ở trên họng súng -> Đi lên
                }
                else if (chechLechY < 0 && transform.position.y > viTriYGoc - phamViDiChuyenY)
                {
                    tocDoKetQua.y = -tocDoHanhQuan; // Quái ở dưới họng súng -> Đi xuống
                }
            }
            else
            {
                tocDoKetQua.y = 0f; // Đã thẳng hàng hoàn hảo với họng súng -> Dừng trục Y để xả súng
            }
        }
        else
        {
            // Nếu không có mục tiêu, từ từ di chuyển về lại cao độ ban đầu
            float khoangCachVeGoc = viTriYGoc - transform.position.y;
            if (Mathf.Abs(khoangCachVeGoc) > 0.1f)
            {
                tocDoKetQua.y = Mathf.Sign(khoangCachVeGoc) * tocDoHanhQuan;
            }
        }

        // Xử lý xoay mặt khi không có mục tiêu cụ thể
        if (mucTieuQuai == null)
        {
            if (lenhHienTai == LenhChienThuat.PhongThu || lenhHienTai == LenhChienThuat.TanCong) XoayMat(transform.position.x + 1f);
            else if (lenhHienTai == LenhChienThuat.RutLui) XoayMat(transform.position.x - 1f);
        }

        return tocDoKetQua;
    }

    bool KiemTraDichDaChet(GameObject go) => go.GetComponent<Health_chaos>()?.Deadre ?? go.CompareTag("Untagged");

    void XoayMat(float x)
    {
        if (dangTrongLoat || dangGiamDap || Mathf.Abs(x - transform.position.x) < 0.05f) return;
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

    int GetRank() => loaiHinhDonVi switch { LoaiLinh.KhoGrak => 0, LoaiLinh.IronStorm => 1, LoaiLinh.Terminator => 2, LoaiLinh.DeadIron => 3, LoaiLinh.Titan => 4, _ => -1 };

    private void OnDisable()
    {
        ResetTarget();
        if (_registered && FormationManager.Instance != null) FormationManager.Instance.Unregister(gameObject);
        if (Titan_animator) Titan_animator.SetBool("Titan_isShooting", false);
        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(false);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (tamQuetGiam == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tamQuetGiam.position, banKinhQuetGiam);
    }
}