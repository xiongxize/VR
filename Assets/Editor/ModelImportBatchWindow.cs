using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lesson.EditorTools
{
    public class ModelImportBatchWindow : EditorWindow
    {
        private enum MaterialCreationMode
        {
            DoNotImport,
            Standard,
            StandardLegacy,
            ViaMaterialDescription
        }

        [Serializable]
        private class FolderModelConfig
        {
            public DefaultAsset folder;
            public bool foldout = true;
            public bool enabled = true;

            // Model tab
            public bool applyGlobalScale;
            public float globalScale = 1f;

            public bool applyUseFileScale;
            public bool useFileScale = true;

            public bool applyMeshCompression;
            public ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.Off;

            public bool applyIsReadable;
            public bool isReadable;

            public bool applyAddCollider;
            public bool addCollider;

            public bool applyImportBlendShapes;
            public bool importBlendShapes = true;

            public bool applyImportCameras;
            public bool importCameras;

            public bool applyImportLights;
            public bool importLights;

            public bool applyImportVisibility;
            public bool importVisibility = true;

            public bool applyImportNormals;
            public ModelImporterNormals importNormals = ModelImporterNormals.Import;

            public bool applyNormalSmoothingAngle;
            public float normalSmoothingAngle = 60f;

            public bool applyImportTangents;
            public ModelImporterTangents importTangents = ModelImporterTangents.Import;

            // Animation tab
            public bool applyImportAnimation = true;
            public bool importAnimation = true;

            public bool applyAnimationType;
            public ModelImporterAnimationType animationType = ModelImporterAnimationType.Generic;

            public bool applyAvatarSetup;
            public ModelImporterAvatarSetup avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            public bool applySourceAvatar;
            public Avatar sourceAvatar;

            public bool applyOptimizeGameObjects;
            public bool optimizeGameObjects = true;

            public bool applyResampleCurves;
            public bool resampleCurves = true;

            public bool applyAnimationCompression;
            public ModelImporterAnimationCompression animationCompression = ModelImporterAnimationCompression.Optimal;

            // Materials tab
            public bool applyMaterialCreationMode;
            public MaterialCreationMode materialCreationMode = MaterialCreationMode.Standard;
        }

        private readonly List<FolderModelConfig> configs = new List<FolderModelConfig>();
        private Vector2 scrollPos;

        [MenuItem("Tools/Model/模型导入批量修改")]
        public static void ShowWindow()
        {
            GetWindow<ModelImportBatchWindow>("模型导入批量修改");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("对多个文件夹分别设置不同的模型导入属性", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("添加文件夹配置", GUILayout.Width(140)))
                {
                    configs.Add(new FolderModelConfig());
                }
            }

            EditorGUILayout.Space();

            if (configs.Count == 0)
            {
                EditorGUILayout.HelpBox("点击“添加文件夹配置”，然后把包含模型（fbx/obj 等）的文件夹拖进来。", MessageType.Info);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < configs.Count; i++)
            {
                var c = configs[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                c.foldout = EditorGUILayout.Foldout(c.foldout, $"配置 {i + 1}", true);
                c.enabled = EditorGUILayout.ToggleLeft("启用", c.enabled, GUILayout.Width(60));

                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    configs.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (c.foldout)
                {
                    EditorGUI.indentLevel++;

                    c.folder = (DefaultAsset)EditorGUILayout.ObjectField("模型文件夹", c.folder, typeof(DefaultAsset), false);

                    EditorGUILayout.Space();
                    DrawModelOptions(c);
                    EditorGUILayout.Space();
                    DrawAnimationOptions(c);
                    EditorGUILayout.Space();
                    DrawMaterialOptions(c);

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("应用所有配置到对应文件夹的模型"))
            {
                ApplyAllConfigs();
            }
        }

        private static void DrawModelOptions(FolderModelConfig c)
        {
            EditorGUILayout.LabelField("Model 导入选项（勾选才会覆盖）", EditorStyles.boldLabel);

            c.applyGlobalScale = EditorGUILayout.BeginToggleGroup("Global Scale", c.applyGlobalScale);
            c.globalScale = EditorGUILayout.FloatField("值", Mathf.Max(0.0001f, c.globalScale));
            EditorGUILayout.EndToggleGroup();

            c.applyUseFileScale = EditorGUILayout.BeginToggleGroup("Use File Scale", c.applyUseFileScale);
            c.useFileScale = EditorGUILayout.Toggle("值", c.useFileScale);
            EditorGUILayout.EndToggleGroup();

            c.applyMeshCompression = EditorGUILayout.BeginToggleGroup("Mesh Compression", c.applyMeshCompression);
            c.meshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup("级别", c.meshCompression);
            EditorGUILayout.EndToggleGroup();

            c.applyIsReadable = EditorGUILayout.BeginToggleGroup("Read/Write Enabled", c.applyIsReadable);
            c.isReadable = EditorGUILayout.Toggle("值", c.isReadable);
            EditorGUILayout.EndToggleGroup();

            c.applyAddCollider = EditorGUILayout.BeginToggleGroup("Generate Colliders", c.applyAddCollider);
            c.addCollider = EditorGUILayout.Toggle("值", c.addCollider);
            EditorGUILayout.EndToggleGroup();

            c.applyImportBlendShapes = EditorGUILayout.BeginToggleGroup("Import BlendShapes", c.applyImportBlendShapes);
            c.importBlendShapes = EditorGUILayout.Toggle("值", c.importBlendShapes);
            EditorGUILayout.EndToggleGroup();

            c.applyImportCameras = EditorGUILayout.BeginToggleGroup("Import Cameras", c.applyImportCameras);
            c.importCameras = EditorGUILayout.Toggle("值", c.importCameras);
            EditorGUILayout.EndToggleGroup();

            c.applyImportLights = EditorGUILayout.BeginToggleGroup("Import Lights", c.applyImportLights);
            c.importLights = EditorGUILayout.Toggle("值", c.importLights);
            EditorGUILayout.EndToggleGroup();

            c.applyImportVisibility = EditorGUILayout.BeginToggleGroup("Import Visibility", c.applyImportVisibility);
            c.importVisibility = EditorGUILayout.Toggle("值", c.importVisibility);
            EditorGUILayout.EndToggleGroup();

            c.applyImportNormals = EditorGUILayout.BeginToggleGroup("Normals", c.applyImportNormals);
            c.importNormals = (ModelImporterNormals)EditorGUILayout.EnumPopup("模式", c.importNormals);
            EditorGUILayout.EndToggleGroup();

            c.applyNormalSmoothingAngle = EditorGUILayout.BeginToggleGroup("Smoothing Angle", c.applyNormalSmoothingAngle);
            c.normalSmoothingAngle = EditorGUILayout.Slider("角度", c.normalSmoothingAngle, 0f, 180f);
            EditorGUILayout.EndToggleGroup();

            c.applyImportTangents = EditorGUILayout.BeginToggleGroup("Tangents", c.applyImportTangents);
            c.importTangents = (ModelImporterTangents)EditorGUILayout.EnumPopup("模式", c.importTangents);
            EditorGUILayout.EndToggleGroup();
        }

        private static void DrawAnimationOptions(FolderModelConfig c)
        {
            EditorGUILayout.LabelField("Animation 导入选项（勾选才会覆盖）", EditorStyles.boldLabel);

            c.applyImportAnimation = EditorGUILayout.BeginToggleGroup("Import Animation", c.applyImportAnimation);
            c.importAnimation = EditorGUILayout.Toggle("值", c.importAnimation);
            EditorGUILayout.EndToggleGroup();

            c.applyAnimationType = EditorGUILayout.BeginToggleGroup("Animation Type", c.applyAnimationType);
            c.animationType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("类型", c.animationType);
            EditorGUILayout.EndToggleGroup();

            c.applyAvatarSetup = EditorGUILayout.BeginToggleGroup("Avatar Definition", c.applyAvatarSetup);
            c.avatarSetup = (ModelImporterAvatarSetup)EditorGUILayout.EnumPopup("模式", c.avatarSetup);
            EditorGUILayout.EndToggleGroup();

            if (IsCopyFromOtherAvatar(c.avatarSetup))
            {
                c.applySourceAvatar = EditorGUILayout.BeginToggleGroup("Source Avatar", c.applySourceAvatar);
                c.sourceAvatar = (Avatar)EditorGUILayout.ObjectField("Avatar", c.sourceAvatar, typeof(Avatar), false);
                EditorGUILayout.EndToggleGroup();
            }
            else
            {
                c.applySourceAvatar = false;
                c.sourceAvatar = null;
            }

            c.applyOptimizeGameObjects = EditorGUILayout.BeginToggleGroup("Optimize Game Objects", c.applyOptimizeGameObjects);
            c.optimizeGameObjects = EditorGUILayout.Toggle("值", c.optimizeGameObjects);
            EditorGUILayout.EndToggleGroup();

            c.applyResampleCurves = EditorGUILayout.BeginToggleGroup("Resample Curves", c.applyResampleCurves);
            c.resampleCurves = EditorGUILayout.Toggle("值", c.resampleCurves);
            EditorGUILayout.EndToggleGroup();

            c.applyAnimationCompression = EditorGUILayout.BeginToggleGroup("Animation Compression", c.applyAnimationCompression);
            c.animationCompression = (ModelImporterAnimationCompression)EditorGUILayout.EnumPopup("模式", c.animationCompression);
            EditorGUILayout.EndToggleGroup();
        }

        private static void DrawMaterialOptions(FolderModelConfig c)
        {
            EditorGUILayout.LabelField("Materials 导入选项（勾选才会覆盖）", EditorStyles.boldLabel);

            c.applyMaterialCreationMode = EditorGUILayout.BeginToggleGroup("Material Creation Mode", c.applyMaterialCreationMode);
            c.materialCreationMode = (MaterialCreationMode)EditorGUILayout.EnumPopup("模式", c.materialCreationMode);
            EditorGUILayout.EndToggleGroup();
        }

        private void ApplyAllConfigs()
        {
            var validConfigs = new List<FolderModelConfig>();
            foreach (var cfg in configs)
            {
                if (!cfg.enabled) continue;
                if (cfg.folder == null) continue;

                var folderPathCheck = AssetDatabase.GetAssetPath(cfg.folder);
                if (string.IsNullOrEmpty(folderPathCheck) || !AssetDatabase.IsValidFolder(folderPathCheck))
                {
                    continue;
                }

                validConfigs.Add(cfg);
            }

            if (validConfigs.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有可用的文件夹配置，请检查是否已选择有效文件夹并勾选启用。", "确定");
                return;
            }

            var allModelInfos = new List<(FolderModelConfig config, string guid)>();
            foreach (var cfg in validConfigs)
            {
                var folderPath = AssetDatabase.GetAssetPath(cfg.folder);
                var guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
                foreach (var g in guids)
                {
                    allModelInfos.Add((cfg, g));
                }
            }

            if (allModelInfos.Count == 0)
            {
                EditorUtility.DisplayDialog("结果", "选中的文件夹中没有找到任何模型资源（t:Model）。", "确定");
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < allModelInfos.Count; i++)
                {
                    var info = allModelInfos[i];
                    var cfg = info.config;
                    var path = AssetDatabase.GUIDToAssetPath(info.guid);

                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    ApplyOne(importer, cfg);

                    EditorUtility.DisplayProgressBar(
                        "应用模型导入属性",
                        $"处理: {path}",
                        (float)(i + 1) / allModelInfos.Count);

                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("完成", $"已处理 {allModelInfos.Count} 个模型资源。", "确定");
        }

        private static void ApplyOne(ModelImporter importer, FolderModelConfig cfg)
        {
            if (cfg.applyGlobalScale)
            {
                importer.globalScale = Mathf.Max(0.0001f, cfg.globalScale);
            }

            if (cfg.applyUseFileScale)
            {
                importer.useFileScale = cfg.useFileScale;
            }

            if (cfg.applyMeshCompression)
            {
                importer.meshCompression = cfg.meshCompression;
            }

            if (cfg.applyIsReadable)
            {
                importer.isReadable = cfg.isReadable;
            }

            if (cfg.applyAddCollider)
            {
                importer.addCollider = cfg.addCollider;
            }

            if (cfg.applyImportBlendShapes)
            {
                importer.importBlendShapes = cfg.importBlendShapes;
            }

            if (cfg.applyImportCameras)
            {
                importer.importCameras = cfg.importCameras;
            }

            if (cfg.applyImportLights)
            {
                importer.importLights = cfg.importLights;
            }

            if (cfg.applyImportVisibility)
            {
                importer.importVisibility = cfg.importVisibility;
            }

            if (cfg.applyImportNormals)
            {
                importer.importNormals = cfg.importNormals;
            }

            if (cfg.applyNormalSmoothingAngle)
            {
                importer.normalSmoothingAngle = Mathf.Clamp(cfg.normalSmoothingAngle, 0f, 180f);
            }

            if (cfg.applyImportTangents)
            {
                importer.importTangents = cfg.importTangents;
            }

            if (cfg.applyImportAnimation)
            {
                importer.importAnimation = cfg.importAnimation;
            }

            if (cfg.applyAnimationType)
            {
                importer.animationType = cfg.animationType;
            }

            if (cfg.applyAvatarSetup)
            {
                importer.avatarSetup = cfg.avatarSetup;
            }

            if (IsCopyFromOtherAvatar(cfg.avatarSetup) && cfg.applySourceAvatar)
            {
                importer.sourceAvatar = cfg.sourceAvatar;
            }

            if (cfg.applyOptimizeGameObjects)
            {
                importer.optimizeGameObjects = cfg.optimizeGameObjects;
            }

            if (cfg.applyResampleCurves)
            {
                importer.resampleCurves = cfg.resampleCurves;
            }

            if (cfg.applyAnimationCompression)
            {
                importer.animationCompression = cfg.animationCompression;
            }

            if (cfg.applyMaterialCreationMode)
            {
                ApplyMaterialCreationMode(importer, cfg.materialCreationMode);
            }
        }

        private static void ApplyMaterialCreationMode(ModelImporter importer, MaterialCreationMode mode)
        {
            // 优先使用新版的 materialImportMode；若不存在则回退到 importMaterials。
            // 由于不同 Unity 版本 API 差异，这里用反射来保证兼容。

            var importerType = importer.GetType();
            var prop = importerType.GetProperty("materialImportMode", BindingFlags.Instance | BindingFlags.Public);

            if (prop != null && prop.PropertyType.IsEnum)
            {
                var enumType = prop.PropertyType;
                var value = GetMaterialImportModeEnumValue(enumType, mode);
                if (value != null)
                {
                    prop.SetValue(importer, value);
                    return;
                }
            }

            var importMaterialsProp = importerType.GetProperty("importMaterials", BindingFlags.Instance | BindingFlags.Public);
            if (importMaterialsProp != null && importMaterialsProp.PropertyType == typeof(bool))
            {
                importMaterialsProp.SetValue(importer, mode != MaterialCreationMode.DoNotImport);
            }
        }

        private static object GetMaterialImportModeEnumValue(Type enumType, MaterialCreationMode mode)
        {
            // 不同版本枚举名可能略有差别，这里尽量做兼容匹配。
            var candidates = mode switch
            {
                MaterialCreationMode.DoNotImport => new[] { "None" },
                MaterialCreationMode.Standard => new[] { "ImportStandard", "Standard" },
                MaterialCreationMode.StandardLegacy => new[] { "ImportStandardLegacy", "StandardLegacy" },
                MaterialCreationMode.ViaMaterialDescription => new[] { "ImportViaMaterialDescription", "ViaMaterialDescription" },
                _ => Array.Empty<string>()
            };

            foreach (var name in candidates)
            {
                if (Enum.IsDefined(enumType, name))
                {
                    return Enum.Parse(enumType, name);
                }
            }

            return null;
        }

        private static bool IsCopyFromOtherAvatar(ModelImporterAvatarSetup setup)
        {
            // 不同 Unity 版本枚举成员命名不同，这里用字符串判断以避免编译期依赖。
            var name = setup.ToString();
            return name == "CopyFromOtherAvatar" || name == "CopyFromOther";
        }
    }
}
