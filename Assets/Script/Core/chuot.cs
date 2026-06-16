using UnityEngine;

public class chuot : MonoBehaviour
{
    [Header("--- CẤU HÌNH QUÉT ĐÈN ---")]
    [Tooltip("Bán kính vùng quét quanh con chuột (độ to của đầu cọ quét)")]
    [SerializeField] private float banKinhQuet = 0.5f;

    [Tooltip("Chọn đúng Layer 'DenLac' của bóng đèn để chuột tập trung quét")]
    [SerializeField] private LayerMask layerCuaDen;

    [Header("--- LỰC TÁC ĐỘNG ---")]
    [Tooltip("Hệ số nhân lực đẩy khiến đèn lắc mạnh hay nhẹ khi chuột đi qua")]
    [SerializeField] private float heSoLucDay = 5f;

    private Vector3 viTriKhungHinhTruoc;

    void Start()
    {
        // Khởi tạo vị trí đầu tiên để tránh bị giật lực ở khung hình đầu tiên
        viTriKhungHinhTruoc = LayViTriChuotTrongWorld();
        transform.position = viTriKhungHinhTruoc;
    }

    void Update()
    {
        Vector3 viTriChuotHienTai = LayViTriChuotTrongWorld();
        transform.position = viTriChuotHienTai;

        // Tính toán hướng di chuyển và khoảng cách chuột đã đi được giữa 2 khung hình
        Vector3 huongDiChuyen = viTriChuotHienTai - viTriKhungHinhTruoc;
        float khoangCachDiChuyen = huongDiChuyen.magnitude;

        // Nếu chuột có di chuyển (dù chậm hay cực kỳ nhanh)
        if (khoangCachDiChuyen > 0.001f)
        {
            Vector2 huongChuanHoa = huongDiChuyen.normalized;

            // Bắn một vệt quét hình hộp ảo (BoxCast) từ vị trí cũ đến vị trí mới để không bỏ sót đèn
            RaycastHit2D[] danhSachDenTrung = Physics2D.BoxCastAll(
                viTriKhungHinhTruoc,
                new Vector2(banKinhQuet * 2, banKinhQuet * 2),
                Vector2.SignedAngle(Vector2.right, huongChuanHoa),
                huongChuanHoa,
                khoangCachDiChuyen,
                layerCuaDen
            );

            // Duyệt qua tất cả các bóng đèn bị vệt chuột cắt qua
            foreach (RaycastHit2D hit in danhSachDenTrung)
            {
                Rigidbody2D rbDen = hit.collider.GetComponent<Rigidbody2D>();
                if (rbDen != null && rbDen.bodyType == RigidbodyType2D.Dynamic)
                {
                    // Tính toán lực đẩy tỷ lệ thuận với tốc độ vẩy chuột của người chơi
                    float tocDoChuot = khoangCachDiChuyen / Time.deltaTime;
                    float lucDayCuoi = tocDoChuot * heSoLucDay;

                    // Giới hạn lực đẩy tối đa để đèn không bị xoay vòng tròn quá đà khi vẩy quá nhanh
                    lucDayCuoi = Mathf.Clamp(lucDayCuoi, 0f, 300f);

                    // Tác dụng lực đẩy vào trục X của Rigidbody2D bóng đèn khiến nó lắc lư
                    rbDen.AddForce(new Vector2(huongChuanHoa.x * lucDayCuoi, 0), ForceMode2D.Force);
                }
            }
        }

        // Lưu lại vị trí để làm mốc tính toán cho khung hình kế tiếp
        viTriKhungHinhTruoc = viTriChuotHienTai;
    }

    private Vector3 LayViTriChuotTrongWorld()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
}