using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 贴图渲染方式
    /// </summary>
    public enum TextureRendererType
    {
        /// <summary>
        /// 假深度图
        /// </summary>
        FakeDepth,
        /// <summary>
        /// 边缘模糊
        /// </summary>
        EdgeBlur,
    }

    internal abstract class TextureRenderer
    {
        public abstract void DrawGUI();

        public void RenderDepthTexture(GameObject target, Vector2 offset, Vector2 size, Quaternion rotation,
            float maxHeight, float minHeight, ref Texture2D tex)
        {
            if (target == null)
            {
                EditorUtility.DisplayDialog("错误", "请先设置目标网格", "确定");
                return;
            }
            Camera newCam = new GameObject("[TestCamera]").AddComponent<Camera>();
            newCam.clearFlags = CameraClearFlags.SolidColor;
            newCam.backgroundColor = Color.black;
            newCam.orthographic = true;
            newCam.aspect = size.x / size.y;
            newCam.orthographicSize = size.y;
            newCam.nearClipPlane = -maxHeight;
            newCam.farClipPlane = minHeight;
            newCam.transform.position = target.transform.position + new Vector3(offset.x, 0, offset.y);
            newCam.transform.rotation = rotation;
            newCam.enabled = false;

            bool isMeshActive = target.activeSelf;
            target.SetActive(false);

            Render(newCam, target.transform.position.y, minHeight, ref tex);

            Object.DestroyImmediate(newCam.gameObject);

            target.SetActive(isMeshActive);
        }

        protected abstract void Render(Camera camera, float height, float minHeight, ref Texture2D tex);
    }
}