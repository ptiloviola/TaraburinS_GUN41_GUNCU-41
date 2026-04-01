using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(PositionSaver))]
	public class ReplayMover : MonoBehaviour
	{
		private PositionSaver _save;

		private int _index;
		private PositionSaver.Data _prev;
		private float _duration;

		private void Start()
		{
			////todo comment: зачем нужны эти проверки?
			/// ! так мы выясняем, что данных для воспроизведения нет: либо нет компонента с данными либо количество точек равно нулю
			if (!TryGetComponent(out _save) || _save.Records.Count == 0)
			{
				Debug.LogError("Records incorrect value", this);
				//todo comment: Для чего выключается этот компонент?
				// ! прекращаем вызов метода Update() таким образом, экономя ресурсы
				enabled = false;
			}
		}

		private void Update()
		{
			var curr = _save.Records[_index];
			//todo comment: Что проверяет это условие (с какой целью)? 
			// ! ждем пока текущее время в игре не станет больше записанного в текущей точке, чтобы перейти к следующей точке
			if (Time.time > curr.Time)
			{
				_prev = curr;
				_index++;
				//todo comment: Для чего нужна эта проверка?
				// ! проверям, что закончили обход всех записанных в Records точке, выключаем компонент и делаем запись в лог
				if (_index >= _save.Records.Count)
				{
					enabled = false;
					Debug.Log($"<b>{name}</b> finished", this);
				}
			}
			//todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
			// ! вычисляем процент(0-1) пройденного пути, чтобы потом получить плавную интерполяцию в Vector3.Lerp(_prev.Position, curr.Position, delta)
			var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
			//todo comment: Зачем нужна эта проверка?
			// ! если curr.Time == _prev.Time, то будет деление на ноль, видимо, это нужно для защиты от этого
			if (float.IsNaN(delta)) delta = 0f;
			//todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
			// ! линейная интерполяция, позволяющая добиться плавного перемещения объекта. return _prev + (curr - _prev) * Clamp01(delta); учитывает промежуточные положение (delta), 
			// чтобы двигаться плавно, а не телепортироваться
			transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
		}
	}
}