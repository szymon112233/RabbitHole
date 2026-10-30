using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.Universal {
[Serializable]
[CreateAssetMenu(fileName = "TelezakupyConfig", menuName = "RabbitHole/TVMango/TelezakupyConfig")]
public class TelezakupyConfig : ScriptableObject {
    
    [SerializeField]
    protected List<PurchasableUpgrade> _InitialUpgrades;
    
    [SerializeField, Tooltip("Video file names inside Assets/StreamingAssets/Video")]
    protected List<string> _NoProgrammVideos;
    
    [SerializeField]
    protected TelezakupyPlayerState _TPlayerState;

    private int _NoProgramId = 0;
    
    /// <summary>Cycles through the "no programme" filler videos. Returns a StreamingAssets/Video file name.</summary>
    public string NoProgramm {
        get {
            if (_NoProgrammVideos == null || _NoProgrammVideos.Count == 0)
                return null;
            _NoProgramId++;
            return _NoProgrammVideos[_NoProgramId % _NoProgrammVideos.Count];
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