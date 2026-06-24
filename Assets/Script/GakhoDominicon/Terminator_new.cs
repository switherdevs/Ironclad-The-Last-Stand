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
    public float TamBan = 9f; // Tầm bắn ngắn hơn IronStorm (vì giáp nặng, mang vũ khí cự ly trung bình-ngắn)
    public float soDanBan = 1f; // Tốc độ đợt hồi bắn
    public int satThuong = 10; // Sát thương mỗi viên đạn
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("--- Cấu Hình Loạt Đạn (ĐẶC TRƯNG) ---")]
    public int soLuongDanTrongLoat = 5; // Đặc trưng: Mỗi đợt bắn xả liên thanh liên tiếp 5 viên đạn
    public float thoiGianCachNhauGiuaCacVien = 0.2f; // Độ trễ giữa từng phát bắn nhỏ bên trong loạt

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung;
    private AudioSource AmthanhTer;

    [Header("--- Di Chuyển & Giãn Cách (ĐỒNG BỘ NV1) ---")]
    public float tocDoHanhQuan = 3f; // Tốc độ di chuyển chậm hơn một chút do mặc giáp Terminator nặng nề
    public float banKinhGianCach = 0.7f; // Cần khoảng cách đứng rộng hơn vì kích thước to lớn
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
        if (heal.Dear) return;
        if (phechinh != null && phechinh.Dear) return;
        // Nếu hệ thống quản lý trò chơi báo kết thúc (GameOver) thì đứng hình, không chạy logic nữa
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieu();
        else ResetTarget();

        // Đồng bộ hiệu ứng hoạt họa dựa theo độ lớn vận tốc thực tế giống hệt như thiết kế của Nhân vật 1
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

        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            float distToTarget = Vector2.Distance(transform.position, ThayDich.position);
            XoayMatTheoHuong(ThayDich.position.x - transform.position.x);

            if (distToTarget > TamBan)
            {
                // Đi tự do áp sát mục tiêu theo mọi hướng chứ không bị gò bó làn đường cố định
                Vector2 huongDi = ((Vector2)ThayDich.position - (Vector2)transform.position).normalized;
                vanTocMongMuon = (huongDi * tocDoHanhQuan) + (lucGianCachHienTai * 0.4f);
            }
            else
            {
                // Khi lính bọc giáp nặng đã vào tầm bắn thì chỉ nhận lực giãn cách để giữ cự ly đứng, dừng hẳn tiến để bắn
                vanTocMongMuon = lucGianCachHienTai;
                if (HoiChieu <= Time.time && !isShooting)
                {
                    StartCoroutine(ChuKyXaDanVaNguongBan());
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

        // Giữ vị trí trong hàng ngũ theo dữ liệu cung cấp từ FormationManager
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

    // Coroutine xả nguyên 1 tràng/loạt đạn liên thanh đặc trưng của Terminator
    IEnumerator ChuKyXaDanVaNguongBan()
    {
        isShooting = true;

        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", true); // Kích hoạt hiệu ứng bắn liên thanh

        if (AmthanhTer != null && TiengSung != null)
        {
            AmthanhTer.PlayOneShot(TiengSung);
        }

        // Vòng lặp bắn ra 5 viên liên tục (`soLuongDanTrongLoat = 5`)
        for (int i = 0; i < soLuongDanTrongLoat; i++)
        {
            // Điểm mấu chốt: Nếu mục tiêu bị hạ gục trước khi lính bắn xong viên thứ 5, hàm sẽ ngắt dòng ngay lập tức!
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            TanCong(); // Gọi hàm sinh đạn bay đi gây sát thương
            yield return new WaitForSeconds(thoiGianCachNhauGiuaCacVien); // Chờ 0.2 giây rồi bắn viên tiếp theo
        }

        if (Terminator_animator != null)
            Terminator_animator.SetBool("Terminator_isShooting", false);

        HoiChieu = Time.time + (1f / soDanBan); // Thiết lập hồi chiêu cho toàn bộ cả loạt bắn lớn tiếp theo
        isShooting = false;
    }

    void TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) return;

        // Độc lập hướng bắn 360 độ: Đạn tự bay hướng xéo chéo thẳng vào địch chứ không bị bay ngang đơ
        Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;
        float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

        // Lấy đạn từ kho Pooling tối ưu tài nguyên ram
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

    // Thuật toán tìm mục tiêu thông minh: Ưu tiên diệt trừ chỉ huy địch (Boss) trước
    void TimMucTieu()
    {
        // 1. Nếu đang có mục tiêu cũ, kiểm tra xem nó có phải Boss không. Nếu đúng và Boss vẫn còn sống thì tuyệt đối bám sát mục tiêu này
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (ThayDich.gameObject.name.Contains("Chao_boss") || ThayDich.GetComponent<BossController>() != null)
            {
                if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
            }
        }

        // 2. Tìm tất cả quái vật mang Tag "Enemy" trong màn chơi
        GameObject[] tatCaKeThu = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject bestQuaiNho = null;
        GameObject bestBoss = null;

        // Gán mốc khoảng cách tối đa ban đầu bằng tầm bắn để lọc những kẻ đứng xa quá
        float bestDistQuaiNho = TamBan;
        float bestDistBoss = TamBan;

        foreach (var q in tatCaKeThu)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);

            if (kc <= TamBan)
            {
                // Kiểm tra tên xem có chứa từ khóa Boss hoặc có đính kèm linh hồn BossController hay không
                if (q.name.Contains("Chao_boss") || q.GetComponent<BossController>() != null)
                {
                    if (kc < bestDistBoss)
                    {
                        bestDistBoss = kc;
                        bestBoss = q; // Ghi nhận con Boss đang ở khoảng cách gần nhất
                    }
                }
                else // Ngược lại nếu chỉ là lâu la quái nhỏ thông thường
                {
                    if (kc < bestDistQuaiNho)
                    {
                        bestDistQuaiNho = kc;
                        bestQuaiNho = q; // Ghi nhận con quái nhỏ gần mình nhất
                    }
                }
            }
        }

        // 3. Phân cấp quyết định nhắm bắn: Nếu có Boss xuất hiện thì dồn lực bắn Boss trước!
        if (bestBoss != null)
        {
            ThayDich = bestBoss.transform;
        }
        else if (bestQuaiNho != null) // Nếu khu vực xung quanh không có Boss nào mới chuyển qua dọn quái cỏ nhỏ
        {
            ThayDich = bestQuaiNho.transform;
        }
        else
        {
            ThayDich = null;
        }
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
        LoaiLinh.KhoGrak => 0,
        LoaiLinh.IronStorm => 1,
        LoaiLinh.Terminator => 2, // Terminator sẽ đứng hàng số 3, nhận nhiệm vụ đỡ đòn cho hàng sau và bắn quét Boss
        LoaiLinh.DeadIron => 3,
        LoaiLinh.Titan => 4,
        _ => -1
    };

    // Hàm vẽ viền trợ năng: Hiển thị một vòng tròn màu xanh lá bao quanh Terminator trong cửa sổ Scene của Unity để bạn dễ dàng căn chỉnh chỉnh sửa độ xa của TamBan trực quan
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