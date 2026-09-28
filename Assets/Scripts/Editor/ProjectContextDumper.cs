using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System.Linq;

namespace Gameplay.Tools
{
    public class ProjectContextDumper : UnityEditor.Editor
    {
        [MenuItem("Tools/TD/Скопировать контекст проекта")]
        public static void CopyProjectContext()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== КОНТЕКСТ ПРОЕКТА TOWER DEFENSE ===");


            sb.AppendLine("\n--- 1. СТРУКТУРА СКРИПТОВ (Assets/Scripts) ---");
            string scriptsPath = Application.dataPath + "/Scripts";
            if (Directory.Exists(scriptsPath)) DumpDirectory(scriptsPath, sb, 0, "*.cs", "📄");
            else sb.AppendLine("Папка Assets/Scripts не найдена!");

            sb.AppendLine("\n--- 2. ИЕРАРХИЯ СЦЕНЫ ---");
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                DumpGameObject(root.transform, sb, 0);
            }

            sb.AppendLine("\n--- 3. ПРЕФАБЫ (Assets/Prefabs) ---");
            string prefabsPath = Application.dataPath + "/Prefabs";
            if (Directory.Exists(prefabsPath)) DumpPrefabsDirectory(prefabsPath, sb, 0);
            else sb.AppendLine("Папка Assets/Prefabs не найдена!");

            GUIUtility.systemCopyBuffer = sb.ToString();
            
            Gameplay.Tools.GameLogger.Log($"<color=green>[TD Tools] Контекст скопирован!</color> Длина текста: {sb.Length} символов.");
            if (sb.Length > 50000)
            {
                Gameplay.Tools.GameLogger.LogWarning("[TD Tools] Внимание: текст получился очень большим! Возможно, придется отправлять его в чат двумя сообщениями.");
            }
        }

        private static void DumpDirectory(string path, StringBuilder sb, int depth, string extension, string icon)
        {
            string indent = new string(' ', depth * 2);
            DirectoryInfo dir = new DirectoryInfo(path);
            sb.AppendLine($"{indent}📁 {dir.Name}");

            foreach (FileInfo file in dir.GetFiles(extension))
            {
                sb.AppendLine($"{indent}  {icon} {file.Name}");
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                DumpDirectory(subDir.FullName, sb, depth + 1, extension, icon);
            }
        }

        private static void DumpGameObject(Transform node, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            
            var components = node.GetComponents<Component>()
                .Where(c => c != null && c.GetType() != typeof(Transform))
                .Select(c => c.GetType().Name);
            
            string compString = string.Join(", ", components);
            string compOutput = string.IsNullOrEmpty(compString) ? "" : $" [{compString}]";
            
            sb.AppendLine($"{indent}📦 {node.name}{compOutput}");

            foreach (Transform child in node)
            {
                DumpGameObject(child, sb, depth + 1);
            }
        }

        private static void DumpPrefabsDirectory(string path, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            DirectoryInfo dir = new DirectoryInfo(path);
            sb.AppendLine($"{indent}📁 {dir.Name}");

            foreach (FileInfo file in dir.GetFiles("*.prefab"))
            {
                
                string assetPath = "Assets" + file.FullName.Substring(Application.dataPath.Length).Replace('\\', '/');
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                if (prefab != null)
                {
                    var components = prefab.GetComponentsInChildren<Component>(true)
                        .Where(c => c != null && c.GetType() != typeof(Transform))
                        .Select(c => c.GetType().Name)
                        .Distinct(); 
                    
                    string compString = string.Join(", ", components);
                    string compOutput = string.IsNullOrEmpty(compString) ? "" : $" [{compString}]";
                    
                    sb.AppendLine($"{indent}  🟦 {file.Name}{compOutput}");
                }
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                DumpPrefabsDirectory(subDir.FullName, sb, depth + 1);
            }
        }
    }
}