using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace DefaultNamespace.UI {
public class UpgradeUI : MonoBehaviour {
    
    [SerializeField]
    protected Image _Rays;
    
    [SerializeField]
    protected Image _RedBg;
    
    [SerializeField]
    protected Image _UpperText;
    
    [SerializeField]
    protected Image _BottomText;
    
    [SerializeField]
    protected Image _UpgradeIcon;
    
    [SerializeField] 
    protected TelezakupyUIObserver _Observer;

    private void Awake() {
        Reset();
    }
    
    private void OnEnable() {
        _Observer.SubscribePurchaseAttempted(OnPurchaseAttempted);
    }

    private void OnDisable() {
        _Observer.UnsubscribePurchaseAttempted(OnPurchaseAttempted);
    }
    
    private void OnPurchaseAttempted(PurchasableUpgrade upgrade, bool isSuccess) {
        if (isSuccess && upgrade != null) {
            AnimateWithIcon(upgrade);
        }
    }

    public void AnimateWithIcon(PurchasableUpgrade upgrade) {
        _UpgradeIcon.sprite = upgrade.UpgradeIcon;
        Animate();
    }
    
    [ContextMenu("Animate")]
    public void Animate() {
        DOTween
            .Sequence()
            .AppendCallback(() => {
                _Rays.gameObject.SetActive(true);
                _Rays.color = new Color(_Rays.color.r, _Rays.color.g, _Rays.color.b, 0.0f);
                _Rays.transform.localScale = Vector3.one * 0.3f;
                
                _RedBg.gameObject.SetActive(true);
                _RedBg.color = new Color(_RedBg.color.r, _RedBg.color.g, _RedBg.color.b, 0.0f);
                _RedBg.transform.localScale = Vector3.one * 3.0f;
                
                _UpperText.gameObject.SetActive(true);
                _UpperText.color = new Color(_UpperText.color.r, _UpperText.color.g, _UpperText.color.b, 0.0f);
                
                _BottomText.gameObject.SetActive(true);
                _BottomText.color = new Color(_BottomText.color.r, _BottomText.color.g, _BottomText.color.b, 0.0f);
            })
            .Insert(0.0f, _RedBg.transform
                .DOScale(Vector3.one, 0.6f)
                .SetEase(Ease.OutBack))
            .Insert(0.0f, _RedBg.DOFade(1.2f, 0.3f))
            .InsertCallback(1.1f, () => {
                _UpgradeIcon.gameObject.SetActive(true);
            })
            .Insert(0.6f, _RedBg.transform
                .DOScale(Vector3.one * 1.2f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo))
            .Insert(0.5f,
                DOTween
                    .Sequence()
                    .Insert(0.0f, _Rays.transform
                        .DOScale(Vector3.one, 1.0f)
                        .SetEase(Ease.OutExpo))
                    .Insert(0.0f, _Rays.DOFade(1.0f, 1.0f)))
            .Insert(1.5f,
                DOTween
                    .Sequence()
                    .Insert(0.0f, _UpperText.DOFade(1.0f, 0.2f)))
            .Insert(1.6f,
                DOTween
                    .Sequence()
                    .Insert(0.0f, _BottomText.DOFade(1.0f, 0.2f)))
            .InsertCallback(4.0f,
                () => {
                    Reset();
                })
            .SetId(this);
    }

    private void Update() {
        _Rays.transform.localRotation =
            Quaternion.Euler(
                _Rays.transform.localRotation.eulerAngles.x,
                _Rays.transform.localRotation.eulerAngles.y,
                _Rays.transform.localRotation.eulerAngles.z + 5f * Time.deltaTime
                );
    }

    private void Reset() {
        _UpgradeIcon.gameObject.SetActive(false);
        _Rays.gameObject.SetActive(false);
        _RedBg.gameObject.SetActive(false);
        _UpperText.gameObject.SetActive(false);
        _BottomText.gameObject.SetActive(false);
        DOTween.Kill(this);
    }
    
}
}