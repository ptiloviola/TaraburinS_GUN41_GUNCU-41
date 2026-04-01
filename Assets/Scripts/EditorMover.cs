using UnityEngine;

namespace DefaultNamespace
{
	
	[RequireComponent(typeof(PositionSaver))]
	public class EditorMover : MonoBehaviour
	{
		private PositionSaver _save;
		private float _currentDelay;
		
		//todo comment: Что произойдёт, если _delay > _duration?
		// ! мы не сможем сохранить траекторию, т.к. интервалы измерения больше всего времени измерения

		[SerializeField][Range(0.2f, 1.0f)]
		private float _delay = 0.5f;

		[SerializeField][Min(0.2f)]
		private float _duration = 5f;

		private void Start()
		{
			if (_duration <= _delay) {
				_duration = _delay * 5;
			}
			//todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
			// ! при выполнении в методе Update, поиск будет вызываться постоянно, 
			// что является пустой тратой ресурсов. Этот компонент не меняется и его достаточно прочитать один раз
			_save = GetComponent<PositionSaver>();
			_save.Records.Clear();
		}

		private void Update()
		{
			_duration -= Time.deltaTime;
			if (_duration <= 0f)
			{
				enabled = false;
				Debug.Log($"<b>{name}</b> finished", this);
				return;
			}
			
			//todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
			// ! в таком случаем мы начнем изменять _delay, а это наш постоянный интервал. мы потерям информацию о том, с каким интервалом мы хотим проводить измерения
			_currentDelay -= Time.deltaTime;
			if (_currentDelay <= 0f)
			{
				_currentDelay = _delay;
				_save.Records.Add(new PositionSaver.Data
				{
					Position = transform.position,
					//todo comment: Для чего сохраняется значение игрового времени?
					// ! для того, чтобы записать метки времени и воссоздать скорость перемещения объекта в ReplayMover
					Time = Time.time,
				});
			}
		}
	}
}