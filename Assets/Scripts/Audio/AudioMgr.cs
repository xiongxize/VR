using UnityEngine;
using System.Collections.Generic;

public enum AudioType
{
    Music,
    SFX
}

public class AudioMgr : MonoBehaviour
{
    // 单例实例
    private static AudioMgr instance;
    public static AudioMgr Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioMgr>();
                if (instance == null)
                {
                    GameObject audioMgrObject = new GameObject("AudioMgr");
                    instance = audioMgrObject.AddComponent<AudioMgr>();
                    DontDestroyOnLoad(audioMgrObject);
                }
            }
            return instance;
        }
    }
    
    // 音频配置
    public AudioConfig audioConfig;
    
    // 音频源
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    // 音量控制
    [Range(0f, 1f)]
    public float MasterVolume = 1f;
    [Range(0f, 1f)]
    public float MusicVolume = 1f;
    [Range(0f, 1f)]
    public float SFXVolume = 1f;
    
    // 当前正在播放的背景音乐名称
    private string currentMusicName;
    
    private void Awake()
    {
        // 确保单例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 初始化音频源
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // 初始化音频配置
        if (audioConfig != null)
        {
            audioConfig.InitializeDictionary();
        }
        else
        {
            Debug.LogWarning("AudioMgr: No AudioConfig assigned.");
        }
    }
    
    /// <summary>
    /// 简化调用方法
    /// </summary>
    /// <param name="audioName">音频名称</param>
    public static void Play(string audioName)
    {
        Instance.Play(audioName, 1f, false);
    }
    
    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="volume">音量</param>
    /// <param name="loop">是否循环</param>
    public void Play(string audioName, float volume = 1f, bool loop = false)
    {
        Play(audioName, volume, loop, loop ? AudioType.Music : AudioType.SFX);
    }
    
    /// <summary>
    /// 播放音频（指定类型）
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="volume">音量</param>
    /// <param name="loop">是否循环</param>
    /// <param name="audioType">音频类型</param>
    public void Play(string audioName, float volume = 1f, bool loop = false, AudioType audioType = AudioType.SFX)
    {
        // 检查音频配置
        if (audioConfig == null || audioConfig.audioDictionary == null)
        {
            Debug.LogWarning("AudioMgr: AudioConfig not initialized.");
            return;
        }
        
        // 查找音频
        if (!audioConfig.audioDictionary.TryGetValue(audioName, out AudioClip clip))
        {
            Debug.LogWarning($"AudioMgr: Audio '{audioName}' not found in configuration.");
            return;
        }
        
        // 确定使用哪个音频源
        AudioSource source = audioType == AudioType.Music ? musicSource : sfxSource;
        float groupVolume = audioType == AudioType.Music ? MusicVolume : SFXVolume;
        
        // 设置音频源属性
        source.clip = clip;
        source.volume = volume * groupVolume * MasterVolume;
        source.loop = loop;
        
        // 播放
        source.Play();
        
        // 记录当前背景音乐名称
        if (audioType == AudioType.Music)
        {
            currentMusicName = audioName;
        }
    }
    
    /// <summary>
    /// 播放一次性音效
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="volume">音量</param>
    public void PlayOneShot(string audioName, float volume = 1f)
    {
        // 检查音频配置
        if (audioConfig == null || audioConfig.audioDictionary == null)
        {
            Debug.LogWarning("AudioMgr: AudioConfig not initialized.");
            return;
        }
        
        // 查找音频
        if (!audioConfig.audioDictionary.TryGetValue(audioName, out AudioClip clip))
        {
            Debug.LogWarning($"AudioMgr: Audio '{audioName}' not found in configuration.");
            return;
        }
        
        // 计算实际音量
        float actualVolume = volume * SFXVolume * MasterVolume;
        
        // 播放一次性音效
        sfxSource.PlayOneShot(clip, actualVolume);
    }
    
    /// <summary>
    /// 停止所有音频
    /// </summary>
    public void StopAll()
    {
        musicSource.Stop();
        sfxSource.Stop();
        currentMusicName = null;
    }
    
    /// <summary>
    /// 停止指定名称的音频
    /// </summary>
    /// <param name="audioName">音频名称</param>
    public void Stop(string audioName)
    {
        // 检查是否是当前背景音乐
        if (audioName == currentMusicName && musicSource.isPlaying)
        {
            musicSource.Stop();
            currentMusicName = null;
        }
        // 注意：无法直接停止 PlayOneShot 播放的音效
    }
    
    /// <summary>
    /// 设置主音量
    /// </summary>
    /// <param name="value">音量值（0-1）</param>
    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        UpdateVolumes();
    }
    
    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="value">音量值（0-1）</param>
    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        if (musicSource.isPlaying)
        {
            musicSource.volume = MusicVolume * MasterVolume;
        }
    }
    
    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="value">音量值（0-1）</param>
    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        // 注意：PlayOneShot 的音量在播放时计算，这里不需要更新
    }
    
    /// <summary>
    /// 更新音量
    /// </summary>
    private void UpdateVolumes()
    {
        if (musicSource.isPlaying)
        {
            musicSource.volume = MusicVolume * MasterVolume;
        }
    }
    
    /// <summary>
    /// 重新初始化音频配置
    /// </summary>
    public void Reinitialize()
    {
        if (audioConfig != null)
        {
            audioConfig.InitializeDictionary();
        }
    }
}

/* 示例用法：

// 1. 创建 AudioConfig 资产：
//    - 在 Project 窗口中右键点击 -> Create -> Audio -> AudioConfig
//    - 命名为 "AudioConfig"
//    - 在 Inspector 中添加音频项，例如：
//      - audioName: "click", audioClip: [拖拽点击音效]
//      - audioName: "jump", audioClip: [拖拽跳跃音效]
//      - audioName: "bgm", audioClip: [拖拽背景音乐]

// 2. 创建 AudioMgr 实例：
//    - 在场景中创建一个空游戏对象，命名为 "AudioMgr"
//    - addComponent -> AudioMgr
//    - 在 Inspector 中拖拽 AudioConfig 资产到 audioConfig 字段

// 3. 在其他脚本中使用：

using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    private void Start()
    {
        // 播放背景音乐
        AudioMgr.Instance.Play("bgm", 0.8f, true);
        
        // 设置音量
        AudioMgr.Instance.SetMasterVolume(0.9f);
        AudioMgr.Instance.SetMusicVolume(0.7f);
        AudioMgr.Instance.SetSFXVolume(1.0f);
    }
    
    public void OnButtonClick()
    {
        // 播放点击音效（简化调用）
        AudioMgr.Play("click");
        
        // 或者使用完整参数
        // AudioMgr.Instance.PlayOneShot("click", 0.8f);
    }
    
    public void OnJump()
    {
        // 播放跳跃音效
        AudioMgr.Instance.PlayOneShot("jump");
    }
    
    public void StopBackgroundMusic()
    {
        // 停止背景音乐
        AudioMgr.Instance.Stop("bgm");
    }
    
    public void StopAllAudio()
    {
        // 停止所有音频
        AudioMgr.Instance.StopAll();
    }
}

*/