// LivelyGeometry.LevelBuilder
// 根据 LevelData 在场景里实例化方块、桥、装饰。
// 所有几何全部基于 UnityEngine.PrimitiveType.Cube 拼装，零美术资源也能跑。
using System.Collections.Generic;
using UnityEngine;

namespace LivelyGeometry
{
    public class LevelBuilder
    {
        public const float TileSize = 1.0f;
        public const float TileHeight = 0.4f; // 普通方块高度
        public const float WaterDepth = 0.2f;

        public static Vector3 CellToWorld(int x, int z, float y = 0f)
        {
            // 居中网格，x 增大往右、z 增大往前（+Z 朝向相机）
            float wx = (x - (GameManager.Instance?.level.width ?? 0) * 0.5f + 0.5f) * TileSize;
            float wz = (z - (GameManager.Instance?.level.height ?? 0) * 0.5f + 0.5f) * TileSize;
            return new Vector3(wx, y, wz);
        }

        public static Vector2Int WorldToCell(Vector3 world)
        {
            var gm = GameManager.Instance;
            int w = gm != null ? gm.level.width  : 7;
            int h = gm != null ? gm.level.height : 7;
            int x = Mathf.RoundToInt(world.x / TileSize + w * 0.5f - 0.5f);
            int z = Mathf.RoundToInt(world.z / TileSize + h * 0.5f - 0.5f);
            return new Vector2Int(x, z);
        }

        public GameObject Build(LevelData level, Transform parent)
        {
            // 根节点
            var root = new GameObject("Level").transform;
            root.SetParent(parent, false);

            // ---------- 地面方块 ----------
            for (int x = 0; x < level.width; x++)
            {
                for (int z = 0; z < level.height; z++)
                {
                    var t = level.Get(x, z);
                    if (t == TileType.Empty) continue;
                    BuildTile(t, x, z, root);
                }
            }

            // ---------- 桥 ----------
            foreach (var b in level.bridges)
            {
                BuildBridge(b, root);
            }

            // ---------- 装饰 ----------
            foreach (var d in level.decorations)
            {
                BuildDecoration(d, root);
            }

            // ---------- 起点标记（小三角） ----------
            BuildStartMarker(level.startCell, root);

            return root.gameObject;
        }

        GameObject BuildTile(TileType t, int x, int z, Transform parent)
        {
            var world = CellToWorld(x, z);
            switch (t)
            {
                case TileType.Grass:
                    return MakeBlock(parent, "Grass_" + x + "_" + z, x, z, world, TileHeight,
                        MaterialFactory.CreateGrass(), TileType.Grass, false);
                case TileType.Dirt:
                    return MakeBlock(parent, "Dirt_" + x + "_" + z, x, z, world, TileHeight * 0.6f,
                        MaterialFactory.CreateDirt(), TileType.Dirt, false);
                case TileType.Stone:
                    return MakeBlock(parent, "Stone_" + x + "_" + z, x, z, world, TileHeight * 0.8f,
                        MaterialFactory.Create("Mat_Stone", Palette.Stone, 0.2f, 0.05f), TileType.Stone, false);
                case TileType.Goal:
                    return MakeGoalBlock(parent, x, z, world);
                case TileType.Water:
                    return MakeWaterBlock(parent, x, z, world);
                default:
                    return null;
            }
        }

        GameObject MakeBlock(Transform parent, string name, int cx, int cz, Vector3 world, float height, Material mat, TileType type, bool isBridge)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position   = new Vector3(world.x, height * 0.5f, world.z);
            go.transform.localScale = new Vector3(TileSize * 0.98f, height, TileSize * 0.98f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            // 关闭阴影投射的接收（在等距视角下不必要的开销）
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<MeshRenderer>().receiveShadows = true;
            // 记录格子坐标
            var tile = go.AddComponent<Tile>();
            tile.cell = new Vector2Int(cx, cz);
            tile.type = type;
            tile.isBridge = isBridge;
            return go;
        }

