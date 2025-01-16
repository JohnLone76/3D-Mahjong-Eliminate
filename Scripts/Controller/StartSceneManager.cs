using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace MahjongProject
{
    /// <summary>
    /// 启动场景管理器：负责管理游戏启动流程
    /// </summary>
    public class StartSceneManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider m_progressBar;           // 进度条
        [SerializeField] private Text m_progressText;           // 进度文本
        [SerializeField] private Text m_tipText;                // 提示文本

        private void Start()
        {
            StartCoroutine(InitializeGame());
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private IEnumerator InitializeGame()
        {
            // 1. 初始化GameManager
            UpdateProgress(0.1f, "初始化游戏管理器...");
            GameManager.Instance.ToString();
            yield return null;

            // 2. 初始化ConfigManager
            UpdateProgress(0.2f, "加载游戏配置...");
            ConfigManager.Instance.ToString();
            yield return null;

            // 3. 加载游戏配置
            UpdateProgress(0.3f, "加载关卡配置...");
            bool configSuccess = false;
            yield return StartCoroutine(LoadConfigs(success => configSuccess = success));
            if (!configSuccess)
            {
                yield break; // 如果加载失败，中断初始化
            }

            // 4. 预热对象池
            UpdateProgress(0.5f, "初始化对象池...");
            bool poolSuccess = false;
            yield return StartCoroutine(PrewarmPools(success => poolSuccess = success));
            if (!poolSuccess)
            {
                yield break; // 如果预热失败，中断初始化
            }

            // 5. 加载音频资源
            UpdateProgress(0.7f, "加载音频资源...");
            bool audioSuccess = false;
            yield return StartCoroutine(LoadAudioResources(success => audioSuccess = success));
            if (!audioSuccess)
            {
                yield break; // 如果加载失败，中断初始化
            }

            // 6. 初始化游戏状态
            UpdateProgress(0.9f, "初始化游戏状态...");
            GameStateData.Instance.ToString();
            yield return null;

            // 7. 完成加载
            UpdateProgress(1.0f, "加载完成！");
            yield return new WaitForSeconds(0.5f);

            // 8. 切换到Menu场景
            SceneManager.LoadScene("Menu");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private IEnumerator LoadConfigs(System.Action<bool> callback)
        {
            bool success = true;

            // 加载方块配置
            UpdateProgress(0.35f, "加载方块配置...");
            try
            {
                // 在这里添加实际的方块配置加载代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载方块配置失败: {e.Message}");
                ShowError("配置加载失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 加载关卡配置
            UpdateProgress(0.4f, "加载关卡配置...");
            try
            {
                // 在这里添加实际的关卡配置加载代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载关卡配置失败: {e.Message}");
                ShowError("配置加载失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 加载游戏设置
            UpdateProgress(0.45f, "加载游戏设置...");
            try
            {
                // 在这里添加实际的游戏设置加载代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载游戏设置失败: {e.Message}");
                ShowError("配置加载失败，请重试");
                success = false;
            }
            yield return null;

            callback?.Invoke(success);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        private IEnumerator PrewarmPools(System.Action<bool> callback)
        {
            bool success = true;

            // 预热方块对象池
            UpdateProgress(0.55f, "预热方块对象池...");
            try
            {
                BlockPool.Instance.Prewarm();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"预热方块对象池失败: {e.Message}");
                ShowError("资源初始化失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 预热特效对象池
            UpdateProgress(0.6f, "预热特效对象池...");
            try
            {
                EffectPool.Instance.Prewarm();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"预热特效对象池失败: {e.Message}");
                ShowError("资源初始化失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 预热音频对象池
            UpdateProgress(0.65f, "预热音频对象池...");
            try
            {
                // 在这里添加实际的音频对象池预热代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"预热音频对象池失败: {e.Message}");
                ShowError("资源初始化失败，请重试");
                success = false;
            }
            yield return null;

            callback?.Invoke(success);
        }

        /// <summary>
        /// 加载音频资源
        /// </summary>
        private IEnumerator LoadAudioResources(System.Action<bool> callback)
        {
            bool success = true;

            // 加载音效
            UpdateProgress(0.75f, "加载音效...");
            try
            {
                // 在这里添加实际的音效加载代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载音效失败: {e.Message}");
                ShowError("音频资源加载失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 加载背景音乐
            UpdateProgress(0.8f, "加载背景音乐...");
            try
            {
                // 在这里添加实际的背景音乐加载代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载背景音乐失败: {e.Message}");
                ShowError("音频资源加载失败，请重试");
                success = false;
            }
            yield return null;

            if (!success)
            {
                callback?.Invoke(false);
                yield break;
            }

            // 初始化音频设置
            UpdateProgress(0.85f, "初始化音频设置...");
            try
            {
                // 在这里添加实际的音频设置初始化代码
            }
            catch (System.Exception e)
            {
                Debug.LogError($"初始化音频设置失败: {e.Message}");
                ShowError("音频资源加载失败，请重试");
                success = false;
            }
            yield return null;

            callback?.Invoke(success);
        }

        /// <summary>
        /// 更新进度显示
        /// </summary>
        private void UpdateProgress(float progress, string message)
        {
            if (m_progressBar != null)
            {
                m_progressBar.value = progress;
            }

            if (m_progressText != null)
            {
                m_progressText.text = $"{(progress * 100):F0}%";
            }

            if (m_tipText != null)
            {
                m_tipText.text = message;
            }
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void ShowError(string message)
        {
            if (m_tipText != null)
            {
                m_tipText.text = $"<color=red>{message}</color>";
            }
            // TODO: 添加重试按钮或自动重试逻辑
        }
    }
} 