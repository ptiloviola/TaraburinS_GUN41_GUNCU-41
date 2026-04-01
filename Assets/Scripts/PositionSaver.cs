using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
	public class PositionSaver : MonoBehaviour
	{

		[Serializable]
		public struct Data
		{
			public Vector3 Position;
			public float Time;
		}

		[SerializeField][ReadOnly][Tooltip("Use Context Menu and Create File")]
		private TextAsset _json;



		[field: SerializeField][HideInInspector]
		public List<Data> Records { get; private set; }

		[Serializable]
		public class SaveData
		{
			public List<Data> Records;
		}

		private void Awake()
		{
			//todo comment: Что будет, если в теле этого условия не сделать выход из метода?
			// ! мы будем тщетно пытаться загрузить отсутствующие данные. выпадет ошибка
			if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}
			
			// ! пришлось переделать через обёртку, ибо в противном случае сериализировалось некорректно
			var loadedData = JsonUtility.FromJson<SaveData>(_json.text);
			if (loadedData != null)
			{
				Records = loadedData.Records;
			}

			// JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			// ! так мы инициализируем список Records, что позволит избежать ошибок NullReferenceException, и позволит добавлять значения в список
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
			//todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
			// ! если у нас нет записей, то и отрисовывать нечего, позволит избежать связанных с этим ошибок, например, невозможность обратиться к data[0].Position;

			if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
			//todo comment: Почему итерация начинается не с нулевого элемента?
			// ! нулевой элемент уже пройден в var prev = data[0].Position;
			for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}
		
#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
			//todo comment: Что происходит в этой строке?
			// Создается файл "Path.txt" в корне проекта
			var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
			//todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
			// ! Освобожадаем занятый потоком ресурс "Path.txt" для дальнейшего использования
			stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				//todo comment: Для чего нужны эти проверки?
				// ! чтобы найти то, что ищем, т.к. UnityEditor.AssetDatabase.FindAssets("t:TextAsset") даст нам все текстовые файлы
				if(asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
					//todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
					// ! уже нашли, незачем продолжать искать, тратя ресурсы
					return;
				}
			}
		}

		private void OnDestroy()
		{
			if (_json == null || Records == null)
			{
				return;
			}
			var saveData = new SaveData
			{
				Records = Records
			};
			var json = JsonUtility.ToJson(saveData, true);
			var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_json);
			var fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
			File.WriteAllText(fullPath, json);
			UnityEditor.AssetDatabase.Refresh();
			Debug.Log($"Saved JSON to: {fullPath}\n{json}");
		}
#endif
	}
}