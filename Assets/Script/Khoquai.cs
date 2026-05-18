using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour
{
    // Đây chính là "Số điện thoại đường dây nóng" công khai
    public static SimpleObjectPool Instance;

    private void Awake()
    {
        Instance = this; // Kích hoạt đường dây nóng ngay khi game chạy
    }

    // Cái kho chứa các hộp đồ chơi (mỗi loại quái là một ngăn chứa riêng)
    private Dictionary<string, List<GameObject>> khoChua = new Dictionary<string, List<GameObject>>();

    // Hàm xuất hàng sang cho bên ChaosDirector gọi
    public GameObject LayQuaiRa(GameObject khuonQuai, Vector3 viTriGiaoHang)
    {
        string tenQuai = khuonQuai.name;

        // Nếu ngăn chứa loại quái này chưa tồn tại, tạo ngăn mới
        if (!khoChua.ContainsKey(tenQuai))
        {
            khoChua.Add(tenQuai, new List<GameObject>());
        }

        // TÌM HÀNG CŨ: Kiểm tra xem có con quái nào đang ngủ đông không
        foreach (GameObject quai in khoChua[tenQuai])
        {
            if (!quai.activeInHierarchy)
            {
                quai.transform.position = viTriGiaoHang; // Đưa đến vị trí xuất hiện
                quai.SetActive(true);                   // Đánh thức quái dậy
                return quai;                            // Giao hàng thành công!
            }
        }

        // ĐÚC HÀNG MỚI: Nếu người chơi diệt quái quá chậm làm kho hết sạch, tự đúc thêm
        GameObject quaiMoi = Instantiate(khuonQuai, viTriGiaoHang, Quaternion.identity);
        khoChua[tenQuai].Add(quaiMoi); // Cất vào kho để lần sau dùng tiếp
        return quaiMoi;
    }
}