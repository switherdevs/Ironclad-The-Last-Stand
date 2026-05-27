using System.Collections; // THÊM MỚI: Thư viện bắt buộc để dùng IEnumerator
using UnityEngine;

public class Terminator : MonoBehaviour
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

    // --- ĐOẠN SỬA ĐỔI: THÊM BIẾN MỚI QUẢN LÝ TRẠNG THÁI NGỪNG BẮN ---
    private bool dangTrongThoiGianNghi = false; // Biến kiểm tra xem nhân vật có đang trong 3 giây dừng bắn hay không
    // -------------------------------------------------------------

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
        TimKiemKeDich();

        if (ThayDich != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - ThayDich.position.y);
            float khoangCachX = Mathf.Abs(transform.position.x - ThayDich.position.x);

            if (khoangCachX <= (TamBan + 2f))
            {
                if (doLechYThucTe > DolechHangY)
                {
                    DiChuyenTrungHangY();
                    return;
                }

                if (khoangCachX <= TamBan)
                {
                    Xoaymat(ThayDich.position.x);

                    // --- ĐOẠN SỬA ĐỔI: LOGIC BẮN LIÊN TỤC VÀ GỌI COROUTINE NGỪNG BẮN ---
                    if (!dangTrongThoiGianNghi)
                    {
                        if (Time.time >= HoiChieu)
                        {
                            TanCong();
                            HoiChieu = Time.time + 1f / soDanBan;

                            // Sau khi bắn xong một viên, kích hoạt chu kỳ kiểm tra ngừng bắn 3 giây
                            StartCoroutine(ChuKyNguongBan());
                        }
                    }
                    // -----------------------------------------------------------------

                    return;
                }
            }
        }

        if (!daDenViTriThu)
        {
            HanhQuanVaoViTri();
        }
    }

    // --- ĐOẠN SỬA ĐỔI: THÊM HÀM IEMUNERATOR XỬ LÝ NGỪNG BẮN 3 GIÂY ---
    IEnumerator ChuKyNguongBan()
    {
        dangTrongThoiGianNghi = true; // Khóa không cho bắn tiếp
        yield return new WaitForSeconds(3f); // Đợi đúng 3 giây
        dangTrongThoiGianNghi = false; // Mở khóa để tiếp tục loạt bắn mới
    }
    // -----------------------------------------------------------------

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
                    khoangCachNganNhat = khoangCach;
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