        GameObject MakeWaterBlock(Transform parent, int x, int z, Vector3 world)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Water_" + x + "_" + z;
            go.transform.SetParent(parent, false);
            go.transform.position   = new Vector3(world.x, -WaterDepth * 0.5f, world.z);
            go.transform.localScale = new Vector3(TileSize * 0.99f, WaterDepth, TileSize * 0.99f);
            go.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.CreateWater();
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // 浮动动画
            var bob = go.AddComponent<WaterBob>();
            bob.baseY = go.transform.position.y;
            return go;
        }

        GameObject MakeGoalBlock(Transform parent, int x, int z, Vector3 world)
        {
            var root = new GameObject("Goal_" + x + "_" + z).transform;
            root.SetParent(parent, false);
            root.position = new Vector3(world.x, 0f, world.z);

            // 底座
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = "GoalBase";
            baseGo.transform.SetParent(root, false);
            baseGo.transform.position   = new Vector3(0, TileHeight * 0.5f, 0);
            baseGo.transform.localScale = new Vector3(TileSize * 0.98f, TileHeight, TileSize * 0.98f);
            baseGo.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_GoalBase", Palette.GrassSide, 0.15f, 0f);
            baseGo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 顶面金色方块
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "GoalTop";
            top.transform.SetParent(root, false);
            top.transform.position   = new Vector3(0, TileHeight + 0.25f, 0);
            top.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            top.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Goal", Palette.Goal, 0.7f, 0.6f);
            top.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 小尖角（金字塔）
            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "GoalTip";
            tip.transform.SetParent(root, false);
            tip.transform.position   = new Vector3(0, TileHeight + 0.7f, 0);
            tip.transform.localScale = new Vector3(0.28f, 0.4f, 0.28f);
            tip.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_GoalTip", Palette.GoalGlow, 0.6f, 0.2f);

            // 上下的呼吸
            var pulse = root.gameObject.AddComponent<GoalPulse>();
            pulse.tip = tip.transform;
            pulse.top = top.transform;

            // Tile 组件
            var tile = root.gameObject.AddComponent<Tile>();
            tile.cell = new Vector2Int(x, z);
            tile.type = TileType.Goal;

            // 碰撞由 baseGo 的 collider 提供
            return root.gameObject;
        }

        GameObject BuildBridge(BridgeDef b, Transform parent)
        {
            var go = new GameObject("Bridge");
            go.transform.SetParent(parent, false);

            // 桥由两块板组成，可整体旋转 90°
            var matBoard = MaterialFactory.Create("Mat_Bridge", Palette.Bridge, 0.25f, 0.05f);
            var matSide  = MaterialFactory.Create("Mat_BridgeSide", Palette.BridgePlank, 0.25f, 0.05f);

            // 第一块板
            var p1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p1.name = "Bridge_A";
            p1.transform.SetParent(go.transform, false);
            p1.transform.position   = Vector3.zero;
            p1.transform.localScale = new Vector3(TileSize * 0.95f, 0.18f, TileSize * 0.45f);
            p1.GetComponent<MeshRenderer>().sharedMaterial = matBoard;
            p1.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 第二块板
            var p2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p2.name = "Bridge_B";
            p2.transform.SetParent(go.transform, false);
            p2.transform.position   = new Vector3(TileSize, 0, 0);
            p2.transform.localScale = new Vector3(TileSize * 0.95f, 0.18f, TileSize * 0.45f);
            p2.GetComponent<MeshRenderer>().sharedMaterial = matBoard;
            p2.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 桥的格子由 Bridge.cs 维护
            var bridge = go.AddComponent<Bridge>();
            bridge.cellA = b.from;
            bridge.cellB = b.to;
            bridge.moveTime = b.moveTime;

            // 桥立柱
            for (int i = 0; i < 2; i++)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "Post_" + i;
                post.transform.SetParent(go.transform, false);
                post.transform.position   = new Vector3(i * TileSize, -0.18f, 0);
                post.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
                post.GetComponent<MeshRenderer>().sharedMaterial = matSide;
                post.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // 初始放置
            go.transform.position = new Vector3(
                (b.from.x + b.to.x) * 0.5f - (GameManager.Instance.level.width  - 1) * 0.5f,
                0.05f,
                (b.from.y + b.to.y) * 0.5f - (GameManager.Instance.level.height - 1) * 0.5f
            );

            return go;
        }

