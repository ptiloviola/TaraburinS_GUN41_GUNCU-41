using UnityEngine;
using DG.Tweening; // Подключаем DOTween

public class WaypointMover : MonoBehaviour
{
    [Header("Настройки путей")]
    public Transform[] path1; // Первый вариант маршрута
    public Transform[] path2; // Второй вариант маршрута
    public float baseDuration = 5f; // Базовое время прохождения всего пути

    [Header("Эффекты")]
    public ParticleSystem dustEffect; // Система частиц для пыли
    public MeshRenderer characterRenderer; // Рендерер для смены цвета
    public Color moveColor = Color.red; // Цвет во время движения

    private Tween _moveTween; // Ссылка на текущую анимацию движения
    private Color _originalColor;
    private Vector3 _originalScale;

    private void Start()
    {
        // Сохраняем исходные параметры персонажа
        if (characterRenderer != null) _originalColor = characterRenderer.material.color;
        _originalScale = transform.localScale;

        // Запускаем движение по первому пути по умолчанию
        MoveAlongPath(path1);
    }

    private void Update()
    {
        // ДОП. ЗАДАНИЕ: Управление скоростью с клавиатуры
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeSpeed(1.5f);  // Ускоряем в 1.5 раза
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeSpeed(0.5f); // Замедляем в 2 раза

        // ДОП. ЗАДАНИЕ: Выбор пути
        if (Input.GetKeyDown(KeyCode.Alpha1)) MoveAlongPath(path1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) MoveAlongPath(path2);
    }

    /// <summary>
    /// Основной метод запуска движения по пути с эффектами
    /// </summary>
    public void MoveAlongPath(Transform[] waypoints)
    {
        // Если уже движемся - убиваем старую анимацию перед запуском новой
        _moveTween?.Kill();
        transform.DOKill(); 
        transform.localScale = _originalScale;

        if (waypoints == null || waypoints.Length == 0) return;

        // Преобразуем Transform[] в Vector3[] для DOTween
        Vector3[] pathPositions = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            pathPositions[i] = waypoints[i].position;
        }

        // Включаем эффект пыли
        if (dustEffect != null) dustEffect.Play();

        // 1. АНИМАЦИЯ ЦВЕТА: Плавно меняем цвет на "боевой" (за 0.5 сек)
        if (characterRenderer != null)
        {
            characterRenderer.material.DOColor(moveColor, 0.5f);
        }

        // 2. АНИМАЦИЯ МАСШТАБА: Игрок забавно сплющивается и растягивается (PunchScale)
        // Параметры: сила эффекта, длительность, вибрация, эластичность
        transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0.2f), 0.5f, 5, 0.5f)
            .SetLoops(-1, LoopType.Yoyo); // Зацикливаем этот эффект навсегда

        // 3. ОСНОВНАЯ АНИМАЦИЯ ПУТИ: DOPath
        // PathType.CatmullRom делает углы скругленными (плавная кривая)
        _moveTween = transform.DOPath(pathPositions, baseDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine) // Плавный старт и плавная остановка
            .SetLookAt(0.05f) // Персонаж сам поворачивается лицом по направлению движения
            .SetLoops(-1, LoopType.Yoyo) // <-- Эта строчка заставит его бесконечно ходить туда-обратно как маятник
            .OnComplete(OnPathCompleted); // Вызов метода, когда дойдем до конца
    }

    /// <summary>
    /// Меняет множитель времени у текущего твина (влияет на скорость)
    /// </summary>
    public void ChangeSpeed(float timeScaleMultiplier)
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.timeScale = timeScaleMultiplier;
        }
    }

    /// <summary>
    /// Срабатывает при достижении последней точки пути
    /// </summary>
    private void OnPathCompleted()
    {
        // Выключаем пыль
        if (dustEffect != null) dustEffect.Stop();

        // Возвращаем исходный цвет
        if (characterRenderer != null)
        {
            characterRenderer.material.DOColor(_originalColor, 0.5f);
        }

        // Убиваем зацикленную анимацию масштаба и возвращаем к оригиналу
        transform.DOKill(); 
        transform.DOScale(_originalScale, 0.3f).SetEase(Ease.OutBounce);
        // === ДОБАВЛЯЕМ ЛОГИКУ ЗАЦИКЛИВАНИЯ ===
        // Перемещаем объект в позицию самой первой точки (индекс 0)
        transform.position = path1[0].position; 
        
        // Запускаем движение по первому пути заново
        MoveAlongPath(path1);
    }
}