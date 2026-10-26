using UnityEngine;

namespace DefaultNamespace
{
	public class HandController : MonoBehaviour
	{
		// TODO - use animation??

		[SerializeField] private GameObject _hand1;
		[SerializeField] private GameObject _hand2;

		[SerializeField] private float _spriteShowDuration = 0.6f;

		private float _spriteChangeTimer;

		private void Start()
		{
			_hand1.SetActive(true);
			_hand2.SetActive(false);

			_spriteChangeTimer = _spriteShowDuration;
		}

		private void Update()
		{
			if (_spriteChangeTimer > 0f)
			{
				_spriteChangeTimer -= Time.deltaTime;
				
				return;
			}
			
			_spriteChangeTimer = _spriteShowDuration;
			
			_hand1.SetActive(!_hand1.activeSelf);
			_hand2.SetActive(!_hand2.activeSelf);
		}
	}
}