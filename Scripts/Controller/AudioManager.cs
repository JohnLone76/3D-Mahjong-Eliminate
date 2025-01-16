using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 音频管理器：负责音频的加载和播放
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理音频资源
    /// 2. 控制音频播放
    /// 3. 管理音频设置
    /// 
    /// 设计考虑：
    /// 1. 使用对象池优化性能
    /// 2. 支持音量控制
    /// 3. 预加载常用音频
    /// </remarks>
    public class AudioManager : BaseController
    {
        private static AudioManager m_instance;
        public static AudioManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    m_instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 音频源对象池
        private BaseObjectPool<AudioSource> m_audioSourcePool;

        // 音频剪辑缓存
        private Dictionary<string, AudioClip> m_audioClips;

        // 音量设置
        private float m_sfxVolume = 1f;
        private float m_bgmVolume = 1f;

        // BGM播放器
        private AudioSource m_bgmPlayer;

        protected override void OnInit()
        {
            base.OnInit();
            m_audioClips = new Dictionary<string, AudioClip>();
            InitializeAudioPool();
            PreloadAudioClips();
        }

        /// <summary>
        /// 初始化音频源对象池
        /// </summary>
        private void InitializeAudioPool()
        {
            // 创建音频源对象池
            var audioSourcePrefab = new GameObject("AudioSourcePrefab").AddComponent<AudioSource>();
            audioSourcePrefab.playOnAwake = false;
            audioSourcePrefab.gameObject.SetActive(false);

            m_audioSourcePool = new AudioSourcePool(
                audioSourcePrefab.gameObject,
                Constants.PoolConfig.INITIAL_AUDIO_POOL_SIZE,
                Constants.PoolConfig.MAX_AUDIO_POOL_SIZE
            );

            // 销毁临时预制体
            Destroy(audioSourcePrefab.gameObject);
        }

        /// <summary>
        /// 预加载音频剪辑
        /// </summary>
        private void PreloadAudioClips()
        {
            // 预加载音效
            PreloadAudioClip("BlockClick");
            PreloadAudioClip("BlockDestroy");
            PreloadAudioClip("BackpackFull");
            PreloadAudioClip("LevelComplete");
            PreloadAudioClip("GameOver");
        }

        /// <summary>
        /// 预加载音频剪辑
        /// </summary>
        private void PreloadAudioClip(string clipName)
        {
            AudioClip clip = ResourceLoader.LoadAudio(clipName);
            if (clip != null)
            {
                m_audioClips[clipName] = clip;
            }
            else
            {
                Debug.LogError($"Failed to load audio clip: {clipName}");
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string clipName)
        {
            if (!m_audioClips.TryGetValue(clipName, out var clip))
            {
                PreloadAudioClip(clipName);
                clip = m_audioClips[clipName];
            }

            if (clip != null)
            {
                var source = m_audioSourcePool.Get();
                source.clip = clip;
                source.volume = m_sfxVolume;
                source.Play();

                // 播放完成后回收
                StartCoroutine(Utils.DelayAction(clip.length, () =>
                {
                    if (source != null)
                    {
                        m_audioSourcePool.ReturnToPool(source);
                    }
                }));
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(string clipName, bool loop = true)
        {
            if (!m_audioClips.TryGetValue(clipName, out var clip))
            {
                PreloadAudioClip(clipName);
                clip = m_audioClips[clipName];
            }

            if (clip != null)
            {
                if (m_bgmPlayer == null)
                {
                    m_bgmPlayer = gameObject.AddComponent<AudioSource>();
                    m_bgmPlayer.playOnAwake = false;
                    m_bgmPlayer.loop = loop;
                }

                m_bgmPlayer.clip = clip;
                m_bgmPlayer.volume = m_bgmVolume;
                m_bgmPlayer.Play();
            }
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (m_bgmPlayer != null && m_bgmPlayer.isPlaying)
            {
                m_bgmPlayer.Stop();
            }
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            m_sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            m_bgmVolume = Mathf.Clamp01(volume);
            if (m_bgmPlayer != null)
            {
                m_bgmPlayer.volume = m_bgmVolume;
            }
        }

        /// <summary>
        /// 清理音频资源
        /// </summary>
        public void ClearAudio()
        {
            StopBGM();
            m_audioSourcePool.Clear();
            m_audioClips.Clear();
        }
    }

    /// <summary>
    /// 音频源对象池：专门用于管理AudioSource组件的对象池
    /// </summary>
    public class AudioSourcePool : BaseObjectPool<AudioSource>
    {
        public AudioSourcePool(GameObject prefab, int initialSize, int maxSize) 
            : base(prefab, initialSize, maxSize)
        {
        }

        protected override void OnGet(AudioSource source)
        {
            base.OnGet(source);
            source.gameObject.SetActive(true);
        }

        protected override void OnReturn(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            base.OnReturn(source);
        }
    }
} 