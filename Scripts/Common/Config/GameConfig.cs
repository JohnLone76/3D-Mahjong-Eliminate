using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 游戏配置数据
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        // 音频设置
        public float MusicVolume = Constants.GameSettings.MUSIC_VOLUME;
        public float SfxVolume = Constants.GameSettings.SFX_VOLUME;
        public bool VibrationEnabled = Constants.GameSettings.VIBRATION_ENABLED;

        // 游戏设置
        public bool TutorialEnabled = Constants.GameSettings.TUTORIAL_ENABLED;
        public float CameraHeight = Constants.CAMERA_HEIGHT;
        public float CameraAngle = Constants.CAMERA_ANGLE;

        // 物理设置
        public float GravityScale = Constants.Physics.GRAVITY_SCALE;
        public float BlockDrag = Constants.Physics.DRAG;
        public float BlockAngularDrag = Constants.Physics.ANGULAR_DRAG;
        public float BlockBounce = Constants.Physics.BOUNCE;
        public float BlockFriction = Constants.Physics.FRICTION;

        // 动画设置
        public float BlockMoveSpeed = Constants.BLOCK_MOVE_SPEED;
        public float BlockRotateSpeed = Constants.BLOCK_ROTATE_SPEED;
        public float MoveDuration = Constants.Animation.MOVE_DURATION;
        public float FadeDuration = Constants.Animation.FADE_DURATION;

        // UI设置
        public float UiScale = Constants.UI.UI_SCALE;
        public int FontSize = Constants.UI.FONT_SIZE;
        public float SafeAreaPadding = Constants.UI.SAFE_AREA_PADDING;

        /// <summary>
        /// 加载配置
        /// </summary>
        public static GameConfig Load()
        {
            string json = PlayerPrefs.GetString(Constants.GameSettings.SAVE_KEY_PREFIX + "GameConfig");
            if (string.IsNullOrEmpty(json))
            {
                return new GameConfig();
            }
            return JsonUtility.FromJson<GameConfig>(json);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(Constants.GameSettings.SAVE_KEY_PREFIX + "GameConfig", json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 重置配置
        /// </summary>
        public void Reset()
        {
            MusicVolume = Constants.GameSettings.MUSIC_VOLUME;
            SfxVolume = Constants.GameSettings.SFX_VOLUME;
            VibrationEnabled = Constants.GameSettings.VIBRATION_ENABLED;
            TutorialEnabled = Constants.GameSettings.TUTORIAL_ENABLED;
            CameraHeight = Constants.CAMERA_HEIGHT;
            CameraAngle = Constants.CAMERA_ANGLE;
            GravityScale = Constants.Physics.GRAVITY_SCALE;
            BlockDrag = Constants.Physics.DRAG;
            BlockAngularDrag = Constants.Physics.ANGULAR_DRAG;
            BlockBounce = Constants.Physics.BOUNCE;
            BlockFriction = Constants.Physics.FRICTION;
            BlockMoveSpeed = Constants.BLOCK_MOVE_SPEED;
            BlockRotateSpeed = Constants.BLOCK_ROTATE_SPEED;
            MoveDuration = Constants.Animation.MOVE_DURATION;
            FadeDuration = Constants.Animation.FADE_DURATION;
            UiScale = Constants.UI.UI_SCALE;
            FontSize = Constants.UI.FONT_SIZE;
            SafeAreaPadding = Constants.UI.SAFE_AREA_PADDING;
        }
    }
} 