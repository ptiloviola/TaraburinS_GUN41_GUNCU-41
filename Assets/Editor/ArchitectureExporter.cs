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
        
        string[] scriptFolders = Directory.GetDirectories(assetsPath, "Scripts", SearchOption.AllDirectories);

        if (scriptFolders.Length > 0)
        {
            foreach (string folderPath in scriptFolders)
            {
                string relativePath = "Assets" + folderPath.Substring(assetsPath.Length).Replace('\\', '/');
                
                report.AppendLine($"--- 📁 Найдена директория: {relativePath} ---");
                BuildDirectoryTree(folderPath, report, 0);
                report.AppendLine();
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

        FileInfo[] files = dir.GetFiles("*.cs");
        foreach (FileInfo file in files)
        {
            report.AppendLine($"{indent}    📄 {file.Name}");
        }

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
        
        var architecturalComponents = components
            .Where(c => c != null && !IgnoredComponents.Contains(c.GetType()))
            .ToList();

        if (architecturalComponents.Count > 0)
        {
            foreach (Component comp in architecturalComponents)
            {
                Type type = comp.GetType();
                
                string icon = (comp is MonoBehaviour) ? "⚙️" : "🔧";
                report.AppendLine($"    {icon} {type.Name}");
            }
        }
        else
        {
            report.AppendLine("    (Нет значимых компонентов)");
        }
        
        report.AppendLine();
    }
}