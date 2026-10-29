using UnityEngine;

namespace DefaultNamespace
{
	public class SoundManager : MonoBehaviour
	{
		[SerializeField] private AudioSource _cashSoundsSource;

		public void PlayCashMoneySound()
		{
			_cashSoundsSource.Play();
		}
	}
}