using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("--- KẾT NỐI CAMERA CHÍNH ---")]
    public Transform cameraChinh;

    [Header("--- MẢNG CHỨA CÁC LỚP NỀN BACKGROUND ---")]
    public Transform[] danhSachCacLopNen;

    [Header("--- MẢNG CẤU HÌNH TỐC ĐỘ CUỘN (0 = VÔ TẬN, 1 = ĐỨNG IM) ---")]
    public float[] tocDoCuonParallax;

    private Vector3 viTriCuCuaCamera;

    // MẢNG MỚI: Lưu trữ tọa độ gốc ban đầu của từng lớp nền (dùng để tính toán lặp lại)
    private float[] viTriGocXOfLayers;
    // MẢNG MỚI: Lưu trữ độ rộng (chiều dài ngang) thực tế của từng bức ảnh nền
    private float[] doRongCacBucAnh;

    void Start()
    {
        if (cameraChinh == null)
        {
            cameraChinh = Camera.main.transform;
        }

        viTriCuCuaCamera = cameraChinh.position;

        // KHỞI TẠO BỘ NHỚ CHO CÁC MẢNG LƯU TRỮ TỰ ĐỘNG
        if (danhSachCacLopNen != null && danhSachCacLopNen.Length > 0)
        {
            int soLuongLayer = danhSachCacLopNen.Length;
            viTriGocXOfLayers = new float[soLuongLayer];
            doRongCacBucAnh = new float[soLuongLayer];

            // VÒNG LẶP THIẾT LẬP BAN ĐẦU: Đo kích thước từng bức ảnh
            for (int i = 0; i < soLuongLayer; i++)
            {
                if (danhSachCacLopNen[i] == null) continue;

                // Ghi nhớ vị trí trục X xuất phát của lớp nền này
                viTriGocXOfLayers[i] = danhSachCacLopNen[i].position.x;

                // LỆNH ĐO ĐỘ RỘNG: Tìm linh kiện SpriteRenderer để đo xem ảnh dài bao nhiêu mét trong Unity
                SpriteRenderer sRenderer = danhSachCacLopNen[i].GetComponent<SpriteRenderer>();
                if (sRenderer != null)
                {
                    // Lấy độ rộng thực tế sau khi đã nhân với tỷ lệ Scale của Object
                    doRongCacBucAnh[i] = sRenderer.bounds.size.x;
                }
                else
                {
                    // Phòng hờ nếu layer đó là Object cha chứa nhiều ảnh con, tự gán tạm độ rộng mặc định
                    doRongCacBucAnh[i] = 15f;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (danhSachCacLopNen == null || danhSachCacLopNen.Length == 0) return;

        // Tính toán độ dịch chuyển của Camera so với khung hình trước
        Vector3 doDichChuyenCuaCamera = cameraChinh.position - viTriCuCuaCamera;

        // VÒNG LẶP XỬ LÝ CHÍNH: Vừa trượt Parallax vừa kiểm tra lặp lại vô tận
        for (int i = 0; i < danhSachCacLopNen.Length; i++)
        {
            if (danhSachCacLopNen[i] == null) continue;

            float tocDoHienTai = (i < tocDoCuonParallax.Length) ? tocDoCuonParallax[i] : 0f;

            // 1. TÍNH TOÁN HIỆU ỨNG TRƯỢT PARALLAX (Code cũ giữ nguyên)
            float diChuyenX = doDichChuyenCuaCamera.x * tocDoHienTai;
            float diChuyenY = doDichChuyenCuaCamera.y * tocDoHienTai;
            danhSachCacLopNen[i].position += new Vector3(diChuyenX, diChuyenY, 0f);

            // 2. THUẬT TOÁN TỰ ĐỘNG DỊCH CHUYỂN NỀN ĐỂ LẶP LẠI VÔ TẬN (MỚI BỔ SUNG)
            // Tính toán khoảng cách tương đối giữa Camera và điểm gốc của bức ảnh nền
            float khoangCachDiDuocCuaCam = cameraChinh.position.x * (1 - tocDoHienTai);

            // Nếu Camera đi vượt quá giới hạn độ rộng của bức ảnh về bên phải
            if (khoangCachDiDuocCuaCam > viTriGocXOfLayers[i] + doRongCacBucAnh[i])
            {
                // Nhấc bức ảnh ném tới trước một khoảng bằng đúng độ rộng của nó để gối đầu liên tục
                viTriGocXOfLayers[i] += doRongCacBucAnh[i];

                // Cập nhật ngay tọa độ X mới cho Object nền ngoài map
                danhSachCacLopNen[i].position = new Vector3(viTriGocXOfLayers[i], danhSachCacLopNen[i].position.y, danhSachCacLopNen[i].position.z);
            }
            // Nếu người chơi đi lùi (Camera vượt quá giới hạn về bên trái)
            else if (khoangCachDiDuocCuaCam < viTriGocXOfLayers[i] - doRongCacBucAnh[i])
            {
                // Giật bức ảnh lùi về phía sau để bù nền kịp thời
                viTriGocXOfLayers[i] -= doRongCacBucAnh[i];
                danhSachCacLopNen[i].position = new Vector3(viTriGocXOfLayers[i], danhSachCacLopNen[i].position.y, danhSachCacLopNen[i].position.z);
            }
        }

        viTriCuCuaCamera = cameraChinh.position;
    }
}