// LivelyGeometry.LevelData
// 关卡数据定义。第一关用纯数据描述，方便后续编辑器扩展。
using System.Collections.Generic;
using UnityEngine;

namespace LivelyGeometry
{
    public enum TileType : byte
    {
        Empty  = 0,   // 不可走、不可见的空气方块
        Grass  = 1,   // 草地（顶面绿色，其它面为土色）
        Dirt   = 2,   // 泥土（统一颜色）
        Water  = 3,   // 水（不可走）
        Stone  = 4,   // 石头（不可走，装饰用）
        Goal   = 5,   // 目标方块（可达）
        Bridge = 6    // 桥（可旋转/平移）
    }

    [System.Serializable]
    public struct DecorationDef
    {
        public enum Kind { Tree, Rock, Lamp, Bench, Bush }
        public Kind kind;
        public Vector2Int cell; // 网格坐标 (x, z)
    }

    [System.Serializable]
    public struct BridgeDef
    {
        public Vector2Int from;     // 占用的两个格子之一
        public Vector2Int to;       // 占用的另一个格子
        public float moveTime;      // 一次完整切换时长
    }

    public class LevelData
    {
        public int width  = 7;
        public int height = 7;
        public TileType[,] tiles;
        public Vector2Int startCell = new Vector2Int(0, 3);
        public Vector2Int goalCell  = new Vector2Int(6, 3);
        public List<BridgeDef> bridges = new List<BridgeDef>();
        public List<DecorationDef> decorations = new List<DecorationDef>();
        public string levelName = "第一章 · 初识几何";

        public TileType Get(int x, int z)
        {
            if (x < 0 || x >= width || z < 0 || z >= height) return TileType.Empty;
            return tiles[x, z];
        }

        public bool IsWalkable(int x, int z)
        {
            var t = Get(x, z);
            return t == TileType.Grass || t == TileType.Dirt || t == TileType.Goal || t == TileType.Bridge;
        }

        // 创建一个示例关卡：起点 -> 需跨过水域 -> 终点。中间有一个可旋转的桥。
        public static LevelData CreateSampleLevel()
        {
            var lvl = new LevelData
            {
                width  = 7,
                height = 7,
                levelName = "第一章 · 初识几何"
            };
            lvl.tiles = new TileType[7, 7];

            for (int x = 0; x < 7; x++)
                for (int z = 0; z < 7; z++)
                    lvl.tiles[x, z] = TileType.Empty;

            // 草地外圈
            for (int x = 0; x < 7; x++)
            {
                lvl.tiles[x, 0] = TileType.Grass;
                lvl.tiles[x, 6] = TileType.Grass;
            }
            for (int z = 0; z < 7; z++)
            {
                lvl.tiles[0, z] = TileType.Grass;
                lvl.tiles[6, z] = TileType.Grass;
            }

            // 中间一条横向通道（草地）
            for (int x = 1; x <= 5; x++)
            {
                lvl.tiles[x, 3] = TileType.Grass;
            }
            // 起点 / 终点位置特殊标识
            lvl.tiles[1, 3] = TileType.Grass;
            lvl.tiles[5, 3] = TileType.Goal;

            // 一段水域阻隔（在第 2、3 行的中间）
            for (int x = 2; x <= 4; x++)
            {
                lvl.tiles[x, 1] = TileType.Water;
                lvl.tiles[x, 2] = TileType.Water;
                lvl.tiles[x, 4] = TileType.Water;
                lvl.tiles[x, 5] = TileType.Water;
            }

            // 桥：连接 (2,3) <-> (3,3)，点击后旋转 90° 变成 (2,3) <-> (2,2) 跨过水域
            lvl.bridges.Add(new BridgeDef
            {
                from = new Vector2Int(2, 3),
                to   = new Vector2Int(3, 3),
                moveTime = 0.6f
            });
            // 强制把桥位标成 Bridge
            lvl.tiles[2, 3] = TileType.Bridge;
            lvl.tiles[3, 3] = TileType.Bridge;

            // 装饰
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(0, 0) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(0, 6) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(6, 0) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(6, 6) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(3, 0) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Tree,  cell = new Vector2Int(3, 6) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Rock,  cell = new Vector2Int(1, 1) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Rock,  cell = new Vector2Int(5, 5) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Rock,  cell = new Vector2Int(5, 1) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Lamp,  cell = new Vector2Int(1, 3) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Lamp,  cell = new Vector2Int(4, 3) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Bench, cell = new Vector2Int(2, 6) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Bush,  cell = new Vector2Int(0, 3) });
            lvl.decorations.Add(new DecorationDef { kind = DecorationDef.Kind.Bush,  cell = new Vector2Int(6, 3) });

            // 起点在 (1,3)、终点在 (5,3)
            lvl.startCell = new Vector2Int(1, 3);
            lvl.goalCell  = new Vector2Int(5, 3);

            return lvl;
        }
    }
}
