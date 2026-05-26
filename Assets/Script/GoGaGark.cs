using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    private Transform ThayDich;
    private float HoiChieu = 0f;

    void Start()
    {
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
        // LUÔN LUÔN quét tìm kiếm kẻ địch liên tục trên bản đồ
        TimKiemKeDich();


        // XỬ LÝ CHIẾN ĐẤU VÀ TỰ CĂN LÀN Y KHI THẤY ĐỊCH

        if (ThayDich != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

            // Nếu kẻ địch lọt vào phạm vi kích hoạt (Tầm bắn + 2 ô)
            if (khoangCachX <= (TamBan + 2f))
            {
                // NẾU BỊ LỆCH LÀN Y: Tự động trượt Y bám theo địch luôn (kể cả khi đang hành quân)
                if (doLechYThucTe > DolechHangY)
                {
                    DiChuyenTrungHangY();
                    return; // Ngăn không cho chạy hàm hành quân phía dưới, tập trung bám làn đón đầu
                }

                // NẾU ĐÃ THẲNG LÀN Y & LỌT TẦM BẮN X: Đứng im tấn công!
                if (khoangCachX <= TamBan)
                {
                    Xoaymat(ThayDich.position.x);

                    if (Time.time >= HoiChieu)
                    {
                        TanCong();
                        HoiChieu = Time.time + 1f / soDanBan;
                    }

                    return; // Chặn đứng mọi di chuyển, đứng im xả đạn
                }
            }
        }

        // NẾU KHÔNG CÓ ĐỊCH (HOẶC ĐỊCH Ở QUÁ XA) -> TIẾP TỤC RA BOX THỦ
        if (!daDenViTriThu)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        Xoaymat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.05f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    // Đã chuyển thành public để đảm bảo Update gọi xuống dễ dàng và không trùng phạm vi
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

    void TimKiemKeDich()
    {
        GameObject[] mangDich = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject dichGanNhat = null;

        foreach (GameObject dich in mangDich)
        {
            if (dich.activeInHierarchy)
            {
                float khoangCach = Vector2.Distance(transform.position, dich.transform.position);
                if (khoangCach < khoangCachNganNhat)
                {
                    khoangCachNganNhat = khoangCach ;
                    dichGanNhat = dich;
                }
            }
        }

        if (dichGanNhat != null) ThayDich = dichGanNhat.transform;
        else ThayDich = null;
    }

    void Xoaymat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    void TanCong()
    {
        if (ThayDich == null || DiemBan == null || QuanLyDan.Instance == null || prefabDanNho == null) return;

        float huongBanX = (ThayDich.position.x < transform.position.x) ? 180f : 0f;
        Quaternion rotation = Quaternion.Euler(0, 0, huongBanX);

        GameObject vienDan = QuanLyDan.Instance.LayDanTuKho(prefabDanNho);
        if (vienDan != null)
        {
            vienDan.transform.position = DiemBan.position;
            vienDan.transform.rotation = rotation;
            vienDan.SetActive(true);

            DanNV1 scriptDan = vienDan.GetComponent<DanNV1>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}