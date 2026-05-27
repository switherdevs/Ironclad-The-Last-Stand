using UnityEngine;

public class NhanVat4 : MonoBehaviour
{
    [Header("Chỉ số chiến đấu (Pháo Hạng Nặng)")]
    public float TamBan = 12f;
    public int satThuong = 150;
    public float tocDoBan = 0.4f;
    public Transform DiemBan;
    public GameObject prefabDanMobi;

    [Header("Chỉ số di chuyển bám làn")]
    public float tocDoDiChuyenY = 3f;
    public float doLechHangY = 0.3f;

    [Header("Vùng Box Phòng Thủ")]
    public BoxCollider2D vungBoxPhongThu;
    public float tocDoHanhQuan = 2f;

    private Vector3 viTriCoDinh;
    private bool daDenViTriThu = false;
    private Transform mucTieuQuai;
    private float thoiGianBanTiepTheo = 0f;

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
        TimQuaiGanNhat();

        bool dangDungBan = false;

        if (mucTieuQuai != null)
        {
            float doLechYThucTe = Mathf.Abs(transform.position.y - mucTieuQuai.position.y);
            float khoangCachXThucTe = Mathf.Abs(transform.position.x - mucTieuQuai.position.x);

            if (khoangCachXThucTe <= (TamBan + 2f))
            {
                if (doLechYThucTe > doLechHangY)
                {
                    DiChuyenTrungHangY();
                }

                if (khoangCachXThucTe <= TamBan)
                {
                    XoMat(mucTieuQuai.position.x);
                    dangDungBan = true;

                    if (Time.time >= thoiGianBanTiepTheo)
                    {
                        BanThienThachPooling();
                        thoiGianBanTiepTheo = Time.time + (1f / tocDoBan);
                    }
                }
            }
        }

        if (!daDenViTriThu && !dangDungBan)
        {
            HanhQuanVaoViTri();
        }
    }

    void HanhQuanVaoViTri()
    {
        XoMat(viTriCoDinh.x);
        transform.position = Vector3.MoveTowards(transform.position, viTriCoDinh, tocDoHanhQuan * Time.deltaTime);

        if (Vector3.Distance(transform.position, viTriCoDinh) < 0.05f)
        {
            transform.position = viTriCoDinh;
            daDenViTriThu = true;
        }
    }

    void DiChuyenTrungHangY()
    {
        if (mucTieuQuai == null) return;
        Vector3 viTriMucTieu = new Vector3(transform.position.x, mucTieuQuai.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, viTriMucTieu, tocDoDiChuyenY * Time.deltaTime);
    }

    void TimQuaiGanNhat()
    {
        GameObject[] mangQuai = GameObject.FindGameObjectsWithTag("Enemy");
        float khoangCachNganNhat = Mathf.Infinity;
        GameObject quaiGanNhat = null;

        foreach (GameObject quai in mangQuai)
        {
            if (quai.activeInHierarchy)
            {
                float kc = Vector2.Distance(transform.position, quai.transform.position);
                if (kc < khoangCachNganNhat)
                {
                    khoangCachNganNhat = kc;
                    quaiGanNhat = quai;
                }
            }
        }

        if (quaiGanNhat != null) mucTieuQuai = quaiGanNhat.transform;
        else mucTieuQuai = null;
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
            // Ép đạn ra ngoài map tự do, không làm con của nhân vật nữa
            vienDan.transform.SetParent(null);
            vienDan.SetActive(true);
        }

        if (vienDan != null)
        {
            // ĐÃ ĐỔI THÀNH DanNv4 ĐỂ KHÔNG BỊ BÁO LỖI MISSING NỮA
            DanNv4 scriptDan = vienDan.GetComponent<DanNv4>();
            if (scriptDan != null)
            {
                scriptDan.satThuong = satThuong;
                scriptDan.KichHoatVienDan();
            }
        }
    }

    void XoMat(float xMucTieu)
    {
        if (xMucTieu < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    Vector3 LayViTriNgauNhienTrongBox(BoxCollider2D box)
    {
        Bounds bounds = box.bounds;
        float xNgauNhien = Random.Range(bounds.min.x, bounds.max.x);
        float yNgauNhien = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(xNgauNhien, yNgauNhien, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, TamBan);
    }
}