using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class NhanVat1 : MonoBehaviour
{
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu;
    public LoaiLinh loaiHinhDonVi = LoaiLinh.KhoGrak;

    [Header("--- Chỉ Số Chiến Đấu ---")]
    public float TamBan = 15f;
    public float soDanBan = 1f;
    public int satThuong = 10;
    public int soVienLoatNay = 1;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung;
    private AudioSource AmthanhLinh;

    [Header("--- Di Chuyển & Giãn Cách ---")]
    public float tocDoHanhQuan = 3f;
    public float banKinhGianCach = 0.7f;
    public float lucDayGianCach = 2.0f;
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f;

    public BoxCollider2D vungBoxPhongThu;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform ThayDich;
    private float HoiChieu = 0f;
    private bool isShooting = false;
    private bool _registered = false;

    private readonly Collider2D[] _buffer = new Collider2D[64];
    private int updateGroup;
    private static int groupCounter = 0;
    public Health_phechinh heal;

    private Vector2 lucGianCachHienTai;

    void Awake()
    {
        heal = GetComponent<Health_phechinh>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        AmthanhLinh = GetComponent<AudioSource>();
        AmthanhLinh.playOnAwake = false;
        AmthanhLinh.loop = false;

        updateGroup = groupCounter % 5;
        groupCounter++;
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        int rank = GetRank();
        if (rank >= 0 && FormationManager.Instance != null)
        {
            _registered = FormationManager.Instance.Register(gameObject, rank);
        }
    }

    void Update()
    {
        if (heal.Dear) return;

        // ĐÃ SỬA LỖI TẠI ĐÂY: Xoá dấu cách bị gõ nhầm để gọi đúng tên hàm gốc của bạn
        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuNgauNhienTrongTam();
        else ResetTarget();

        if (anim != null)
        {
            if (isShooting)
            {
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
        if (heal.Dear) return;

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
        if (anim != null) anim.SetBool("Khogark_isShooting", true);

        if (AmthanhLinh != null && TiengSung != null)
        {
            AmthanhLinh.PlayOneShot(TiengSung);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < soVienLoatNay; i++)
        {
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            if (DiemBan != null)
            {
                Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;
                float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

                if (prefabDanNho != null)
                {
                    if (QuanLyDan.Instance != null)
                    {
                        GameObject dan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
                        if (dan != null)
                        {
                            dan.transform.position = DiemBan.position;
                            dan.transform.rotation = Quaternion.Euler(0, 0, gocXoay);
                            dan.SetActive(true);
                        }
                    }
                    else
                    {
                        Instantiate(prefabDanNho, DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
                    }
                }
            }

            if (soVienLoatNay > 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (anim != null) anim.SetBool("Khogark_isShooting", false);

        HoiChieu = Time.time + (1f / soDanBan);
        isShooting = false;
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

    void TimMucTieuNgauNhienTrongTam()
    {
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
        }

        GameObject[] tatCaKeThu = GameObject.FindGameObjectsWithTag("Enemy");
        List<Transform> danhSachKẻThùHợpLệ = new List<Transform>();

        foreach (var q in tatCaKeThu)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);
            if (kc <= TamBan)
            {
                danhSachKẻThùHợpLệ.Add(q.transform);
            }
        }

        if (danhSachKẻThùHợpLệ.Count > 0)
        {
            int indexNgauNhien = Random.Range(0, danhSachKẻThùHợpLệ.Count);
            ThayDich = danhSachKẻThùHợpLệ[indexNgauNhien];
        }
        else
        {
            ThayDich = null;
        }
    }

    private bool KiemTraDichDaChet(GameObject keThich)
    {
        Health_chaos mauQuai = keThich.GetComponent<Health_chaos>();
        if (mauQuai != null) return mauQuai.Deadre;
        if (keThich.CompareTag("Untagged")) return true;
        return false;
    }

    void ResetTarget() { ThayDich = null; }

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
        if (_registered && FormationManager.Instance != null)
            FormationManager.Instance.Unregister(gameObject);

        if (anim != null)
        {
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", false);
        }
    }
}