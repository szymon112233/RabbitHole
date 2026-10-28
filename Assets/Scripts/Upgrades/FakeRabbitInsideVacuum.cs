using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Upgrades
{
	public class FakeRabbitInsideVacuum : MonoBehaviour
	{
		[SerializeField] private float _maxRotationSpeed;
		[SerializeField] private  MeshRenderer _rabbitRenderer;
		[SerializeField] private Material _defaultMaterial;
		[SerializeField] private Material _goldenMaterial;
		
		private Rigidbody _rigidbody;
		
		public ERabbitType Type { get; private set; }

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void OnEnable()
		{
			_rigidbody.angularVelocity = Random.insideUnitSphere * _maxRotationSpeed;
		}

		public void SetType(ERabbitType type)
		{
			Type = type;
			
			switch (Type)
			{
				case ERabbitType.Golden:
					_rabbitRenderer.material = _goldenMaterial;
					break;
				
				default:
					_rabbitRenderer.material = _defaultMaterial;
					break;
			}
		}
	}
}