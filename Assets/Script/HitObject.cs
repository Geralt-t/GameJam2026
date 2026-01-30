using System;
using TMPro;
using UnityEngine;

public class HitObject : MonoBehaviour
{
    [Header("References")]
    public Transform approachCircle;
    public event Action<KeyCode> OnObjectDestroyed;
    public TMP_Text keyText;

    [Header("Settings")]
    public float approachTime = 1.0f;
    public float hitWindow = 0.15f;

    public float startScale = 3.0f;
    public float endScale = 1.0f;


    private HitObjectData _data;
    private float _elapsedTime = 0f;
    private bool _isInitialized = false;
    private bool _isProcessed = false;

    public void Initialize(HitObjectData data)
    {
        _data = data;
        transform.position = new Vector3(data.position.x, data.position.y, 0);
        if (keyText != null)
        {
            keyText.text = data.hitKey.ToString();
        }
        _isInitialized = true;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        float t = _elapsedTime / _data.hitTime;

        if (approachCircle != null)
        {
            float scale = Mathf.Lerp(startScale, endScale, Mathf.Clamp01(t));
            approachCircle.localScale = new Vector3(scale, scale, 1);
        }
        if (Input.GetKeyDown(_data.hitKey))
        {
            CheckHit();
        }

        if (_elapsedTime >= _data.hitTime + hitWindow)
        {
            OnResult("Miss");
        }
    }

    private void CheckHit()
    {
        float diff = Mathf.Abs(_elapsedTime - _data.hitTime);
        if (diff <= hitWindow)
        {
            OnResult("Hit! Perfect!");
        }
    }

    private void OnResult(string msg)
    {
        _isProcessed = true;
        Debug.Log(msg);
        OnObjectDestroyed?.Invoke(_data.hitKey);
        Destroy(gameObject);
    }

}
