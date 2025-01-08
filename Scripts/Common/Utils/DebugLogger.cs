using UnityEngine;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace MahjongProject
{
    /// <summary>
    /// 调试工具
    /// </summary>
    public static class DebugLogger
    {
        private static StringBuilder m_logBuilder = new StringBuilder();
        private static Stopwatch m_stopwatch = new Stopwatch();

        /// <summary>
        /// 输出普通日志
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            if (!Constants.DebugSettings.LOG_EVENTS) return;
            Debug.Log($"{Constants.DebugSettings.LOG_PREFIX} {message}");
            AppendToLog("INFO", message);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{Constants.DebugSettings.LOG_PREFIX} {message}");
            AppendToLog("WARNING", message);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"{Constants.DebugSettings.LOG_PREFIX} {message}");
            AppendToLog("ERROR", message);
        }

        /// <summary>
        /// 开始性能计时
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void StartTimer()
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();
        }

        /// <summary>
        /// 结束性能计时并输出结果
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void EndTimer(string operation)
        {
            m_stopwatch.Stop();
            Log($"{operation} 耗时：{m_stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// 输出内存使用情况
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogMemoryUsage()
        {
            if (!Constants.DebugSettings.SHOW_MEMORY) return;

            long totalMemory = System.GC.GetTotalMemory(false) / 1024 / 1024;
            Log($"内存使用：{totalMemory}MB");
        }

        /// <summary>
        /// 输出对象池信息
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogPoolInfo(string poolName, int activeCount, int inactiveCount)
        {
            if (!Constants.DebugSettings.SHOW_POOL_INFO) return;

            Log($"对象池 {poolName} - 活动对象：{activeCount}，非活动对象：{inactiveCount}");
        }

        /// <summary>
        /// 将日志添加到日志文件
        /// </summary>
        private static void AppendToLog(string level, string message)
        {
            m_logBuilder.AppendLine($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");

            // 如果日志内容过长，保存并清空
            if (m_logBuilder.Length > 1024 * 1024) // 1MB
            {
                SaveLog();
            }
        }

        /// <summary>
        /// 保存日志到文件
        /// </summary>
        public static void SaveLog()
        {
            if (m_logBuilder.Length == 0) return;

            string logPath = $"{Application.persistentDataPath}/Logs";
            if (!System.IO.Directory.Exists(logPath))
            {
                System.IO.Directory.CreateDirectory(logPath);
            }

            string fileName = $"{logPath}/game_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            System.IO.File.WriteAllText(fileName, m_logBuilder.ToString());
            m_logBuilder.Clear();
        }

        /// <summary>
        /// 清理旧日志文件
        /// </summary>
        public static void CleanOldLogs(int keepDays = 7)
        {
            string logPath = $"{Application.persistentDataPath}/Logs";
            if (!System.IO.Directory.Exists(logPath)) return;

            var directory = new System.IO.DirectoryInfo(logPath);
            var files = directory.GetFiles("game_log_*.txt");
            var now = System.DateTime.Now;

            foreach (var file in files)
            {
                if ((now - file.CreationTime).TotalDays > keepDays)
                {
                    file.Delete();
                }
            }
        }
    }
} 