using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace MahjongProject.Editor
{
    /// <summary>
    /// 关卡配置编辑器：用于编辑和管理关卡配置
    /// </summary>
    public class LevelConfigEditor : EditorWindow
    {
        private Vector2 m_scrollPosition;
        private List<LevelConfig> m_levels = new List<LevelConfig>();
        private string m_configPath;
        private Dictionary<int, bool> m_blockTypesFoldouts = new Dictionary<int, bool>();  // 记录每个关卡的折叠状态

        [MenuItem("Tools/Level Config Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelConfigEditor>("Level Config Editor");
        }

        private void OnEnable()
        {
            m_configPath = Path.Combine(Application.dataPath, "Resources/Configs/LevelConfig.json");
            LoadLevelConfigs();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // 工具栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("添加关卡", EditorStyles.toolbarButton))
            {
                AddNewLevel();
            }
            if (GUILayout.Button("保存", EditorStyles.toolbarButton))
            {
                SaveLevelConfigs();
            }
            if (GUILayout.Button("加载", EditorStyles.toolbarButton))
            {
                LoadLevelConfigs();
            }
            EditorGUILayout.EndHorizontal();

            // 关卡列表
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            for (int i = 0; i < m_levels.Count; i++)
            {
                DrawLevelConfig(i);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelConfig(int index)
        {
            var level = m_levels[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 标题栏
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Level {level.LevelNumber}", EditorStyles.boldLabel);

            // 添加应用推荐值按钮
            if (GUILayout.Button("应用推荐值", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("应用推荐值",
                    "这将根据关卡编号覆盖当前值为推荐值。是否继续？",
                    "确定", "取消"))
                {
                    level.ApplyRecommendedValues();
                    level.ApplyRecommendedBlockTypes();
                }
            }

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("删除关卡", 
                    $"确定要删除关卡 {level.LevelNumber} 吗？", "确定", "取消"))
                {
                    m_levels.RemoveAt(index);
                    m_blockTypesFoldouts.Remove(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            // 基本属性
            EditorGUI.BeginChangeCheck();
            int newLevelId = EditorGUILayout.IntField("关卡ID", level.LevelId);
            if (EditorGUI.EndChangeCheck())
            {
                level.SetLevelId(newLevelId);
            }

            EditorGUI.BeginChangeCheck();
            int newLevelNumber = EditorGUILayout.IntField("关卡编号", level.LevelNumber);
            if (EditorGUI.EndChangeCheck())
            {
                level.SetLevelNumber(newLevelNumber);
            }

            // 显示时间限制（带推荐值对比）
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            float newTimeLimit = EditorGUILayout.FloatField("时间限制(秒)", level.TimeLimit);
            if (EditorGUI.EndChangeCheck())
            {
                level.SetTimeLimit(newTimeLimit);
            }
            EditorGUILayout.LabelField($"(推荐值: {level.GetRecommendedTimeLimit()}秒)", 
                EditorStyles.miniLabel, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            // 显示方块数量（带推荐值对比）
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            int newBlockCount = EditorGUILayout.IntField("方块数量", level.BlockCount);
            if (EditorGUI.EndChangeCheck())
            {
                level.SetBlockCount(newBlockCount);
            }
            EditorGUILayout.LabelField($"(推荐值: {level.GetRecommendedBlockCount()})", 
                EditorStyles.miniLabel, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            // 可用方块类型（带折叠功能）
            EditorGUILayout.Space();
            if (!m_blockTypesFoldouts.ContainsKey(index))
            {
                m_blockTypesFoldouts[index] = false;
            }

            EditorGUILayout.BeginHorizontal();
            m_blockTypesFoldouts[index] = EditorGUILayout.Foldout(m_blockTypesFoldouts[index], 
                $"可用方块类型 ({level.AvailableBlockTypes?.Count ?? 0})", true);

            // 添加应用推荐方块类型按钮
            if (GUILayout.Button("应用推荐方块", GUILayout.Width(150)))
            {
                if (EditorUtility.DisplayDialog("应用推荐方块类型",
                    "这将覆盖当前方块类型为推荐类型。是否继续？",
                    "确定", "取消"))
                {
                    level.ApplyRecommendedBlockTypes();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (m_blockTypesFoldouts[index])
            {
                EditorGUI.indentLevel++;
                
                var blockTypes = level.AvailableBlockTypes;
                if (blockTypes == null)
                {
                    blockTypes = new List<int>();
                    level.SetAvailableBlockTypes(blockTypes);
                }

                for (int i = 0; i < blockTypes.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // 方块类型值
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    int newType = EditorGUILayout.IntField($"类型 {i + 1}", blockTypes[i]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        blockTypes[i] = newType;
                        level.SetAvailableBlockTypes(blockTypes);
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        blockTypes.RemoveAt(i);
                        level.SetAvailableBlockTypes(blockTypes);
                        i--;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    // 预制体路径
                    EditorGUI.BeginChangeCheck();
                    string prefabPath = EditorGUILayout.TextField("预制体路径", level.GetBlockPrefabPath(blockTypes[i]));
                    if (EditorGUI.EndChangeCheck())
                    {
                        level.SetBlockPrefabPath(blockTypes[i], prefabPath);
                    }

                    // 添加预制体选择按钮
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" ");
                    if (GUILayout.Button("选择预制体", GUILayout.Width(100)))
                    {
                        GameObject selectedPrefab = Selection.activeObject as GameObject;
                        if (selectedPrefab != null)
                        {
                            // 获取预制体的路径
                            string assetPath = AssetDatabase.GetAssetPath(selectedPrefab);
                            if (assetPath.StartsWith("Assets/Resources/"))
                            {
                                // 只保留Resources文件夹之后的路径，并移除.prefab后缀
                                string resourcePath = assetPath.Substring("Assets/Resources/".Length);
                                resourcePath = Path.ChangeExtension(resourcePath, null);
                                level.SetBlockPrefabPath(blockTypes[i], resourcePath);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("无效路径", 
                                    "请从Resources文件夹中选择预制体。", "确定");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("未选择预制体", 
                                "请先在Project窗口中选择一个预制体。", "确定");
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("添加方块类型"))
                {
                    blockTypes.Add(0);
                    level.SetAvailableBlockTypes(blockTypes);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void AddNewLevel()
        {
            var newLevel = new LevelConfig(m_levels.Count + 1);
            
            // 如果有前一个关卡，复制其配置
            if (m_levels.Count > 0)
            {
                var prevLevel = m_levels[m_levels.Count - 1];
                newLevel.SetLevelId(prevLevel.LevelId + 1);
                newLevel.SetLevelNumber(prevLevel.LevelNumber + 1);
                newLevel.SetTimeLimit(prevLevel.TimeLimit);
                newLevel.SetBlockCount(prevLevel.BlockCount);
                
                // 复制方块类型列表
                if (prevLevel.AvailableBlockTypes != null)
                {
                    newLevel.SetAvailableBlockTypes(new List<int>(prevLevel.AvailableBlockTypes));
                }
            }

            m_levels.Add(newLevel);
            m_blockTypesFoldouts[m_levels.Count - 1] = false; // 新关卡默认隐藏方块类型列表
        }

        private void LoadLevelConfigs()
        {
            if (File.Exists(m_configPath))
            {
                string json = File.ReadAllText(m_configPath);
                var wrapper = JsonUtility.FromJson<LevelConfigWrapper>(json);
                m_levels = new List<LevelConfig>(wrapper.levels);
                
                // 重置所有折叠状态
                m_blockTypesFoldouts.Clear();
                for (int i = 0; i < m_levels.Count; i++)
                {
                    m_blockTypesFoldouts[i] = false;
                    // 确保反序列化后恢复预制体路径
                    m_levels[i].OnAfterDeserialize();
                }
            }
            else
            {
                m_levels = new List<LevelConfig>();
                m_blockTypesFoldouts.Clear();
            }
        }

        private void SaveLevelConfigs()
        {
            // 确保所有关卡的预制体路径都已更新到序列化列表中
            foreach (var level in m_levels)
            {
                level.UpdateSerializedPrefabPaths();
            }

            var wrapper = new LevelConfigWrapper { levels = m_levels.ToArray() };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(m_configPath, json);
            AssetDatabase.Refresh();
            Debug.Log("关卡配置保存成功！");
        }
    }

    [System.Serializable]
    public class LevelConfigWrapper
    {
        public LevelConfig[] levels;
    }
} 