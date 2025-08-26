#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using System.Threading;

namespace DSGarage.UnityRemoteServer
{
    public static class RemoteOps
    {
        [Serializable]
        class RefreshReq { public string[] reimport; public bool force = false; }

        [Serializable]
        class WaitReq { public int timeoutSec = 60; }

        [Serializable]
        class BuildReq {
            public string target; // "Android" | "iOS"
            public string outputPath;
            public string[] scenes;
            public bool development = false;
            public bool clean = true;
        }

        public static void Refresh(string json, System.Net.HttpListenerContext ctx)
        {
            try
            {
                var req = JsonUtility.FromJson<RefreshReq>(string.IsNullOrEmpty(json) ? "{}" : json);
                if (req.reimport != null && req.reimport.Length > 0)
                {
                    foreach (var p in req.reimport) AssetDatabase.ImportAsset(p, req.force ? ImportAssetOptions.ForceUpdate : ImportAssetOptions.Default);
                }
                else
                {
                    AssetDatabase.Refresh(req.force ? ImportAssetOptions.ForceUpdate : ImportAssetOptions.Default);
                }
                RemoteServer.WriteJson(ctx, 200, "{\"ok\":true}");
            }
            catch (Exception e) { RemoteServer.WriteJson(ctx, 500, "{\"error\":\""+Escape(e.Message)+"\"}"); }
        }

        public static void AwaitCompile(string json, System.Net.HttpListenerContext ctx)
        {
            try
            {
                var req = JsonUtility.FromJson<WaitReq>(string.IsNullOrEmpty(json) ? "{}" : json);
                var timeout = DateTime.UtcNow.AddSeconds(Math.Max(1, req.timeoutSec));
                while (EditorApplication.isCompiling && DateTime.UtcNow < timeout)
                {
                    Thread.Sleep(100);
                }
                bool compiling = EditorApplication.isCompiling;
                if (compiling) RemoteServer.WriteJson(ctx, 408, "{\"timeout\":true}");
                else RemoteServer.WriteJson(ctx, 200, "{\"ok\":true}");
            }
            catch (Exception e) { RemoteServer.WriteJson(ctx, 500, "{\"error\":\""+Escape(e.Message)+"\"}"); }
        }

        public static void Build(string json, System.Net.HttpListenerContext ctx)
        {
            try
            {
                var req = JsonUtility.FromJson<BuildReq>(json);
                if (req == null || string.IsNullOrEmpty(req.target) || string.IsNullOrEmpty(req.outputPath) || req.scenes == null || req.scenes.Length == 0)
                {
                    RemoteServer.WriteJson(ctx, 400, "{\"error\":\"missing fields: target/outputPath/scenes\"}"); return;
                }
                var target = ParseTarget(req.target);
                if (target == BuildTarget.NoTarget) { RemoteServer.WriteJson(ctx, 400, "{\"error\":\"unsupported target\"}"); return; }

                if (req.clean && Directory.Exists(req.outputPath))
                {
                    try { Directory.Delete(req.outputPath, true); } catch {}
                }
                Directory.CreateDirectory(Path.GetDirectoryName(req.outputPath) ?? ".");

                // Switch build target group
                var group = BuildPipeline.GetBuildTargetGroup(target);
                if (EditorUserBuildSettings.activeBuildTarget != target)
                {
                    if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
                    {
                        RemoteServer.WriteJson(ctx, 500, "{\"error\":\"failed to switch build target\"}"); return;
                    }
                }

                var opts = new BuildPlayerOptions
                {
                    scenes = req.scenes,
                    target = target,
                    locationPathName = req.outputPath,
                    options = req.development ? BuildOptions.Development : BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(opts);
                var summary = report.summary;
                var ok = summary.result == BuildResult.Succeeded;
                var res = "{\"ok\":" + (ok ? "true" : "false") + ",\"result\":\"" + summary.result + "\",\"output\":\"" + Escape(req.outputPath) + "\"}";
                RemoteServer.WriteJson(ctx, ok ? 200 : 500, res);
            }
            catch (Exception e) { RemoteServer.WriteJson(ctx, 500, "{\"error\":\""+Escape(e.Message)+"\"}"); }
        }

        static BuildTarget ParseTarget(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "android": return BuildTarget.Android;
                case "ios": return BuildTarget.iOS;
                default: return BuildTarget.NoTarget;
            }
        }

        static string Escape(string s) => (s ?? "").Replace("\\","\\\\").Replace("\"","\\\"");
        
