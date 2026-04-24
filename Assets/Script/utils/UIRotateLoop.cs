using UnityEngine;

/// <summary>
/// Lightweight unscaled-time UI rotation loop for RectTransform.
/// </summary>
[DisallowMultipleComponent]
public class UIRotateLoop : MonoBehaviour
{
    [SerializeField] RectTransform target;
    [SerializeField] float zDegreesPerSecond = 80f;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] bool resetOnEnable = false;
    [SerializeField] float initialZAngle = 0f;

    void Awake()
    {
        if (target == null)
            target = transform as RectTransform;
    }

    void OnEnable()
    {
        if (resetOnEnable)
            SetAngle(initialZAngle);
    }

    void Update()
    {
        if (target == null || Mathf.Approximately(zDegreesPerSecond, 0f))
            return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        target.Rotate(0f, 0f, zDegreesPerSecond * dt);
    }

    public void Configure(float speed, bool unscaled)
    {
        zDegreesPerSecond = speed;
        useUnscaledTime = unscaled;
    }

    public void SetAngle(float zAngle)
    {
        if (target == null)
            return;

        Vector3 euler = target.localEulerAngles;
        euler.z = zAngle;
        target.localEulerAngles = euler;
    }
}
