using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("--- KẾT NỐI CAMERA CHÍNH ---")]
    public Transform cameraChinh;

    [Header("--- MẢNG CHỨA CÁC LỚP NỀN BACKGROUND ---")]
    public Transform[] danhSachCacLopNen;

    [Header("--- TỐC ĐỘ CUỘN (Số càng lớn trượt càng nhanh, 0 = đứng im) ---")]
    [Tooltip("Ví dụ: Đất nền sát chân lính để 0.5 -> 1. Núi xa để 0.1 -> 0.2")]
    public float[] tocDoCuonParallax;

    private float[] viTriGocXOfLayers;
    private float[] doRongCacBucAnh;
    private Vector3 viTriCuCuaCamera;

    void Start()
    {
        if (cameraChinh == null)
        {
            cameraChinh = Camera.main.transform;
        }

        viTriCuCuaCamera = cameraChinh.position;

        if (danhSachCacLopNen != null && danhSachCacLopNen.Length > 0)
        {
            int soLuongLayer = danhSachCacLopNen.Length;
            viTriGocXOfLayers = new float[soLuongLayer];
            doRongCacBucAnh = new float[soLuongLayer];

            for (int i = 0; i < soLuongLayer; i++)
            {
                if (danhSachCacLopNen[i] == null) continue;

                viTriGocXOfLayers[i] = danhSachCacLopNen[i].position.x;

                SpriteRenderer sRenderer = danhSachCacLopNen[i].GetComponent<SpriteRenderer>();
                if (sRenderer != null)
                {
                    // Lấy độ rộng chuẩn của ảnh dựa trên kích thước thực tế ngoài Map
                    doRongCacBucAnh[i] = sRenderer.bounds.size.x;
                }
                else
                {
                    doRongCacBucAnh[i] = 15f;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (danhSachCacLopNen == null || danhSachCacLopNen.Length == 0) return;

        // Tính khoảng cách Camera đã di chuyển được giữa 2 khung hình
        Vector3 deltaMovement = cameraChinh.position - viTriCuCuaCamera;

        for (int i = 0; i < danhSachCacLopNen.Length; i++)
        {
            if (danhSachCacLopNen[i] == null) continue;

            // Lấy hệ số tốc độ cuộn từ mảng cấu hình
            float heSoTocDo = (i < tocDoCuonParallax.Length) ? tocDoCuonParallax[i] : 0f;

            // 1. DI CHUYỂN NỀN THEO CAMERA (Nhân trực tiếp với hệ số để tăng tốc độ cuộn)
            danhSachCacLopNen[i].position += new Vector3(deltaMovement.x * heSoTocDo, deltaMovement.y * heSoTocDo, 0f);

            // 2. KIỂM TRA ĐỂ NHẤC ẢNH GỐI ĐẦU VÔ TẬN
            // Tính khoảng cách lệch giữa tâm Camera và tâm của bức ảnh nền hiện tại
            float khoangCachLech = cameraChinh.position.x - danhSachCacLopNen[i].position.x;

            // Nếu Camera đi lệch quá một nửa độ rộng bức ảnh về bên phải -> Nhấc ảnh tiến lên
            if (khoangCachLech > doRongCacBucAnh[i] * 0.5f)
            {
                danhSachCacLopNen[i].position += new Vector3(doRongCacBucAnh[i], 0f, 0f);
            }
            // Nếu Camera đi lệch quá một nửa độ rộng bức ảnh về bên trái -> Nhấc ảnh lùi lại
            else if (khoangCachLech < -doRongCacBucAnh[i] * 0.5f)
            {
                danhSachCacLopNen[i].position -= new Vector3(doRongCacBucAnh[i], 0f, 0f);
            }
        }

        // Lưu lại vị trí Camera để tính toán cho khung hình tiếp theo
        viTriCuCuaCamera = cameraChinh.position;
    }
}