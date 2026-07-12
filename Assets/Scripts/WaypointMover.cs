using UnityEngine;
using DG.Tweening; // Подключаем DOTween

public class WaypointMover : MonoBehaviour
{
    [Header("Настройки путей")]
    public Transform[] path1;
    public Transform[] path2;
    public float baseDuration = 5f;

    [Header("Эффекты")]
    public ParticleSystem dustEffect; 
    public MeshRenderer characterRenderer;
    public Color moveColor = Color.red;

    private Tween _moveTween;
    private Color _originalColor;
    private Vector3 _originalScale;

    private void Start()
    {

        if (characterRenderer != null) _originalColor = characterRenderer.material.color;
        _originalScale = transform.localScale;

        MoveAlongPath(path1);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeSpeed(1.5f);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeSpeed(0.5f);


        if (Input.GetKeyDown(KeyCode.Alpha1)) MoveAlongPath(path1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) MoveAlongPath(path2);
    }

    public void MoveAlongPath(Transform[] waypoints)
    {

        _moveTween?.Kill();
        transform.DOKill(); 
        transform.localScale = _originalScale;

        if (waypoints == null || waypoints.Length == 0) return;

        Vector3[] pathPositions = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            pathPositions[i] = waypoints[i].position;
        }


        if (dustEffect != null) dustEffect.Play();


        if (characterRenderer != null)
        {
            characterRenderer.material.DOColor(moveColor, 0.5f);
        }

        transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0.2f), 0.5f, 5, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);

        _moveTween = transform.DOPath(pathPositions, baseDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine) 
            .SetLookAt(0.05f)
            .SetLoops(-1, LoopType.Yoyo)
            .OnComplete(OnPathCompleted);
    }

    public void ChangeSpeed(float timeScaleMultiplier)
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.timeScale = timeScaleMultiplier;
        }
    }

    private void OnPathCompleted()
    {

        if (dustEffect != null) dustEffect.Stop();


        if (characterRenderer != null)
        {
            characterRenderer.material.DOColor(_originalColor, 0.5f);
        }


        transform.DOKill(); 
        transform.DOScale(_originalScale, 0.3f).SetEase(Ease.OutBounce);
        transform.position = path1[0].position; 
        

        MoveAlongPath(path1);
    }
}