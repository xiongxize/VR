# Unity 音效管理器 (AudioMgr) 技术文档

## 1. 功能介绍

AudioMgr 是一个专为 Unity 游戏开发设计的音效管理系统，提供以下核心功能：

- **统一音频配置**：通过 ScriptableObject 集中管理所有音频资源
- **单例模式**：全局唯一实例，跨场景不销毁
- **多种播放模式**：支持背景音乐和短音效播放
- **分层音量控制**：主音量、音乐音量、音效音量独立调节
- **便捷的 API**：支持通过名称播放音频，简化调用

## 2. 系统架构

### 2.1 核心组件

| 组件 | 类型 | 职责 |
|------|------|------|
| AudioConfig | ScriptableObject | 音频资源配置管理 |
| AudioMgr | MonoBehaviour | 音效管理器核心逻辑 |

### 2.2 类关系

```
AudioConfig (ScriptableObject)
  └── 管理音频列表和字典

AudioMgr (MonoBehaviour)
  ├── 单例模式实现
  ├── 音频播放控制
  ├── 音量管理
  └── 与 AudioConfig 交互
```

## 3. 实现原理

### 3.1 音频配置系统

- 使用 ScriptableObject 创建可配置的音频资产
- 在 Inspector 中可视化管理音频列表
- 运行时自动将列表转换为 Dictionary，提高查找效率

### 3.2 单例模式

- 确保全局只有一个 AudioMgr 实例
- 使用 DontDestroyOnLoad 保证跨场景不销毁
- 懒加载模式，首次访问时自动创建

### 3.3 音频播放机制

- 背景音乐：使用独立 AudioSource，支持循环播放
- 短音效：使用 PlayOneShot 方法，支持多音效重叠
- 音量计算：实际音量 = 传入音量 × 分组音量 × 主音量

## 4. 代码实现

### 4.1 AudioConfig.cs

```csharp
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
```

### 4.2 AudioMgr.cs

```csharp
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
```

## 5. 使用指南

### 5.1 配置步骤

1. **创建 AudioConfig 资产**
   - 在 Project 窗口右键点击 → Create → Audio → AudioConfig
   - 命名为 "AudioConfig"
   - 在 Inspector 中添加音频项：
     - 点击 "Size" 增加条目
     - 为每个条目设置 audioName 和 audioClip

2. **创建 AudioMgr 实例**
   - 在场景中创建一个空游戏对象，命名为 "AudioMgr"
   - 添加 AudioMgr 组件
   - 在 Inspector 中拖拽 AudioConfig 资产到 audioConfig 字段

### 5.2 代码调用示例

```csharp
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
```

## 6. 最佳实践

### 6.1 音频命名规范

- 使用小写字母和下划线，例如：`button_click`、`player_jump`、`bgm_main`
- 按功能分类，例如：`ui_` 前缀表示UI音效，`sfx_` 前缀表示游戏音效，`bgm_` 前缀表示背景音乐

### 6.2 性能优化

- 对于频繁播放的短音效，使用 `PlayOneShot` 方法
- 对于较长的音频或背景音乐，使用 `Play` 方法
- 合理设置音量，避免过大的音量影响游戏体验

### 6.3 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 音频未播放 | 音频名称不存在 | 检查 AudioConfig 中的音频名称是否正确 |
| 音量异常 | 音量设置问题 | 检查 MasterVolume、MusicVolume、SFXVolume 的值 |
| 音效重叠 | 音频源不足 | 对于需要同时播放多个音效的场景，考虑增加额外的 AudioSource |

## 7. 扩展建议

1. **音频淡入淡出**：添加淡入淡出功能，使音频切换更平滑
2. **音频优先级**：实现音频优先级系统，在音频源不足时自动管理
3. **音频池**：为短音效实现音频源对象池，提高性能
4. **音频事件系统**：结合事件系统，实现更灵活的音频触发机制
5. **音频本地化**：支持多语言音频切换

## 8. 总结

AudioMgr 提供了一个简洁而强大的音效管理解决方案，通过 ScriptableObject 实现可视化配置，单例模式确保全局访问，分层音量控制满足不同需求。它不仅简化了音频管理代码，还提高了游戏开发效率，是 Unity 项目中音效管理的理想选择。