        GameObject BuildDecoration(DecorationDef d, Transform parent)
        {
            var world = CellToWorld(d.cell.x, d.cell.y);
            switch (d.kind)
            {
                case DecorationDef.Kind.Tree:  return BuildTree(parent, d.cell, world);
                case DecorationDef.Kind.Rock:  return BuildRock(parent, d.cell, world);
                case DecorationDef.Kind.Lamp:  return BuildLamp(parent, d.cell, world);
                case DecorationDef.Kind.Bench: return BuildBench(parent, d.cell, world);
                case DecorationDef.Kind.Bush:  return BuildBush(parent, d.cell, world);
            }
            return null;
        }

        GameObject BuildTree(Transform parent, Vector2Int cell, Vector3 world)
        {
            var go = new GameObject("Tree").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0, world.z);

            // 树干
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trunk.name = "Trunk";
            trunk.transform.SetParent(go, false);
            trunk.transform.position   = new Vector3(0, 0.45f, 0);
            trunk.transform.localScale = new Vector3(0.22f, 0.9f, 0.22f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Trunk", Palette.Trunk, 0.05f, 0f);
            trunk.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 三层树冠（错位叠）
            float[] sizes = { 0.85f, 0.65f, 0.45f };
            float[] heights = { 1.2f, 1.7f, 2.1f };
            Color[] leafCols = { Palette.LeafLight, Palette.LeafDark, Palette.LeafLight };
            for (int i = 0; i < 3; i++)
            {
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leaf.name = "Leaf_" + i;
                leaf.transform.SetParent(go, false);
                leaf.transform.position   = new Vector3(0, heights[i], 0);
                leaf.transform.localScale = new Vector3(sizes[i], sizes[i] * 0.7f, sizes[i]);
                leaf.transform.rotation   = Quaternion.Euler(0f, i * 25f, 0f);
                leaf.GetComponent<MeshRenderer>().sharedMaterial =
                    MaterialFactory.Create("Mat_Leaf_" + i, leafCols[i], 0.1f, 0f);
                leaf.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return go.gameObject;
        }

        GameObject BuildRock(Transform parent, Vector2Int cell, Vector3 world)
        {
            var go = new GameObject("Rock").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0, world.z);

            // 主石
            var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
            main.transform.SetParent(go, false);
            main.transform.position   = new Vector3(0, 0.15f, 0);
            main.transform.localScale = new Vector3(0.4f, 0.3f, 0.35f);
            main.transform.rotation   = Quaternion.Euler(0, 25, 0);
            main.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Rock", Palette.Stone, 0.18f, 0.05f);
            main.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 副石
            var sub = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sub.transform.SetParent(go, false);
            sub.transform.position   = new Vector3(0.25f, 0.08f, 0.18f);
            sub.transform.localScale = new Vector3(0.22f, 0.18f, 0.2f);
            sub.transform.rotation   = Quaternion.Euler(0, -10, 0);
            sub.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_RockSub", Palette.StoneDark, 0.18f, 0.05f);
            sub.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return go.gameObject;
        }

        GameObject BuildLamp(Transform parent, Vector2Int cell, Vector3 world)
        {
            var go = new GameObject("Lamp").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0, world.z);

