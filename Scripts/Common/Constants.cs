namespace MahjongProject
{
    /// <summary>
    /// 游戏常量
    /// </summary>
    public static class Constants
    {
        // 场景相关
        public const float SCENE_WIDTH = 10f;
        public const float SCENE_LENGTH = 20f;
        public const float BLOCK_DROP_HEIGHT = 10f;
        public const float CAMERA_HEIGHT = 15f;
        public const float CAMERA_ANGLE = 45f;

        // 游戏相关
        public const int MAX_BACKPACK_SIZE = 6;
        public const int EXTENDED_BACKPACK_SIZE = 8;
        public const int LEVEL_TIME_LIMIT = 600; // 10分钟
        public const int EXTENDED_TIME = 300; // 5分钟
        public const float BLOCK_MOVE_SPEED = 5f;
        public const float BLOCK_ROTATE_SPEED = 180f;
        public const float BLOCK_SCALE = 1f;

        // 物理相关
        public static class Physics
        {
            public const float GRAVITY_SCALE = 1f;
            public const float DRAG = 0.5f;
            public const float ANGULAR_DRAG = 0.5f;
            public const float BOUNCE = 0.3f;
            public const float FRICTION = 0.6f;
            public const bool USE_GRAVITY = true;
            public const bool IS_KINEMATIC = false;
        }

        // 动画相关
        public static class Animation
        {
            public const float MOVE_DURATION = 0.5f;
            public const float FADE_DURATION = 0.3f;
            public const float SHAKE_DURATION = 0.2f;
            public const float SHAKE_STRENGTH = 0.1f;
            public const float SCALE_DURATION = 0.2f;
            public const float ROTATE_DURATION = 0.3f;
        }

        // UI相关
        public static class UI
        {
            public const float FADE_TIME = 0.3f;
            public const float POPUP_TIME = 0.5f;
            public const int FONT_SIZE = 24;
            public const float UI_SCALE = 1f;
            public const float SAFE_AREA_PADDING = 20f;
        }

        // 事件名称
        public static class EventNames
        {
            // 游戏流程相关
            public const string GAME_START = "GameStart";
            public const string GAME_PAUSE = "GamePause";
            public const string GAME_RESUME = "GameResume";
            public const string GAME_OVER = "GameOver";
            public const string LEVEL_STARTED = "LevelStarted";    // 关卡开始事件
            public const string LEVEL_COMPLETE = "LevelComplete";

            // 方块相关
            public const string BLOCK_CLICKED = "BlockClicked";
            public const string BLOCK_ADDED_TO_BACKPACK = "BlockAddedToBackpack";
            public const string BLOCKS_ELIMINATED = "BlocksEliminated";
            public const string BLOCK_MOVE_START = "BlockMoveStart";
            public const string BLOCK_MOVE_END = "BlockMoveEnd";

            // 背包相关
            public const string BACKPACK_FULL = "BackpackFull";
            public const string BACKPACK_EXTENDED = "BackpackExtended";
            public const string BACKPACK_ITEM_ADDED = "BackpackItemAdded";
            public const string BACKPACK_ITEM_REMOVED = "BackpackItemRemoved";

            // 广告相关
            public const string AD_START = "AdStart";
            public const string AD_COMPLETE = "AdComplete";
            public const string AD_FAILED = "AdFailed";
            public const string AD_SKIPPED = "AdSkipped";

            // UI相关
            public const string UI_BUTTON_CLICK = "UIButtonClick";
            public const string UI_POPUP_OPEN = "UIPopupOpen";
            public const string UI_POPUP_CLOSE = "UIPopupClose";
        }

        // 资源路径
        public static class ResourcePaths
        {
            // 预制体路径
            public static class Prefabs
            {
                public const string BLOCK_PATH = "Prefabs/Blocks/";
                public const string EFFECT_PATH = "Prefabs/Effects/";
                public const string UI_PATH = "Prefabs/UI/";
                public const string POPUP_PATH = "Prefabs/UI/Popups/";
            }

            // UI资源路径
            public static class UI
            {
                public const string SPRITE_PATH = "UI/Sprites/";
                public const string ANIMATION_PATH = "UI/Animations/";
                public const string FONT_PATH = "UI/Fonts/";
            }

            // 配置文件路径
            public static class Configs
            {
                public const string BLOCK_CONFIG = "Configs/BlockConfig";
                public const string LEVEL_CONFIG = "Configs/LevelConfig";
                public const string GAME_CONFIG = "Configs/GameConfig";
            }

            // 音效路径
            public static class Audio
            {
                public const string BGM_PATH = "Audio/BGM/";
                public const string SFX_PATH = "Audio/SFX/";
                public const string UI_SOUND_PATH = "Audio/UI/";
            }
        }

        // 动画参数
        public static class AnimParams
        {
            public const string IS_ELIMINATING = "IsEliminating";
            public const string IS_DROPPING = "IsDropping";
            public const string IS_MOVING = "IsMoving";
            public const string IS_ROTATING = "IsRotating";
            public const string TRIGGER_SHAKE = "TriggerShake";
            public const string TRIGGER_POPUP = "TriggerPopup";
        }

        // 对象池配置
        public static class PoolConfig
        {
            public const int INITIAL_BLOCK_POOL_SIZE = 50;  // 200/4
            public const int MAX_BLOCK_POOL_SIZE = 250;     // 200+50
            public const int INITIAL_EFFECT_POOL_SIZE = 10;
            public const int MAX_EFFECT_POOL_SIZE = 30;
            public const float POOL_EXPAND_RATE = 1.5f;
            public const int POOL_EXPAND_MIN = 10;
        }

        // 游戏设置
        public static class GameSettings
        {
            public const float MUSIC_VOLUME = 0.7f;
            public const float SFX_VOLUME = 1.0f;
            public const bool VIBRATION_ENABLED = true;
            public const bool TUTORIAL_ENABLED = true;
            public const string SAVE_KEY_PREFIX = "MahjongGame_";
            public const int AUTO_SAVE_INTERVAL = 60;  // 秒

            /// <summary>
            /// 存档键名
            /// </summary>
            public const string SAVE_KEY = "GameSave";

            /// <summary>
            /// 当前存档版本号
            /// </summary>
            public const int SAVE_VERSION = 1;

            /// <summary>
            /// 广告延长时间（秒）
            /// </summary>
            public const float EXTENDED_TIME = 300f;  // 5分钟

            /// <summary>
            /// 背包扩展格数
            /// </summary>
            public const int EXTENDED_SLOTS = 2;
        }

        // 调试设置
        public static class DebugSettings
        {
            public const bool SHOW_FPS = true;
            public const bool SHOW_MEMORY = true;
            public const bool SHOW_POOL_INFO = true;
            public const bool LOG_EVENTS = true;
            public const string LOG_PREFIX = "[麻将消除]";
        }
    }
} 