using System;
using TMPro;
using UnityEngine;

public abstract class HitObject : MonoBehaviour
{
    [Header("Base References")]
    public Transform approachCircle;
    public TMP_Text keyText;
    public event Action<KeyCode> OnObjectDestroyed;

    [Header("Base Settings")]
    public float hitWindow = 0.15f;
    public float startScale = 3.0f;
    public float endScale = 1.0f;

    protected HitObjectData _data;
    protected float _elapsedTime = 0f;
    protected bool _isProcessed = false;

    public virtual void Initialize(HitObjectData data)
    {
        _data = data;
        transform.position = new Vector3(data.position.x, data.position.y, 0);
        if (keyText != null)
        {
            keyText.text = data.hitKey.ToString();
        }
    }

    protected virtual void Update()
    {
        if (_isProcessed) return;

        _elapsedTime += Time.deltaTime;

        UpdateVisuals();
        CheckInput();

        if (_elapsedTime >= _data.hitTime + hitWindow)
        {
            OnResult("Miss");
        }
    }

    protected abstract void UpdateVisuals();

    protected abstract void CheckInput();

    protected void OnResult(string msg)
    {
        if (_isProcessed) return;
        
        _isProcessed = true;
        Debug.Log(msg);
        OnObjectDestroyed?.Invoke(_data.hitKey);
        Destroy(gameObject);
    }
}