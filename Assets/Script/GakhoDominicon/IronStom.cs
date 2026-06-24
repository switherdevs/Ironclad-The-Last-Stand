using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Đảm bảo GameObject luôn có Rigidbody2D và AudioSource đi kèm để tránh lỗi thiếu Component
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class NhanVat2 : MonoBehaviour
{
    // Khai báo các trạng thái chiến thuật của binh lính
    public enum LenhChienThuat { PhongThu, TanCong, RutLui }
    // Khai báo các loại đơn vị quân để phân cấp Rank trong đội hình
    public enum LoaiLinh { Titan, KhoGrak, IronStorm, Terminator, DeadIron, Servitor }

    [Header("--- Cấu Hình Trạng Thái ---")]
    public LenhChienThuat lenhHienTai = LenhChienThuat.PhongThu; // Lệnh mặc định ban đầu
    public LoaiLinh loaiHinhDonVi = LoaiLinh.IronStorm; // Loại lính mặc định là IronStorm

    [Header("--- Chỉ Số Chiến Đấu ---")]
    public float TamBan = 20f; // Khoảng cách tối đa có thể bắn kẻ địch
    [Tooltip("Số đợt bắn (loạt) trong 1 giây. Càng cao bắn càng nhanh.")]
    public float soDanBan = 1.2f; // Tốc độ hồi giữa các loạt bắn
    public int satThuong = 15; // Lượng máu kẻ địch bị trừ trên mỗi viên đạn
    [Tooltip("Số viên đạn bắn ra trong một loạt. Tốc độ giãn cách giữa các viên tự động điều chỉnh theo soDanBan.")]
    public int soVienLoatNay = 1; // Số lượng đạn bắn ra mỗi đợt
    public Transform DiemBan; // Vị trí đầu nòng súng để đạn bay ra
    public GameObject prefabDanNho; // File thiết kế viên đạn (Prefab)

    [Header("--- Hiệu Ứng Khai Hỏa (MỚI) ---")]
    [Tooltip("Kéo Prefab hiệu ứng tia lửa nòng/khói súng vào đây.")]
    public GameObject prefabHieuUngBan; // Hiệu ứng lóe sáng ở nòng súng khi bắn

    [Header("--- Âm Thanh ---")]
    [SerializeField] private AudioClip TiengSung; // File âm thanh tiếng súng .mp3 hoặc .wav
    private AudioSource AmthanhLinh; // Linh hồn phát âm thanh của Object này

    [Header("--- Di Chuyển & Giãn Cách ---")]
    public float tocDoHanhQuan = 3.5f; // Tốc độ di chuyển tối đa
    public float banKinhGianCach = 0.6f; // Khoảng cách an toàn giữa các đồng đội
    public float lucDayGianCach = 2.0f; // Lực đẩy ra khi lính đứng quá sát nhau
    [Range(0.05f, 0.5f)] public float doMuotDiChuyen = 0.15f; // Hệ số Lerp làm mượt vận tốc

    [Header("--- Vùng Box Phòng Thủ ---")]
    public BoxCollider2D vungBoxPhongThu; // Collider xác định khu vực cố thủ

    // Các biến nội bộ dùng để quản lý logic ngầm
    private Rigidbody2D rb; // Linh hồn xử lý vật lý di chuyển 2D
    private Animator anim; // Biến điều khiển các hiệu ứng hoạt họa hoạt hình (Run, Shoot)
    private Health_phechinh phechinh; // Thành phần quản lý máu của bản thân nhân vật này
    private Transform ThayDich; // Lưu tọa độ vị trí của mục tiêu đang bị nhắm bắn
    private float HoiChieu = 0f; // Mốc thời gian được phép bắn phát tiếp theo
    private bool isShooting = false; // Đánh dấu xem nhân vật có đang trong trạng thái xả đạn không

    private int _rank = -1; // Cấp bậc hàng ngũ trong đội hình
    private bool _registered = false; // Kiểm tra xem đã đăng ký vào FormationManager chưa
    private readonly Collider2D[] _buffer = new Collider2D[64]; // Mảng đệm chứa các Collider quét được (tránh rác bộ nhớ)
    private int updateGroup; // ID nhóm tối ưu hóa hiệu năng vật lý
    private static int groupCounter = 0; // Biến đếm tĩnh để tăng dần ID nhóm cho các lính sinh sau

    private Vector2 lucGianCachHienTai; // Lưu lực đẩy tách biệt tính được ở khung hình hiện tại
    public Health_phechinh heal; // Biến phụ để kiểm tra máu (bạn chạy Awake gán trùng với phechinh)

    void Awake()
    {
        // Khởi tạo và liên kết các thành phần vật lý, âm thanh ngay khi Object vừa sinh ra
        heal = GetComponent<Health_phechinh>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Khóa trọng lực bằng 0 vì đây là game góc nhìn Top-down hoặc lính di chuyển tự do 2D mặt phẳng
        rb.linearDamping = 2f; // Tạo độ ma sát cản lực giúp lính dừng lại không bị trượt patin
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Làm mượt vị trí giữa các khung hình vật lý
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Khóa không cho nhân vật bị xoay tròn lật ngửa khi va chạm
        phechinh = GetComponent<Health_phechinh>();

        AmthanhLinh = GetComponent<AudioSource>();
        AmthanhLinh.playOnAwake = false; // Không tự động phát nhạc khi vừa vào game
        AmthanhLinh.loop = false; // Không lặp đi lặp lại tiếng súng liên tục
        anim = GetComponentInChildren<Animator>(); // Tìm Animator ở con của Object này (thường Sprite nằm ở Object con)

        // Phân chia nhóm: lính thứ 1 nhóm 0, lính thứ 2 nhóm 1... lính thứ 6 quay lại nhóm 0.
        updateGroup = groupCounter % 5;
        groupCounter++;
    }

    void Start()
    {
        _rank = GetRank(); // Lấy số thứ tự ưu tiên dựa vào enum loại lính
        // Nếu có hệ thống quản lý đội hình, tiến hành xếp hàng vào vị trí
        if (_rank >= 0 && FormationManager.Instance != null)
            _registered = FormationManager.Instance.Register(gameObject, _rank);
    }

    void Update()
    {
        // Nếu bản thân đã chết thì dừng mọi xử lý logic tìm địch và hoạt họa
        if (heal.Dear || (phechinh != null && phechinh.Dear)) return;

        // Nếu không phải lệnh rút lui thì liên tục quét tìm mục tiêu ngầu nhiên trong tầm bắn
        if (lenhHienTai != LenhChienThuat.RutLui) TimMucTieuNgauNhienTrongTam();
        else ResetTarget(); // Nếu rút lui thì bỏ mục tiêu

        // Xử lý bật tắt chuyển động của Animator hoạt họa
        if (anim != null)
        {
            if (isShooting)
            {
                anim.SetBool("Khogark_isMoving", false); // Đang bắn thì không chạy hoạt họa di chuyển
                float thoiGianMotVongMoPhong = 1f / soDanBan;
                // Tự động tăng tốc độ phát của Animation súng nếu tốc độ bắn được nâng cấp cao
                anim.speed = Mathf.Clamp(1.2f / thoiGianMotVongMoPhong, 1f, 5f);
            }
            else
            {
                anim.speed = 1f; // Trả về tốc độ hoạt họa bình thường
                // Nếu vận tốc thực tế lớn hơn 0.6f thì bật hoạt họa chạy, ngược lại đứng im
                bool isMoving = rb.linearVelocity.magnitude > 0.6f;
                anim.SetBool("Khogark_isMoving", isMoving);
            }
        }
    }

    void FixedUpdate()
    {
        if (phechinh != null && phechinh.Dear) return;

        // Tối ưu hiệu năng: 5 khung hình mới tính toán lực giãn cách đồng đội 1 lần
        if (Time.frameCount % 5 == updateGroup)
        {
            lucGianCachHienTai = TinhLucGianCach();
        }

        Vector2 vanTocMongMuon = Vector2.zero;

        // Nếu có mục tiêu hợp lệ và mục tiêu đó chưa chết
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            float distToTarget = Vector2.Distance(transform.position, ThayDich.position);
            // Xoay mặt lính nhìn về phía kẻ địch (Trái/Phải)
            XoayMatTheoHuong(ThayDich.position.x - transform.position.x);

            if (distToTarget > TamBan)
            {
                // Nếu đứng xa quá tầm bắn: Tính hướng tiến về phía địch kết hợp lực giãn cách nhẹ để không đè lên nhau
                Vector2 huongDi = ((Vector2)ThayDich.position - (Vector2)transform.position).normalized;
                vanTocMongMuon = (huongDi * tocDoHanhQuan) + (lucGianCachHienTai * 0.4f);
            }
            else
            {
                // Đã vào tầm bắn: Đứng im tại chỗ
                vanTocMongMuon = Vector2.zero;

                // Nếu đã hết thời gian hồi chiêu và chưa ở trong Coroutine bắn thì xả đạn
                if (HoiChieu <= Time.time && !isShooting)
                {
                    StartCoroutine(CoroutineBanLoat());
                }
            }
        }
        else
        {
            // Nếu không có địch: Di chuyển hành quân theo vị trí được chỉ định từ FormationManager
            vanTocMongMuon = TinhVanTocDoiHinh(lucGianCachHienTai);
        }

        // Áp dụng vận tốc vào Rigidbody thông qua hàm Lerp giúp lính tăng/giảm tốc mượt mà, không bị khựng giật
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, vanTocMongMuon, doMuotDiChuyen);
    }

    Vector2 TinhVanTocDoiHinh(Vector2 lucGianCach)
    {
        if (FormationManager.Instance == null) return lucGianCach;

        // Lấy vận tốc cần thiết để đuổi kịp vị trí hàng ngũ được giao
        Vector2 slotVelocity = FormationManager.Instance.GetSlotVelocity(gameObject, tocDoHanhQuan);

        // Tự động xoay mặt lính ra phía trước (phải) khi tiến công/phòng thủ, hoặc xoay về sau (trái) khi rút lui
        if (lenhHienTai == LenhChienThuat.PhongThu || lenhHienTai == LenhChienThuat.TanCong)
        {
            XoayMatTheoHuong(1f);
        }
        else if (lenhHienTai == LenhChienThuat.RutLui)
        {
            XoayMatTheoHuong(-1f);
        }

        // Nếu đã đứng gần như chuẩn vị trí đội hình (vận tốc slot nhỏ) thì giảm lực giãn cách để tránh lính bị rung lắc
        if (slotVelocity.magnitude < 0.5f)
        {
            return slotVelocity + (lucGianCach * 0.3f);
        }

        return slotVelocity + lucGianCach;
    }

    Vector2 TinhLucGianCach()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Unit")); // Chỉ quét những vật thể thuộc Layer "Unit" (đồng minh)
        // Tạo một vòng tròn ảo xung quanh lính để dò tìm va chạm, lưu kết quả vào mảng _buffer
        int count = Physics2D.OverlapCircle(transform.position, banKinhGianCach, filter, _buffer);

        Vector2 tong = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            if (_buffer[i].gameObject == gameObject) continue; // Bỏ qua chính bản thân mình
            if (_buffer[i].name.Contains("servitor")) continue; // Bỏ qua lính thợ Servitor (không đẩy lính thợ)

            // Tính toán hướng đẩy ra xa khỏi đồng minh đó
            Vector2 huong = (Vector2)transform.position - (Vector2)_buffer[i].transform.position;
            float kc = huong.magnitude;

            if (kc < banKinhGianCach && kc > 0.01f)
            {
                // Tỷ lệ khoảng cách: Càng sát nhau thì lực đẩy `tile` càng tiến gần về 1 (đẩy rất mạnh)
                float tile = (banKinhGianCach - kc) / banKinhGianCach;
                tong += huong.normalized * tile * lucDayGianCach;
            }
        }
        // Giới hạn lực đẩy tối đa để lính không bị bắn bay vèo vèo ra ngoài bản đồ do dồn lực quá lớn
        return Vector2.ClampMagnitude(tong, lucDayGianCach * 1.5f);
    }

    // Coroutine xử lý chu kỳ xả đạn
    private IEnumerator CoroutineBanLoat()
    {
        isShooting = true;

        if (anim != null)
        {
            anim.SetBool("Khogark_isMoving", false);
            anim.SetBool("Khogark_isShooting", true); // Bật hiệu ứng hoạt họa súng giật/khai hỏa
        }

        // Điều chỉnh độ trầm bổng (Pitch) của tiếng súng dựa trên tốc độ bắn (bắn nhanh nghe tiếng súng dồn dập thanh hơn)
        if (AmthanhLinh != null)
        {
            if (soDanBan > 3.0f)
                AmthanhLinh.pitch = Mathf.Clamp(soDanBan / 3.0f, 1.0f, 1.8f);
            else
                AmthanhLinh.pitch = 1.0f;
        }

        // Tự động tính toán khoảng thời gian chờ giữa mỗi viên đạn trong loạt dựa theo tốc độ soDanBan
        float thoiGianGiuaMoiVien = (1f / soDanBan) / (soVienLoatNay + 1f);
        thoiGianGiuaMoiVien = Mathf.Clamp(thoiGianGiuaMoiVien, 0.02f, 0.15f);

        for (int i = 0; i < soVienLoatNay; i++)
        {
            // Nếu địch đột ngột chết giữa loạt bắn thì dừng vòng lặp ngay lập tức, không bắn vào không khí
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy || KiemTraDichDaChet(ThayDich.gameObject)) break;

            if (AmthanhLinh != null && TiengSung != null)
            {
                AmthanhLinh.PlayOneShot(TiengSung); // Phát âm thanh tiếng súng một lần duy nhất
            }

            if (DiemBan != null)
            {
                // Tính toán hướng góc từ nòng súng thẳng tới tâm kẻ địch theo hệ số góc 360 độ
                Vector2 huongCoDinh = ((Vector2)ThayDich.position - (Vector2)DiemBan.position).normalized;
                float gocXoay = Mathf.Atan2(huongCoDinh.y, huongCoDinh.x) * Mathf.Rad2Deg;

                // Tạo hiệu ứng lửa khói nòng súng (Muzzle Flash)
                if (prefabHieuUngBan != null)
                {
                    if (QuanLyDan.Instance != null) // Ưu tiên sử dụng Object Pooling (QuanLyDan) để tránh lag game
                    {
                        GameObject hieuUng = QuanLyDan.Instance.LayDanTuKho(prefabHieuUngBan);
                        if (hieuUng != null)
                        {
                            hieuUng.transform.position = DiemBan.position;
                            hieuUng.transform.rotation = Quaternion.Euler(0, 0, gocXoay);
                            hieuUng.SetActive(true);
                        }
                    }
                    else // Nếu không có kho lưu trữ Pooling thì tạo mới thông thường (tốn tài nguyên hơn)
                    {
                        Instantiate(prefabHieuUngBan, DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
                    }
                }

                // Tạo thực thể đạn thực tế để bay đi gây sát thương
                if (prefabDanNho != null)
                {
                    GameObject dan = null;
                    if (QuanLyDan.Instance != null)
                    {
                        dan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
                        if (dan != null)
                        {
                            dan.transform.position = DiemBan.position;
                            dan.transform.rotation = Quaternion.Euler(0, 0, gocXoay);
                            dan.SetActive(true);
                        }
                    }
                    else
                    {
                        dan = Instantiate(prefabDanNho, DiemBan.position, Quaternion.Euler(0, 0, gocXoay));
                    }

                    if (dan != null)
                    {
                        // Truy cập vào script viên đạn giao sát thương của nhân vật và kích hoạt nó bay đi
                        DanNV1 script = dan.GetComponent<DanNV1>();
                        if (script != null) { script.satThuong = satThuong; script.KichHoatVienDan(); }
                    }
                }
            }
            yield return new WaitForSeconds(thoiGianGiuaMoiVien); // Chờ một khoảng thời gian nhỏ rồi mới bắn viên tiếp theo
        }

        // Tạo độ trễ ngắn để hoạt họa bắn kịp hoàn thành mượt mà trước khi tắt hẳn
        float delayEndAnimation = Mathf.Clamp(0.15f / (soDanBan / 1.2f), 0.03f, 0.15f);
        yield return new WaitForSeconds(delayEndAnimation);

        if (anim != null) anim.SetBool("Khogark_isShooting", false); // Tắt trạng thái hoạt họa bắn

        HoiChieu = Time.time + (1f / soDanBan); // Thiết lập mốc thời gian hồi chiêu tiếp theo
        isShooting = false;
    }

    void TimMucTieuNgauNhienTrongTam()
    {
        // Nếu đã có mục tiêu cũ, và mục tiêu đó vẫn sống + nằm gọn trong tầm bắn thì tiếp tục bắn con đó, không đổi mục tiêu
        if (ThayDich != null && ThayDich.gameObject.activeInHierarchy && !KiemTraDichDaChet(ThayDich.gameObject))
        {
            if (Vector2.Distance(transform.position, ThayDich.position) <= TamBan) return;
        }

        // Ngược lại, tìm kiếm danh sách tất cả các Object có Tag là "Enemy"
        GameObject[] tatCaKeThu = GameObject.FindGameObjectsWithTag("Enemy");
        List<Transform> danhSachKẻThùHợpLệ = new List<Transform>();

        foreach (var q in tatCaKeThu)
        {
            if (q == null || !q.activeInHierarchy || KiemTraDichDaChet(q)) continue;

            float kc = Vector2.Distance(transform.position, q.transform.position);
            if (kc <= TamBan)
            {
                danhSachKẻThùHợpLệ.Add(q.transform); // Thêm những con quái đang đứng trong tầm bắn vào danh sách chờ
            }
        }

        if (danhSachKẻThùHợpLệ.Count > 0)
        {
            // Chọn ngẫu nhiên hoàn toàn 1 mục tiêu bất kỳ từ danh sách hợp lệ để tạo tính hỗn loạn chiến trường
            int indexNgauNhien = Random.Range(0, danhSachKẻThùHợpLệ.Count);
            ThayDich = danhSachKẻThùHợpLệ[indexNgauNhien];
        }
        else
        {
            ThayDich = null; // Không có ai xung quanh thì hủy mục tiêu
        }
    }

    private bool KiemTraDichDaChet(GameObject keThich)
    {
        Health_chaos mauQuai = keThich.GetComponent<Health_chaos>();
        if (mauQuai != null) return mauQuai.Deadre; // Trả về true nếu quái đã chết (Deadre == true)
        if (keThich.CompareTag("Untagged")) return true; // Nếu quái mất Tag chứng tỏ nó đang trong trạng thái phân hủy/bị hủy
        return false;
    }

    void ResetTarget()
    {
        if (ThayDich != null)
        {
            // Trả tự do/Mở khóa cho mục tiêu nếu có hệ thống khóa mục tiêu
            var km = ThayDich.GetComponent<KhoaMucTieu>();
            if (km != null) km.daBiKhoaMucTieu = false;
        }
        ThayDich = null;
    }

    public void XoayMatTheoHuong(float huongX)
    {
        if (Mathf.Abs(huongX) < 0.05f) return; // Nếu độ lệch quá nhỏ thì không cần xoay làm gì

        float dir = Mathf.Sign(huongX); // Lấy dấu âm (-) hoặc dương (+) biểu thị bên Trái hay Phải
        float heSoDaoSprite = 1f;
        float ketQuaScaleX = dir * heSoDaoSprite * Mathf.Abs(transform.localScale.x);

        // Đảo ngược trục X của localScale để lật ngược hình ảnh nhân vật (Flip Sprite) sang hướng di chuyển tương ứng
        if (Mathf.Sign(transform.localScale.x) != Mathf.Sign(ketQuaScaleX))
        {
            transform.localScale = new Vector3(ketQuaScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    // Biến enum thành chỉ số nguyên tương ứng để gửi vào FormationManager phân cấp hàng trước/hàng sau
    int GetRank() => loaiHinhDonVi switch
    {
        LoaiLinh.KhoGrak => 0,   // Hàng tiên phong chặn quái
        LoaiLinh.IronStorm => 1, // Hàng 2 xả đạn tầm trung
        LoaiLinh.Terminator => 2,
        LoaiLinh.DeadIron => 3,
        LoaiLinh.Titan => 4,     // Siêu cơ giáp khổng lồ bọc hậu phía sau cùng
        _ => -1
    };

    void OnDisable()
    {
        // Khi lính bị ẩn đi hoặc chết, tự động giải phóng mục tiêu và hủy đăng ký khỏi đội hình để tránh tràn bộ nhớ
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