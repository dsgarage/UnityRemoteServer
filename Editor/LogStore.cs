#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace DSGarage.UnityRemoteServer
{
    // 起動時に必ず購読開始
    [InitializeOnLoad]
    public static class LogStore
    {
        [Serializable] public class LogEntry
        {
            public string time;
            public string message;
            public string stack;
            public string type; // Log, Warning, Error, Exception, Assert
        }

        static readonly int Capacity = 5000;
        static readonly List<LogEntry> Entries = new List<LogEntry>(Capacity);
        static bool _initialized;

        static LogStore()
        {
            EnsureInit();
        }

        public static void EnsureInit()
        {
            if (_initialized) return;
            _initialized = true;
            Application.logMessageReceivedThreaded += OnLog;
            // 起動直後の目印（購読開始が分かるよう1件だけ残す）
            Add(new LogEntry {
                time = DateTime.Now.ToString("HH:mm:ss.fff"),
                message = "[LogStore] subscription started",
                stack = string.Empty,
                type = LogType.Log.ToString()
            });
        }

        static void OnLog(string condition, string stackTrace, LogType type)
        {
            Add(new LogEntry {
                time = DateTime.Now.ToString("HH:mm:ss.fff"),
                message = condition,
                stack = stackTrace,
                type = type.ToString()
            });
        }

        static void Add(LogEntry e)
        {
            lock (Entries)
            {
                if (Entries.Count >= Capacity) Entries.RemoveAt(0);
                Entries.Add(e);
            }
        }

        public static string ToJson(string level = null, int limit = 200)
        {
            // level: "error" | "warning" | "log" | "all"(or null)
            IEnumerable<LogEntry> q;
            lock (Entries)
            {
                q = Entries.ToArray(); // スナップショット
            }

            if (!string.IsNullOrEmpty(level))
            {
                var l = level.ToLowerInvariant();
                if (l != "all")
                {
                    q = q.Where(e =>
                    {
                        var t = e.type.ToLowerInvariant();
                        if (l == "error")   return t == "error" || t == "exception" || t == "assert";
                        if (l == "warning") return t == "warning";
                        if (l == "log")     return t == "log";
                        return true;
                    });
                }
            }

            q = q.TakeLast(Math.Max(1, limit));
            // 素直な JSON 配列で返す（ラップしない）
            return JsonArray(q);
        }

        public static void Clear()
        {
            lock (Entries) Entries.Clear();
        }

        static string JsonArray(IEnumerable<LogEntry> items)
        {
            // 依存を増やさず軽量に
            var esc = new Func<string, string>(s => (s ?? "").Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n"));
            var parts = items.Select(e =>
                $"{{\"time\":\"{esc(e.time)}\",\"message\":\"{esc(e.message)}\",\"stack\":\"{esc(e.stack)}\",\"type\":\"{esc(e.type)}\"}}"
            );
            return "[" + string.Join(",", parts) + "]";
        }
    }

    public static class ErrorsEndpoint
    {
        public static void GetErrors(System.Net.HttpListenerContext ctx)
        {
            LogStore.EnsureInit();
            var req = ctx.Request;
            // ?level=error|warning|log|all&limit=200
            var query = req.Url.Query;
            // 安全にパース
            string level = null; int limit = 200;
            try
            {
                if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
                {
                    var queryParams = query.Substring(1).Split('&');
                    foreach (var param in queryParams)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0];
                            var value = System.Uri.UnescapeDataString(parts[1]);
                            
                            if (key == "level") level = value;
                            else if (key == "limit") int.TryParse(value, out limit);
                        }
                    }
                }
            } catch {}
            var json = LogStore.ToJson(level, limit <= 0 ? 200 : limit);
            RemoteServer.WriteJson(ctx, 200, json);
        }

        public static void ClearErrors(System.Net.HttpListenerContext ctx)
        {
            LogStore.EnsureInit();
            LogStore.Clear();
            RemoteServer.WriteJson(ctx, 200, "{\"ok\":true}");
        }
    }
}
#endif