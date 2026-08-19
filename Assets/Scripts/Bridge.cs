// LivelyGeometry.Bridge
// 桥：点击后会"旋转 90°"，将两块桥板位置互换，从而改变哪些格子可走。
// 在 Monument Valley 风格的玩法里这是最基础的"改变建筑"机制。
using UnityEngine;

namespace LivelyGeometry
{
    public class Bridge : MonoBehaviour
    {
        public Vector2Int cellA;   // 当前第一块板所占格子
        public Vector2Int cellB;   // 当前第二块板所占格子
        public float moveTime = 0.6f;

        bool horizontal = true;   // 当前是否横向
        bool animating = false;
        float animT = 0f;
        Vector3 startPos;
        Quaternion startRot;
        Vector3 endPos;
        Quaternion endRot;

        void Start()
        {
            ApplyPosition();
        }

        public void Toggle()
        {
            if (animating) return;
            // 计算绕中心旋转 90° 后的两格
            var center = (CellToWorld(cellA) + CellToWorld(cellB)) * 0.5f;
            // 旋转前的世界偏移
            var offA = CellToWorld(cellA) - center;
            var offB = CellToWorld(cellB) - center;
            // 90° 旋转（绕 Y 轴）
            var newOffA = new Vector3(offB.z, offA.y, -offB.x);
            var newOffB = new Vector3(offA.z, offB.y, -offA.x);
            var newA = center + newOffA;
            var newB = center + newOffB;

            // 起点 / 终点
            startPos = transform.position;
            startRot = transform.rotation;
            endPos = (newA + newB) * 0.5f;
            endRot = transform.rotation * Quaternion.Euler(0, 90, 0);
            animT = 0f;
            animating = true;

            // 更新格子占用（旋转后）
            cellA = WorldToCell(newA);
            cellB = WorldToCell(newB);
            horizontal = !horizontal;
        }

        void Update()
        {
            if (!animating) return;
            animT += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(animT));
            transform.position = Vector3.Lerp(startPos, endPos, k);
            transform.rotation = Quaternion.Slerp(startRot, endRot, k);
            if (animT >= 1f)
            {
                animating = false;
                ApplyPosition();
            }
        }

        void ApplyPosition()
        {
            var a = CellToWorld(cellA);
            var b = CellToWorld(cellB);
            transform.position = (a + b) * 0.5f + Vector3.up * 0.05f;
            if (horizontal)
                transform.rotation = Quaternion.identity;
            else
                transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        Vector3 CellToWorld(Vector2Int cell)
        {
            var lvl = GameManager.Instance != null ? GameManager.Instance.level : null;
            int w = lvl != null ? lvl.width  : 7;
            int h = lvl != null ? lvl.height : 7;
            return new Vector3(
                (cell.x - w * 0.5f + 0.5f) * LevelBuilder.TileSize,
                0f,
                (cell.y - h * 0.5f + 0.5f) * LevelBuilder.TileSize
            );
        }

        Vector2Int WorldToCell(Vector3 w)
        {
            var lvl = GameManager.Instance != null ? GameManager.Instance.level : null;
            int width  = lvl != null ? lvl.width  : 7;
            int height = lvl != null ? lvl.height : 7;
            int x = Mathf.RoundToInt(w.x / LevelBuilder.TileSize + width  * 0.5f - 0.5f);
            int z = Mathf.RoundToInt(w.z / LevelBuilder.TileSize + height * 0.5f - 0.5f);
            return new Vector2Int(x, z);
        }

        // 点击命中
        void OnMouseDown() { } // 不使用旧输入系统
    }
}
