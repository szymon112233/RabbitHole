using System;
using UnityEngine;

namespace DefaultNamespace.UI {
public class Shaker : MonoBehaviour {

    [SerializeField] 
    protected AnimationCurve IntensityOverDuration;
    
    [SerializeField] 
    protected float Duration;

    [SerializeField] 
    protected float PerlinMoveSpeed = 10;
    
    Vector2 _InitialPosition;

    private float _ShakeTime = 0;

    private bool _FirstShake;

    private void Start() {
        _InitialPosition = transform.localPosition;
    }

    [ContextMenu("Shake")]
    public void Shake() {
        _ShakeTime = Duration;
    }

    private void Update() {

        if (_ShakeTime > 0) {
            _FirstShake = true;
            
            _ShakeTime = Mathf.Max(0, _ShakeTime - Time.deltaTime);
            var shakeProgress = Mathf.Clamp01(_ShakeTime / Duration);
            var delta = new Vector2(
                Mathf.Lerp(-1.0f, 1.0f, Mathf.PerlinNoise(0, shakeProgress * PerlinMoveSpeed)),
                Mathf.Lerp(-1.0f, 1.0f, Mathf.PerlinNoise(shakeProgress * PerlinMoveSpeed, 0)));
        
            transform.localPosition = 
                _InitialPosition + 
                new Vector2(delta.x * IntensityOverDuration.Evaluate(shakeProgress),
                    delta.y * IntensityOverDuration.Evaluate(shakeProgress));    
        }
        else {
            if (!_FirstShake)
                _InitialPosition = transform.localPosition;
        }
        
    }
}
}