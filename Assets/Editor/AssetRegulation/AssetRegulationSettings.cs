using UnityEngine;
using System.Collections.Generic;

namespace AssetRegulation
{
    [CreateAssetMenu(fileName = "AssetRegulationSettings", menuName = "Asset Regulation/Settings")]
    public class AssetRegulationSettings : ScriptableObject
    {
        [Header("命名规范")]
        public string texturePrefix = "tex_";
        public string materialPrefix = "mat_";
        public string prefabPrefix = "prefab_";
        public string modelPrefix = "model_";
        public string animationPrefix = "anim_";
        public string audioPrefix = "audio_";

        [Header("路径规范")]
        public string texturesPath = "Art/Textures";
        public string materialsPath = "Art/Materials";
        public string modelsPath = "Art/Models";
        public string prefabsPath = "Art/Prefabs";
        public string animationsPath = "Art/Animations";
        public string audioPath = "Art/Audio";

        [Header("纹理设置")]
        public bool enforcePowerOfTwo = true;
        public int maxTextureSize = 2048;

        [Header("模型设置")]
        public int maxTriangleCount = 10000;

        [Header("材质设置")]
        public int maxMaterialChannels = 4;

        [Header("验证设置")]
        public bool enableValidation = true;
        public bool showWarnings = true;
        public bool autoFixIssues = false;
    }
}
