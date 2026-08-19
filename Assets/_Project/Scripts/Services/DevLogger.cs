using UnityEngine;
using System.Diagnostics;

    public static class DevLogger
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        // Ошибки мы ОБЯЗАТЕЛЬНО оставляем в билде, чтобы отловить краши
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
