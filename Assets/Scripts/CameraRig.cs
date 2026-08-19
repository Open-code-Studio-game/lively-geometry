// LivelyGeometry.CameraRig
// 等距（Monument Valley 风）摄像机。Orthographic 投影，固定俯角 ~35°。
// 跟随玩家，平滑过渡。
using UnityEngine;

namespace LivelyGeometry
{
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 9f, -11f);
        public float followLerp = 4.5f;
        public float orthoSize  = 5.2f;
        public float pitch = 30f;     // 俯角
        public float yaw   = 0f;      // 方位

        Camera cam;
        bool inited;

        public void Setup()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane  = 100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Palette.SkyTop;

            // 旋转 + 位置
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = (target != null ? target.position : Vector3.zero) +
                                 Quaternion.Euler(0, yaw, 0) * offset;
            inited = true;
        }

        void LateUpdate()
        {
            if (!inited) Setup();
            if (target == null) return;
            Vector3 desired = target.position + Quaternion.Euler(0, yaw, 0) * offset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followLerp);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
    }
}
