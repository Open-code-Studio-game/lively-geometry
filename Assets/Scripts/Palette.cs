// LivelyGeometry.Palette
// 统一存放游戏中所有"色调"常量，方便整体调色。
// 所有颜色基于参考图（暖色夕阳 + 冷暖对比 + 几何感）。
using UnityEngine;

namespace LivelyGeometry
{
    public static class Palette
    {
        // 地面
        public static readonly Color GrassTop    = new Color(0.486f, 0.702f, 0.259f); // #7CB342
        public static readonly Color GrassSide   = new Color(0.553f, 0.431f, 0.388f); // #8D6E63
        public static readonly Color Dirt        = new Color(0.631f, 0.510f, 0.427f); // #A1826D
        public static readonly Color Cobble      = new Color(0.522f, 0.404f, 0.310f); // #856752
        public static readonly Color Stone       = new Color(0.620f, 0.620f, 0.620f); // #9E9E9E
        public static readonly Color StoneDark   = new Color(0.420f, 0.420f, 0.420f); // #6B6B6B
        public static readonly Color Water       = new Color(0.149f, 0.306f, 0.459f); // #264E75
        public static readonly Color WaterDeep   = new Color(0.090f, 0.220f, 0.380f); // #173860

        // 装饰
        public static readonly Color LeafLight   = new Color(0.349f, 0.682f, 0.290f); // #59AE4A
        public static readonly Color LeafDark    = new Color(0.231f, 0.498f, 0.247f); // #3B7F3F
        public static readonly Color Trunk       = new Color(0.365f, 0.251f, 0.216f); // #5D4037
        public static readonly Color BenchWood   = new Color(0.627f, 0.431f, 0.235f); // #A06E3C
        public static readonly Color LampPost    = new Color(0.235f, 0.235f, 0.235f); // #3C3C3C
        public static readonly Color LampGlow    = new Color(1.000f, 0.870f, 0.620f); // warm light
        public static readonly Color Bridge      = new Color(0.722f, 0.510f, 0.310f); // #B8824F
        public static readonly Color BridgePlank = new Color(0.560f, 0.380f, 0.220f); // #8F6138

        // 角色 / 目标
        public static readonly Color PlayerBody  = new Color(0.953f, 0.612f, 0.388f); // warm coral
        public static readonly Color PlayerHat   = new Color(0.890f, 0.345f, 0.392f); // hat red
        public static readonly Color PlayerFace  = new Color(0.984f, 0.851f, 0.682f); // skin
        public static readonly Color Goal        = new Color(1.000f, 0.843f, 0.000f); // #FFD700
        public static readonly Color GoalGlow    = new Color(1.000f, 0.953f, 0.620f); // #FFF39E

        // 光照
        public static readonly Color SunWarm     = new Color(1.000f, 0.780f, 0.560f); // 暖色阳光
        public static readonly Color SkyTop      = new Color(0.984f, 0.745f, 0.557f); // 顶光
        public static readonly Color SkyBottom   = new Color(0.890f, 0.820f, 0.706f); // 雾色
        public static readonly Color FogColor    = new Color(0.910f, 0.820f, 0.706f);
    }
}
