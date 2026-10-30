using System.Collections.Generic;
using DefaultNamespace.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DefaultNamespace
{
	public enum HandState
	{
		Idle,
		Grab,
		TVRemote,
	}
	
	public class HandController : MonoBehaviour
	{
		
		[SerializeField] 
		protected TelezakupyUIObserver _Observer;

		[SerializeField] 
		protected TelezakupyPlayerState _PlayerState;
		
		// TODO - use animation??

		[SerializeField] private GameObject _hand1;
		[SerializeField] private GameObject _hand2;
		[SerializeField] private GameObject _grabHand;
		[SerializeField] private GameObject _TVRemote;
		[SerializeField] private GameObject _TVRemotePressed;
		[SerializeField] private GameObject _BuyPhone;
		[SerializeField] private GameObject _BuyPhonePressed;
		[SerializeField] private GameObject _LegKick;
		[SerializeField] private List<Shaker> _NoMoneyShaker;

		[SerializeField] private float _spriteShowDuration = 0.6f;

		private float _spriteChangeTimer;
		private HandState _currentState;
		private bool _HasGrabState;
		private bool _CanSwitchPrograms;
		private float _ProgramSwitchedTimer;
		private float _PurchaseAttemptedTimer;
		
		private void Start()
		{
			_spriteChangeTimer = _spriteShowDuration;
			ChangeState(HandState.Idle);
			_LegKick.SetActive(false);
		}

		private void OnEnable() {
			_Observer.SubscribeCanSwitchPrograms(OnCanSwitchPrograms);
			_Observer.SubscribeProgramSwitched(OnProgramSwitched);
			_Observer.SubscribePurchaseAttempted(OnPurchaseAttempted);
		}

		private void OnDisable() {
			_Observer.UnsubscribeCanSwitchPrograms(OnCanSwitchPrograms);
			_Observer.UnsubscribeProgramSwitched(OnProgramSwitched);
			_Observer.UnsubscribePurchaseAttempted(OnPurchaseAttempted);
		}
		
		private void OnCanSwitchPrograms(bool canSwitch) {
			_CanSwitchPrograms = canSwitch;
		}
		
		private void OnProgramSwitched() {
			_ProgramSwitchedTimer = 0.1f;
		}

		private void OnPurchaseAttempted(bool isSuccess) {
			_PurchaseAttemptedTimer = 0.1f;
			
			if(!isSuccess)
				_NoMoneyShaker.ForEach(x => x.Shake());
		}

		private void Update() {
			var newState = HandState.Idle;
			
			// switching programs has a priority
			if (_CanSwitchPrograms)
				newState = HandState.TVRemote;
			else if (_HasGrabState) 
				newState = HandState.Grab;

			if (newState != _currentState) {
				ChangeState(newState);
			}
				
			switch (_currentState) {
				case HandState.Idle:
					if (_spriteChangeTimer > 0f)
					{
						_spriteChangeTimer -= Time.deltaTime;
				
						return;
					}
			
					_spriteChangeTimer = _spriteShowDuration;
			
					_hand1.SetActive(!_hand1.activeSelf);
					_hand2.SetActive(!_hand2.activeSelf);
					break;
				case HandState.TVRemote:
					if (_ProgramSwitchedTimer > 0) {
						_ProgramSwitchedTimer -= Time.deltaTime;
						
						_TVRemotePressed.gameObject.SetActive(true);
						_TVRemote.gameObject.SetActive(false);
					}
					else {
						_TVRemotePressed.gameObject.SetActive(false);
						_TVRemote.gameObject.SetActive(true);
					}

					_BuyPhone.gameObject.SetActive(false);
					_BuyPhonePressed.gameObject.SetActive(false);
					if (_PlayerState.CurrentlyAiringUpgrade != null) {
						if (_PurchaseAttemptedTimer > 0f) {
							_PurchaseAttemptedTimer -= Time.deltaTime;
							_BuyPhonePressed.gameObject.SetActive(true);
						}
						else {
							_BuyPhone.gameObject.SetActive(true);
						}
					}
					
					break;
			}
			
		}

		public void EnterGrabState() 
		{
			_HasGrabState = true;
			ChangeState(HandState.Grab);
		}
		
		public void ExitGrabState() 
		{
			_HasGrabState = false;
			ChangeState(HandState.Idle);
		}

		public void EnableKick()
        {
			_LegKick.SetActive(true);
		}

		public void DisableKick()
		{
			_LegKick.SetActive(false);
		}

		private void ChangeState(HandState newState) {
			_currentState = newState;
			_hand1.SetActive(false);
			_hand2.SetActive(false);
			_grabHand.SetActive(false);
			_TVRemote.SetActive(false);
			_TVRemotePressed.SetActive(false);
			_BuyPhone.SetActive(false);
			_BuyPhonePressed.SetActive(false);
			
			switch (newState) {
				case HandState.Idle:
					_hand1.SetActive(true);
					GameManager.Instance.mouseUI.SetEmptyHand();
					break;
				case HandState.Grab:
					_grabHand.SetActive(true);
					break;
				case HandState.TVRemote:
					_TVRemote.gameObject.SetActive(true);
					GameManager.Instance.mouseUI.SetTV();
					break;
			}
		}
	}
}