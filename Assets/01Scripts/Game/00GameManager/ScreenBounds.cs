using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBounds : MonoBehaviour
{
    public BoxCollider2D top;
    public BoxCollider2D bottom;
    public BoxCollider2D left;
    public BoxCollider2D right;

    // Cho phép điều chỉnh độ dày của biên trong Inspector
    public float thickness = 1f;

    void Start()
    {
        UpdateBounds();
    }

    // Dùng LateUpdate để đảm bảo bounds cập nhật sau khi Camera có thể đã di chuyển hoặc thay đổi size
    void LateUpdate()
    {
        UpdateBounds();
    }

    void UpdateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Lấy tọa độ thế giới của góc dưới trái và trên phải màn hình
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRight   = cam.ViewportToWorldPoint(new Vector2(1, 1));

        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        // Top - Đặt tâm sao cho mép dưới của collider chạm mép trên màn hình
        top.size = new Vector2(width + thickness * 2, thickness); // Mở rộng thêm bề ngang để che kín 2 góc
        top.offset = new Vector2(0, height / 2 + thickness / 2);

        // Bottom
        bottom.size = new Vector2(width + thickness * 2, thickness);
        bottom.offset = new Vector2(0, -height / 2 - thickness / 2);

        // Left
        left.size = new Vector2(thickness, height + thickness * 2);
        left.offset = new Vector2(-width / 2 - thickness / 2, 0);

        // Right
        right.size = new Vector2(thickness, height + thickness * 2);
        right.offset = new Vector2(width / 2 + thickness / 2, 0);
    }
}
