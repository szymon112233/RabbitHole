using UnityEngine;

namespace DefaultNamespace
{
	public enum HandState
	{
		Idle,
		Grab
	}
	
	public class HandController : MonoBehaviour
	{
		// TODO - use animation??

		[SerializeField] private GameObject _hand1;
		[SerializeField] private GameObject _hand2;
		[SerializeField] private GameObject _grabHand;

		[SerializeField] private float _spriteShowDuration = 0.6f;

		private float _spriteChangeTimer;
		private HandState _currentState;

		private void Start()
		{
			_hand1.SetActive(true);
			_hand2.SetActive(false);
			_grabHand.SetActive(false);

			_spriteChangeTimer = _spriteShowDuration;
			_currentState = HandState.Idle;
		}

		private void Update()
		{
			if (_currentState != HandState.Idle)
			{
				return;
			}
			
			if (_spriteChangeTimer > 0f)
			{
				_spriteChangeTimer -= Time.deltaTime;
				
				return;
			}
			
			_spriteChangeTimer = _spriteShowDuration;
			
			_hand1.SetActive(!_hand1.activeSelf);
			_hand2.SetActive(!_hand2.activeSelf);
		}

		public void EnterGrabState()
		{
			_currentState = HandState.Grab;
			
			_hand1.SetActive(false);
			_hand2.SetActive(false);
			_grabHand.SetActive(true);
		}
		
		public void ExitGrabState()
		{
			_currentState = HandState.Idle;
			
			_hand1.SetActive(true);
			_hand2.SetActive(false);
			_grabHand.SetActive(false);
		}
	}
}