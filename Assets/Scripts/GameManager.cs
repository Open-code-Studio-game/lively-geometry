// LivelyGeometry.GameManager
// 单例：构建关卡、维护玩家、桥的引用；处理点击 → 寻路/旋转桥的派发；
// 监听玩家到达格子、检查胜利、对外暴露重置接口。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LivelyGeometry
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("关卡")]
        public LevelData level;
        public bool autoBuildOnStart = true;

        [Header("引用")]
        public Player player;
        public List<Bridge> allBridges = new List<Bridge>();
        public Tile[,] tileMap;        // 格子 → Tile 组件

        [Header("状态")]
        public bool won = false;

        public enum GameState { Playing, Win }
        public GameState state = GameState.Playing;

        // ---------- 单例 ----------
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (autoBuildOnStart)
            {
                BuildLevel();
            }
        }

        public void BuildLevel()
        {
            // 清旧
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            if (level == null) level = LevelData.CreateSampleLevel();
            var builder = new LevelBuilder();
            builder.Build(level, transform);

            // 收集 Tile / Bridge
            tileMap = new Tile[level.width, level.height];
            foreach (var t in GetComponentsInChildren<Tile>(true))
            {
                if (t.cell.x >= 0 && t.cell.x < level.width && t.cell.y >= 0 && t.cell.y < level.height)
                    tileMap[t.cell.x, t.cell.y] = t;
            }
            allBridges.Clear();
            allBridges.AddRange(GetComponentsInChildren<Bridge>(true));

            // 玩家
            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(transform, false);
            player = playerGo.AddComponent<Player>();
            player.Init(level.startCell);

            // 灯光
            SetupLighting();

            // 摄像机
            SetupCamera();

            // HUD
            GameHUD.Instance?.ShowLevelName(level.levelName);

            won = false;
            state = GameState.Playing;
        }

        void SetupLighting()
        {
            // 移除已经存在的灯光
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) Destroy(l.gameObject);

            var go = new GameObject("Sun");
            go.transform.rotation = Quaternion.Euler(48f, 32f, 0f);
            var sun = go.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = Palette.SunWarm;
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;

            // 全局环境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = Palette.SkyTop;
            RenderSettings.ambientEquatorColor = Palette.SkyBottom;
            RenderSettings.ambientGroundColor  = new Color(0.30f, 0.25f, 0.20f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Palette.FogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 8f;
            RenderSettings.fogEndDistance = 26f;
        }

        void SetupCamera()
        {
            // 找或建摄像机
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("MainCamera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            var camRig = cam.GetComponent<CameraRig>();
            if (camRig == null) camRig = cam.gameObject.AddComponent<CameraRig>();
            camRig.target = player != null ? player.transform : null;
            camRig.Setup();
        }

        // ---------- 输入派发 ----------
        void Update()
        {
            if (state == GameState.Win)
            {
                // 胜利后只接受 R 重启
                var kb = Keyboard.current;
                if (kb != null && kb.rKey.wasPressedThisFrame)
                {
                    GameHUD.Instance?.HideWinPanel();
                    BuildLevel();
                }
                return;
            }

            // 鼠标左键
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                HandleClick(mouse.position.ReadValue());
            }

            // 键盘 R 重启
            var k = Keyboard.current;
            if (k != null && k.rKey.wasPressedThisFrame)
            {
                BuildLevel();
            }
            // 空格也可重启（防误操作）
            if (k != null && k.spaceKey.wasPressedThisFrame && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed))
            {
                BuildLevel();
            }
        }

        void HandleClick(Vector2 screenPos)
        {
            // 不能点到 UI 上（避免按钮也被派发）
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            var cam = Camera.main;
            if (cam == null) return;
            Ray ray = cam.ScreenPointToRay(screenPos);
            // 优先命中桥
            if (Physics.Raycast(ray, out var hit, 200f))
            {
                var bridge = hit.collider.GetComponentInParent<Bridge>();
                if (bridge != null)
                {
                    bridge.Toggle();
                    return;
                }
                // 否则判断是否点击了玩家附近的格子
                var tile = hit.collider.GetComponentInParent<Tile>();
                if (tile != null)
                {
                    TryWalkToTile(tile);
                    return;
                }
            }
        }

        void TryWalkToTile(Tile tile)
        {
            if (player == null) return;
            // 仅允许点击与玩家相邻或更近的可走格子
            if (!level.IsWalkable(tile.cell.x, tile.cell.y)) return;
            player.TryWalkTo(tile.cell);
        }

        public void OnPlayerArrivedCell(Vector2Int cell)
        {
            if (won) return;
            if (cell == level.goalCell)
            {
                won = true;
                state = GameState.Win;
                GameHUD.Instance?.ShowWinPanel();
            }
        }
    }
}
