using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class SimpleGridMesh : IMeshGenerator
    {
        public void DrawGUI()
        {
        }

        public void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight)
        {
        }

        public Mesh GenerateMesh(Texture2D texture)
        {
            return null;  
        }

        public void SetSize(Vector2 size)
        {
            
        }

        public Vector2 GetSize()
        {
            return default(Vector2);
        }
    }
}