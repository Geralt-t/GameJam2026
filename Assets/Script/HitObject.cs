using UnityEngine;

// Class cha (Abstract) - Quy định luật chơi chung
public abstract class HitObject : MonoBehaviour
{
    protected HitObjectData _data;
    protected float _startTime;
    protected bool _hasHit = false;

    // Biến dùng chung để tính toán thời gian
    protected float _timeAlive => Time.time - _startTime;
    
    // Settings
    public float hitWindow = 0.2f;

    public virtual void Initialize(HitObjectData data)
    {
        _data = data;
        _startTime = Time.time;
        transform.position = _data.position;
    }

    private void Update()
    {
        if (_hasHit)
        {
           
            return;
        }

        // 1. Kiểm tra nếu hết giờ (Miss)
        if (_timeAlive > _data.hitTime + hitWindow)
        {
            OnFail(); // Hết giờ mà chưa bấm -> Thua
            return;
        }

        // 2. Gọi logic hình ảnh (Do lớp con tự viết)
        UpdateVisuals();

        // 3. Gọi logic nhập liệu (Do lớp con tự viết)
        CheckInput();
    }

    // Các hàm Abstract bắt buộc lớp con phải có
    protected abstract void UpdateVisuals();
    protected abstract void CheckInput();

    // Logic Thắng/Thua
    protected virtual void OnSuccess()
    {
        _hasHit = true;
        AudioManager.Instance.PlaySFX("ping");
        LevelManager.Instance.AddProgress();
        Destroy(gameObject);
    }

    protected virtual void OnFail()
    {
        _hasHit = true;
        LevelManager.Instance.SubtractProgress();
        
        // Hiệu ứng Fail (đổi màu đỏ...) có thể xử lý ở đây hoặc lớp con
        // Tạm thời destroy sau 0.1s
        Destroy(gameObject, 0.1f);
    }
}