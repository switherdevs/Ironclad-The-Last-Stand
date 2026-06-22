using UnityEngine;

public class Dead_iron_new : MonoBehaviour
{
    [Header("Chỉ số chiến đấu (Pháo Hạng Nặng)")]
    public float TamBan = 12f;
    public int satThuong = 150;
    public Transform DiemBan;
    public GameObject prefabDanMobi;

    [Header("Hiệu ứng khi bắn")]
    public GameObject hieuUngKhiBan; // HIỆU ỨNG MỚI: Tự động ẩn/hiện độc lập khi bắn đạn

    [Header("Hiệu ứng nạp năng lượng")]
    public GameObject hieuUngNapNangLuong;
    public float thoiGianNapNangLuong = 5f;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 4f;
    public float doLechHangY = 0.2f;

    [Header("Đồng bộ Xếp Hàng (Rank 3 cho DeadIron)")]
    public int capBacRank = 3;
    public float tocDoHanhQuan = 3f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f;
    private bool dangTrongLuongBan = false;
    public Health_phechinh phechinh;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator DeadIron_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────
    // Am Thanh
    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Shoot;

    private bool daDangKyFormation = false;

    void Start()
    {
        Amthanh = GetComponent<AudioSource>();
        phechinh = GetComponent<Health_phechinh>();
        DeadIron_animator = GetComponentInChildren<Animator>();

        // Đăng ký vào hệ thống quản lý hàng ngũ ngay khi vừa xuất hiện
        KichHoatXepHang();

        // Tối ưu tắt mặc định ban đầu cho cả 2 loại hiệu ứng độc lập
        if (hieuUngNapNangLuong != null) hieuUngNapNangLuong.SetActive(false);
        if (hieuUngKhiBan != null) hieuUngKhiBan.SetActive(false);
    }

    void Update()
    {
        if (phechinh != null && phechinh.Dear) return;

        // Cập nhật liên tục vị trí ô xếp hàng được phân công thay thế cho BoxCollider
        CapNhatViTriSlotTuFormation();

        if (mucTieuQuai == null || !mucTieuQuai.gameObject.activeInHierarchy)
        {
            mucTieuQuai = null;
            if (!dangTrongLuongBan)
            {
                if (DeadIron_animator != null)
                    DeadIron_animator.SetBool("DeadIron_isShooting", false);
            }
            TimMucTieuThongMinh();
        }

        // ── [ANIMATION LOGIC SỬA ĐỔI]: Ưu tiên hoạt ảnh Bắn tuyệt đối ──
        if (DeadIron_animator != null)
        {
            if (dangTrongLuongBan)
            {
                // Nếu đang trong luồng nạp năng lượng hoặc đang bắn, ép trạng thái di chuyển về false 
                // để hoạt ảnh di chuyển không thể nhảy vào đè lên hoạt ảnh bắn của xương được.
                DeadIron_animator.SetBool("DeadIron_isMoving", false);
            }
            else
            {
                bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
                DeadIron_animator.SetBool("DeadIron_isMoving", dangDiChuyen);
            }
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            // Giữ nguyên tầm kích hoạt của bạn (Tầm bắn + 5f)
            if (khoangCachXThucTe <= (TamBan + 5f))
            {
                dangDungBan = true;

                // Đồng bộ bám trục Y của quái vật cực mượt
                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                }

                if (doLechYThucTe <= doLechHangY && khoangCachXThucTe <= TamBan)
                {
                    XoayMat(mucTieuQuai.position.x);

                    if (Time.time >= thoiGianBanTiepTheo && !dangTrongLuongBan)
                    {
                        StartCoroutine(LuongBanNapNangLuong());
                    }
                }
            }
        }

