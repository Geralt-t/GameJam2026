using UnityEngine;
using System;
using DG.Tweening;

// Đảm bảo có BoxCollider2D để bắt sự kiện click
[RequireComponent(typeof(BoxCollider2D))] 
public class MaskObject : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer; // Vẫn giữ cái này theo ý bạn

    [Header("Data")]
    private Sprite _currentWholeSprite;
    private Sprite _currentBrokenSprite;
    
    [Header("Events")]
    public Action OnMaskClicked;
    public event Action OnMaskBroken; // Biến này giữ lại cho code cũ (nếu có dùng)

    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 200f; // Khoảng cách (Pixel nếu trong Canvas)
    [SerializeField] private float animDuration = 0.5f;
    
    private Vector3 originalLocalPos;
    private bool _canInteract = false;

    private void Awake()
    {
        // Lấy vị trí Local ban đầu (RectTransform hay Transform thường đều dùng được localPosition)
        originalLocalPos = transform.localPosition;
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // --- HÀM NẠP DỮ LIỆU ---
    public void SetupMask(Sprite whole, Sprite broken)
    {
        _currentWholeSprite = whole;
        _currentBrokenSprite = broken;
        if (spriteRenderer != null) spriteRenderer.sprite = _currentWholeSprite;
    }

    public void EnableInteraction()
    {
        _canInteract = true;
        // Đảm bảo Collider kích thước khớp với Sprite để click cho dễ
        // Nếu Collider bé quá thì sẽ khó click
    }

    public void ShowMaskAnimated()
    {
        gameObject.SetActive(true);

        // 1. Tính toán vị trí bắt đầu
        // Nếu dùng RectTransform trong Canvas, 1 đơn vị Move = 1 Pixel (thường là vậy)
        transform.localPosition = originalLocalPos + new Vector3(0, moveDistance, 0);

        // 2. Reset màu để làm hiệu ứng Fade In
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        _canInteract = false; // Khóa click khi đang bay

        // 3. Tạo Sequence DOTween
        Sequence mySequence = DOTween.Sequence();

        // Dùng DOLocalMove là an toàn nhất cho cả Transform thường lẫn RectTransform
        mySequence.Join(transform.DOLocalMove(originalLocalPos, animDuration).SetEase(Ease.OutBack));
        mySequence.Join(spriteRenderer.DOFade(1f, animDuration));

        // 4. Xong phim thì cho phép click
        mySequence.OnComplete(() => {
            EnableInteraction();
        });
    }

    public void HealMask()
    {
        if (spriteRenderer != null && _currentWholeSprite != null)
        {
            spriteRenderer.sprite = _currentWholeSprite;
        }
        gameObject.SetActive(true);
    }

    // Dùng OnMouseDown vì đây là SpriteRenderer (cần Collider)
    private void OnMouseDown()
    {
        if (_canInteract)
        {
            Debug.Log("Đã click vào mặt nạ (Sprite)!");
            _canInteract = false;
            OnMaskClicked?.Invoke();
        }
    }
}