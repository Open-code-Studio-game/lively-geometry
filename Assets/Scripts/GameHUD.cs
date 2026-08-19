// LivelyGeometry.GameHUD
// 用 UGUI 显示关卡名 / 胜利面板。
// 启动时若场景里没有 Canvas，则在运行时创建一个。
using UnityEngine;
using UnityEngine.UI;

namespace LivelyGeometry
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        Canvas canvas;
        Text levelNameText;
        Text tipText;
        GameObject winPanel;
        Text winText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            EnsureCanvas();
        }

        void EnsureCanvas()
        {
            if (canvas != null) return;
            // 优先用场景里已有的 Canvas
            var existing = FindFirstObjectByType<Canvas>();
            if (existing != null) canvas = existing;
            else
            {
                var go = new GameObject("HUDCanvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                go.AddComponent<GraphicRaycaster>();
            }
            BuildLevelName();
            BuildTip();
        }

        void BuildLevelName()
        {
            var go = new GameObject("LevelName");
            go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -36);
            rt.sizeDelta = new Vector2(800, 80);
            levelNameText = go.AddComponent<Text>();
            levelNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelNameText.alignment = TextAnchor.MiddleCenter;
            levelNameText.fontSize = 44;
            levelNameText.color = new Color(0.18f, 0.12f, 0.08f, 0.9f);
            levelNameText.text = "";
        }

        void BuildTip()
        {
            var go = new GameObject("Tip");
            go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 28);
            rt.sizeDelta = new Vector2(900, 60);
            tipText = go.AddComponent<Text>();
            tipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.fontSize = 22;
            tipText.color = new Color(0.20f, 0.16f, 0.12f, 0.75f);
            tipText.text = "点击相邻的方块走过去 ·  点击桥可旋转  ·  R 重新开始";
        }

        public void ShowLevelName(string name)
        {
            if (levelNameText == null) EnsureCanvas();
            levelNameText.text = name;
            // 淡入
            StopAllCoroutines();
            StartCoroutine(FadeInLevelName());
        }

        System.Collections.IEnumerator FadeInLevelName()
        {
            var c = levelNameText.color;
            c.a = 0f;
            levelNameText.color = c;
            for (float t = 0; t < 1f; t += Time.deltaTime * 1.5f)
            {
                c.a = Mathf.Lerp(0f, 0.9f, t);
                levelNameText.color = c;
                yield return null;
            }
        }

        public void ShowWinPanel()
        {
            if (winPanel != null) Destroy(winPanel);
            var panel = new GameObject("WinPanel");
            panel.transform.SetParent(canvas.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);

            // 中间卡片
            var card = new GameObject("Card");
            card.transform.SetParent(panel.transform, false);
            var crt = card.AddComponent<RectTransform>();
            crt.sizeDelta = new Vector2(640, 360);
            crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
            var cimg = card.AddComponent<Image>();
            cimg.color = new Color(0.99f, 0.93f, 0.78f, 0.96f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(card.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(40, 40);
            trt.offsetMax = new Vector2(-40, -40);
            winText = textGo.AddComponent<Text>();
            winText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            winText.alignment = TextAnchor.MiddleCenter;
            winText.fontSize = 56;
            winText.color = new Color(0.20f, 0.14f, 0.10f, 1f);
            winText.text = "你到了！\n按 R 重新开始";

            winPanel = panel;
        }

        public void HideWinPanel()
        {
            if (winPanel != null) Destroy(winPanel);
            winPanel = null;
        }
    }
}