        // 🎯 ĐÃ SỬA: Khi hết kẻ thù (dangDungBan = false) hoặc kẻ thù nằm ngoài tầm ngắm, quay về hàng ngay lập tức
        if (!daDenViTriThu && !dangDungBan && !dangTrongLuongBan)
        {
            HanhQuanVaoViTri();
        }
    }

    private System.Collections.IEnumerator LuongBanNapNangLuong()
    {
        dangTrongLuongBan = true;

        if (hieuUngNapNangLuong != null)
            hieuUngNapNangLuong.SetActive(true);

        if (DeadIron_animator != null)
            DeadIron_animator.SetBool("DeadIron_isShooting", true);

        yield return new WaitForSeconds(2);
        if (Amthanh != null && Shoot != null) Amthanh.PlayOneShot(Shoot);

        yield return new WaitForSeconds(thoiGianNapNangLuong);

        // Kích hoạt bắn đạn và xử lý hiệu ứng bắn đi kèm
        if (mucTieuQuai != null)
        {
            BanThienThachPooling();

            // Kích hoạt hiệu ứng bắn (Muzzle flash/khói lửa súng)
            if (hieuUngKhiBan != null) hieuUngKhiBan.SetActive(true);
        }

        if (hieuUngNapNangLuong != null)
            hieuUngNapNangLuong.SetActive(false);

        // Chờ thêm một nhịp ngắn 0.5s để hiệu ứng súng kịp hiển thị rồi tắt đi cùng hoạt ảnh bắn
        yield return new WaitForSeconds(0.5f);

        if (hieuUngKhiBan != null)
            hieuUngKhiBan.SetActive(false);

        if (DeadIron_animator != null)
            DeadIron_animator.SetBool("DeadIron_isShooting", false);

        thoiGianBanTiepTheo = Time.time + 0.5f;
        dangTrongLuongBan = false;
    }

    void HanhQuanVaoViTri()
    {
        XoayMat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.2f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null || dangTrongLuongBan) return;
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    // ── ĐỒNG BỘ MA TRẬN XẾP HÀNG ───────────────────────────────────────────
    private void KichHoatXepHang()
    {
        if (FormationManager.Instance != null && !daDangKyFormation)
        {
            daDangKyFormation = FormationManager.Instance.Register(gameObject, capBacRank);
        }
    }

    private void CapNhatViTriSlotTuFormation()
    {
        if (FormationManager.Instance != null && daDangKyFormation)
        {
            Vector2 viTriSlot;
            if (FormationManager.Instance.TryGetSlot(gameObject, out viTriSlot))
            {
                viTriCoDinh = new Vector3(viTriSlot.x, viTriSlot.y, transform.position.z);

                // Nếu vị trí đứng bị thay đổi (do hàng trước dồn lên hoặc có thêm lính tràn hàng)
                if (Vector3.Distance(transform.position, viTriCoDinh) > 0.3f)
                {
                    daDenViTriThu = false;
                }
            }
        }
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
                KhoaMucTieu marker = quai.GetComponent<KhoaMucTieu>();
                if (marker == null) marker = quai.AddComponent<KhoaMucTieu>();

                if (!marker.daBiKhoaMucTieu)
                {
                    if (kc < khoangCachNganNhat)
                    {
                        khoangCachNganNhat = kc;
                        quaiUuTien = quai;
                    }
                }
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

    void XacDinhVaKhoaMucTieu(GameObject quaiUuTien, GameObject quaiDuPhong)
    {
        if (quaiUuTien != null)
        {
            mucTieuQuai = quaiUuTien.transform;
            KhoaMucTieu marker = quaiUuTien.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = true;
        }
        else if (quaiDuPhong != null)
        {
            mucTieuQuai = quaiDuPhong.transform;
        }
        else
        {
            mucTieuQuai = null;
        }
    }

    void BanThienThachPooling()
    {
        if (DiemBan == null || prefabDanMobi == null) return;

        float huongBanX = (mucTieuQuai.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = null;
        if (QuanLyDan.Instance != null)
        {
            vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanMobi);
        }

        if (vienDan == null)
        {
            vienDan = Instantiate(prefabDanMobi, DiemBan.position, rotation);
        }
        else
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);
        }

        if (vienDan != null)
        {
            DanNv4 scriptDan = vienDan.GetComponent<DanNv4>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    void XoayMat(float xMucTieu)
    {
        if (dangTrongLuongBan) return;
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void OnDisable()
    {
        if (mucTieuQuai != null)
        {
            KhoaMucTieu marker = mucTieuQuai.GetComponent<KhoaMucTieu>();
            if (marker != null) marker.daBiKhoaMucTieu = false;
        }

        // Giải phóng Slot đứng để lính dự bị phía sau dồn hàng lên thay thế
        if (FormationManager.Instance != null && daDangKyFormation)
        {
            FormationManager.Instance.Unregister(gameObject);
            daDangKyFormation = false;
        }

        if (DeadIron_animator != null)
        {
            DeadIron_animator.SetBool("DeadIron_isMoving", false);
            DeadIron_animator.SetBool("DeadIron_isShooting", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}