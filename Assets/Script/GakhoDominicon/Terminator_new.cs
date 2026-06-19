using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class Terminator_new : MonoBehaviour
{
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu;
    public LoaiLinh loaiHinhDonVi = LoaiLinh.Terminator;

    [Header("--- Chỉ Số Chiến Đấu ---")]
    public float TamBan = 9f;
    public float soDanBan = 1f;
    public int satThuong = 10;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("--- Cấu Hình Loạt Đạn (ĐẶC TRƯNG) ---")]
    public int soLuongDanTrongLoat = 5;
    public float thoiGianCachNhauGiuaCacVien = 0.2f;

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung;
    private AudioSource AmthanhTer;

    [Header("--- Di Chuyển & Giãn Cách (ĐỒNG BỘ NV1) ---")]
    public float tocDoHanhQuan = 3f;
    public float banKinhGianCach = 0.7f;
    public float lucDayGianCach = 2.0f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;

    [Header("--- Vùng Box Phòng Thủ ---")]
    public BoxCollider2D vungBoxPhongThu;

    private Rigidbody2D rb;
    private Animator Terminator_animator;
    private Health_phechinh phechinh;
    private Transform ThayDich;
    private float HoiChieu = 0f;
    private bool isShooting = false;
    private bool _registered = false;

    private readonly Collider2D[] _buffer = new Collider2D[64];
    private int updateGroup;
    private static int groupCounter = 0;

    private Vector2 lucGianCachHienTai;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        phechinh = GetComponent<Health_phechinh>();
        AmthanhTer = GetComponent<AudioSource>();
        AmthanhTer.playOnAwake = false;
        AmthanhTer.loop = false;

        updateGroup = groupCounter % 5;
        groupCounter++;
    }

    void Start()
    {
        Terminator_animator = GetComponentInChildren<Animator>();

        int rank = GetRank();
        if (rank >= 0 && FormationManager.Instance != null)
        {
            _registered = FormationManager.Instance.Register(gameObject, rank);
        }
    }

    void Update()
    {
        if (phechinh != null && phechinh.Dear) return;
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieu();
        else ResetTarget();

        // ĐỒNG BỘ ANIMATION DI CHUYỂN: Dựa trên vận tốc thực tế của Rigidbody giống NV1
        if (Terminator_animator != null)
        {
            bool isMoving = rb.linearVelocity.magnitude > 0.6f;
            Terminator_animator.SetBool("Terminator_isMoving", isMoving);
        }
    }

    void FixedUpdate()
    {
        if (phechinh != null && phechinh.Dear) return;

        if (Time.frameCount % 5 == updateGroup)
        {
            lucGianCachHienTai = TinhLucGianCach();
        }

        Vector2 vanTocMongMuon = Vector2.zero;

        // 🎯 ĐÃ BỎ LÀN Y: Di chuyển tự do áp sát hoặc giữ khoảng cách giống NV1
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            float distToTarget = Vector2.Distance(transform.position, ThayDich.position);

            XoayMatTheoHuong(ThayDich.position.x - transform.position.x);

            if (distToTarget > TamBan)
            {
                // Ngoài tầm bắn: Tiến thẳng về phía địch
                Vector2 huongDi = ((Vector2)ThayDich.position - (Vector2)transform.position).normalized;
                vanTocMongMuon = (huongDi * tocDoHanhQuan) + (lucGianCachHienTai * 0.4f);
            }
            else
            {
                // Đã vào tầm bắn: Đứng im giữ vị trí và kích hoạt xả đạn
                vanTocMongMuon = lucGianCachHienTai;
                if (HoiChieu <= Time.time && !isShooting)
                {
                    StartCoroutine(ChuKyXaDanVaNguongBan());
                }
            }
        }
        else
        {
            // Không có địch: Đi theo hàng ngũ của FormationManager
            vanTocMongMuon = TinhVanTocDoiHinh(lucGianCachHienTai);
        }

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, vanTocMongMuon, doMuotDiChuyen);
    }

    Vector2 TinhVanTocDoiHinh(Vector2 lucGianCach)
    {
        if (FormationManager.Instance == null) return lucGianCach;

        Vector2 slotVelocity = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan);

        if (lenhHienTai == LenhChienThuat.PhongThu || lenhHienTai == LenhChienThuat.TanCong)
        {
            XoayMatTheoHuong(1f);
        }
        else if (lenhHienTai == LenhChienThuat.RutLui)
        {
            XoayMatTheoHuong(-1f);
        }

        if (slotVelocity.magnitude < 0.5f)
        {
            return slotVelocity + (lucGianCach * 0.3f);
        }

        return slotVelocity + lucGianCach;
    }

    Vector2 TinhLucGianCach()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Unit"));
        int count = Physics2D.OverlapCircle(transform.position, banKinhGianCach, filter, _buffer);

        Vector2 tong = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            if (_buffer[i].gameObject == gameObject) continue;
            if (_buffer[i].name.Contains("servitor")) continue;

            Vector2 huong = (Vector2)transform.position - (Vector2)_buffer[i].transform.position;
            float kc = huong.magnitude;

            if (kc < banKinhGianCach && kc > 0.01f)
            {
                float tile = (banKinhGianCach - kc) / banKinhGianCach;
                tong += huong.normalized * tile * lucDayGianCach;
            }
        }
        return Vector2.ClampMagnitude(tong, lucDayGianCach * 1.5f);
    }

    // 🎯 GIỮ NGUYÊN CƠ CHẾ BẮN LOẠT ĐẶC TRƯNG + FIX LỖI XÁC CHẾT
    IEnumerator ChuKyXaDanVaNguongBan()
    {
        isShooting = true;

        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", true);

        if (AmthanhTer != null && TiengSung != null)
        {
            AmthanhTer.PlayOneShot(TiengSung);
        }

        for (int i = 0; i < soLuongDanTrongLoat; i++)
        {
            // Kiểm tra quái chết giữa loạt bắn để dừng ngay lập tức
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            TanCong();
            yield return new WaitForSeconds(thoiGianCachNhauGiuaCacVien);
        }

        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", false);

        // Hồi chiêu tổng của cả loạt đạn
        HoiChieu = Time.time + (1f / soDanBan);
        isShooting = false;
    }

    void TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) return;

        // Tính góc xoay thực tế hướng trực diện vào mục tiêu 360 độ giống NV1 và NV2
        Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;
        float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = Quaternion.Euler(0, 0, gocXoay);
            vienDan.SetActive(true);

            DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    void TimMucTieu()
    {
        // Ưu tiên giữ mục tiêu cũ nếu còn sống và trong tầm bắn
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
        }

        GameObject best = null;
        float bestDist = TamBan;
        foreach (var q in EnemyManager.Instance.danhSachDich)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);
            if (kc < bestDist) { bestDist = kc; best = q; }
        }
        ThayDich = (best != null) ? best.transform : null;
    }

    private bool KiemTraDichDaChet(GameObject keThich)
    {
        Health_chaos mauQuai = keThich.GetComponent<Health_chaos>();
        if (mauQuai != null)
        {
            return mauQuai.Deadre;
        }
        if (keThich.CompareTag("Untagged"))
        {
            return true;
        }
        return false;
    }

    public void XoayMatTheoHuong(float huongX)
    {
        if (Mathf.Abs(huongX) < 0.05f) return;

        float dir = Mathf.Sign(huongX);
        float heSoDaoSprite = 1f;
        float ketQuaScaleX = dir * heSoDaoSprite * Mathf.Abs(transform.localScale.x);

        if (Mathf.Sign(transform.localScale.x) != Mathf.Sign(ketQuaScaleX))
        {
            transform.localScale = new Vector3(ketQuaScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    void ResetTarget() { ThayDich = null; }

    int GetRank() => loaiHinhDonVi switch
    {
        LoaiLinh.Titan => 0,
        LoaiLinh.KhoGrak => 1,
        LoaiLinh.IronStorm => 2,
        LoaiLinh.Terminator => 3,
        LoaiLinh.DeadIron => 4,
        _ => -1
    };

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }

    private void OnDisable()
    {
        if (_registered && FormationManager.Instance != null)
            FormationManager.Instance.Unregister(gameObject);

        isShooting = false;
        if (Terminator_animator != null)
        {
            Terminator_animator.SetBool("Terminator_isMoving", false);
            Terminator_animator.SetBool("Terminator_isShooting", false);
        }
    }
}