using UnityEngine;

public class GoldMine : MonoBehaviour
{
    public float khoangCachDungBen = 2f;
    public int soThoDaoToiDa = 1;
    private bool[] slotDaDung = new bool[1];

    // Chỉ check slot 0 thôi, tránh out of range
    public bool CoChoTrong => !slotDaDung[0];

    public int DangKyLaySlot()
    {
        if (!slotDaDung[0])
        {
            slotDaDung[0] = true;
            return 0;
        }
        return -1; // Đầy
    }

    public void TraSlot(int indexSlot)
    {
        if (indexSlot == 0)
            slotDaDung[0] = false;
    }

    public Vector3 LayViTriSlot(int index)
    {
        // 1 thợ/cục → đứng giữa tâm cục vàng
        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        // Hiện 1 vòng tròn ở tâm
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}