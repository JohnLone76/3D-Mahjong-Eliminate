using UnityEngine;
using System;
using System.Collections;

namespace MahjongProject
{
    /// <summary>
    /// 通用工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>协程迭代器</returns>
        public static IEnumerator DelayAction(float delay, Action action)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            action?.Invoke();
        }

        /// <summary>
        /// 延迟一帧执行
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>协程迭代器</returns>
        public static IEnumerator DelayOneFrame(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        /// <summary>
        /// 等待条件满足后执行
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="action">要执行的操作</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>协程迭代器</returns>
        public static IEnumerator WaitUntil(Func<bool> condition, Action action, float timeout = 5f)
        {
            float timer = 0f;
            while (!condition() && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            action?.Invoke();
        }

        /// <summary>
        /// 平滑插值
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="end">结束值</param>
        /// <param name="duration">持续时间</param>
        /// <param name="onUpdate">更新回调</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>协程迭代器</returns>
        public static IEnumerator SmoothLerp(float start, float end, float duration, Action<float> onUpdate, Action onComplete = null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float value = Mathf.Lerp(start, end, t);
                onUpdate?.Invoke(value);
                yield return null;
            }
            onUpdate?.Invoke(end);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 震动效果
        /// </summary>
        /// <param name="target">目标Transform</param>
        /// <param name="duration">持续时间</param>
        /// <param name="strength">震动强度</param>
        /// <returns>协程迭代器</returns>
        public static IEnumerator Shake(Transform target, float duration, float strength)
        {
            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = originalPosition.x + UnityEngine.Random.Range(-1f, 1f) * strength;
                float y = originalPosition.y + UnityEngine.Random.Range(-1f, 1f) * strength;
                target.localPosition = new Vector3(x, y, originalPosition.z);
                yield return null;
            }

            target.localPosition = originalPosition;
        }
    }
} 