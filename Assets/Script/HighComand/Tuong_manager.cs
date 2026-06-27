using UnityEngine;

public class HeroSelectionManager : MonoBehaviour
{
    [Header("--- MẢNG DANH SÁCH TƯỚNG ---")]
    [Tooltip("Bỏ bao nhiêu con tướng tùy thích vào đây. Thứ tự trong mảng tương ứng với ID (Phần tử 0 là ID 0).")]
    public GameObject[] danhSachTuongMenu;

    [Header("--- HIỆU ỨNG ÁNH SÁNG ---")]
    [Tooltip("Kéo GameObject vòng sáng dưới chân hoặc hiệu ứng hào quang vào đây.")]
    public GameObject anhSangChonTuong;

    private SaveSystem quanLySave;
    private int idTuongDangChonHienTai = -1;

    private void Start()
    {
        quanLySave = Object.FindFirstObjectByType<SaveSystem>();
        KhoiTaoTuongTheoSave();
    }

    private void KhoiTaoTuongTheoSave()
    {
        int idTarget = 0;

        if (quanLySave != null && quanLySave.KiemTraCoFileSave())
        {
            idTarget = quanLySave.DocTuongDaChon();
        }

        if (danhSachTuongMenu == null || danhSachTuongMenu.Length == 0) return;

        DiChuyenVongSangDenTuong(idTarget);
    }

    public void ChonTuongBằngClick(int idCuaTuongDuocClick)
    {
        if (danhSachTuongMenu == null || danhSachTuongMenu.Length == 0) return;

        if (idCuaTuongDuocClick < 0 || idCuaTuongDuocClick >= danhSachTuongMenu.Length)
        {
            idCuaTuongDuocClick = 0;
        }

        if (idCuaTuongDuocClick == idTuongDangChonHienTai) return;

        idTuongDangChonHienTai = idCuaTuongDuocClick;
        DiChuyenVongSangDenTuong(idCuaTuongDuocClick);

        if (quanLySave != null)
        {
            quanLySave.LuuTuongDaChon(idCuaTuongDuocClick);
            Debug.Log($"<color=cyan><b>[Menu]</b> Đã lưu tướng ID {idCuaTuongDuocClick} vào Save System!</color>");
        }
    }

    private void DiChuyenVongSangDenTuong(int idTarget)
    {
        if (anhSangChonTuong == null || danhSachTuongMenu == null || danhSachTuongMenu.Length == 0) return;

        if (idTarget < 0 || idTarget >= danhSachTuongMenu.Length)
        {
            idTarget = 0;
        }

        GameObject tuongMucTieu = danhSachTuongMenu[idTarget];

        if (tuongMucTieu != null)
        {
            anhSangChonTuong.SetActive(true);
            anhSangChonTuong.transform.position = tuongMucTieu.transform.position;
            idTuongDangChonHienTai = idTarget;
        }
    }
}