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
    [Tooltip("Số đợt bắn (loạt) trong 1 giây. Càng cao bắn càng nhanh.")]
    public float soDanBan = 1.2f;
    public int satThuong = 15;
    [Tooltip("Số viên đạn bắn ra trong một loạt. Tốc độ giãn cách giữa các viên tự động điều chỉnh theo soDanBan.")]
    public int soVienLoatNay = 1;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung;
    private AudioSource AmthanhLinh;

    [Header("--- Di Chuyển & Giãn Cách ---")]
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
    public Health_phechinh heal;

    void Awake()
    {
        heal = GetComponent<Health_phechinh>();
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
        if (heal.Dear || (phechinh != null && phechinh.Dear)) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuNgauNhien();
        else ResetTarget();

        if (anim != null)
        {
            if (isShooting)
            {
                anim.SetBool("Khogark_isMoving", false);

                // 🔥 TỐI ƯU 1: Đồng bộ tốc độ Animation với Tốc độ bắn thực tế
                // Tốc độ bắn gốc thiết kế tầm 1.2 -> Tính tỉ lệ tăng tốc tương ứng
                float thoiGianMotVongMoPhong = 1f / soDanBan;
                anim.speed = Mathf.Clamp(1.2f / thoiGianMotVongMoPhong, 1f, 5f);
            }
            else
            {
                anim.speed = 1f; // Trở về tốc độ bình thường khi di chuyển/idle
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
                vanTocMongMuon = Vector2.zero; // Ưu tiên đứng yên bắn địch

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

        if (anim != null)
        {
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", true);
        }

        // 🔥 TỐI ƯU 2: Tăng tốc độ Pitch âm thanh dựa trên tốc độ bắn cao, giúp âm thanh dứt khoát không bị rè chồng lấp
        if (AmthanhLinh != null)
        {
            if (soDanBan > 3.0f)
                AmthanhLinh.pitch = Mathf.Clamp(soDanBan / 3.0f, 1.0f, 1.8f);
            else
                AmthanhLinh.pitch = 1.0f;
        }

        // 🔥 TỐI ƯU 3: Tính toán khoa học thời gian giãn cách giữa các viên dựa vào tốc độ bắn thực tế
        // Nếu soVienLoatNay > 1, thời gian nghỉ giữa mỗi viên sẽ thu ngắn lại để vừa khít tổng thời gian nạp chiêu
        float thoiGianGiuaMoiVien = (1f / soDanBan) / (soVienLoatNay + 1f);
        thoiGianGiuaMoiVien = Mathf.Clamp(thoiGianGiuaMoiVien, 0.02f, 0.15f); // Ngăn lỗi số quá nhỏ làm treo luồng

        for (int i = 0; i < soVienLoatNay; i++)
        {
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            if (AmthanhLinh != null && TiengSung != null)
            {
                AmthanhLinh.PlayOneShot(TiengSung);
            }

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
            yield return new WaitForSeconds(thoiGianGiuaMoiVien);
        }

        // Thời gian chờ trễ nhỏ để trả Animator về trạng thái nghỉ một cách mượt mà
        float delayEndAnimation = Mathf.Clamp(0.15f / (soDanBan / 1.2f), 0.03f, 0.15f);
        yield return new WaitForSeconds(delayEndAnimation);

        if (anim != null) anim.SetBool("Khogark_isShooting", false);

        // Đặt hồi chiêu chuẩn xác theo công thức 1 giây / số viên loạt bắn
        HoiChieu = Time.time + (1f / soDanBan);
        isShooting = false;
    }

    void TimMucTieuNgauNhien()
    {
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (ThayDich.gameObject.name.Contains("Chao_boss") || ThayDich.GetComponent<BossController>() != null)
            {
                if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
            }
        }

        GameObject[] tatCaKeThu = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestQuaiNho = null;
        GameObject bestBoss = null;
        float bestDistQuaiNho = TamBan;
        float bestDistBoss = TamBan;

        foreach (var q in tatCaKeThu)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);

            if (kc <= TamBan)
            {
                if (q.name.Contains("Chao_boss") || q.GetComponent<BossController>() != null)
                {
                    if (kc < bestDistBoss)
                    {
                        bestDistBoss = kc;
                        bestBoss = q;
                    }
                }
                else
                {
                    if (kc < bestDistQuaiNho)
                    {
                        bestDistQuaiNho = kc;
                        bestQuaiNho = q;
                    }
                }
            }
        }

        if (bestBoss != null) ThayDich = bestBoss.transform;
        else if (bestQuaiNho != null) ThayDich = bestQuaiNho.transform;
        else ThayDich = null;
    }

    private bool KiemTraDichDaChet(GameObject keThich)
    {
        Health_chaos mauQuai = keThich.GetComponent<Health_chaos>();
        if (mauQuai != null) return mauQuai.Deadre;
        if (keThich.CompareTag("Untagged")) return true;
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

    int GetRank() => loaiHinhDonVi switch
    {
        LoaiLinh.KhoGrak => 0,
        LoaiLinh.IronStorm => 1,
        LoaiLinh.Terminator => 2,
        LoaiLinh.DeadIron => 3,
        LoaiLinh.Titan => 4,
        _ => -1
    };

    void OnDisable()
    {
        ResetTarget();
        if (_registered && FormationManager.Instance != null) FormationManager.Instance.Unregister(gameObject);

        if (anim != null)
        {
            anim.speed = 1f;
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", false);
        }
    }
}