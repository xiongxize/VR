using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [System.Serializable]
    public class AudioItem
    {
        public string audioName;
        public AudioClip audioClip;
    }
    
    public List<AudioItem> audioItems = new List<AudioItem>();
    
    [HideInInspector]
    public Dictionary<string, AudioClip> audioDictionary = new Dictionary<string, AudioClip>();
    
    /// <summary>
    /// 初始化音频字典
    /// </summary>
    public void InitializeDictionary()
    {
        audioDictionary.Clear();
        foreach (AudioItem item in audioItems)
        {
            if (!string.IsNullOrEmpty(item.audioName) && item.audioClip != null)
            {
                if (audioDictionary.ContainsKey(item.audioName))
                {
                    Debug.LogWarning($"AudioConfig: Duplicate audio name '{item.audioName}' found. Only the first one will be used.");
                }
                else
                {
                    audioDictionary.Add(item.audioName, item.audioClip);
                }
            }
        }
    }
}