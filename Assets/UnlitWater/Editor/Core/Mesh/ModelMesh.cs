using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 从模型文件生成的Mesh
    /// </summary>
    [System.Serializable]
    internal class ModelMesh : IMeshGenerator
    {
        public Vector2 size;

        public void DrawGUI()
        {
            size = EditorGUILayout.Vector2Field(new GUIContent("区域大小", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的区域大小"),
          size);
        }

        public void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight)
        {
            UnlitWaterHandles.DrawUnlitWaterArea(
                   target.transform.position + new Vector3(offset.x, 0, offset.y),
                   Quaternion.Euler(0, rotY, 0), size,
                   new Vector2(minHeight, maxHeight), Color.green);
        }

        public Mesh GenerateMesh(Texture2D texture)
        {
            //模型文件生成的Mesh不会调用该方法
            return null;
        }

        public Vector2 GetSize()
        {
            return size;
        }

        public void SetSize(Vector2 size)
        {
            this.size = size;
        }
    }
}