#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Web;

namespace DSGarage.UnityRemoteServer
{
    [InitializeOnLoad]
    public static class RemoteServer
    {
        static HttpListener _listener;
        static Thread _thread;
        static volatile bool _running;
        public static bool IsRunning => _running;

        static readonly ConcurrentQueue<Action> _mainThreadQ = new ConcurrentQueue<Action>();
        static readonly Dictionary<string, Action<HttpListenerContext>> _customEndpoints = new Dictionary<string, Action<HttpListenerContext>>();
        
        public static int Port { get; set; } = 8787;
        public static bool EnableDebugLogging { get; set; } = false;

        static RemoteServer()
        {
            // Ensure log subscription starts with editor load
            LogStore.EnsureInit();
            EditorApplication.update += Pump;
            Start();
        }

        public static void Start()
        {
            if (_running) return;
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                _listener.Start();
                _running = true;
                _thread = new Thread(ListenLoop){ IsBackground = true };
                _thread.Start();
                Debug.Log($"[Remote] Listening on http://127.0.0.1:{Port}/");
            }
            catch (Exception e)
            {
                Debug.LogError("[Remote] Failed to start: " + e);
                _running = false;
            }
        }

        public static void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch {}
            try { _thread?.Join(200); } catch {}
            _listener = null;
            _thread = null;
            Debug.Log("[Remote] Stopped");
        }

        static void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    var ctx = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => Handle(ctx));
                }
                catch (HttpListenerException)
                {
                    if (!_running) break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Remote] Accept error: " + e.Message);
                }
            }
        }

        static void Pump()
        {
            while (_mainThreadQ.TryDequeue(out var a))
            {
                try { a(); } catch (Exception e) { Debug.LogException(e); }
            }
        }

        static void Handle(HttpListenerContext ctx)
        {
            try
            {
                var req = ctx.Request;
                var path = req.Url.AbsolutePath;
                if (path == "/health")
                {
                    var j = $"{{\"ok\":true,\"compiling\":{EditorApplication.isCompiling.ToString().ToLower()} }}";
                    WriteJson(ctx, 200, j);
                }
                else if (path == "/refresh" && req.HttpMethod == "POST")
                {
                    var body = ReadBody(req);
                    _mainThreadQ.Enqueue(() => RemoteOps.Refresh(body, ctx));
                }
                else if (path == "/awaitCompile" && req.HttpMethod == "POST")
                {
                    var body = ReadBody(req);
                    _mainThreadQ.Enqueue(() => RemoteOps.AwaitCompile(body, ctx));
                }
                else if (path == "/errors" && req.HttpMethod == "GET")
                {
                    _mainThreadQ.Enqueue(() => ErrorsEndpoint.GetErrors(ctx));
                }
                else if (path == "/errors/clear" && req.HttpMethod == "POST")
                {
                    _mainThreadQ.Enqueue(() => ErrorsEndpoint.ClearErrors(ctx));
                }
                else if (path == "/build" && req.HttpMethod == "POST")
                {
                    var body = ReadBody(req);
                    _mainThreadQ.Enqueue(() => RemoteOps.Build(body, ctx));
                }
                else
                {
                    // Check custom endpoints
                    bool handled = false;
                    foreach (var kvp in _customEndpoints)
                    {
                        if (path == kvp.Key || path.StartsWith(kvp.Key + "/"))
                        {
                            _mainThreadQ.Enqueue(() => kvp.Value(ctx));
                            handled = true;
                            break;
                        }
                    }
                    
                    if (!handled)
                    {
                        WriteJson(ctx, 404, "{\"error\":\"not found\"}");
                    }
                }
            }
            catch (Exception e)
            {
                try { WriteJson(ctx, 500, "{\"error\":\"" + Escape(e.Message) + "\"}"); } catch {}
            }
        }

        static string Escape(string s) => s.Replace("\"","\\\"");

        static string ReadBody(HttpListenerRequest req)
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8);
            return sr.ReadToEnd();
        }

        public static void WriteJson(HttpListenerContext ctx, int status, string json)
        {
            var resp = ctx.Response;
            resp.StatusCode = status;
            var data = Encoding.UTF8.GetBytes(json);
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength64 = data.Length;
            using var s = resp.OutputStream;
            s.Write(data, 0, data.Length);
        }
        
        public static void Restart()
        {
            Stop();
            Start();
        }
        
        public static void RegisterCustomEndpoint(string path, Action<HttpListenerContext> handler)
        {
            if (string.IsNullOrEmpty(path) || handler == null) return;
            _customEndpoints[path] = handler;
            if (EnableDebugLogging)
                Debug.Log($"[Remote] Registered custom endpoint: {path}");
        }
        
        public static void UnregisterCustomEndpoint(string path)
        {
            _customEndpoints.Remove(path);
        }
    }
}
#endif
