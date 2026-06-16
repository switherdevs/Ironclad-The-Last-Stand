using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("--- Di Chuyển & Giãn Cách ---")]
    public float tocDoHanhQuan = 3f;
    public float banKinhGianCach = 0.7f;
    public float lucDayGianCach = 2.0f;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 10f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

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
        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieu();
        else ResetTarget();

        if (anim != null)
        {
            bool isMoving = rb.linearVelocity.magnitude > 0.6f;
            anim.SetBool("Khogark_isMoving", isMoving);
        }
    }

    void FixedUpdate()
    {
        if (Time.frameCount % 5 != updateGroup) return;

        Vector2 lucGianCach = TinhLucGianCach();

        if (ThayDich != null)
        {
            float distToTarget = Vector2.Distance(transform.position, ThayDich.position);

            // LUÔN LUÔN XOAY MẶT VỀ PHÍA ĐỊCH KHI ĐÃ XÁC ĐỊNH ĐƯỢC MỤC TIÊU
            Xoaymat(ThayDich.position.x);

            if (distToTarget > TamBan)
            {
                // Trường hợp 1: Địch ở xa -> Di chuyển tiếp cận
                Vector2 huongDi = ((Vector2)ThayDich.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = (huongDi * tocDoHanhQuan) + lucGianCach;
            }
            else
            {
                // Trường hợp 2: Địch đã vào tầm bắn -> Đứng im bám trụ + Bắn liên tục
                rb.linearVelocity = lucGianCach;

                // Kiểm tra hồi chiêu để thực hiện bắn
                if (HoiChieu <= Time.time && !isShooting)
                {
                    StartCoroutine(CoroutineBanLoat());
                }
            }
        }
        else
        {
            // Không có địch -> Di chuyển theo logic đội hình/phòng thủ mặc định
            DiChuyen(lucGianCach);
        }
    }
    void DiChuyen(Vector2 lucGianCach)
    {
        if (FormationManager.Instance == null)
        {
            rb.linearVelocity = lucGianCach;
            return;
        }

        // Lấy vận tốc để lính di chuyển về đúng slot trong đội hình
        Vector2 slotVelocity = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan);

        // Cộng vận tốc di chuyển vào đội hình + lực giãn cách để tránh chồng lính
        rb.linearVelocity = slotVelocity + lucGianCach;

        // Xoay mặt về phía hướng di chuyển nếu cần
        if (slotVelocity.magnitude > 0.1f)
        {
            Xoaymat(transform.position.x + slotVelocity.x);
        }
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
            Vector2 huong = (Vector2)transform.position - (Vector2)_buffer[i].transform.position;
            float kc = huong.magnitude;
            if (kc < banKinhGianCach && kc > 0.01f)
                tong += huong.normalized * ((banKinhGianCach - kc) / banKinhGianCach) * lucDayGianCach;
        }
        return Vector2.ClampMagnitude(tong, lucDayGianCach * 2f);
    }

    private IEnumerator CoroutineBanLoat()
    {
        isShooting = true;
        if (anim != null) anim.SetBool("Khogark_isShooting", true);

        // Chờ đến thời điểm phát hỏa trong Animation (Ví dụ: 0.3 giây)
        yield return new WaitForSeconds(0.3f);

        // Đảm bảo có đủ điều kiện: nòng súng, mục tiêu, và prefab loại đạn cụ thể của lính này
        if (prefabDanNho != null && DiemBan != null && ThayDich != null)
        {
            // 1. Tính toán hướng cố định từ Điểm Bắn đến Địch ngay tại khung hình này
            Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;

            // 2. Tính góc xoay độ (Z-axis) để Sprite viên đạn hướng đầu chéo lên/xuống chuẩn xác
            float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

            if (QuanLyDan.Instance != null)
            {
                // 3. Gọi đạn từ kho Pooling và TRUYỀN prefabDanNho vào để đối chiếu đúng loại đạn
                GameObject dan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);

                if (dan != null)
                {
                    // 4. Đặt vị trí xuất phát tại nòng súng
                    dan.transform.position = DiemBan.position;

                    // 5. Ép góc xoay chéo hướng về phía mục tiêu
                    dan.transform.rotation = Quaternion.Euler(0, 0, gocXoay);

                    // 6. Kích hoạt viên đạn (Khi OnEnable chạy, nó tự bay thẳng bằng Translate Space.Self theo góc này)
                    dan.SetActive(true);
                }
            }
            else
            {
                // Phương án dự phòng nếu bạn quên đặt QuanLyDan trong Scene (Dùng Instantiate thông thường)
                Instantiate(prefabDanNho, DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
            }
        }

        if (anim != null) anim.SetBool("Khogark_isShooting", false);
        HoiChieu = Time.time + (1f / soDanBan);
        isShooting = false;
    }

    public void Xoaymat(float xMucTieu)
    {
        float dir = Mathf.Sign(xMucTieu - transform.position.x);
        if (Mathf.Abs(transform.localScale.x - dir) > 0.1f)
            transform.localScale = new Vector3(dir * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void TimMucTieu()
    {
        GameObject best = null;
        float bestDist = TamBan;
        foreach (var q in EnemyManager.Instance.danhSachDich)
        {
            if (q == null || !q.activeInHierarchy) continue;
            float kc = Vector2.Distance(transform.position, q.transform.position);
            if (kc < bestDist) { bestDist = kc; best = q; }
        }
        ThayDich = (best != null) ? best.transform : null;
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

    void OnDisable()
    {
        if (_registered && FormationManager.Instance != null)
            FormationManager.Instance.Unregister(gameObject);
    }
}
