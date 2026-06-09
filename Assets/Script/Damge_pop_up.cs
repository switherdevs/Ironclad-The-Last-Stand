using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("--- KẾT NỐI TEXT MESH PRO ---")]
    public TextMeshProUGUI textHienThi;

    [Header("--- CẤU HÌNH HIỆU ỨNG ---")]
    public float tocDoBayLen = 2.5f;
    public float thoiGianBienMat = 0.6f;
    public Color mauSacChu = Color.red;

    private float thoiGianDaQua;

    void Start()
    {
        if (textHienThi == null) textHienThi = GetComponentInChildren<TextMeshProUGUI>();
        if (textHienThi != null) textHienThi.color = mauSacChu;

        Destroy(gameObject, thoiGianBienMat);
    }

    void Update()
    {
        // Cho chữ bay lên trên
        transform.Translate(Vector3.up * tocDoBayLen * Time.deltaTime);

        // Tính toán để làm mờ chữ dần theo thời gian
        thoiGianDaQua += Time.deltaTime;
        if (textHienThi != null)
        {
            float tyLeThoiGian = 1f - (thoiGianDaQua / thoiGianBienMat);
            Color mauHienTai = textHienThi.color;
            mauHienTai.a = Mathf.Clamp01(tyLeThoiGian); // Giảm dần độ Alpha về 0
            textHienThi.color = mauHienTai;
        }
    }

    public void ThietLapThongSo(int soSatThuong)
    {
        if (textHienThi == null) textHienThi = GetComponentInChildren<TextMeshProUGUI>();

        if (textHienThi != null)
        {
            textHienThi.text = "-" + soSatThuong.ToString();
        }
    }
}