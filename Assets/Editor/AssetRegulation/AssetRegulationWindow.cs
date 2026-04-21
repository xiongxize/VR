using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

namespace AssetRegulation
{
    public class AssetRegulationWindow : EditorWindow
    {
        private AssetRegulationSettings settings;
        private Vector2 scrollPosition;

        [MenuItem("Window/Asset Regulation")]
        public static void ShowWindow()
        {
            GetWindow<AssetRegulationWindow>("Asset Regulation");
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:AssetRegulationSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<AssetRegulationSettings>(path);
            }
            else
            {
                // 如果没有设置文件，创建一个默认的
                settings = CreateInstance<AssetRegulationSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Editor/AssetRegulation/AssetRegulationSettings.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private void OnGUI()
        {
            if (settings == null)
            {
                LoadSettings();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("美术资源规范管理", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 基本设置
            settings.enableValidation = EditorGUILayout.Toggle("启用验证", settings.enableValidation);
            settings.showWarnings = EditorGUILayout.Toggle("显示警告", settings.showWarnings);
            settings.autoFixIssues = EditorGUILayout.Toggle("自动修复问题", settings.autoFixIssues);

            EditorGUILayout.Space();

            // 命名规范
            EditorGUILayout.LabelField("命名规范", EditorStyles.boldLabel);
            settings.texturePrefix = EditorGUILayout.TextField("纹理前缀", settings.texturePrefix);
            settings.materialPrefix = EditorGUILayout.TextField("材质前缀", settings.materialPrefix);
            settings.prefabPrefix = EditorGUILayout.TextField("预制体前缀", settings.prefabPrefix);
            settings.modelPrefix = EditorGUILayout.TextField("模型前缀", settings.modelPrefix);
            settings.animationPrefix = EditorGUILayout.TextField("动画前缀", settings.animationPrefix);
            settings.audioPrefix = EditorGUILayout.TextField("音频前缀", settings.audioPrefix);

            EditorGUILayout.Space();

            // 路径规范
            EditorGUILayout.LabelField("路径规范", EditorStyles.boldLabel);
            settings.texturesPath = EditorGUILayout.TextField("纹理路径", settings.texturesPath);
            settings.materialsPath = EditorGUILayout.TextField("材质路径", settings.materialsPath);
            settings.modelsPath = EditorGUILayout.TextField("模型路径", settings.modelsPath);
            settings.prefabsPath = EditorGUILayout.TextField("预制体路径", settings.prefabsPath);
            settings.animationsPath = EditorGUILayout.TextField("动画路径", settings.animationsPath);
            settings.audioPath = EditorGUILayout.TextField("音频路径", settings.audioPath);

            EditorGUILayout.Space();

            // 资源限制
            EditorGUILayout.LabelField("资源限制", EditorStyles.boldLabel);
            settings.enforcePowerOfTwo = EditorGUILayout.Toggle("强制2的幂次方", settings.enforcePowerOfTwo);
            settings.maxTextureSize = EditorGUILayout.IntField("最大纹理尺寸", settings.maxTextureSize);
            settings.maxTriangleCount = EditorGUILayout.IntField("最大三角面数", settings.maxTriangleCount);
            settings.maxMaterialChannels = EditorGUILayout.IntField("最大材质通道数", settings.maxMaterialChannels);

            EditorGUILayout.Space();

            // 操作按钮
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);

            if (GUILayout.Button("验证所有资源"))
            {
                ValidateAllAssets();
            }

            if (GUILayout.Button("生成报告"))
            {
                GenerateReport();
            }

            if (GUILayout.Button("修复命名"))
            {
                FixNamingConventions();
            }

            EditorGUILayout.EndScrollView();

            // 保存设置
            if (GUI.changed && settings != null)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        private void ValidateAllAssets()
        {
            AssetRegulationProcessor.ValidateAllAssets();
            Debug.Log("资源验证完成");
        }

        private void GenerateReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== 美术资源规范报告 ===");
            report.AppendLine($"生成时间: {System.DateTime.Now}");
            report.AppendLine();

            // 统计资源数量
            int textureCount = 0, materialCount = 0, modelCount = 0, prefabCount = 0;
            int textureErrors = 0, materialErrors = 0, modelErrors = 0, prefabErrors = 0;

            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssets)
            {
                if (assetPath.StartsWith("Assets/Art"))
                {
                    if (assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".tga"))
                    {
                        textureCount++;
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.texturePrefix))
                        {
                            textureErrors++;
                            report.AppendLine($"纹理命名错误: {fileName}");
                        }
                        if (!assetPath.Contains(settings.texturesPath))
                        {
                            textureErrors++;
                            report.AppendLine($"纹理路径错误: {assetPath}");
                        }
                    }
                    else if (assetPath.EndsWith(".mat"))
                    {
                        materialCount++;
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.materialPrefix))
                        {
                            materialErrors++;
                            report.AppendLine($"材质命名错误: {fileName}");
                        }
                        if (!assetPath.Contains(settings.materialsPath))
                        {
                            materialErrors++;
                            report.AppendLine($"材质路径错误: {assetPath}");
                        }
                    }
                    else if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".obj"))
                    {
                        modelCount++;
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.modelPrefix))
                        {
                            modelErrors++;
                            report.AppendLine($"模型命名错误: {fileName}");
                        }
                        if (!assetPath.Contains(settings.modelsPath))
                        {
                            modelErrors++;
                            report.AppendLine($"模型路径错误: {assetPath}");
                        }
                    }
                    else if (assetPath.EndsWith(".prefab"))
                    {
                        prefabCount++;
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.prefabPrefix))
                        {
                            prefabErrors++;
                            report.AppendLine($"预制体命名错误: {fileName}");
                        }
                        if (!assetPath.Contains(settings.prefabsPath))
                        {
                            prefabErrors++;
                            report.AppendLine($"预制体路径错误: {assetPath}");
                        }
                    }
                }
            }

            report.AppendLine();
            report.AppendLine("=== 统计信息 ===");
            report.AppendLine($"纹理资源: {textureCount} (错误: {textureErrors})");
            report.AppendLine($"材质资源: {materialCount} (错误: {materialErrors})");
            report.AppendLine($"模型资源: {modelCount} (错误: {modelErrors})");
            report.AppendLine($"预制体资源: {prefabCount} (错误: {prefabErrors})");

            string reportPath = Path.Combine(Application.dataPath, "AssetRegulationReport.txt");
            File.WriteAllText(reportPath, report.ToString());

            Debug.Log($"报告已生成: {reportPath}");
            EditorUtility.RevealInFinder(reportPath);
        }

        private void FixNamingConventions()
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            int fixedCount = 0;

            foreach (string assetPath in allAssets)
            {
                if (assetPath.StartsWith("Assets/Art"))
                {
                    if (assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".tga"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.texturePrefix))
                        {
                            string newName = settings.texturePrefix + fileName;
                            string newPath = Path.Combine(Path.GetDirectoryName(assetPath), newName + Path.GetExtension(assetPath));
                            AssetDatabase.RenameAsset(assetPath, newName);
                            fixedCount++;
                        }
                    }
                    else if (assetPath.EndsWith(".mat"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.materialPrefix))
                        {
                            string newName = settings.materialPrefix + fileName;
                            string newPath = Path.Combine(Path.GetDirectoryName(assetPath), newName + Path.GetExtension(assetPath));
                            AssetDatabase.RenameAsset(assetPath, newName);
                            fixedCount++;
                        }
                    }
                    else if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".obj"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.modelPrefix))
                        {
                            string newName = settings.modelPrefix + fileName;
                            string newPath = Path.Combine(Path.GetDirectoryName(assetPath), newName + Path.GetExtension(assetPath));
                            AssetDatabase.RenameAsset(assetPath, newName);
                            fixedCount++;
                        }
                    }
                    else if (assetPath.EndsWith(".prefab"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        if (!fileName.StartsWith(settings.prefabPrefix))
                        {
                            string newName = settings.prefabPrefix + fileName;
                            string newPath = Path.Combine(Path.GetDirectoryName(assetPath), newName + Path.GetExtension(assetPath));
                            AssetDatabase.RenameAsset(assetPath, newName);
                            fixedCount++;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"修复了 {fixedCount} 个资源的命名");
        }
    }
}
