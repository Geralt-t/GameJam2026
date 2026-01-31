using UnityEngine;
using System;

public class MaskObject : MonoBehaviour
{
    public Sprite maskWhole;  // Ảnh mặt nạ nguyên
    public Sprite maskBroken; // Ảnh mặt nạ vỡ
    public SpriteRenderer spriteRenderer;

    // Sự kiện để báo cho Manager biết là đã bị chạm
    public event Action OnMaskBroken; 

    private bool _isInteractable = false;

    void Start()
    {
        spriteRenderer.sprite = maskWhole; // Ban đầu là nguyên
    }

    // Hàm này được Manager gọi để cho phép người chơi bấm
    public void EnableInteraction()
    {
        _isInteractable = true;
        // Có thể thêm hiệu ứng phát sáng ở đây để gợi ý người chơi bấm
    }

    void OnMouseDown() // Xử lý khi click chuột hoặc chạm vào Collider 2D
    {
        if (_isInteractable)
        {
            BreakMask();
        }
    }

    void BreakMask()
    {
        _isInteractable = false;
        spriteRenderer.sprite = maskBroken; // Đổi sang ảnh vỡ
        
        // Báo hiệu ra ngoài
        OnMaskBroken?.Invoke();
        
        // Hiệu ứng âm thanh vỡ kính ở đây (nếu có)
    }

    public void HealMask()
    {
        spriteRenderer.sprite = maskWhole; // Quay lại ảnh nguyên
        // Hiệu ứng phép thuật hồi phục ở đây
    }
}