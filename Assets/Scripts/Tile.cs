// LivelyGeometry.Tile
// 每个方块上的逻辑组件：记录格子坐标、类型，并响应点击。
using UnityEngine;
using UnityEngine.EventSystems;

namespace LivelyGeometry
{
    [RequireComponent(typeof(Collider))]
    public class Tile : MonoBehaviour
    {
        public Vector2Int cell;
        public TileType type = TileType.Grass;
        public bool isBridge = false;
        public bool isOccupied = false;

        public Vector3 WalkPoint
        {
            get
            {
                // 角色最终站在该方块中心 + 一点高度
                var p = transform.position;
                p.y = Mathf.Max(p.y + 0.5f, 0.5f);
                return p;
            }
        }

        void OnMouseDown()
        {
            // 新 Input System 不会触发 OnMouseDown；点击由 GameManager 统一派发。
        }
    }
}