            // 杆
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.SetParent(go, false);
            post.transform.position   = new Vector3(0, 0.7f, 0);
            post.transform.localScale = new Vector3(0.08f, 1.4f, 0.08f);
            post.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_LampPost", Palette.LampPost, 0.3f, 0.4f);
            post.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 横臂
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.transform.SetParent(go, false);
            arm.transform.position   = new Vector3(0.18f, 1.32f, 0);
            arm.transform.localScale = new Vector3(0.4f, 0.06f, 0.06f);
            arm.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_LampArm", Palette.LampPost, 0.3f, 0.4f);
            arm.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 灯罩
            var shade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shade.transform.SetParent(go, false);
            shade.transform.position   = new Vector3(0.36f, 1.22f, 0);
            shade.transform.localScale = new Vector3(0.18f, 0.16f, 0.18f);
            shade.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_LampShade", Palette.LampGlow, 0.2f, 0f);
            shade.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 简易点光（弱强度）
            var lightGo = new GameObject("LampLight");
            lightGo.transform.SetParent(go, false);
            lightGo.transform.position = new Vector3(0.36f, 1.18f, 0);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Palette.LampGlow;
            light.intensity = 0.6f;
            light.range = 3.5f;
            light.shadows = LightShadows.None;

            return go.gameObject;
        }

        GameObject BuildBench(Transform parent, Vector2Int cell, Vector3 world)
        {
            var go = new GameObject("Bench").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0, world.z);

            var seatMat = MaterialFactory.Create("Mat_Bench", Palette.BenchWood, 0.15f, 0f);
            var legMat  = MaterialFactory.Create("Mat_BenchLeg", Palette.LampPost, 0.3f, 0.2f);

            // 座面
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(go, false);
            seat.transform.position   = new Vector3(0, 0.22f, 0);
            seat.transform.localScale = new Vector3(0.7f, 0.06f, 0.28f);
            seat.GetComponent<MeshRenderer>().sharedMaterial = seatMat;
            seat.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 靠背
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.SetParent(go, false);
            back.transform.position   = new Vector3(0, 0.42f, -0.11f);
            back.transform.localScale = new Vector3(0.7f, 0.3f, 0.06f);
            back.GetComponent<MeshRenderer>().sharedMaterial = seatMat;
            back.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 两条腿
            for (int i = 0; i < 2; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(go, false);
                leg.transform.position   = new Vector3((i - 0.5f) * 0.5f, 0.11f, 0);
                leg.transform.localScale = new Vector3(0.08f, 0.22f, 0.22f);
                leg.GetComponent<MeshRenderer>().sharedMaterial = legMat;
                leg.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return go.gameObject;
        }

        GameObject BuildBush(Transform parent, Vector2Int cell, Vector3 world)
        {
            var go = new GameObject("Bush").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0, world.z);

            var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
            main.transform.SetParent(go, false);
            main.transform.position   = new Vector3(0, 0.1f, 0);
            main.transform.localScale = new Vector3(0.35f, 0.2f, 0.35f);
            main.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Bush", Palette.LeafDark, 0.1f, 0f);
            main.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.gameObject;
        }

        GameObject BuildStartMarker(Vector2Int cell, Transform parent)
        {
            var world = CellToWorld(cell.x, cell.y);
            var go = new GameObject("StartMarker").transform;
            go.SetParent(parent, false);
            go.position = new Vector3(world.x, 0.05f, world.z);

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ring.transform.SetParent(go, false);
            ring.transform.position   = new Vector3(0, 0.01f, 0);
            ring.transform.localScale = new Vector3(0.55f, 0.02f, 0.55f);
            ring.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Start", Palette.PlayerBody, 0.4f, 0.1f);
            ring.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // 透明
            if (ring.GetComponent<MeshRenderer>().material.HasProperty("_BaseColor"))
            {
                var c = Palette.PlayerBody; c.a = 0.7f;
                ring.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
            }
            return go.gameObject;
        }
    }
}
