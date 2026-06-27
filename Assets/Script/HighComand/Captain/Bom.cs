using UnityEngine;

public class AirStrikeBomb : MonoBehaviour
{
    private Vector3 viTriDichDen;
    private float tocDoRoi = 15f;
    private System.Action hanhDongKhiNo;
    private bool daDenDich = false;

    // Hàm khởi hành nhận tọa độ đích và thực thi di chuyển
    public void KhoiHanh(Vector3 viTriNo, float doCaoBatDau, float tocDo, System.Action callbackNo)
    {
        viTriDichDen = viTriNo;
        tocDoRoi = tocDo;
        hanhDongKhiNo = callbackNo;

        // Đặt vị trí ban đầu của quả bom ở trên cao (trục Y dâng lên theo cấu hình vùng)
        transform.position = new Vector3(viTriNo.x, viTriNo.y + doCaoBatDau, viTriNo.z);
        daDenDich = false;
    }

    private void Update()
    {
        if (daDenDich) return;

        // Di chuyển tịnh tiến hình học mượt mà về tọa độ đích dưới đất
        transform.position = Vector3.MoveTowards(transform.position, viTriDichDen, tocDoRoi * Time.deltaTime);

        // Thuật toán kiểm tra khoảng cách thay cho va chạm Collider vật lý
        if (Vector3.Distance(transform.position, viTriDichDen) < 0.1f)
        {
            daDenDich = true;
            ThucThiNo();
        }
    }

    private void ThucThiNo()
    {
        // Kích hoạt hàm sinh vùng gây sát thương được truyền từ Map qua
        if (hanhDongKhiNo != null)
        {
            hanhDongKhiNo.Invoke();
        }

        // Tự hủy bản thân quả bom sau khi chạm đất thành công
        Destroy(gameObject);
    }
}