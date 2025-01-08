using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 资源加载工具
    /// </summary>
    public static class ResourceLoader
    {
        private static Dictionary<string, Object> m_resourceCache = new Dictionary<string, Object>();

        /// <summary>
        /// 加载资源
        /// </summary>
        public static T Load<T>(string path) where T : Object
        {
            // 检查缓存
            string cacheKey = $"{typeof(T).Name}_{path}";
            if (m_resourceCache.TryGetValue(cacheKey, out Object cachedResource))
            {
                return cachedResource as T;
            }

            // 加载资源
            T resource = Resources.Load<T>(path);
            if (resource != null)
            {
                m_resourceCache[cacheKey] = resource;
            }
            else
            {
                Debug.LogError($"找不到资源：{path}");
            }

            return resource;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        public static GameObject LoadPrefab(string name, string category = "")
        {
            string path = string.IsNullOrEmpty(category) ? 
                Constants.ResourcePaths.Prefabs.BLOCK_PATH + name :
                $"{Constants.ResourcePaths.Prefabs.BLOCK_PATH}{category}/{name}";
            
            return Load<GameObject>(path);
        }

        /// <summary>
        /// 加载UI预制体
        /// </summary>
        public static GameObject LoadUIPrefab(string name)
        {
            return Load<GameObject>(Constants.ResourcePaths.Prefabs.UI_PATH + name);
        }

        /// <summary>
        /// 加载特效预制体
        /// </summary>
        public static GameObject LoadEffectPrefab(string name)
        {
            return Load<GameObject>(Constants.ResourcePaths.Prefabs.EFFECT_PATH + name);
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static TextAsset LoadConfig(string name)
        {
            return Load<TextAsset>($"{Constants.ResourcePaths.Configs.BLOCK_CONFIG}/{name}");
        }

        /// <summary>
        /// 加载精灵
        /// </summary>
        public static Sprite LoadSprite(string name)
        {
            return Load<Sprite>(Constants.ResourcePaths.UI.SPRITE_PATH + name);
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        public static AudioClip LoadAudio(string name, bool isBGM = false)
        {
            string path = isBGM ? 
                Constants.ResourcePaths.Audio.BGM_PATH + name :
                Constants.ResourcePaths.Audio.SFX_PATH + name;
            
            return Load<AudioClip>(path);
        }

        /// <summary>
        /// 清理资源缓存
        /// </summary>
        public static void ClearCache()
        {
            m_resourceCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        public static void PreloadResources(string[] paths, System.Type type)
        {
            foreach (string path in paths)
            {
                string cacheKey = $"{type.Name}_{path}";
                if (!m_resourceCache.ContainsKey(cacheKey))
                {
                    Object resource = Resources.Load(path, type);
                    if (resource != null)
                    {
                        m_resourceCache[cacheKey] = resource;
                    }
                }
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public static ResourceRequest LoadAsync<T>(string path) where T : Object
        {
            return Resources.LoadAsync<T>(path);
        }
    }
} 