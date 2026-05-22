using Gameplay.Grid;
using UnityEngine;
using Zenject;

namespace Gameplay.Tools
{
    public class DebugCameraFitter : MonoBehaviour
    {
        private GridGenerator _gridGenerator;

        // Zenject автоматически найдет этот метод при старте сцены 
        // и прокинет ссылку на генератор
        [Inject]
        public void Construct(GridGenerator gridGenerator)
        {
            _gridGenerator = gridGenerator;
        }

        private void Start()
        {
            if (_gridGenerator.EditorConfig == null) return;

            // 1. Узнаем реальные физические размеры сетки в юнитах Unity
            float realWidth = _gridGenerator.EditorConfig.width * _gridGenerator.Spacing;
            float realHeight = _gridGenerator.EditorConfig.height * _gridGenerator.Spacing;

            // 2. Вычисляем координаты центра. 
            // Отнимаем половину Spacing, так как кубы растут от 0,0 в положительную сторону
            float centerX = (realWidth / 2f) - (_gridGenerator.Spacing / 2f);
            float centerZ = (realHeight / 2f) - (_gridGenerator.Spacing / 2f);
            Vector3 centerPoint = new Vector3(centerX, 0f, centerZ);

            // 3. Вычисляем масштаб для отлета камеры. 
            // Берем самую длинную сторону, чтобы прямоугольная сетка точно влезла в кадр.
            float maxDimension = Mathf.Max(realWidth, realHeight);
            
            // 4. Позиционируем камеру (здесь заданы комфортные углы для TD)
            float heightOffset = maxDimension * 0.8f; // Высота подъема камеры
            float backOffset = maxDimension * 0.4f;   // Сдвиг назад (для изометрии)

            transform.position = centerPoint + new Vector3(0f, heightOffset, -backOffset);
            
            // 5. Направляем фокус ровно в вычисленный центр
            transform.LookAt(centerPoint);
            
            Debug.Log($"<color=gray>[DebugCameraFitter] Камера сфокусирована на центре {centerPoint}</color>");
        }
    }
}