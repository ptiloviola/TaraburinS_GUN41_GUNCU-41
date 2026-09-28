using UnityEngine;
using System.Diagnostics; 

namespace Gameplay.Tools 
{
    public static class GameLogger
    {
        // --- Базовые логи ---
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message) => UnityEngine.Debug.Log(message);

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message) => UnityEngine.Debug.LogError(message);

        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context) => UnityEngine.Debug.Log(message, context);

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context) => UnityEngine.Debug.LogWarning(message, context);

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context) => UnityEngine.Debug.LogError(message, context);
    }
}