using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using VacuumSim.Robotics.Signals;
using VacuumSim.Pathfinding;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Input
{
    public class PlayerInputHandler : ITickable
    {
        private readonly SignalBus _signalBus;
        private readonly PathfindingGrid _grid;
        private bool _isWaitingForClick;
        private int _frameWhenEnabled;

        private GameObject _markerInstance;
        private Material _markerMaterial;

        public PlayerInputHandler(SignalBus signalBus, PathfindingGrid grid)
        {
            _signalBus = signalBus;
            _grid = grid;
            CreateMarker();
        }

        private void CreateMarker()
        {
            // ТОТ САМЫЙ код из первого варианта, который работал!
            _markerInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(_markerInstance.GetComponent<Collider>());
            _markerInstance.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f); 
            
            _markerMaterial = new Material(Shader.Find("Sprites/Default"));
            _markerInstance.GetComponent<Renderer>().material = _markerMaterial;
            
            _markerInstance.SetActive(false);
        }

        public void EnableTargetSelection()
        {
            _isWaitingForClick = true;
            _frameWhenEnabled = Time.frameCount; // Защита от мгновенного клика
            
            _markerInstance.SetActive(true);
            SetMarkerColor(Color.yellow);
        }

        public void Tick()
        {
            if (!_isWaitingForClick) return;

            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            Plane dynamicFloorPlane = new Plane(Vector3.up, _grid.transform.position);

            if (dynamicFloorPlane.Raycast(ray, out float distance))
            {
                // Идеальная точка на плоскости
                Vector3 hitPoint = ray.GetPoint(distance);
                // Идеальная ячейка сетки
                Node node = _grid.NodeFromWorldPoint(hitPoint);

                if (node != null)
                {
                    // РАЗГАДКА ЗДЕСЬ: X и Z берем от ячейки (Снаппинг), а Y берем от плоскости!
                    _markerInstance.transform.position = new Vector3(node.WorldPosition.x, hitPoint.y + 1.2f, node.WorldPosition.z);

                    bool isValid = node.IsWalkable;
                    SetMarkerColor(isValid ? Color.yellow : Color.red);

                    if (UnityEngine.Input.GetMouseButtonDown(0) && Time.frameCount > _frameWhenEnabled)
                    {
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
                            return;

                        if (isValid)
                        {
                            _isWaitingForClick = false;
                            
                            AnimateAndHideMarker().Forget(); // Та самая плавная анимация из 1 версии
                            _signalBus.Fire(new TargetPointSelectedSignal { Point = node.WorldPosition });
                        }
                    }
                }
            }
        }

        private void SetMarkerColor(Color color)
        {
            color.a = 0.6f;
            _markerMaterial.color = color;
        }

        private async UniTaskVoid AnimateAndHideMarker()
        {
            SetMarkerColor(Color.green);
            float duration = 1.0f;
            float time = 0;
            Color startColor = _markerMaterial.color;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(0.6f, 0f, time / duration);
                Color c = startColor;
                c.a = alpha;
                _markerMaterial.color = c;
                await UniTask.Yield();
            }
            
            _markerInstance.SetActive(false);
        }
    }
}