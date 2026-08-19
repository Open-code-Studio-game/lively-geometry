// LivelyGeometry.GoalPulse
// 让目标方块的金色顶部"呼吸"上下浮动 + 缩放。
using UnityEngine;

namespace LivelyGeometry
{
    public class GoalPulse : MonoBehaviour
    {
        public Transform top;
        public Transform tip;
        public float amplitude = 0.06f;
        public float frequency = 1.6f;

        Vector3 topBaseLocal;
        Vector3 tipBaseLocal;
        Vector3 topBaseScale;
        Vector3 tipBaseScale;

        void Start()
        {
            if (top != null) { topBaseLocal = top.localPosition; topBaseScale = top.localScale; }
            if (tip != null) { tipBaseLocal = tip.localPosition; tipBaseScale = tip.localScale; }
        }

        void Update()
        {
            float p = (Mathf.Sin(Time.time * frequency) + 1f) * 0.5f;
            if (top != null)
            {
                top.localPosition = topBaseLocal + Vector3.up * p * amplitude;
                float s = 1f + 0.05f * Mathf.Sin(Time.time * frequency * 2f);
                top.localScale = topBaseScale * s;
            }
            if (tip != null)
            {
                tip.localPosition = tipBaseLocal + Vector3.up * p * amplitude;
                tip.Rotate(Vector3.up, 60f * Time.deltaTime);
            }
        }
    }
}
