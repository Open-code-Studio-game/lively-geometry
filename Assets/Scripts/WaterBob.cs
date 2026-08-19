// LivelyGeometry.WaterBob
// 水面方块的轻飘浮动，营造"灵动"感。
using UnityEngine;

namespace LivelyGeometry
{
    public class WaterBob : MonoBehaviour
    {
        public float baseY;
        public float amplitude = 0.04f;
        public float frequency = 1.4f;
        float t0;

        void OnEnable()
        {
            t0 = Time.time;
        }

        void Update()
        {
            float y = baseY + Mathf.Sin((Time.time - t0) * frequency) * amplitude;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }
}
