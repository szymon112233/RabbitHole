using UnityEngine;

[ExecuteAlways]
public class SimplePositionOscillator : CachedMonoBehaviour
{
    [SerializeField] private float m_Amplitude = 1f;
    [SerializeField] private float m_Frequency = 2f;
    void Start()
    {
    }

    void Update()
    {
        Transform.localPosition = Vector3.up * Mathf.Sin(Time.realtimeSinceStartup * m_Frequency) * m_Amplitude;
    }
}
