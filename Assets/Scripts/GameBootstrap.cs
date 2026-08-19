// LivelyGeometry.GameBootstrap
// 把这个脚本挂到场景的 GameObject 上，就能跑起整个游戏。
// 也可以什么都不挂 —— 通过 RuntimeInitializeOnLoadMethod 自动启动。
using UnityEngine;
using UnityEngine.EventSystems;

namespace LivelyGeometry
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            // 避免重复：在已激活的游戏对象上挂 GameBootstrap
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            if (GameManager.Instance != null) return;
            var go = new GameObject("[LivelyGeometry Bootstrap]");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            // 1) EventSystem
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 2) GameManager
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }

            // 3) HUD
            if (GameHUD.Instance == null)
            {
                var hud = new GameObject("GameHUD");
                hud.AddComponent<GameHUD>();
            }

            // 4) Main Camera（GameManager 会自动接管）
            if (Camera.main == null)
            {
                var camGo = new GameObject("MainCamera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
            }
        }
    }
}
