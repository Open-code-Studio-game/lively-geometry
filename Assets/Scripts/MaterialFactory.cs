// LivelyGeometry.MaterialFactory
// 缓存运行时创建的 URP/Lit 材质，避免重复分配。
using UnityEngine;

namespace LivelyGeometry
{
    public static class MaterialFactory
    {
        // URP/Lit 在 URP 项目里就是通用 PBR 着色器。
        // 兜底用 Standard，保证在 Build/Player 里也尽量不报粉材质。
        private static Shader _litShader;

        public static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (_litShader == null) _litShader = Shader.Find("Standard");
                }
                return _litShader;
            }
        }

        public static Material Create(string name, Color color, float smoothness = 0.18f, float metallic = 0.0f)
        {
            var mat = new Material(LitShader) { name = name };
            // 兼容 URP 和 Standard
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic"))  mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        public static Material CreateGrass()
        {
            // 草地用一个统一材质（顶面颜色由面材质 + 顶点色近似，这里先取顶面色）
            return Create("Mat_Grass", Palette.GrassTop, 0.12f, 0.0f);
        }

        public static Material CreateDirt()
        {
            return Create("Mat_Dirt", Palette.Dirt, 0.10f, 0.0f);
        }

        public static Material CreateWater()
        {
            var mat = Create("Mat_Water", Palette.Water, 0.85f, 0.05f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 透明
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            // 启用 URP 的透明设置
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite"))   mat.SetFloat("_ZWrite", 0f);
            if (mat.HasProperty("_BaseColor"))
            {
                var c = Palette.Water; c.a = 0.78f;
                mat.SetColor("_BaseColor", c);
            }
            return mat;
        }
    }
}
