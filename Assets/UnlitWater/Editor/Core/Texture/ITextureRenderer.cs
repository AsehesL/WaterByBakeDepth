using UnityEngine;
using System.Collections;

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

    internal interface ITextureRenderer
    {
        void DrawGUI();

        RenderTexture Render(Camera camera, float height, float minHeight);
    }
}