using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class EdgeBlurTexture : TextureRenderer
    {
        public float offset;
        public int iterations;
        public float power = 1;
        public float edgeBeginWidth;

        public override void DrawGUI()
        {
            offset = Mathf.Max(0,
                EditorGUILayout.Slider(
                    new GUIContent("偏移", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制贴图边缘模糊偏移量"),
                    offset, 0, 0.2f));
            iterations =
                EditorGUILayout.IntSlider(
                    new GUIContent("迭代次数", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"),
                        "控制边缘模糊迭代次数"),
                    iterations, 1, 7);
            edgeBeginWidth =
                EditorGUILayout.Slider(
                    new GUIContent("边缘起始宽度", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"),
                        "控制边缘模糊起始宽度"), edgeBeginWidth, 0, 0.99f);
            power = Mathf.Max(0,
               EditorGUILayout.FloatField(new GUIContent("模糊增强", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制渲染深度的效果增强或减弱，默认为1表示不增强"),
                   power));
        }

        protected override void Render(Camera camera, float height, float minHeight, ref Texture2D tex)
        {
            RenderTexture src = RenderTexture.GetTemporary(4096, 4096, 24);
            camera.targetTexture = src;

            Shader.SetGlobalFloat("height", height);
            Shader.SetGlobalFloat("minheight", minHeight);
            camera.RenderWithShader(Shader.Find("Hidden/DepthMapRenderer"), "RenderType");

            Material blurMat = new Material(Shader.Find("Hidden/EdgeBlur"));
            blurMat.hideFlags = HideFlags.HideAndDontSave;
            blurMat.SetFloat("_Offset", offset);
            blurMat.SetFloat("_EdgeBeginWidth", edgeBeginWidth);
            blurMat.SetFloat("_Power", power);

            RenderTexture rt = RenderTexture.GetTemporary(src.width / 4, src.height / 4);
            Graphics.Blit(src, rt);
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(src.width / 4, src.height / 4);
                Graphics.Blit(rt, rt2, blurMat, 0);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(src.width / 4, src.height / 4);
                Graphics.Blit(rt, rt2, blurMat, 1);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            blurMat.SetTexture("_Mix", rt);

            RenderTexture dst = RenderTexture.GetTemporary(src.width, src.height, src.depth);

            Graphics.Blit(src, dst, blurMat, 2);

            camera.targetTexture = null;

            RenderTexture.ReleaseTemporary(src);

            tex = UnlitWaterUtils.RenderTextureToTexture2D(dst);

            RenderTexture.ReleaseTemporary(dst);
        }
    }
}