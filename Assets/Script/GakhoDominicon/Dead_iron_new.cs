using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class DeadIronWalk_new : MonoBehaviour
{
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu;
    public LoaiLinh loaiHinhDonVi = LoaiLinh.DeadIron;
    public float TamBan = 40f;

    [Header("====== KHU VỰC 1: PHÁO PLASMA TỤ LỰC ======")]
    public int satThuongPlasma = 120;
    public Transform DiemBanPlasma;
    public GameObject prefabDanPlasma;
    [Tooltip("Thời gian tụ lực/nạp năng lượng trước khi bắn Plasma")]
    public float thoiGianNapPlasma = 5f;
    [Tooltip("Thời gian hồi chiêu sau khi bắn Plasma")]
    public float thoiGianHoiChieuPlasma = 5f;
    [Tooltip("Object hiệu ứng tụ lực (vòng sáng gom năng lượng ở đầu nòng)")]
    public GameObject vfxNapPlasma;
    [Tooltip("Prefab hiệu ứng khai hỏa lóe sáng (Muzzle Flash) khi bắn Plasma")]
    public GameObject vfxKhaiHoaPlasma;
    [Tooltip("File âm thanh tiếng rít nạp năng lượng kéo dài")]
    public AudioClip amThanhNapPlasma;
    [Tooltip("File âm thanh tiếng nổ Plasma bắn ra")]
    public AudioClip amThanhBanPlasma;

    [Header("====== KHU VỰC 2: TÊN LỬA OANH TẠC (BURST) ======")]
    public int satThuongTenLua = 50;
    public Transform DiemBanTenLua;
    public GameObject prefabTenLua;
    public int soTenLuaMoiLoat = 4;
    [Tooltip("Khoảng cách thời gian delay giữa từng quả tên lửa phóng ra trong một loạt")]
    public float delayGiuaCacQuaTenLua = 0.2f;
    [Tooltip("Thời gian đứng yên Idle sau khi kết thúc loạt tên lửa")]
    public float thoiGianIdleSauTenLua = 2f;
    [Tooltip("Âm thanh phóng tên lửa")]
    public AudioClip amThanhPhongTenLua;

    [Header("Cấu hình Vòng lặp Kỹ năng (Mới)")]
    [Tooltip("Số lần bắn Plasma cần thiết để kích hoạt tên lửa")]
    public int soLanPlasmaDeBanTenLua = 3;
    [SerializeField] private int demSoLanBaoNhieuPlasma = 0;

    [Header("Cấu hình Gia Tốc Tên Lửas")]
    [Tooltip("Thời gian tên lửa bay mồi lên không trung trước khi khựng lại")]
    public float thoiGianBayMoi = 0.15f;
    [Tooltip("Tốc độ bay mồi ban đầu khi vừa rời bệ phóng")]
    public float tocDoBayMoi = 8f;
    [Tooltip("Thời gian tên lửa dừng khựng lại để kích hoạt động cơ đẩy")]
    public float thoiGianKhungLai = 0.2f;

    [Header("--- Hệ Thống Giẫm Đạp (Cận Chiến) ---")]
    [Tooltip("Vị trí tùy chỉnh của vòng quét nhận diện quái (ví dụ vị trí bàn chân)")]
    public Transform tamQuetGiam;
    [Tooltip("Số lượng quái tối thiểu lọt vào tầm để kích hoạt cú giẫm")]
    public int soQuoiDeGiam = 3;
    [Tooltip("Bán kính quét hình tròn để nhận diện quái vật")]
    public float banKinhQuetGiam = 3.5f;
    [Tooltip("Thời gian hoãn từ lúc nâng chân đến khi thực sự GIẪM CHẠM ĐẤT để bật hitbox")]
    public float thoiGianChoDenKhiGiamTrung = 0.4f;
    [Tooltip("GameObject hiệu ứng ở chân sẽ active khi chuyển sang trạng thái giẫm")]
    public GameObject vfxChanKhiGiam;
    [Tooltip("GameObject khác sẽ xuất hiện/active khi giẫm (để xử lý gây dam riêng)")]
    public GameObject objectKhiGiam;
    [Tooltip("Tổng thời gian thực hiện hành động giẫm (khóa mọi hành động khác)")]
    public float thoiGianGiam = 1.2f;
    [SerializeField] private AudioClip amThanhGiam;

    [Header("Di chuyển & Độ mượt")]
    public float tocDoHanhQuan = 2.5f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;
    [Range(0.1f, 5f)] public float heSoTangToc = 1f;

    [Header("Cấu hình di chuyển thông minh (Trục Y)")]
    [Tooltip("Khoảng cách tối đa có thể di chuyển lên/xuống theo trục Y so với vị trí gốc")]
    public float phamViDiChuyenY = 5f;
    [Tooltip("Sai số trục Y cho phép giữa họng súng chính và quái để Dead Iron đứng yên bắn")]
    public float saiSoCanhY = 0.1f;

    private Rigidbody2D rb;
    private Transform mucTieuQuai;
    private bool _registered = false;
    private bool dangTrongLoat = false;
    private bool dangBanTenLua = false;
    private bool dangGiamDap = false;
    private float viTriYGoc;

    private float thoiGianHoiPlasmaTiepTheo = 0f;

    public Health_phechinh phechinh;
    private Animator DeadIron_animator;
    private AudioSource Amthanh;

    private float HoiChieuPlasmaThucTe => thoiGianHoiChieuPlasma / Mathf.Max(heSoTangToc, 0.01f);

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
        DeadIron_animator = GetComponentInChildren<Animator>();

        if (tamQuetGiam == null) tamQuetGiam = transform;

        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(false);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(false);
        if (vfxNapPlasma != null) vfxNapPlasma.SetActive(false);

        viTriYGoc = transform.position.y;

        KichHoatXepHang();
    }

    void Update()
    {
        if ((phechinh != null && phechinh.Dear) || (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)) return;

        // Ưu tiên 1: Đang giẫm đạp thì không làm gì khác
        if (dangGiamDap) return;

        // Ưu tiên 2: Kiểm tra kẻ địch áp sát để giẫm (Xử lý ngay lập tức trong Update để ưu tiên cao nhất)
        if (KiemTraVaKichHoatGiam()) return;

        // Ưu tiên 3: Nếu đang trong trạng thái xả loạt đạn/tên lửa hoặc đang đứng im Idle thì khóa không tìm mục tiêu mới
        if (dangTrongLoat || dangBanTenLua) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuThongMinh();
        else ResetTarget();

        if (mucTieuQuai != null && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            XoayMat(mucTieuQuai.position.x);
        }

        if (DeadIron_animator != null)
        {
            DeadIron_animator.SetBool("DeadIron_isMoving", !dangTrongLoat && !dangBanTenLua && rb.linearVelocity.magnitude > 0.4f);
        }
    }

    void FixedUpdate()
    {
        if (phechinh != null && phechinh.Dear) return;

        // Khóa cứng di chuyển khi giẫm hoặc bắn tên lửa
        if (dangGiamDap || dangBanTenLua || dangTrongLoat)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 vanTocMongMuon = TinhVanTocCanhGocYThongMinh();

        if (mucTieuQuai != null && mucTieuQuai.gameObject.activeInHierarchy && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            float distToTargetX = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            // Nếu lọt vào tầm bắn và hệ thống vũ khí sẵn sàng
            if (distToTargetX <= TamBan)
            {
                vanTocMongMuon.x = 0; // Đứng lại giữ đội hình hàng ngang

                float hienTaiY = DiemBanPlasma != null ? DiemBanPlasma.position.y : transform.position.y;
                float chechLechY = mucTieuQuai.position.y - hienTaiY;

                // Khi đã thẳng hàng trục Y gần như tuyệt đối
                if (Mathf.Abs(chechLechY) <= saiSoCanhY)
                {
                    // Kiểm tra xem đã đến lượt bắn tên lửa chưa
                    if (demSoLanBaoNhieuPlasma >= soLanPlasmaDeBanTenLua)
                    {
                        StartCoroutine(ThucHienLoatTenLua());
                    }
                    // Nếu chưa thì bắn Plasma (nếu hết thời gian hồi chiêu)
                    else if (Time.time >= thoiGianHoiPlasmaTiepTheo)
                    {
                        StartCoroutine(ThucHienLoatPlasma());
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

        // Ngắt các trạng thái tấn công khác ngay lập tức
        dangTrongLoat = false;
        dangBanTenLua = false;

        if (vfxNapPlasma != null) vfxNapPlasma.SetActive(false);
        if (Amthanh != null) Amthanh.Stop();

        if (DeadIron_animator != null)
        {
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
            DeadIron_animator.SetBool("DeadIron_isMissiling", false);
            DeadIron_animator.SetTrigger("DeadIron_StompTrigger");
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

        // Tạo khoảng hoãn nhỏ sau khi giẫm xong
        thoiGianHoiPlasmaTiepTheo = Time.time + 0.5f;
    }

    IEnumerator ThucHienLoatPlasma()
    {
        dangTrongLoat = true;

        // Bật animation Bắn ngay từ lúc nạp năng lượng theo yêu cầu của bạn
        if (DeadIron_animator != null) DeadIron_animator.SetBool("DeadIron_isShooting", true);

        if (vfxNapPlasma != null && DiemBanPlasma != null)
        {
            vfxNapPlasma.transform.SetPositionAndRotation(DiemBanPlasma.position, DiemBanPlasma.rotation);
            vfxNapPlasma.SetActive(true);
        }
        if (Amthanh != null && amThanhNapPlasma != null)
        {
            Amthanh.clip = amThanhNapPlasma;
            Amthanh.loop = true;
            Amthanh.Play();
        }

        yield return new WaitForSeconds(thoiGianNapPlasma);

        if (vfxNapPlasma != null) vfxNapPlasma.SetActive(false);
        if (Amthanh != null) Amthanh.Stop();

        if (mucTieuQuai != null && !KiemTraDichDaChet(mucTieuQuai.gameObject) && !dangGiamDap)
        {
            Vector2 dir = ((Vector2)mucTieuQuai.position - (Vector2)DiemBanPlasma.position).normalized;
            DeadIronBanDanPooling(DiemBanPlasma, prefabDanPlasma, satThuongPlasma, dir);

            if (vfxKhaiHoaPlasma != null && DiemBanPlasma != null)
            {
                Instantiate(vfxKhaiHoaPlasma, DiemBanPlasma.position, DiemBanPlasma.rotation);
            }
            if (Amthanh != null && amThanhBanPlasma != null)
            {
                Amthanh.PlayOneShot(amThanhBanPlasma);
            }
        }

        // Tăng biến đếm số lần bắn Plasma thành công
        demSoLanBaoNhieuPlasma++;

        dangTrongLoat = false;
        thoiGianHoiPlasmaTiepTheo = Time.time + HoiChieuPlasmaThucTe;
        if (DeadIron_animator != null) DeadIron_animator.SetBool("DeadIron_isShooting", false);
    }

    IEnumerator ThucHienLoatTenLua()
    {
        dangBanTenLua = true;

        // Khi bắn tên lửa: Tắt di chuyển, chuyển sang animation Idle rồi mới bắn
        if (DeadIron_animator != null)
        {
            DeadIron_animator.SetBool("DeadIron_isMoving", false);
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
            DeadIron_animator.SetBool("DeadIron_isMissiling", true); // Kích hoạt hiệu ứng/anim bắn tên lửa
        }

        for (int i = 0; i < soTenLuaMoiLoat; i++)
        {
            if (mucTieuQuai == null || KiemTraDichDaChet(mucTieuQuai.gameObject) || dangGiamDap) break;

            if (DiemBanTenLua != null && prefabTenLua != null)
            {
                Transform mucTieuNgauNhien = LayMucTieuNgauNhienTrongTam();
                if (mucTieuNgauNhien != null)
                {
                    StartCoroutine(LogicQuyDaoTenLua(mucTieuNgauNhien));
                }
            }

            if (Amthanh != null && amThanhPhongTenLua != null)
            {
                Amthanh.PlayOneShot(amThanhPhongTenLua);
            }

            yield return new WaitForSeconds(delayGiuaCacQuaTenLua);
        }

        if (DeadIron_animator != null) DeadIron_animator.SetBool("DeadIron_isMissiling", false);

        // ⏳ Trạng thái chờ: Đứng yên Idle một lúc sau khi bắn xong loạt tên lửa
        yield return new WaitForSeconds(thoiGianIdleSauTenLua);

        // Reset lại mục tiêu cũ để bắt đầu thuật toán tìm kiếm mục tiêu mới hoàn toàn
        ResetTarget();

        // Đặt lại biến đếm loạt Plasma về số 0
        demSoLanBaoNhieuPlasma = 0;
        dangBanTenLua = false;
    }

    IEnumerator LogicQuyDaoTenLua(Transform target)
    {
        GameObject tenLua = QuanLyDan.Instance != null ? QuanLyDan.Instance.LayDanTuKho(prefabTenLua) : Instantiate(prefabTenLua);
        if (tenLua == null) yield break;

        float huongMat = transform.localScale.x < 0 ? -1f : 1f;
        Vector2 huongBayMoi = new Vector2(huongMat * 0.4f, 1f).normalized;
        float gocBanDau = Mathf.Atan2(huongBayMoi.y, huongBayMoi.x) * Mathf.Rad2Deg;

        tenLua.transform.SetPositionAndRotation(DiemBanTenLua.position, Quaternion.Euler(0, 0, gocBanDau));
        tenLua.SetActive(true);

        Rigidbody2D rbDan = tenLua.GetComponent<Rigidbody2D>();
        Dannv2 scriptDan = tenLua.GetComponent<Dannv2>();

        float timer = 0f;
        while (timer < thoiGianBayMoi && tenLua.activeInHierarchy)
        {
            if (rbDan != null) rbDan.linearVelocity = huongBayMoi * tocDoBayMoi;
            timer += Time.deltaTime;
            yield return null;
        }

        if (rbDan != null) rbDan.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(thoiGianKhungLai);

        if (tenLua.activeInHierarchy && target != null)
        {
            Vector2 huongLao = ((Vector2)target.position - (Vector2)tenLua.transform.position).normalized;
            float gocLao = Mathf.Atan2(huongLao.y, huongLao.x) * Mathf.Rad2Deg;
            tenLua.transform.rotation = Quaternion.Euler(0, 0, gocLao);

            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuongTenLua;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    Transform LayMucTieuNgauNhienTrongTam()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        List<Transform> danhSachTrongTam = new List<Transform>();

        foreach (var q in mangQuai)
        {
            if (!q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float khoangCach = Vector2.Distance(transform.position, q.transform.position);
            if (khoangCach <= TamBan)
            {
                danhSachTrongTam.Add(q.transform);
            }
        }

        if (danhSachTrongTam.Count > 0)
        {
            int indexNgauNhien = Random.Range(0, danhSachTrongTam.Count);
            return danhSachTrongTam[indexNgauNhien];
        }
        return mucTieuQuai;
    }

    void DeadIronBanDanPooling(Transform diemXuatPhat, GameObject prefabDan, int damage, Vector2 dir)
    {
        if (diemXuatPhat == null || prefabDan == null) return;

        GameObject vienDan = QuanLyDan.Instance != null ? QuanLyDan.Instance.LayDanTuKho(prefabDan) : Instantiate(prefabDan);
        if (vienDan != null)
        {
            float gocXoay = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            vienDan.transform.SetPositionAndRotation(diemXuatPhat.position, Quaternion.Euler(0, 0, gocXoay));
            vienDan.SetActive(true);

            Dannv2 scriptDan = vienDan.GetComponent<Dannv2>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = damage;
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

    Vector2 TinhVanTocCanhGocYThongMinh()
    {
        Vector2 tocDoKetQua = Vector2.zero;

        if (FormationManager.Instance != null)
        {
            tocDoKetQua.x = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan).x;
        }

        if (mucTieuQuai != null && mucTieuQuai.gameObject.activeInHierarchy && !KiemTraDichDaChet(mucTieuQuai.gameObject))
        {
            float hienTaiY = DiemBanPlasma != null ? DiemBanPlasma.position.y : transform.position.y;
            float mucTieuY = mucTieuQuai.position.y;

            float chechLechY = mucTieuY - hienTaiY;

            if (Mathf.Abs(chechLechY) > saiSoCanhY)
            {
                if (chechLechY > 0 && transform.position.y < viTriYGoc + phamViDiChuyenY)
                {
                    tocDoKetQua.y = tocDoHanhQuan;
                }
                else if (chechLechY < 0 && transform.position.y > viTriYGoc - phamViDiChuyenY)
                {
                    tocDoKetQua.y = -tocDoHanhQuan;
                }
            }
            else
            {
                tocDoKetQua.y = 0f;
            }
        }
        else
        {
            float khoangCachVeGoc = viTriYGoc - transform.position.y;
            if (Mathf.Abs(khoangCachVeGoc) > 0.1f)
            {
                tocDoKetQua.y = Mathf.Sign(khoangCachVeGoc) * tocDoHanhQuan;
            }
        }

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
        if (DeadIron_animator)
        {
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
            DeadIron_animator.SetBool("DeadIron_isMissiling", false);
        }
        if (vfxChanKhiGiam != null) vfxChanKhiGiam.SetActive(false);
        if (objectKhiGiam != null) objectKhiGiam.SetActive(false);
        if (vfxNapPlasma != null) vfxNapPlasma.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (tamQuetGiam == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tamQuetGiam.position, banKinhQuetGiam);
    }
}