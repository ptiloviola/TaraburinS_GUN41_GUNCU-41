using System.Collections;
using UnityEngine;

namespace Netologia.Homework
{
	public class Player : MonoBehaviour
	{
		private bool _ready;
		private Rigidbody _ball;
		
		[SerializeField]
		private Rigidbody _ballPrefab;
		[SerializeField]
		private float _startVelocity;
		[SerializeField]
		private float _lifetime;

		[SerializeField]
		private float _respawnDelay;

		private void Update()
		{
			if (!_ready) return;
			if (Input.GetKey(KeyCode.Space))
			{
				StartCoroutine(Reloader());
				_ball.isKinematic = false;
				_ball.transform.parent = null;
				_ball.velocity = transform.forward * _startVelocity;
				Destroy(_ball.gameObject, _lifetime);
			}
		}

		private IEnumerator Reloader()
		{
			_ready = false;
			yield return new WaitForSeconds(_respawnDelay);
			Spawn();
		}

		private void Spawn()
		{
			_ball = Instantiate(_ballPrefab, transform);
			//Вопрос: префаб мяча (скейл 1:1:1) при спауне изменял свою форму по скейлу объекта Player.
			// пришлось сделать этот костыль, чтобы префаб мяча не скейлился по родителю
			// либо можно было в иерархии создать корневой объект для player со скейлом 1:1:1
			// не очень понятно, почему на видео готового задания этой проблемы не наблюдается.
			_ball.transform.localScale = new Vector3(
				1f / transform.lossyScale.x,
				1f / transform.lossyScale.y,
				1f / transform.lossyScale.z
			);


			_ball.isKinematic = true;
			_ready = true;
		}

		private void Start()
		{
			Spawn();
		}
	}
}