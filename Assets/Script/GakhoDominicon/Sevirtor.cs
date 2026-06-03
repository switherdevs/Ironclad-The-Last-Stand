using UnityEngine;

public class Sevirtor : MonoBehaviour
{
    public enum MinerState { DiToiBaiVang, DangDaoVang, VeNhaChinh, CatVang }

    [Header("Trạng thái hiện tại")]
    public MinerState trangThaiHienTai = MinerState.DiToiBaiVang;

    [Header("Cấu hình vị trí")]
    public Transform DiemNhaChinh;

    [Header("Chỉ số đào vàng")]
    public float tocDoDiChuyen = 3f;
    public float thoiGianDaoVang = 3f;
    public int luongVangMangTheo = 10;

    private float demThoiGianDao;
    private float doLechToiThieu = 0.15f;

    private GoldMine moDangDao = null;
    private int indexSlotCuaTho = -1;       // Index slot thợ này đang giữ

    void Start()
    {
        if (DiemNhaChinh == null)
            DiemNhaChinh = GameObject.FindWithTag("Home")?.transform;
        TimVaDangKyMo();
    }

    void Update()
    {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        switch (trangThaiHienTai)
        {
            case MinerState.DiToiBaiVang: HanhDong_DiToiBaiVang(); break;
            case MinerState.DangDaoVang: HanhDong_DangDaoVang(); break;
            case MinerState.VeNhaChinh: HanhDong_VeNhaChinh(); break;
            case MinerState.CatVang: HanhDong_CatVang(); break;
        }
    }

    void TimVaDangKyMo()
    {
        GiaiPhongMoHienTai();

        GameObject[] tatCaMo = GameObject.FindGameObjectsWithTag("gold");
        GoldMine moGanNhat = null;
        float khoangCachNgan = Mathf.Infinity;

        foreach (GameObject mo in tatCaMo)
        {
            GoldMine goldMine = mo.GetComponent<GoldMine>();
            if (goldMine == null) goldMine = mo.AddComponent<GoldMine>();

            if (!goldMine.CoChoTrong) continue;

            float khoangCach = Vector3.Distance(transform.position, mo.transform.position);
            if (khoangCach < khoangCachNgan)
            {
                khoangCachNgan = khoangCach;
                moGanNhat = goldMine;
            }
        }

        if (moGanNhat != null)
        {
            int slot = moGanNhat.DangKyLaySlot();
            if (slot != -1)
            {
                moDangDao = moGanNhat;
                indexSlotCuaTho = slot;
                trangThaiHienTai = MinerState.DiToiBaiVang;
                return;
            }
        }

        Invoke(nameof(TimVaDangKyMo), 1f);
    }

    void GiaiPhongMoHienTai()
    {
        if (moDangDao != null)
        {
            moDangDao.TraSlot(indexSlotCuaTho);
            moDangDao = null;
            indexSlotCuaTho = -1;
        }
    }

    void HanhDong_DiToiBaiVang()
    {
        if (moDangDao == null) { TimVaDangKyMo(); return; }

        // Lấy vị trí slot realtime (theo cục vàng nếu nó di chuyển)
        Vector3 viTriSlot = moDangDao.LayViTriSlot(indexSlotCuaTho);

        XoayMat(viTriSlot.x);
        transform.position = Vector3.MoveTowards(
            transform.position, viTriSlot, tocDoDiChuyen * Time.deltaTime);
        if (Vector3.Distance(transform.position, viTriSlot) <= doLechToiThieu)
            BatDauDaoVang();
    }

    void HanhDong_DangDaoVang()
    {
        // Xoay mặt nhìn vào tâm cục vàng khi đào

        if (moDangDao != null)
            XoayMat(moDangDao.transform.position.x);

        demThoiGianDao -= Time.deltaTime;
        if (demThoiGianDao <= 0f)
        {
            GiaiPhongMoHienTai();
            trangThaiHienTai = MinerState.VeNhaChinh;
        }
    }

    void HanhDong_VeNhaChinh()
    {
        if (DiemNhaChinh == null) return;

        XoayMat(DiemNhaChinh.position.x);
        transform.position = Vector3.MoveTowards(
            transform.position, DiemNhaChinh.position, tocDoDiChuyen * Time.deltaTime);

        if (Vector3.Distance(transform.position, DiemNhaChinh.position) <= doLechToiThieu)
            trangThaiHienTai = MinerState.CatVang;
    }

    void HanhDong_CatVang()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.TangTien(luongVangMangTheo);
        else
            Debug.LogError("Không tìm thấy ResourceManager!");

        TimVaDangKyMo();
    }

    void BatDauDaoVang()
    {
        if (trangThaiHienTai == MinerState.DiToiBaiVang)
        {
            trangThaiHienTai = MinerState.DangDaoVang;
            demThoiGianDao = thoiGianDaoVang;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (trangThaiHienTai == MinerState.DiToiBaiVang && collision.CompareTag("gold"))
            BatDauDaoVang();

        if (trangThaiHienTai == MinerState.VeNhaChinh && collision.CompareTag("Home"))
            trangThaiHienTai = MinerState.CatVang;
    }

    void OnDisable() => GiaiPhongMoHienTai();
    void OnDestroy() => GiaiPhongMoHienTai();

    void XoayMat(float xMucTieu)
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(
            xMucTieu < transform.position.x ? -scaleX : scaleX,
            transform.localScale.y, transform.localScale.z);
    }
}
