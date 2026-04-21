using UnityEditor;
using UnityEngine;
using System.IO;

namespace AssetRegulation
{
    public class AssetRegulationProcessor : AssetPostprocessor
    {
        private static AssetRegulationSettings settings;

        private static AssetRegulationSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:AssetRegulationSettings");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        settings = AssetDatabase.LoadAssetAtPath<AssetRegulationSettings>(path);
                    }
                }
                return settings;
            }
        }

        void OnPreprocessTexture()
        {
            if (!IsEnabled()) return;

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null) return;

            ValidateTextureImport(importer);
        }

        void OnPreprocessModel()
        {
            if (!IsEnabled()) return;

            ModelImporter importer = assetImporter as ModelImporter;
            if (importer == null) return;

            ValidateModelImport(importer);
        }

        void OnPreprocessMaterial()
        {
            if (!IsEnabled()) return;

            ValidateMaterialImport();
        }

        void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundleName, string newAssetBundleName)
        {
            if (!IsEnabled()) return;

            ValidateAssetPath(assetPath);
        }

        private void ValidateTextureImport(TextureImporter importer)
        {
            string assetPath = assetImporter.assetPath;

            // 验证命名
            ValidateAssetName(assetPath, Settings.texturePrefix, "Texture");

            // 验证路径
            ValidateAssetPath(assetPath, Settings.texturesPath, "Textures");

            // 验证纹理设置
            if (Settings.enforcePowerOfTwo)
            {
                TextureImporterSettings textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);

                // 这里可以添加纹理尺寸检查逻辑
            }
        }

        private void ValidateModelImport(ModelImporter importer)
        {
            string assetPath = assetImporter.assetPath;

            // 验证命名
            ValidateAssetName(assetPath, Settings.modelPrefix, "Model");

            // 验证路径
            ValidateAssetPath(assetPath, Settings.modelsPath, "Models");
        }

        private void ValidateMaterialImport()
        {
            string assetPath = assetImporter.assetPath;

            // 验证命名
            ValidateAssetName(assetPath, Settings.materialPrefix, "Material");

            // 验证路径
            ValidateAssetPath(assetPath, Settings.materialsPath, "Materials");
        }

        private void ValidateAssetName(string assetPath, string expectedPrefix, string assetType)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            if (!fileName.StartsWith(expectedPrefix))
            {
                string message = $"{assetType} 命名不符合规范: {fileName}\n应该以 '{expectedPrefix}' 开头";
                if (Settings.showWarnings)
                {
                    Debug.LogWarning(message);
                }
            }
        }

        private void ValidateAssetPath(string assetPath, string expectedPath, string assetType)
        {
            if (!assetPath.Contains(expectedPath))
            {
                string message = $"{assetType} 路径不符合规范: {assetPath}\n应该放在 '{expectedPath}' 目录下";
                if (Settings.showWarnings)
                {
                    Debug.LogWarning(message);
                }
            }
        }

        private static bool IsEnabled()
        {
            return Settings != null && Settings.enableValidation;
        }

        public static void ValidateAllAssets()
        {
            if (!IsEnabled()) return;

            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssets)
            {
                if (assetPath.StartsWith("Assets/Art"))
                {
                    ValidateAssetPath(assetPath);
                }
            }
        }

        private static void ValidateAssetPath(string assetPath)
        {
            if (assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".tga"))
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!fileName.StartsWith(Settings.texturePrefix))
                {
                    Debug.LogWarning($"Texture 命名不符合规范: {fileName}");
                }
                if (!assetPath.Contains(Settings.texturesPath))
                {
                    Debug.LogWarning($"Texture 路径不符合规范: {assetPath}");
                }
            }
            else if (assetPath.EndsWith(".mat"))
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!fileName.StartsWith(Settings.materialPrefix))
                {
                    Debug.LogWarning($"Material 命名不符合规范: {fileName}");
                }
                if (!assetPath.Contains(Settings.materialsPath))
                {
                    Debug.LogWarning($"Material 路径不符合规范: {assetPath}");
                }
            }
            else if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".obj"))
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!fileName.StartsWith(Settings.modelPrefix))
                {
                    Debug.LogWarning($"Model 命名不符合规范: {fileName}");
                }
                if (!assetPath.Contains(Settings.modelsPath))
                {
                    Debug.LogWarning($"Model 路径不符合规范: {assetPath}");
                }
            }
            else if (assetPath.EndsWith(".prefab"))
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!fileName.StartsWith(Settings.prefabPrefix))
                {
                    Debug.LogWarning($"Prefab 命名不符合规范: {fileName}");
                }
                if (!assetPath.Contains(Settings.prefabsPath))
                {
                    Debug.LogWarning($"Prefab 路径不符合规范: {assetPath}");
                }
            }
        }
    }
}
