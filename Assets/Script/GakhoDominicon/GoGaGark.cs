using UnityEditor.Timeline;
using UnityEngine;

public class NhanVat1Controller : MonoBehaviour
{
    [Header("Chỉ số chiến đấu")]
    public float TamBan = 9f;
    public float soDanBan = 1f;
    public int satThuong = 10;
    public Transform DiemBan;
    public GameObject prefabDanNho;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 5f;
    public float DolechHangY = 0.3f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform ThayDich; // Đây chính là mucTieuQuai của Nhân vật 1
    private float HoiChieu = 0f;
    [SerializeField]
    private int soVienLoatNay = 1;

    private Animator Khogark_animatior;
    private Vector3 viTriKhungHinhTruoc;
    void Start()
    {
        Khogark_animatior = GetComponentInChildren<Animator>();
        if (vungBoxPhongThu != null)
        {
            viTriCoDinh = LayViTriNgauNhienTrongBox(vungBoxPhongThu);
        }
        else
        {
            viTriCoDinh = transform.position;
            daDenViTriThu = true;
        }
    }

    void Update()
    {
        // 1. KIỂM TRA VÀ QUÉT TÌM KIẾM ĐỊCH THÔNG MINH CHỐNG DỒN ĐAM
        if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy)
        {
            ThayDich = null; // Reset nếu quái cũ đã chết hoặc bị ẩn
            TimMucTieuThongMinh();
        }

        bool dangDungBan = false;
        if (Khogark_animatior != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;

            Khogark_animatior.SetBool("isMoving", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // 2. XỬ LÝ CHIẾN ĐẤU VÀ TỰ CĂN LÀN Y KHI THẤY ĐỊCH
        if (ThayDich != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

            // Kẻ địch lọt vào phạm vi kích hoạt bám đuổi (Tầm bắn + 2 ô)
            if (khoangCachX <= (TamBan + 2f))
            {
                dangDungBan = true; // Chặn trạng thái đi lùi về vị trí thủ

                // NẾU BỊ LỆCH LÀN Y: Tự động trượt Y bám theo làn địch trước
                if (doLechYThucTe > DolechHangY)
                {
                    DiChuyenTrungHangY();
                }

                // NẾU ĐÃ THẲNG LÀN Y & LỌT TẦM BẮN X: Đứng im tấn công!
                if (doLechYThucTe <= DolechHangY && khoangCachX <= TamBan)
                {
                    Xoaymat(ThayDich.position.x);

                    // Nếu chưa có luồng bắn nào đang chạy thì mới bắt đầu bắn
                    if (HoiChieu <= Time.time)
                    {
                        StartCoroutine(TanCong());
                        HoiChieu = Time.time + (1f / soDanBan); // Giữ mốc thời gian chặn Update gọi trùng
                    }
                }
            }
        }

        // 3. NẾU KHÔNG CÓ ĐỊCH (HOẶC ĐỊCH Ở QUÁ XA) -> TIẾP TỤC RA BOX THỦ
        if (!daDenViTriThu && !dangDungBan)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        Xoaymat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);


        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    public void DiChuyenTrungHangY()
    {
        if (ThayDich == null) return;

        Vector3 viTriMucTieu = new Vector3(transform.position.x, ThayDich.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    void TimMucTieuThongMinh()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject quaiUuTien = null;
        GameObject quaiDuPhong = null;
        float kcDuPhongNganNhat = Mathf.Infinity;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float kc = Vector2.Distance(transform.position, quai.transform.position);

                // Tự động kiểm tra và thêm component đánh dấu nếu quái chưa có
                KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>();
                if (marker == null) marker = quai.AddComponent<KhoaMucTieu>();

                // Ưu tiên 1: Gần nhất và CHƯA bị ai khóa
                if (!marker.daBiKhoaMucTieu)
                {
                    if (kc < khoangCachNganNhat)
                    {
                        khoangCachNganNhat = kc;
                        quaiUuTien = quai;
                    }
                }
                // Dự phòng: Gần nhất nhưng đã bị khóa
                else
                {
                    if (kc < kcDuPhongNganNhat)
                    {
                        kcDuPhongNganNhat = kc;
                        quaiDuPhong = quai;
                    }
                }
            }
        }
        XacDinhVaKhoaMucTieu(quaiUuTien, quaiDuPhong);
    }

    // HÀM XỬ LÝ GÁN KHÓA VÀ ĐỒNG BỘ VÀO BIẾN ThayDich
    void XacDinhVaKhoaMucTieu(GameObject quaiUuTien, GameObject quaiDuPhong)
    {
        if (quaiUuTien != null)
        {
            ThayDich = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
        {
            ThayDich = quaiDuPhong.transform; // Chấp nhận bắn chung nếu map hết quái tự do
        }
        else
        {
            ThayDich = null;
        }
    }

    void Xoaymat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // Sử dụng IEnumerator để tự quản lý thời gian diễn hoạt ảnh, an toàn 100%
    private System.Collections.IEnumerator TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) yield break;

        // 1. BẬT TRẠNG THÁI BẮN (Nhân vật sẽ giữ nguyên tư thế giương súng, không bị giật hoạt ảnh)
        Khogark_animatior.SetBool("isAttacking", true);

        // Giả sử mỗi lần kích hoạt, con lính sẽ bắn liên tiếp 3 viên đạn

        for (int i = 0; i < soVienLoatNay; i++)
        {
            // Kiểm tra bảo hiểm nếu đang bắn mà quái bị chết thì dừng loạt đạn
            if (ThayDich == null || !ThayDich.gameObject.activeInHierarchy) break;

            // Tự động xoay mặt theo địch trước mỗi viên đạn
            Xoaymat(ThayDich.position.x);

            // LOGIC TẠO ĐẠN GỐC CỦA BẠN (Giữ nguyên hoàn toàn)
            float huongBanX = (ThayDich.position.x < transform.position.x) ? 180f : 0f;
            Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

            GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
            if (vienDan != null)
            {
                vienDan.transform.position = DiemBan.position;
                vienDan.transform.rotation = rotation;
                vienDan.transform.SetParent(null);
                vienDan.SetActive(true);

                DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
                if (scriptDan != null)
                {
                    scriptDan.satThuong = satThuong;
                    scriptDan.KichHoatVienDan();
                }
            }

            // KHOẢNG CÁCH GIỮA CÁC VIÊN ĐẠN TRONG 1 LOẠT: Cách nhau 0.1 giây rất nhanh và mượt
            yield return new WaitForSeconds(0.1f);
        }

        // 2. TẮT TRẠNG THÁI BẮN (Bắn xong cả loạt thì cho nhân vật hạ súng xuống)
        Khogark_animatior.SetBool("isAttacking", false);

        // 3. THỜI GIAN HỒI CHIÊU GIỮA CÁC LOẠT ĐẠN
        yield return new WaitForSeconds(1f / soDanBan);
    }

    // GIẢI PHÓNG QUÁI KHI NHÂN VẬT 1 BỊ CHẾT HOẶC THU HỒI
    private void OnDisable()
    {
        if (ThayDich != null)
        {
            KhoaMucTieu marker = ThayDich.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}