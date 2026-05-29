using UnityEngine;

public class MouseScanner : MonoBehaviour
{
    // Скорость, с которой маска будет догонять курсор (для плавности)
    public float smoothSpeed = 10f;

    void Update()
    {
        Debug.Log("Скрипт работает!");
        // 1. Получаем координаты мыши на экране
        Vector3 mouseScreenPosition = Input.mousePosition;
        
        // 2. Говорим камере перевести эти пиксели в координаты игрового мира
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        
        // 3. Жестко фиксируем Z, чтобы маска не улетела за камеру (в 2D это критично)
        mouseWorldPosition.z = transform.position.z;

        // 4. Плавно двигаем объект (на котором висит скрипт) к позиции мыши
        transform.position = Vector3.Lerp(transform.position, mouseWorldPosition, smoothSpeed * Time.deltaTime);
    }
}