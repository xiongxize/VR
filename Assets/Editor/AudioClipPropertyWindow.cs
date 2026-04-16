using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioClipImportBatchWindow : EditorWindow
{
    [System.Serializable]
    private class FolderAudioConfig
    {
        public DefaultAsset folder;
        public bool foldout = true;
        public bool enabled = true;

        public bool applyForceToMono = true;
        public bool forceToMono = true;

        public bool applyLoadInBackground;
        public bool loadInBackground = true;

        public bool applyPreloadAudioData = true;
        public bool preloadAudioData = true;

        public bool applyLoadType = true;
        public AudioClipLoadType loadType = AudioClipLoadType.DecompressOnLoad;

        public bool applyCompressionQuality = true;
        public int compressionQuality = 60;

        public bool applyCompressionFormat;
        public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;

        public bool applySampleRateSetting;
        public AudioSampleRateSetting sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

        public bool applySampleRateOverride;
        public int sampleRateOverride = 44100;
    }

    private readonly List<FolderAudioConfig> configs = new List<FolderAudioConfig>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Audio/音频属性批量修改")]
    public static void ShowWindow()
    {
        GetWindow<AudioClipImportBatchWindow>("音频属性批量修改");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("对多个文件夹分别设置不同的导入属性", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("添加文件夹配置", GUILayout.Width(140)))
            {
                configs.Add(new FolderAudioConfig());
            }
        }

        EditorGUILayout.Space();

        if (configs.Count == 0)
        {
            EditorGUILayout.HelpBox("点击“添加文件夹配置”，再把包含 AudioClip 的文件夹拖进来。", MessageType.Info);
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

                c.folder = (DefaultAsset)EditorGUILayout.ObjectField("音频文件夹", c.folder, typeof(DefaultAsset), false);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("要修改的属性（勾选才会覆盖）", EditorStyles.boldLabel);

                c.applyForceToMono = EditorGUILayout.BeginToggleGroup("Force To Mono", c.applyForceToMono);
                if (c.applyForceToMono)
                {
                    EditorGUI.indentLevel++;
                    c.forceToMono = EditorGUILayout.Toggle("值", c.forceToMono);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applyLoadInBackground = EditorGUILayout.BeginToggleGroup("Load In Background", c.applyLoadInBackground);
                if (c.applyLoadInBackground)
                {
                    EditorGUI.indentLevel++;
                    c.loadInBackground = EditorGUILayout.Toggle("值", c.loadInBackground);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applyPreloadAudioData = EditorGUILayout.BeginToggleGroup("Preload Audio Data", c.applyPreloadAudioData);
                if (c.applyPreloadAudioData)
                {
                    EditorGUI.indentLevel++;
                    c.preloadAudioData = EditorGUILayout.Toggle("值", c.preloadAudioData);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applyLoadType = EditorGUILayout.BeginToggleGroup("Load Type", c.applyLoadType);
                if (c.applyLoadType)
                {
                    EditorGUI.indentLevel++;
                    c.loadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("模式", c.loadType);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applyCompressionQuality = EditorGUILayout.BeginToggleGroup("Compression Quality (0-100)", c.applyCompressionQuality);
                if (c.applyCompressionQuality)
                {
                    EditorGUI.indentLevel++;
                    c.compressionQuality = EditorGUILayout.IntSlider("质量", c.compressionQuality, 0, 100);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applyCompressionFormat = EditorGUILayout.BeginToggleGroup("Compression Format", c.applyCompressionFormat);
                if (c.applyCompressionFormat)
                {
                    EditorGUI.indentLevel++;
                    c.compressionFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("格式", c.compressionFormat);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                c.applySampleRateSetting = EditorGUILayout.BeginToggleGroup("Sample Rate Setting", c.applySampleRateSetting);
                if (c.applySampleRateSetting)
                {
                    EditorGUI.indentLevel++;
                    c.sampleRateSetting = (AudioSampleRateSetting)EditorGUILayout.EnumPopup("模式", c.sampleRateSetting);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                if (c.sampleRateSetting == AudioSampleRateSetting.OverrideSampleRate)
                {
                    c.applySampleRateOverride = EditorGUILayout.BeginToggleGroup("Override Sample Rate (Hz)", c.applySampleRateOverride);
                    if (c.applySampleRateOverride)
                    {
                        EditorGUI.indentLevel++;
                        c.sampleRateOverride = EditorGUILayout.IntPopup(
                            "采样率",
                            c.sampleRateOverride,
                            new[] { "22050", "32000", "44100", "48000" },
                            new[] { 22050, 32000, 44100, 48000 }
                        );
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndToggleGroup();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("应用所有配置到对应文件夹的 AudioClip"))
        {
            ApplyAllConfigs();
        }
    }

    private void ApplyAllConfigs()
    {
        var validConfigs = new List<FolderAudioConfig>();
        foreach (var cfg in configs)
        {
            if (!cfg.enabled) continue;
            if (cfg.folder == null) continue;

            var folderPathCheck = AssetDatabase.GetAssetPath(cfg.folder);
            if (string.IsNullOrEmpty(folderPathCheck) || !AssetDatabase.IsValidFolder(folderPathCheck))
                continue;

            validConfigs.Add(cfg);
        }

        if (validConfigs.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有可用的文件夹配置，请检查是否已选择文件夹并勾选启用。", "确定");
            return;
        }

        try
        {
            AssetDatabase.StartAssetEditing();

            var allClipInfos = new List<(FolderAudioConfig config, string guid)>();

            foreach (var cfg in validConfigs)
            {
                var folderPath = AssetDatabase.GetAssetPath(cfg.folder);
                var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
                foreach (var g in guids)
                {
                    allClipInfos.Add((cfg, g));
                }
            }

            if (allClipInfos.Count == 0)
            {
                EditorUtility.DisplayDialog("结果", "选中的文件夹中没有找到任何 AudioClip。", "确定");
                return;
            }

            var totalClips = allClipInfos.Count;

            for (int i = 0; i < allClipInfos.Count; i++)
            {
                var info = allClipInfos[i];
                var cfg = info.config;
                var path = AssetDatabase.GUIDToAssetPath(info.guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                if (cfg.applyForceToMono)
                {
                    importer.forceToMono = cfg.forceToMono;
                }

                if (cfg.applyLoadInBackground)
                {
                    importer.loadInBackground = cfg.loadInBackground;
                }

                var settings = importer.defaultSampleSettings;
                var changedSampleSettings = false;

                if (cfg.applyLoadType)
                {
                    settings.loadType = cfg.loadType;
                    changedSampleSettings = true;
                }

                if (cfg.applyCompressionQuality)
                {
                    settings.quality = Mathf.Clamp01(cfg.compressionQuality / 100f);
                    changedSampleSettings = true;
                }

                if (cfg.applyCompressionFormat)
                {
                    settings.compressionFormat = cfg.compressionFormat;
                    changedSampleSettings = true;
                }

                if (cfg.applySampleRateSetting)
                {
                    settings.sampleRateSetting = cfg.sampleRateSetting;
                    changedSampleSettings = true;
                }

                if (cfg.sampleRateSetting == AudioSampleRateSetting.OverrideSampleRate && cfg.applySampleRateOverride)
                {
                    settings.sampleRateOverride = (uint)Mathf.Max(1, cfg.sampleRateOverride);
                    changedSampleSettings = true;
                }

                if (cfg.applyPreloadAudioData)
                {
                    settings.preloadAudioData = cfg.preloadAudioData;
                    changedSampleSettings = true;
                }

                if (changedSampleSettings)
                {
                    importer.defaultSampleSettings = settings;
                }

                EditorUtility.DisplayProgressBar(
                    "应用音频属性",
                    $"处理: {path}",
                    (float)(i + 1) / totalClips);

                importer.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        EditorUtility.DisplayDialog("完成", "已对所有启用的文件夹中的 AudioClip 应用配置。", "确定");
    }
}
