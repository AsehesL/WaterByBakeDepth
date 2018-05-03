using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class EdgeBlurTexture : ITextureRenderer
    {
        public float offset;
        public int iterations;

        public void DrawGUI()
        {
            offset = Mathf.Max(0,
          EditorGUILayout.FloatField(new GUIContent("偏移", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制贴图边缘模糊偏移量"),
              offset));
            iterations =
                EditorGUILayout.IntSlider(
                    new GUIContent("迭代次数", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"),
                        "控制边缘模糊迭代次数"),
                    iterations, 1, 7);
        }

        public RenderTexture Render(Camera camera, float height, float minHeight)
        {
            Shader.SetGlobalFloat("height", height);
            Shader.SetGlobalFloat("minheight", minHeight);
            camera.RenderWithShader(Shader.Find("Hidden/DepthMapRenderer"), "RenderType");

            Material blurMat = new Material(Shader.Find("Hidden/EdgeBlur"));
            blurMat.hideFlags = HideFlags.HideAndDontSave;
            blurMat.SetFloat("_Offset", offset);

            RenderTexture rt = RenderTexture.GetTemporary(camera.targetTexture.width/4, camera.targetTexture.height/4);
            
            Graphics.Blit(camera.targetTexture, rt);
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(camera.targetTexture.width / 4, camera.targetTexture.height / 4);
                Graphics.Blit(rt, rt2, blurMat, 0);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(camera.targetTexture.width / 4, camera.targetTexture.height / 4);
                Graphics.Blit(rt, rt2, blurMat, 1);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            blurMat.SetTexture("_Mix", rt);

            RenderTexture dst = new RenderTexture(camera.targetTexture.width, camera.targetTexture.height, camera.targetTexture.depth);

            Graphics.Blit(camera.targetTexture, dst, blurMat, 2);

            Object.DestroyImmediate(camera.targetTexture);

            camera.targetTexture = null;

            return dst;
        }
    }
}