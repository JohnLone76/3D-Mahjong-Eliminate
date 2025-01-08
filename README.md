# 3D麻将消除游戏

基于Unity开发的3D消除游戏，玩家需要在限定时间内消除所有方块。

## 游戏特点

- 3D物理效果：方块具有真实的物理效果，包括掉落和碰撞
- 独特的消除规则：支持相同数字和连续数字的匹配
- 关卡系统：动态难度调整，随关卡进度增加挑战性
- 道具系统：支持通过广告获取游戏道具

## 技术特点

- 基于Unity 2022.3.5f1c1开发
- 严格遵循MVC架构
- 使用对象池优化性能
- 完整的事件系统
- 详细的代码注释和文档

## 项目结构

```
Assets/
├── Scripts/
│   ├── Model/           # 数据模型
│   ├── View/            # UI和显示
│   ├── Controller/      # 控制器
│   └── Common/          # 通用工具
├── Prefabs/             # 预制体
├── Scenes/              # 场景文件
├── Resources/           # 资源文件
└── UI/                  # UI资源
```

## 开发进度

- [x] 第一周：基础框架搭建
  - [x] MVC框架
  - [x] 对象池系统
  - [x] 数据结构设计

- [x] 第二周：核心玩法实现
  - [x] 方块生成系统
  - [x] 物理系统集成
  - [x] 点击检测系统

- [ ] 第三周：背包系统
  - [ ] 背包UI实现
  - [ ] 方块移动动画
  - [ ] 消除机制

- [ ] 第四周：关卡系统
- [ ] 第五周：特效和动画
- [ ] 第六周：广告和优化

## 安装和使用

1. 克隆仓库
2. 使用Unity 2022.3.5f1c1打开项目
3. 打开Scenes/Main场景
4. 运行游戏

## 贡献

欢迎提交Issue和Pull Request

## 许可证

MIT License 