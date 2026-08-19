using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ArchitectureExporter
{
    // Список компонентов, которые не несут архитектурной ценности для отчета
    private static readonly HashSet<Type> IgnoredComponents = new HashSet<Type>
    {
        typeof(Transform),
        typeof(MeshFilter),
        typeof(MeshRenderer),
        typeof(SkinnedMeshRenderer),
        typeof(ParticleSystemRenderer),
        typeof(CanvasRenderer)
    };

    [MenuItem("Tools/Export Architecture Report")]
    public static void ExportArchitecture()
    {
        StringBuilder report = new StringBuilder();
        
        report.AppendLine("=========================================");
        report.AppendLine("=== СТРУКТУРА ПАПОК SCRIPTS ===");
        report.AppendLine("=========================================\n");
        
        string assetsPath = Application.dataPath;
        
        // Ищем все папки с именем "Scripts" во всех вложенных директориях внутри Assets
        string[] scriptFolders = Directory.GetDirectories(assetsPath, "Scripts", SearchOption.AllDirectories);

        if (scriptFolders.Length > 0)
        {
            foreach (string folderPath in scriptFolders)
            {
                // Делаем путь относительным и красивым для отчета (например, Assets/_Project/Scripts)
                string relativePath = "Assets" + folderPath.Substring(assetsPath.Length).Replace('\\', '/');
                
                report.AppendLine($"--- 📁 Найдена директория: {relativePath} ---");
                BuildDirectoryTree(folderPath, report, 0);
                report.AppendLine(); // Пустая строка для разделения, если папок Scripts несколько
            }
        }
        else
        {
            report.AppendLine($"Папка 'Scripts' не найдена нигде внутри {assetsPath}");
        }

        report.AppendLine("=========================================");
        report.AppendLine("=== ИЕРАРХИЯ АКТИВНОЙ СЦЕНЫ ===");
        report.AppendLine("=========================================\n");

        Scene activeScene = SceneManager.GetActiveScene();
        report.AppendLine($"Сцена: {activeScene.name}\n");

        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects)
        {
            BuildSceneHierarchy(rootObj, report);
        }

        string savePath = EditorUtility.SaveFilePanel(
            "Сохранить архитектурный отчет",
            "",
            $"ArchitectureReport_{activeScene.name}.txt",
            "txt");

        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllText(savePath, report.ToString());
            DevLogger.Log($"[ArchitectureExporter] Отчет успешно сохранен: {savePath}");
            EditorUtility.RevealInFinder(savePath);
        }
    }

    private static void BuildDirectoryTree(string path, StringBuilder report, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        DirectoryInfo dir = new DirectoryInfo(path);
        
        report.AppendLine($"{indent}📁 {dir.Name}/");

        // Выводим C# скрипты
        FileInfo[] files = dir.GetFiles("*.cs");
        foreach (FileInfo file in files)
        {
            report.AppendLine($"{indent}    📄 {file.Name}");
        }

        // Рекурсивно обходим вложенные папки
        DirectoryInfo[] subDirs = dir.GetDirectories();
        foreach (DirectoryInfo subDir in subDirs)
        {
            BuildDirectoryTree(subDir.FullName, report, indentLevel + 1);
        }
    }

    private static void BuildSceneHierarchy(GameObject go, StringBuilder report)
    {
        report.AppendLine($"📦 [Root] {go.name}");

        Component[] components = go.GetComponents<Component>();
        
        // Фильтруем "пустые" компоненты (Missing Scripts) и те, что в игнор-листе
        var architecturalComponents = components
            .Where(c => c != null && !IgnoredComponents.Contains(c.GetType()))
            .ToList();

        if (architecturalComponents.Count > 0)
        {
            foreach (Component comp in architecturalComponents)
            {
                Type type = comp.GetType();
                
                // Разделяем визуально кастомную бизнес-логику (MonoBehaviour) 
                // и встроенные компоненты движка (RigidBody, NavMeshAgent и т.д.)
                string icon = (comp is MonoBehaviour) ? "⚙️" : "🔧";
                report.AppendLine($"    {icon} {type.Name}");
            }
        }
        else
        {
            report.AppendLine("    (Нет значимых компонентов)");
        }
        
        report.AppendLine(); // Пустая строка для читаемости между объектами
    }
}