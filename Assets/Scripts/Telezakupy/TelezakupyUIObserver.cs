using System;

namespace UnityEngine.Rendering.Universal {

[CreateAssetMenu(fileName = "TelezakupyUIObserver", menuName = "RabbitHole/TVMango/TelezakupyUIObserver")]
public class TelezakupyUIObserver : ScriptableObject {
    
    private Action<PurchasableUpgrade> _OnUpgradeAvailableForPurchase;
    
    private Action<bool> _CanSwitchPrograms;
    
    private Action _ProgramSwitched;
    
    private Action<PurchasableUpgrade, bool> _PurchaseAttempted;

    // Call this method to trigger the event
    public void RaiseUpgradesForPurchase(PurchasableUpgrade purchasableUpgrade) {
        _OnUpgradeAvailableForPurchase?.Invoke(purchasableUpgrade);
    }
    
    public void RaiseProgramSwitched() {
        _ProgramSwitched?.Invoke();
    }

    public void RaisePurchaseAttempted(PurchasableUpgrade upgrade, bool isSuccess) {
        _PurchaseAttempted?.Invoke(upgrade, isSuccess);
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
    
    public void SubscribePurchaseAttempted(Action<PurchasableUpgrade, bool> listener) {
        _PurchaseAttempted += listener;
    }

    public void UnsubscribePurchaseAttempted(Action<PurchasableUpgrade, bool> listener) {
        _PurchaseAttempted -= listener;
    }
}
}