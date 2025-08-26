using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DSGarage.UnityRemoteServer
{
    public class ScriptInstallerWindow : EditorWindow
    {
        private string installPath = "";
        private bool scriptsIncluded = false;
        private Vector2 scrollPos;
        
        private static readonly string[] scriptFiles = new string[]
        {
            "unity-build.sh",
            "unity-monitor.sh", 
            "unity-control.py",
            "README.md"
        };

        [MenuItem("Tools/Unity Remote Server/Script Installer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptInstallerWindow>("Remote Server Scripts");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // Check if scripts are included in the package
            CheckScriptsAvailability();
            
            // Set default install path to project root/BuildScripts
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            installPath = Path.Combine(projectRoot, "BuildScripts");
        }

        private void CheckScriptsAvailability()
        {
            string packagePath = GetPackageScriptsPath();
            scriptsIncluded = !string.IsNullOrEmpty(packagePath) && Directory.Exists(packagePath);
        }

        private string GetPackageScriptsPath()
        {
            // Try multiple possible locations
            string[] possiblePaths = new string[]
            {
                Path.Combine(Application.dataPath, "UnityRemoteServer", "Examples", "Scripts"),
                Path.Combine(Application.dataPath, "Plugins", "UnityRemoteServer", "Examples", "Scripts"),
                Path.Combine("Packages", "com.dsgarage.unity-remote-server", "Examples", "Scripts"),
                Path.Combine(Application.dataPath, "Examples", "Scripts")
            };

            foreach (var path in possiblePaths)
            {
                string fullPath = path.StartsWith("Packages") ? path : path;
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Unity Remote Server - Script Installer", titleStyle);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "These automation scripts should be installed OUTSIDE your Unity project's Assets folder.\n" +
                "They are used for CI/CD automation, build pipelines, and external control of Unity.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Scripts availability status
            if (scriptsIncluded)
            {
                EditorGUILayout.HelpBox("✓ Scripts are available in the package", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("✗ Scripts not found. Please ensure the package is properly installed.", MessageType.Warning);
                if (GUILayout.Button("Refresh"))
                {
                    CheckScriptsAvailability();
                }
                return;
            }

            EditorGUILayout.Space(10);

            // Installation path
            EditorGUILayout.LabelField("Installation Path:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            installPath = EditorGUILayout.TextField(installPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Installation Directory", installPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    installPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Recommended locations:\n" +
                "• ProjectRoot/BuildScripts\n" +
                "• ProjectRoot/CI\n" +
                "• ProjectRoot/Automation",
                MessageType.None
            );

            EditorGUILayout.Space(10);

            // Scripts to install
            EditorGUILayout.LabelField("Scripts to Install:", EditorStyles.boldLabel);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
            foreach (var script in scriptFiles)
            {
                EditorGUILayout.LabelField("• " + script);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Install button
            GUI.enabled = scriptsIncluded && !string.IsNullOrEmpty(installPath);
            if (GUILayout.Button("Install Scripts", GUILayout.Height(35)))
            {
                InstallScripts();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // Additional actions
            EditorGUILayout.LabelField("Quick Actions:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("https://github.com/dsgarage/UnityRemoteServer/wiki");
            }
            if (GUILayout.Button("Test Server"))
            {
                TestServerConnection();
            }
            EditorGUILayout.EndHorizontal();

            // Show server status
            EditorGUILayout.Space(10);
            ShowServerStatus();
        }

        private void InstallScripts()
        {
            string sourcePath = GetPackageScriptsPath();
            if (string.IsNullOrEmpty(sourcePath))
            {
                EditorUtility.DisplayDialog("Error", "Could not find source scripts directory.", "OK");
                return;
            }

            try
            {
                // Create target directory if it doesn't exist
                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                int copiedCount = 0;
                List<string> copiedFiles = new List<string>();

                foreach (var scriptFile in scriptFiles)
                {
                    string sourceFile = Path.Combine(sourcePath, scriptFile);
                    if (File.Exists(sourceFile))
                    {
                        string targetFile = Path.Combine(installPath, scriptFile);
                        File.Copy(sourceFile, targetFile, true);
                        
                        // Make shell scripts executable on Unix-like systems
                        if (scriptFile.EndsWith(".sh") && Application.platform != RuntimePlatform.WindowsEditor)
                        {
                            MakeExecutable(targetFile);
                        }
                        
                        copiedFiles.Add(scriptFile);
                        copiedCount++;
                    }
                }

                if (copiedCount > 0)
                {
                    string message = $"Successfully installed {copiedCount} files to:\n{installPath}\n\n";
                    message += "Installed files:\n";
                    foreach (var file in copiedFiles)
                    {
                        message += $"• {file}\n";
                    }
                    
                    EditorUtility.DisplayDialog("Success", message, "OK");
                    
                    // Open the folder in file explorer
                    if (EditorUtility.DisplayDialog("Open Folder", "Would you like to open the installation folder?", "Yes", "No"))
                    {
                        EditorUtility.RevealInFinder(installPath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "No files were copied. Scripts may already be up to date.", "OK");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to install scripts:\n{e.Message}", "OK");
            }
        }

        private void MakeExecutable(string filePath)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{filePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(startInfo).WaitForExit();
            }
            catch
            {
                // Silently ignore on systems where chmod is not available
            }
        }

        private void TestServerConnection()
        {
            try
            {
                var request = UnityEngine.Networking.UnityWebRequest.Get("http://127.0.0.1:8787/health");
                var operation = request.SendWebRequest();
                
                // Simple synchronous wait for testing
                while (!operation.isDone)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    EditorUtility.DisplayDialog("Success", $"Server is running!\n\nResponse:\n{request.downloadHandler.text}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Server is not responding. Make sure Unity Remote Server is running.", "OK");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to test server:\n{e.Message}", "OK");
            }
        }

        private void ShowServerStatus()
        {
            EditorGUILayout.LabelField("Server Status:", EditorStyles.boldLabel);
            
            bool serverRunning = RemoteServer.Instance != null && RemoteServer.IsRunning;
            
            if (serverRunning)
            {
                EditorGUILayout.HelpBox($"✓ Server is running on port {RemoteServer.Port}", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("✗ Server is not running", MessageType.Warning);
                if (GUILayout.Button("Start Server"))
                {
                    RemoteServer.StartServer();
                }
            }
        }
    }

    // Auto-installer that runs when package is imported
    [InitializeOnLoad]
    public static class ScriptAutoInstaller
    {
        private static readonly string installedKey = "UnityRemoteServer.ScriptsInstalled";
        
        static ScriptAutoInstaller()
        {
            EditorApplication.delayCall += CheckFirstInstall;
        }

        private static void CheckFirstInstall()
        {
            // Check if this is the first time installing
            if (!EditorPrefs.GetBool(installedKey, false))
            {
                // Check if scripts are available
                string packagePath = Path.Combine(Application.dataPath, "UnityRemoteServer", "Examples", "Scripts");
                if (Directory.Exists(packagePath))
                {
                    if (EditorUtility.DisplayDialog(
                        "Unity Remote Server",
                        "Unity Remote Server has been installed!\n\n" +
                        "Would you like to install the automation scripts to your project?\n" +
                        "(These scripts are used for CI/CD and should be placed outside the Assets folder)",
                        "Yes, install scripts",
                        "No, I'll do it later"))
                    {
                        ScriptInstallerWindow.ShowWindow();
                    }
                    
                    EditorPrefs.SetBool(installedKey, true);
                }
            }
        }

        [MenuItem("Tools/Unity Remote Server/Reset First Install Flag")]
        private static void ResetFirstInstallFlag()
        {
            EditorPrefs.DeleteKey(installedKey);
            Debug.Log("First install flag reset. The installer will show on next Unity restart.");
        }
    }
}