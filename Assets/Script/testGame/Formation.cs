using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FormationManager — Quản lý đội hình ma trận cho tất cả lính (trừ Servitor).
/// Đã tối ưu hóa thuật toán chia hàng/cột so le và dọn sạch code thừa.
/// </summary>
public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance { get; private set; }

    [System.Serializable]
    public class RankConfig
    {
        public string tenLoai;      // Tên loại lính (Ví dụ: Titan, KhoGrak...)
        public int rank;            // Chỉ số Rank tương ứng (0, 1, 2, 3...)
        public int maxPerColumn;    // Số quân tối đa trên một cột dọc trước khi lùi sang cột mới
        public float xOffset;       // Khoảng cách lùi mặc định của lớp lính này (trục X âm)
    }

    [Header("--- CẤU HÌNH TỪNG LOẠI LÍNH ---")]
    public RankConfig[] rankConfigs = new RankConfig[]
    {
        // Khoảng cách xOffset đã được tối ưu giãn cách xa nhau để các lớp lính không dẫm lên nhau
        new RankConfig { tenLoai = "Titan",      rank = 4, maxPerColumn = 2,  xOffset =  0f   },
        new RankConfig { tenLoai = "KhoGrak",    rank = 0, maxPerColumn = 5,  xOffset = -3.5f },
        new RankConfig { tenLoai = "IronStorm",  rank = 1, maxPerColumn = 5,  xOffset = -7.0f },
        new RankConfig { tenLoai = "Terminator", rank = 2, maxPerColumn = 5,  xOffset = -10.5f},
        new RankConfig { tenLoai = "DeadIron",   rank = 3, maxPerColumn = 4,  xOffset = -14.0f },
    };

    [Header("--- CÀI ĐẶT GIÃN CÁCH CHUNG ---")]
    [Tooltip("Vị trí trục X của hàng đầu tiên (gần địch nhất)")]
    public float baseX = -2f; //

    [Tooltip("Trục Y trung tâm của đội hình (giữa làn đường chạy)")]
    public float centerY = -1.5f; //

    [Tooltip("Khoảng cách dọc giữa các con lính trong cùng một cột (Tăng lên để không bị chồng Sprite)")]
    public float spacingY = 2.5f; // Đã tối ưu lên 2.5 để lính không đè đầu cưỡi cổ nhau

    [Tooltip("Khoảng lùi về sau (trục X) khi lính đầy cột và tràn sang cột thứ 2, thứ 3...")]
    public float columnDepthSpacing = 2.5f; // Đã tối ưu lên 2.5 để các hàng dọc cách xa nhau rõ ràng

    [Tooltip("Tốc độ lính di chuyển tịnh tiến về vị trí ô slot")]
    public float snapSpeed = 5.0f;

    [Tooltip("Khoảng cách tối thiểu để coi như lính đã đứng đúng vị trí")]
    public float arrivalThreshold = 0.15f;

    // ── Dữ liệu quản lý Slot nội bộ ──────────────────────────────────────────────
    private class Slot
    {
        public Vector2 position;
        public int instanceID; // 0 = Slot đang trống
    }

    private readonly Dictionary<int, List<Slot>> _slots = new Dictionary<int, List<Slot>>();
    private readonly Dictionary<int, Slot> _unitSlot = new Dictionary<int, Slot>();
    private readonly Dictionary<int, RankConfig> _cfgMap = new Dictionary<int, RankConfig>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } //
        Instance = this; //

        foreach (var cfg in rankConfigs) //
        {
            _cfgMap[cfg.rank] = cfg; //
            _slots[cfg.rank] = new List<Slot>(); //
        }
    }

    // ── Public API Điều khiển Đội Hình ───────────────────────────────────────────────────

    /// <summary>
    /// Đăng ký lính vào hệ thống slot ngay khi vừa được sinh ra
    /// </summary>
    public bool Register(GameObject go, int rank)
    {
        int id = go.GetInstanceID(); //
        if (_unitSlot.ContainsKey(id)) return true; //
        if (!_cfgMap.TryGetValue(rank, out RankConfig cfg)) return false; //

        List<Slot> slots = _slots[cfg.rank]; //

        // 1. Tìm xem có slot cũ nào đang trống (do lính đứng trước đó vừa chết) để điền vào không
        Slot slot = null; //
        foreach (var s in slots) //
        {
            if (s.instanceID == 0) { slot = s; break; } //
        }

        // 2. Nếu không có slot trống, tính toán tạo thêm một Slot hoàn toàn mới ở rìa đội hình
        if (slot == null) //
        {
            slot = new Slot { position = CalcPosition(cfg, slots.Count) }; //
            slots.Add(slot); //
        }

        slot.instanceID = id; //
        _unitSlot[id] = slot; //
        return true; //
    }

    /// <summary>
    /// Hủy đăng ký khi lính chết - Giải phóng slot nhưng GIỮ NGUYÊN vị trí cố định của Slot đó
    /// </summary>
    public void Unregister(GameObject go)
    {
        int id = go.GetInstanceID(); //
        if (!_unitSlot.TryGetValue(id, out Slot slot)) return; //
        slot.instanceID = 0; // Đánh dấu ô trống để con lính sinh sau tự động chạy vào điền chỗ trống
        _unitSlot.Remove(id); //
    }

    /// <summary>
    /// Lấy tọa độ đích của ô slot tương ứng với con lính
    /// </summary>
    public bool TryGetSlot(GameObject go, out Vector2 pos)
    {
        if (_unitSlot.TryGetValue(go.GetInstanceID(), out Slot s)) //
        {
            pos = s.position; //
            return true; //
        }
        pos = Vector2.zero; //
        return false; //
    }

    /// <summary>
    /// Kiểm tra xem lính đã đứng khít vào ô slot hay chưa
    /// </summary>
    public bool IsAtSlot(GameObject go)
    {
        if (!_unitSlot.TryGetValue(go.GetInstanceID(), out Slot s)) return false; //
        return Vector2.Distance(go.transform.position, s.position) <= arrivalThreshold; //
    }

    /// Trả về Vận tốc tịnh tiến (Velocity) cần thiết để di chuyển lính về đúng slot
    /// </summary>
    public Vector2 GetSlotVelocity(GameObject go, float overrideSpeed = -1f)
    {
        if (!_unitSlot.TryGetValue(go.GetInstanceID(), out Slot s)) return Vector2.zero; //
        Vector2 toSlot = s.position - (Vector2)go.transform.position; //
        if (toSlot.magnitude <= arrivalThreshold) return Vector2.zero; //
        float spd = overrideSpeed > 0f ? overrideSpeed : snapSpeed; //
        return toSlot.normalized * spd; //
    }

    // ── Thuật toán tính toán vị trí ma trận nâng cao ──────────────────────

    private Vector2 CalcPosition(RankConfig cfg, int index)
    {
        int col = index / cfg.maxPerColumn; // Cột thứ mấy lùi về sau
        int row = index % cfg.maxPerColumn; // Hàng thứ mấy trong cột dọc đó

        // 1. Tính toán vị trí trục X (Giữ nguyên logic của bạn)
        float finalX = baseX + cfg.xOffset - (col * columnDepthSpacing);

        // 2. TÍNH TOÁN TRỤC Y CHUẨN STICK WAR (Cố định khoảng cách bám theo tâm centerY):
        // Thay vì tính giật lùi phức tạp, ta lấy centerY làm gốc chính giữa.
        // Các hàng lính (row) sẽ được xếp đối xứng qua tâm bằng cách trừ đi một nửa số lượng hàng.
        float nửaĐộiHình = (cfg.maxPerColumn - 1) * 0.5f;
        float finalY = centerY + ((row - nửaĐộiHình) * spacingY);

        // 3. Hiệu ứng so le răng cưa (Zigzag) giữa các cột để không che tầm bắn
        if (col % 2 != 0)
        {
            finalY += spacingY * 0.3f; // Nhích nhẹ một chút cho đẹp đội hình hàng sau
        }

        // Tịnh tiến theo Object quản lý
        return new Vector2(transform.position.x + finalX, transform.position.y + finalY);
    }
}
