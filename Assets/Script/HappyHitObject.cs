using UnityEngine;

public class HappyHitObject : HitObject
{
    protected override void UpdateVisuals()
    {
        if (approachCircle != null)
        {
            float t = _elapsedTime / _data.hitTime;
            float scale = Mathf.Lerp(startScale, endScale, Mathf.Clamp01(t));
            approachCircle.localScale = new Vector3(scale, scale, 1);
        }
    }

    protected override void CheckInput()
    {
        if (Input.GetKeyDown(_data.hitKey))
        {
            float diff = Mathf.Abs(_elapsedTime - _data.hitTime);
            if (diff <= hitWindow)
            {
                OnResult("Hit! Perfect!");
            }
        }
    }
}
