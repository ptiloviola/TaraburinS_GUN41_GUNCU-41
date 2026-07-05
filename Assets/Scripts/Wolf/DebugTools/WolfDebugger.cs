using System.Text;
using MeatMushrooms.Wolf.Components;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.DebugTools
{
    public class WolfDebugger : MonoBehaviour
    {
        private WolfBrain _brain;
        private WolfPerception _perception;
        private Camera _cam;

        [Inject]
        public void Construct(WolfBrain brain, WolfPerception perception)
        {
            _brain = brain;
            _perception = perception;
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private void OnGUI()
        {
            if (_cam == null || _brain == null || _perception == null) return;

            Vector3 worldPos = transform.position + Vector3.up * 2.5f;
            Vector3 screenPos = _cam.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<color=#00FF00><b>[{_brain.CurrentStateName}]</b></color>");
            sb.AppendLine($"Suspicion: <b>{_perception.CurrentSuspicion:F1}</b>");
            sb.AppendLine("--- Scores ---");
            
            foreach (var kvp in _brain.StateScores)
            {
                if (kvp.Key == _brain.CurrentStateName)
                    sb.AppendLine($"<color=yellow>{kvp.Key}: {kvp.Value:F1}</color>");
                else
                    sb.AppendLine($"<color=#B0B0B0>{kvp.Key}: {kvp.Value:F1}</color>");
            }

            string debugText = sb.ToString();

            GUIStyle style = new GUIStyle();
            style.richText = true;
            style.fontSize = 14;
            style.alignment = TextAnchor.UpperCenter;

            Rect rect = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 100, 200, 200);
            
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), debugText, style);
            
            style.normal.textColor = Color.white;
            GUI.Label(rect, debugText, style);
        }
    }
}