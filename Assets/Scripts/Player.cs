// LivelyGeometry.Player
// 玩家：一个小立方体角色。点击目标方块后，自动沿格子一格一格走过去。
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LivelyGeometry
{
    public class Player : MonoBehaviour
    {
        public float moveDuration = 0.22f;
        public float jumpHeight   = 0.12f;

        public bool isMoving { get; private set; }
        public Vector2Int cell { get; private set; }

        Vector3 basePos;
        Coroutine moveCo;

        // 各身体部位（用于行走时上下"弹跳"）
        Transform body;
        Transform hat;
        Transform face;
        Transform eyeL;
        Transform eyeR;
        Vector3 bodyBaseScale;

        public void Init(Vector2Int startCell)
        {
            cell = startCell;
            transform.position = CellToWorld(cell);
            basePos = transform.position;
            BuildVisual();
        }

        void BuildVisual()
        {
            // 头部
            body = NewPart("Body", Palette.PlayerBody, new Vector3(0, 0.45f, 0), new Vector3(0.4f, 0.4f, 0.4f));
            bodyBaseScale = body.localScale;
            // 脸（前面一个色块）
            face = NewPart("Face", Palette.PlayerFace, new Vector3(0, 0.5f, 0.21f), new Vector3(0.3f, 0.18f, 0.02f));
            // 眼睛
            eyeL = NewPart("EyeL", new Color(0.12f, 0.12f, 0.12f), new Vector3(-0.07f, 0.52f, 0.215f), new Vector3(0.05f, 0.05f, 0.02f));
            eyeR = NewPart("EyeR", new Color(0.12f, 0.12f, 0.12f), new Vector3( 0.07f, 0.52f, 0.215f), new Vector3(0.05f, 0.05f, 0.02f));
            // 帽子
            hat = NewPart("Hat", Palette.PlayerHat, new Vector3(0, 0.78f, 0), new Vector3(0.36f, 0.18f, 0.36f));
            // 帽尖
            NewPart("HatTip", Palette.PlayerHat, new Vector3(0, 0.93f, 0), new Vector3(0.12f, 0.12f, 0.12f));
        }

        Transform NewPart(string n, Color c, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n;
            // 移除碰撞，避免被射线打到
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Create("Mat_Player_" + n, c, 0.25f, 0.05f);
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.transform;
        }

        public void TryWalkTo(Vector2Int target)
        {
            if (isMoving) return;
            if (target == cell) return;
            // BFS 找一条到 target 的最短路径
            var path = FindPath(cell, target);
            if (path == null || path.Count == 0) return;
            if (moveCo != null) StopCoroutine(moveCo);
            moveCo = StartCoroutine(WalkPath(path));
        }

        IEnumerator WalkPath(List<Vector2Int> path)
        {
            isMoving = true;
            foreach (var step in path)
            {
                yield return StartCoroutine(MoveOneStep(step));
                cell = step;
                // 每走一步，通知 GameManager 检查是否到达目标
                GameManager.Instance?.OnPlayerArrivedCell(cell);
            }
            isMoving = false;
            moveCo = null;
        }

        IEnumerator MoveOneStep(Vector2Int target)
        {
            Vector3 from = transform.position;
            Vector3 to   = CellToWorld(target);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDuration;
                float k = Mathf.SmoothStep(0f, 1f, t);
                Vector3 pos = Vector3.Lerp(from, to, k);
                pos.y += Mathf.Sin(k * Mathf.PI) * jumpHeight;
                transform.position = pos;
                // 身体小弹跳
                if (body != null)
                {
                    float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.08f;
                    body.localScale = new Vector3(bodyBaseScale.x, bodyBaseScale.y * s, bodyBaseScale.z);
                }
                yield return null;
            }
            transform.position = to;
            if (body != null) body.localScale = bodyBaseScale;
        }

        // ---------- 寻路 ----------
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            // 简化版：BFS + 父节点回溯
            // 动态占用：当前桥实际占用的格子 + 水的格子视为不可走
            var lvl = GameManager.Instance.level;
            var w = lvl.width;
            var h = lvl.height;

            var blocked = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int z = 0; z < h; z++)
                {
                    var t = lvl.Get(x, z);
                    if (t == TileType.Water || t == TileType.Stone || t == TileType.Empty)
                        blocked[x, z] = true;
                }
            // 桥的格子
            foreach (var br in GameManager.Instance.allBridges)
            {
                blocked[br.cellA.x, br.cellA.y] = false;
                blocked[br.cellB.x, br.cellB.y] = false;
            }
            // 起点若被标记为 blocked，临时解封
            blocked[start.x, start.y] = false;
            // 终点也解封
            blocked[end.x, end.y] = false;

            if (blocked[end.x, end.y]) return null;

            var came = new Dictionary<Vector2Int, Vector2Int>();
            var q = new Queue<Vector2Int>();
            q.Enqueue(start);
            came[start] = start;

            Vector2Int[] dirs = {
                new Vector2Int( 1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 0,-1)
            };

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur == end) break;
                foreach (var d in dirs)
                {
                    var n = cur + d;
                    if (n.x < 0 || n.x >= w || n.y < 0 || n.y >= h) continue;
                    if (blocked[n.x, n.y]) continue;
                    if (came.ContainsKey(n)) continue;
                    came[n] = cur;
                    q.Enqueue(n);
                }
            }

            if (!came.ContainsKey(end)) return null;
            var path = new List<Vector2Int>();
            var c = end;
            while (c != start)
            {
                path.Add(c);
                c = came[c];
            }
            path.Reverse();
            return path;
        }

        Vector3 CellToWorld(Vector2Int c)
        {
            var lvl = GameManager.Instance.level;
            int w = lvl.width, h = lvl.height;
            return new Vector3(
                (c.x - w * 0.5f + 0.5f) * LevelBuilder.TileSize,
                0f,
                (c.y - h * 0.5f + 0.5f) * LevelBuilder.TileSize
            );
        }
    }
}
