using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class TextureRendererFactory
    {

        [SerializeField] private EdgeBlurTexture m_EdgeBlurTextureRenderer;
        [SerializeField] private FakeDepthTexture m_FakeDepthTextureRenderer;

        public TextureRenderer GetRenderer(TextureRendererType type)
        {
            switch (type)
            {
                case TextureRendererType.EdgeBlur:
                    if (m_EdgeBlurTextureRenderer == null)
                        m_EdgeBlurTextureRenderer = new EdgeBlurTexture();
                    return m_EdgeBlurTextureRenderer;
                case TextureRendererType.FakeDepth:
                    if (m_FakeDepthTextureRenderer == null)
                        m_FakeDepthTextureRenderer = new FakeDepthTexture();
                    return m_FakeDepthTextureRenderer;
                default:
                    if (m_FakeDepthTextureRenderer == null)
                        m_FakeDepthTextureRenderer = new FakeDepthTexture();
                    return m_FakeDepthTextureRenderer;
            }
        }
    }
}