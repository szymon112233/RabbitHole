using System;

namespace UnityEngine.Rendering.Universal {

[CreateAssetMenu(fileName = "TelezakupyUIObserver", menuName = "RabbitHole/TVMango/TelezakupyUIObserver")]
public class TelezakupyUIObserver : ScriptableObject {
    
    private Action<PurchasableUpgrade> _OnUpgradeAvailableForPurchase;
    
    private Action<bool> _CanSwitchPrograms;
    
    private Action _ProgramSwitched;
    
    private Action _PurchaseAttempted;

    // Call this method to trigger the event
    public void RaiseUpgradesForPurchase(PurchasableUpgrade purchasableUpgrade) {
        _OnUpgradeAvailableForPurchase?.Invoke(purchasableUpgrade);
    }
    
    public void RaiseProgramSwitched() {
        _ProgramSwitched?.Invoke();
    }

    public void RaisePurchaseAttempted() {
        _PurchaseAttempted?.Invoke();
    }

    // Public methods handling subscription safety
    public void SubscribeUpgradesForPurchase(Action<PurchasableUpgrade> listener) {
        _OnUpgradeAvailableForPurchase += listener;
    }

    public void UnsubscribeUpgradesForPurchase(Action<PurchasableUpgrade> listener) {
        _OnUpgradeAvailableForPurchase -= listener;
    }
    
    // Call this method to trigger the event
    public void RaiseCanSwitchPrograms(bool can) {
        _CanSwitchPrograms?.Invoke(can);
    }

    // Public methods handling subscription safety
    public void SubscribeCanSwitchPrograms(Action<bool> listener) {
        _CanSwitchPrograms += listener;
    }

    public void UnsubscribeCanSwitchPrograms(Action<bool> listener) {
        _CanSwitchPrograms -= listener;
    }
    
    public void SubscribeProgramSwitched(Action listener) {
        _ProgramSwitched += listener;
    }

    public void UnsubscribeProgramSwitched(Action listener) {
        _ProgramSwitched -= listener;
    }
    
    public void SubscribePurchaseAttempted(Action listener) {
        _PurchaseAttempted += listener;
    }

    public void UnsubscribePurchaseAttempted(Action listener) {
        _PurchaseAttempted -= listener;
    }
}
}