using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
[Serializable]
[CreateAssetMenu(fileName = "TelezakupyConfig", menuName = "RabbitHole/TVMango/TelezakupyConfig")]
public class TelezakupyConfig : ScriptableObject {
    
    [SerializeField]
    protected List<PurchasableUpgrade> _InitialUpgrades;
    
    [SerializeField] 
    protected List<VideoClip> _NoProgramm;
    
    [SerializeField]
    protected TelezakupyPlayerState _TPlayerState;

    private int _NoProgramId = 0;
    
    public VideoClip NoProgramm {
        get {
            _NoProgramId++;
            return _NoProgramm[_NoProgramId % _NoProgramm.Count];
        }
    }

    public List<PurchasableUpgrade> GetPurchasedUpgrades() {
        var result = new List<PurchasableUpgrade>();

        Queue<PurchasableUpgrade> queue = new Queue<PurchasableUpgrade>(_InitialUpgrades);

        while (queue.Count > 0) {
            var test = queue.Dequeue();

            if (_TPlayerState.IsUpgradePurchased(test.ID)) {
                result.Add(test);
                test.Unlocks.ForEach(x => queue.Enqueue(x));
            }
        }
        
        return result;
    }
    
    public List<PurchasableUpgrade> GetAvailableUpgrades() {
        var result = _InitialUpgrades.Where(x => !_TPlayerState.IsUpgradePurchased(x)).ToList();
        
        GetPurchasedUpgrades()
            .ForEach(x => {
                x.Unlocks.ForEach(u => {
                    if (_TPlayerState.IsUpgradePurchased(u)) return;
                    if (result.Exists(y => y.ID == u.ID))
                        return;
                    
                    result.Add(u);
                });
            });
        
        return result.ToList();
    }
    
}
}