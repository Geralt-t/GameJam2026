using UnityEngine;
using TMPro;

public class TapHitObject : HitObject
{
    [Header("Visual References")]
    public Transform approachCircle;
    public SpriteRenderer circleSprite;
    public TextMeshPro keyText;

    [Header("Settings")]
    public float startScale = 2f;
    public float endScale = 1f;

    public override void Initialize(HitObjectData data)
    {
        base.Initialize(data);

        // Setup hiển thị ban đầu
        if (keyText != null) keyText.text = _data.hitKey.ToString();
        if (approachCircle != null) approachCircle.localScale = Vector3.one * startScale;
    }

    protected override void UpdateVisuals()
    {
        if (approachCircle == null) return;

        // 1. Tính toán Scale (Lerp từ to -> nhỏ)
        // _timeAlive và _data được lấy từ class cha
        float t = _timeAlive / _data.hitTime; 
        float currentScale = Mathf.Lerp(startScale, endScale, t);
        approachCircle.localScale = Vector3.one * currentScale;

        // 2. Logic Đổi Màu Xanh (Feedback thị giác)
        float timeRemaining = _data.hitTime - _timeAlive;
        
        // Nếu vào vùng hitWindow -> Màu xanh
        if (Mathf.Abs(timeRemaining) <= hitWindow)
        {
            if (circleSprite != null) circleSprite.color = Color.mediumSeaGreen;
        }
    }

    protected override void CheckInput()
    {
        // Kiểm tra đúng phím
        if (Input.GetKeyDown(_data.hitKey))
        {
            float timeRemaining = _data.hitTime - _timeAlive;
            float diff = Mathf.Abs(timeRemaining);

            if (diff <= hitWindow)
            {
                // Bấm đúng lúc -> Thắng
                Debug.Log("Perfect Hit!");
                OnSuccess();
            }
            else
            {
                // Bấm quá sớm -> Thua (Trừ điểm)
                Debug.Log("Too Early! Missed.");
                
                // Đổi màu đỏ cho người chơi biết là sai
                if (circleSprite != null) circleSprite.color = Color.softRed;
                
                OnFail();
            }
        }
    }
}