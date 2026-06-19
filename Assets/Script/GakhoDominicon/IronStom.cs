using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class NhanVat2 : MonoBehaviour
{
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu;
    public LoaiLinh loaiHinhDonVi = LoaiLinh.IronStorm;

    [Header("--- Chỉ Số Chiến Đấu ---")]
    public float TamBan = 20f;
    public float soDanBan = 1.2f;
    public int satThuong = 15;
    public int soVienLoatNay = 1;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung;
    private AudioSource AmthanhLinh;

    [Header("--- Di Chuyển & Giãn Cách (GIỐNG NV1) ---")]
    public float tocDoHanhQuan = 3.5f;
    public float banKinhGianCach = 0.6f;
    public float lucDayGianCach = 2.0f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;

    [Header("--- Vùng Box Phòng Thủ ---")]
    public BoxCollider2D vungBoxPhongThu;

    private Rigidbody2D rb;
    private Animator anim;
    private Health_phechinh phechinh;
    private Transform ThayDich;
    private float HoiChieu = 0f;
    private bool isShooting = false;

    private int _rank = -1;
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

        AmthanhLinh = GetComponent<AudioSource>();
        AmthanhLinh.playOnAwake = false;
        AmthanhLinh.loop = false;
        anim = GetComponentInChildren<Animator>();

        updateGroup = groupCounter % 5;
        groupCounter++;
    }

    void Start()
    {
        _rank = GetRank();
        if (_rank >= 0 && FormationManager.Instance != null)
            _registered = FormationManager.Instance.Register(gameObject, _rank);
    }

    void Update()
    {
        if (phechinh != null && phechinh.Dear) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuNgauNhien();
        else ResetTarget();

        // 🎯 ĐÃ SỬA: Cập nhật thông số Animator chống xung đột trạng thái
        if (anim != null)
        {
            if (isShooting)
            {
                // Khi đang bắn, bắt buộc tắt trạng thái di chuyển để tránh bị đè animation
                anim.SetBool("Khogark_isMoving", false);
            }
            else
            {
                bool isMoving = rb.linearVelocity.magnitude > 0.6f;
                anim.SetBool("Khogark_isMoving", isMoving);
            }
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

        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            float distToTarget = Vector2.Distance(transform.position, ThayDich.position);

            XoayMatTheoHuong(ThayDich.position.x - transform.position.x);

            if (distToTarget > TamBan)
            {
                Vector2 huongDi = ((Vector2)ThayDich.position - (Vector2)transform.position).normalized;
                vanTocMongMuon = (huongDi * tocDoHanhQuan) + (lucGianCachHienTai * 0.4f);
            }
            else
            {
                // Khi lọt vào tầm bắn, triệt tiêu vận tốc mong muốn để đứng yên bắn cho chuẩn
                vanTocMongMuon = lucGianCachHienTai * 0.1f;

                if (HoiChieu <= Time.time && !isShooting)
                {
                    StartCoroutine(CoroutineBanLoat());
                }
            }
        }
        else
        {
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

    private IEnumerator CoroutineBanLoat()
    {
        isShooting = true;

        // Đảm bảo cập nhật ngay lập tức các parameter của animator tại đây
        if (anim != null)
        {
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", true);
        }

        if (AmthanhLinh != null && TiengSung != null)
        {
            AmthanhLinh.PlayOneShot(TiengSung);
        }

        for (int i = 0; i < soVienLoatNay; i++)
        {
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            if (prefabDanNho != null && DiemBan != null)
            {
                Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;
                float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

                if (QuanLyDan.Instance != null)
                {
                    GameObject dan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
                    if (dan != null)
                    {
                        dan.transform.position = DiemBan.position;
                        dan.transform.rotation = Quaternion.Euler(0, 0, gocXoay);
                        dan.SetActive(true);

                        DanNV1 script = dan.GetComponent<DanNV1>();
                        if (script != null) { script.satThuong = satThuong; script.KichHoatVienDan(); }
                    }
                }
                else
                {
                    GameObject danXoay = Instantiate(prefabDanNho, DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
                    DanNV1 script = danXoay.GetComponent<DanNV1>();
                    if (script != null) { script.satThuong = satThuong; script.KichHoatVienDan(); }
                }
            }

            // Thời gian chờ giãn cách giữa từng viên trong loạt bắn
            yield return new WaitForSeconds(0.1f);
        }

        // Bắn xong một loạt, chờ thêm một chút (nếu cần) trước khi tắt hẳn trạng thái bắn để animation kịp diễn ra
        yield return new WaitForSeconds(0.15f);

        if (anim != null) anim.SetBool("Khogark_isShooting", false);
        HoiChieu = Time.time + (1f / soDanBan);
        isShooting = false;
    }

    void TimMucTieuNgauNhien()
    {
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
        }

        if (EnemyManager.Instance == null || EnemyManager.Instance.danhSachDich == null) return;

        List<GameObject> danhSachTrongTam = new List<GameObject>();
        foreach (var q in EnemyManager.Instance.danhSachDich)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);
            if (kc <= TamBan) danhSachTrongTam.Add(q);
        }

        if (danhSachTrongTam.Count > 0)
        {
            ThayDich = danhSachTrongTam[Random.Range(0, danhSachTrongTam.Count)].transform;
        }
        else ThayDich = null;
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

    void ResetTarget()
    {
        if (ThayDich != null)
        {
            var km = ThayDich.GetComponent<KhoaMucTieu>();
            if (km != null) km.daBiKhoaMucTieu = false;
        }
        ThayDich = null;
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

    int GetRank() => loaiHinhDonVi switch { LoaiLinh.Titan => 0, LoaiLinh.KhoGrak => 1, LoaiLinh.IronStorm => 2, LoaiLinh.Terminator => 3, LoaiLinh.DeadIron => 4, _ => -1 };

    void OnDisable()
    {
        ResetTarget();
        if (_registered && FormationManager.Instance != null) FormationManager.Instance.Unregister(gameObject);

        if (anim != null)
        {
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", false);
        }
    }
}