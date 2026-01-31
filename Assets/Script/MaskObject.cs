using UnityEngine;
using System;

public class MaskObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    // Biến lưu trữ sprite tạm thời của NPC hiện tại
    private Sprite _currentWholeSprite;
    private Sprite _currentBrokenSprite;

    public event Action OnMaskBroken;
    private bool _canInteract = false;

    // --- HÀM MỚI: ĐỂ NPC NẠP DỮ LIỆU VÀO ---
    public void SetupMask(Sprite whole, Sprite broken)
    {
        _currentWholeSprite = whole;
        _currentBrokenSprite = broken;

        // Mặc định ban đầu là hiển thị mặt nạ nguyên
        spriteRenderer.sprite = _currentWholeSprite;
    }
    // ---------------------------------------

    public void EnableInteraction()
    {
        _canInteract = true;
    }

    public void HealMask()
    {
        // Hồi phục lại dùng sprite nguyên vẹn hiện tại
        spriteRenderer.sprite = _currentWholeSprite;
        gameObject.SetActive(true);
    }

    void OnMouseDown()
    {
        if (_canInteract)
        {
            _canInteract = false;
            
            // Đổi sang sprite vỡ hiện tại
            if (_currentBrokenSprite != null)
                spriteRenderer.sprite = _currentBrokenSprite;
            
            OnMaskBroken?.Invoke();

            // Ẩn mask sau 0.5s hoặc ẩn ngay tùy bạn
            // gameObject.SetActive(false); 
        }
    }
}