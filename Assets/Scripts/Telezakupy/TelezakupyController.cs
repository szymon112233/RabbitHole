using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
public class TelezakupyController : MonoBehaviour {

    [SerializeField]
    protected TelezakupyConfig  _TelezakupyConfig;
    
    [SerializeField]
    protected TelezakupyPlayerState  _PlayerState;

    [SerializeField] 
    protected VideoPlayer _VideoPlayer;

    [SerializeField]
    public LayerMask _TriggerLayers;
    
    [SerializeField]
    protected float _InteractionDistance = 2;
    
    [SerializeField]
    protected TelezakupyUIObserver _TelezakupyUIObserver;

    private int _LastReportedProgram = -1;
        [SerializeField]
        protected GameObject m_CTAText;

    private bool _WasIsInteractionReadz = false;
    
    public void SwitchProgram() {
        var availableUpgrades = _TelezakupyConfig.GetAvailableUpgrades();
        var totalProgramsCount = availableUpgrades.Count + 1;
        _PlayerState.CurrentProgram = (_PlayerState.CurrentProgram + 1 % totalProgramsCount);
        UpdateVideoProgram();
    }

    public void TryBuyUpgrade() {
        var currentUpgrade = CurrentlyAiringUpgrade();

        if (currentUpgrade == null)
            return;

        var hasMoney = GameManager.Instance.TrySpendMoney(currentUpgrade.Price);
        
        _TelezakupyUIObserver.RaisePurchaseAttempted(hasMoney);
        
        // register
        if(hasMoney) {
            currentUpgrade.RuntimeFields.Bought();

            _PlayerState.SavePurchasedUpgrade(currentUpgrade);
            _PlayerState.CurrentProgram = 1;
            UpdateVideoProgram();

            var curr = CurrentlyAiringUpgrade();
            _TelezakupyUIObserver.RaiseUpgradesForPurchase(curr);
            
        }
    }

        private void CanBuyUpgrade()
        {
            bool canBuyAnyUpgrade = false;
            var availableUpgrades = _TelezakupyConfig.GetAvailableUpgrades();
            for (int i = 0; i < availableUpgrades.Count; i++)
            {
                if (GameManager.Instance.HasMoney(availableUpgrades[i].Price))
                {
                    canBuyAnyUpgrade = true;
                    break;
                }
            }

            m_CTAText.SetActive(canBuyAnyUpgrade);
        }
        private PurchasableUpgrade CurrentlyAiringUpgrade() {
        var availableUpgrades = _TelezakupyConfig.GetAvailableUpgrades();
        var totalProgramsCount = availableUpgrades.Count + 1;
        var currProgramId = _PlayerState.CurrentProgram % totalProgramsCount;

        if (currProgramId == 0) {
            return null;
        }
        else {
            return availableUpgrades[currProgramId - 1];
        }
    }

    private void UpdateVideoProgram() {
        var currProgram = CurrentlyAiringUpgrade();

        _PlayerState.CurrentlyAiringUpgrade = currProgram;
        
        _VideoPlayer.clip = currProgram == null 
            ? _TelezakupyConfig.NoProgramm
            : currProgram.AdClip;
        
        if (currProgram == null)
        {
            GameManager.Instance?.mouseUI.SetTVnoBuy();
        }
        else
        {
            GameManager.Instance?.mouseUI.SetTV();
        }
    }

    private void Start() {
        _PlayerState.Clear();
        SwitchProgram();
            m_CTAText.SetActive(false);
            GameManager.Instance.OnMoneyStateChange += CanBuyUpgrade;
        }

    private void Update() {
        if(GameManager.Instance.DebugEnabled)
        {
            foreach(PurchasableUpgrade upgrade in _TelezakupyConfig.GetAvailableUpgrades())
            {
                upgrade.RuntimeFields.Bought();
                _PlayerState.SavePurchasedUpgrade(upgrade);

            }
        }
    // 1. Define the origin and direction (e.g., forward from the camera or player)
    Vector3 origin = Camera.main.transform.position;
    Vector3 direction = Camera.main.transform.forward;
    var isInteractionReady = false;
    
    // 2. Perform the Raycast forcing it to collide with Triggers
    if (Physics.Raycast(origin, direction, out RaycastHit hit, _InteractionDistance, _TriggerLayers, QueryTriggerInteraction.Collide))
    {
        
        // 3. Double-check if the hit collider is actually a trigger
        if (hit.collider.isTrigger) {
            isInteractionReady = true;

            GameManager.Instance.RabbitHoldController.ResetToolToHand();    
            // Put your interaction or UI highlighting logic here
        }
    }

    if (isInteractionReady != _WasIsInteractionReadz) {
        _WasIsInteractionReadz = isInteractionReady;
        _TelezakupyUIObserver.RaiseCanSwitchPrograms(isInteractionReady);
    }
    
    var newProgram = isInteractionReady ? _PlayerState.CurrentProgram : -1;
    if (newProgram != _LastReportedProgram) {
        _LastReportedProgram = newProgram;
        
        _TelezakupyUIObserver.RaiseUpgradesForPurchase(_LastReportedProgram < 0 
            ? null
            : CurrentlyAiringUpgrade());
    }

    if (isInteractionReady) {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            _TelezakupyUIObserver.RaiseProgramSwitched();
            SwitchProgram();
        }
        
        if (Mouse.current.rightButton.wasPressedThisFrame) {
            TryBuyUpgrade();
        }
    }
}
}
}