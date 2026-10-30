using UnityEngine;

namespace DefaultNamespace
{
	public class SoundManager : MonoBehaviour
	{
		[SerializeField] private AudioSource _cashSoundsSource;
		[SerializeField] private AudioSource _punchSoundsSource;

		public void PlayCashMoneySound()
		{
			_cashSoundsSource.Play();
		}

		public void PlayPunchSound()
		{
			_punchSoundsSource.Play();
		}
	}
}