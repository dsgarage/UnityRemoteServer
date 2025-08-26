using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DSGarage.UnityRemoteServer
{
    public class PackageExporter : EditorWindow
    {
        private string packageName = "UnityRemoteServer-v1.0.1.unitypackage";
        private string outputPath = "";
        private List<string> assetPaths = new List<string>();
        private Vector2 scrollPos;
        
        [MenuItem("Tools/Unity Remote Server/Export Package")]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageExporter>("Export Unity Package");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        private void OnEnable()
        {
            outputPath = Path.GetDirectoryName(Application.dataPath);
            RefreshAssetList();
        }

        private void RefreshAssetList()
        {
            assetPaths.Clear();
            
            // Find all Unity Remote Server assets
            string[] searchPaths = new string[]
            {
                "Assets/UnityRemoteServer",
                "Assets/Plugins/UnityRemoteServer",
                "Packages/com.dsgarage.unity-remote-server"
            };

            foreach (var searchPath in searchPaths)
            {
                if (AssetDatabase.IsValidFolder(searchPath))
                {
                    AddAssetsFromPath(searchPath);
                    break;
                }
            }

            // Also check for individual files in Assets root
            string[] rootFiles = new string[]
            {
                "Assets/UnityRemoteServer.meta"
            };

            foreach (var file in rootFiles)
            {
                if (File.Exists(file))
                {
                    assetPaths.Add(file);
                }
            }
        }

        private void AddAssetsFromPath(string path)
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var asset in allAssets)
            {
                if (asset.StartsWith(path))
                {
                    assetPaths.Add(asset);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Unity Remote Server Package Exporter", titleStyle);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This tool exports Unity Remote Server as a .unitypackage file including all scripts and examples.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Package name
            EditorGUILayout.LabelField("Package Name:", EditorStyles.boldLabel);
            packageName = EditorGUILayout.TextField(packageName);

            // Output path
            EditorGUILayout.LabelField("Output Directory:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField(outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.SaveFolderPanel("Select Output Directory", outputPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    outputPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Assets to include
            EditorGUILayout.LabelField($"Assets to Export ({assetPaths.Count} items):", EditorStyles.boldLabel);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (var asset in assetPaths)
            {
                EditorGUILayout.LabelField(asset, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Asset List"))
            {
                RefreshAssetList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Export button
            GUI.enabled = assetPaths.Count > 0 && !string.IsNullOrEmpty(outputPath);
            if (GUILayout.Button("Export Package", GUILayout.Height(35)))
            {
                ExportPackage();
            }
            GUI.enabled = true;

            // Quick export button
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Quick Export (Default Settings)", GUILayout.Height(25)))
            {
                QuickExport();
            }
        }

        private void ExportPackage()
        {
            string fullPath = Path.Combine(outputPath, packageName);
            
            try
            {
                EditorUtility.DisplayProgressBar("Exporting Package", "Preparing assets...", 0.3f);
                
                AssetDatabase.ExportPackage(
                    assetPaths.ToArray(),
                    fullPath,
                    ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                );
                
                EditorUtility.ClearProgressBar();
                
                EditorUtility.DisplayDialog(
                    "Success",
                    $"Package exported successfully!\n\nLocation:\n{fullPath}\n\nSize: {GetFileSize(fullPath)}",
                    "OK"
                );

                if (EditorUtility.DisplayDialog("Open Folder", "Would you like to open the output folder?", "Yes", "No"))
                {
                    EditorUtility.RevealInFinder(fullPath);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Failed to export package:\n{e.Message}", "OK");
            }
        }

        private void QuickExport()
        {
            packageName = $"UnityRemoteServer-v{System.DateTime.Now:yyyy-MM-dd}.unitypackage";
            outputPath = Path.GetDirectoryName(Application.dataPath);
            RefreshAssetList();
            ExportPackage();
        }

        private string GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                long bytes = new FileInfo(filePath).Length;
                if (bytes < 1024)
                    return $"{bytes} bytes";
                else if (bytes < 1024 * 1024)
                    return $"{bytes / 1024.0:F2} KB";
                else
                    return $"{bytes / (1024.0 * 1024.0):F2} MB";
            }
            return "Unknown";
        }
    }

    // Menu item for direct export
    public static class DirectPackageExporter
    {
        [MenuItem("Tools/Unity Remote Server/Quick Export to Desktop")]
        public static void QuickExportToDesktop()
        {
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string packageName = $"UnityRemoteServer-{System.DateTime.Now:yyyy-MM-dd-HHmm}.unitypackage";
            string fullPath = Path.Combine(desktopPath, packageName);

            List<string> assetPaths = new List<string>();
            
            // Find package location
            string[] searchPaths = new string[]
            {
                "Assets/UnityRemoteServer",
                "Assets/Plugins/UnityRemoteServer",
                "Packages/com.dsgarage.unity-remote-server"
            };

            foreach (var searchPath in searchPaths)
            {
                if (AssetDatabase.IsValidFolder(searchPath))
                {
                    string[] allAssets = AssetDatabase.GetAllAssetPaths();
                    foreach (var asset in allAssets)
                    {
                        if (asset.StartsWith(searchPath))
                        {
                            assetPaths.Add(asset);
                        }
                    }
                    break;
                }
            }

            if (assetPaths.Count > 0)
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Exporting", "Creating Unity Package...", 0.5f);
                    
                    AssetDatabase.ExportPackage(
                        assetPaths.ToArray(),
                        fullPath,
                        ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                    );
                    
                    EditorUtility.ClearProgressBar();
                    
                    if (EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Package exported to:\n{fullPath}",
                        "Open Folder",
                        "OK"))
                    {
                        EditorUtility.RevealInFinder(fullPath);
                    }
                }
                catch (System.Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError($"Failed to export package: {e.Message}");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not find Unity Remote Server assets to export.", "OK");
            }
        }
    }
}