        public static void GetErrors(string queryString, System.Net.HttpListenerContext ctx)
        {
            try
            {
                // Parse query parameters manually
                string levelFilter = "error";
                int limit = 200;
                
                if (!string.IsNullOrEmpty(queryString) && queryString.StartsWith("?"))
                {
                    var queryParams = queryString.Substring(1).Split('&');
                    foreach (var param in queryParams)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0];
                            var value = System.Uri.UnescapeDataString(parts[1]);
                            
                            if (key == "level") levelFilter = value;
                            else if (key == "limit") int.TryParse(value, out limit);
                        }
                    }
                    limit = Math.Min(Math.Max(1, limit), 1000);
                }
                
                var errors = new List<object>();
                
                // Debug: Add current logs
                Debug.Log("[RemoteOps] GetErrors called with level=" + levelFilter + ", limit=" + limit);
                
                var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");
                var logEntryType = System.Type.GetType("UnityEditor.LogEntry, UnityEditor");
                
                if (logEntries != null && logEntryType != null)
                {
                    var getCount = logEntries.GetMethod("GetCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var getEntry = logEntries.GetMethod("GetEntryInternal", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    
                    Debug.Log($"[RemoteOps] LogEntries found. getCount={getCount != null}, getEntry={getEntry != null}");
                    
                    if (getCount != null && getEntry != null)
                    {
                        int count = (int)getCount.Invoke(null, null);
                        Debug.Log($"[RemoteOps] Total log entries: {count}");
                        for (int i = 0; i < count && errors.Count < limit; i++)
                        {
                            var entry = Activator.CreateInstance(logEntryType);
                            getEntry.Invoke(null, new object[] { i, entry });
                            
                            var conditionField = entry.GetType().GetField("condition");
                            var modeField = entry.GetType().GetField("mode");
                            
                            if (conditionField != null && modeField != null)
                            {
                                var condition = conditionField.GetValue(entry) as string;
                                var modeValue = modeField.GetValue(entry);
                                int mode = modeValue != null ? (int)modeValue : 0;
                                
                                // mode: 1=Error, 2=Assert, 4=Log, 8=Fatal, 16=DontPreprocessCondition, 32=AssetImportError, 64=AssetImportWarning, 128=Warning, 256=ScriptingError, 512=ScriptingWarning, 1024=ScriptingLog
                                string level = "info";
                                if ((mode & 1) != 0 || (mode & 256) != 0 || (mode & 32) != 0) level = "error";
                                else if ((mode & 128) != 0 || (mode & 512) != 0 || (mode & 64) != 0) level = "warning";
                                
                                if (!string.IsNullOrEmpty(condition))
                                {
                                    // Apply level filter
                                    bool shouldInclude = false;
                                    if (levelFilter == "all") shouldInclude = true;
                                    else if (levelFilter == "error" && level == "error") shouldInclude = true;
                                    else if (levelFilter == "warning" && (level == "warning" || level == "error")) shouldInclude = true;
                                    else if (levelFilter == "log") shouldInclude = true;
                                    
                                    if (shouldInclude)
                                    {
                                        errors.Add(new { message = condition, level = level });
                                    }
                                }
                            }
                        }
                    }
                }
                
                var errorJsons = new List<string>();
                foreach (var e in errors)
                {
                    dynamic err = e;
                    string msg = err.message ?? "";
                    string lvl = err.level ?? "info";
                    errorJsons.Add("{\"message\":\"" + Escape(msg) + "\",\"level\":\"" + lvl + "\"}");
                }
                
                var json_result = "{\"errors\":[" + string.Join(",", errorJsons) + "]}";
                RemoteServer.WriteJson(ctx, 200, json_result);
            }
            catch (Exception e) 
            { 
                RemoteServer.WriteJson(ctx, 500, "{\"error\":\""+Escape(e.Message)+"\"}"); 
            }
        }
        
        public static void ClearErrors(string json, System.Net.HttpListenerContext ctx)
        {
            try
            {
                var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");
                if (logEntries != null)
                {
                    var clear = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    clear?.Invoke(null, null);
                }
                RemoteServer.WriteJson(ctx, 200, "{\"ok\":true}");
            }
            catch (Exception e) 
            { 
                RemoteServer.WriteJson(ctx, 500, "{\"error\":\""+Escape(e.Message)+"\"}"); 
            }
        }
    }
}
#endif
