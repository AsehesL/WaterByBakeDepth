using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class FakeDepthTexture : TextureRenderer
    {
        public float maxDepth = 1;
        public float depthPower = 1;

        public override void DrawGUI()
        {
            maxDepth = Mathf.Max(0,
           EditorGUILayout.FloatField(new GUIContent("最大深度范围", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制渲染的最大深度范围，默认为1"),
               maxDepth));
            depthPower = Mathf.Max(0,
               EditorGUILayout.FloatField(new GUIContent("深度增强", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制渲染深度的效果增强或减弱，默认为1表示不增强"),
                   depthPower));
        }

        protected override void Render(Camera camera, float height, float minHeight, ref Texture2D tex)
        {
            RenderTexture rt = new RenderTexture(4096, 4096, 24);
            rt.hideFlags = HideFlags.HideAndDontSave;

            camera.targetTexture = rt;

            Shader.SetGlobalFloat("depth", maxDepth);
            Shader.SetGlobalFloat("power", depthPower);
            Shader.SetGlobalFloat("height", height);
            Shader.SetGlobalFloat("minheight", minHeight);
            camera.RenderWithShader(Shader.Find("Hidden/DepthMapRenderer"), "RenderType");

            tex = UnlitWaterUtils.RenderTextureToTexture2D(rt);

            camera.targetTexture = null;
            Object.DestroyImmediate(rt);
            
        }
    }
}