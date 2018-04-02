using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 简单格子Mesh
    /// </summary>
    [System.Serializable]
    internal class SimpleGridMesh : IMeshGenerator
    {
        public float widthX;
        public float widthZ;

        public int cellSizeX;
        public int cellSizeZ;

        public float uvDir;

        public int samples = 2;

        public void DrawGUI()
        {
            widthX = Mathf.Max(0.01f, EditorGUILayout.FloatField("Width", widthX));
            widthZ = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height", widthZ));
            cellSizeX = Mathf.Max(1, EditorGUILayout.IntField("CellWidth", cellSizeX));
            cellSizeZ = Mathf.Max(1, EditorGUILayout.IntField("CellHeight", cellSizeZ));
            uvDir = EditorGUILayout.Slider("UV水平方向", uvDir, 0, 360);
            samples = EditorGUILayout.IntSlider("不可见三角剔除采样", samples, 1, 4);
        }

        public void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight)
        {
            UnlitWaterHandles.DrawUnlitWaterArea(
                  target.transform.position + new Vector3(offset.x, 0, offset.y),
                  Quaternion.Euler(0, rotY, 0), new Vector2(widthX, widthZ),
                  new Vector2(minHeight, maxHeight), Color.green);

            UnlitWaterHandles.DrawUnlitWaterGrid(target.transform.position + new Vector3(offset.x, 0, offset.y),
                Quaternion.Euler(0, rotY, 0), new Vector2(widthX, widthZ), cellSizeX, cellSizeZ);

            float sz = Mathf.Max(widthX, widthZ) / 10;
            UnlitWaterHandles.DrawDirArrow(
                target.transform.position + new Vector3(offset.x, 0, offset.y), uvDir, sz,
                Color.cyan);
        }

        public Mesh GenerateMesh(Texture2D texture)
        {
            if (cellSizeX <= 0 || cellSizeZ <= 0 || widthX <= 0 || widthZ <= 0)
                return null;

            float deltaX = widthX*2/cellSizeX;
            float deltaY = widthZ*2/cellSizeZ;

            MeshVertexData cache = new MeshVertexData(cellSizeX, cellSizeZ, deltaX, deltaY, -widthX, -widthZ);

            for (int i = 0; i < cellSizeZ; i++)
            {
                for (int j = 0; j < cellSizeX; j++)
                {
                    Vector3 p0 = new Vector3(-widthX + j*deltaX, 0, -widthZ + i*deltaY);
                    Vector3 p1 = new Vector3(-widthX + j*deltaX, 0, -widthZ + i*deltaY + deltaY);
                    Vector3 p2 = new Vector3(-widthX + j*deltaX + deltaX, 0, -widthZ + i*deltaY + deltaY);
                    Vector3 p3 = new Vector3(-widthX + j*deltaX + deltaX, 0, -widthZ + i*deltaY);

                    cache.AddVertex(p0);
                    cache.AddVertex(p1);
                    cache.AddVertex(p2);
                    cache.AddVertex(p3);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);

                    cache.index += 4;
                }
            }

            Mesh mesh = cache.Apply(texture, uvDir, samples);

            return mesh;
        }

        public void SetSize(Vector2 size)
        {
            widthX = size.x;
            widthZ = size.y;
        }

        public Vector2 GetSize()
        {
            return new Vector2(widthX, widthZ);
        }

        private Color GetColor(Texture2D tex, Vector2 uv)
        {
            int x = (int)(uv.x * tex.width);
            int y = (int)(uv.y * tex.height);
            if (x < 0)
                x = 0;
            if (x >= tex.width)
                x = tex.width - 1;
            if (y < 0)
                y = 0;
            if (y >= tex.height)
                y = tex.height - 1;
            Color col = tex.GetPixel(x, y);
            return col;
        }
    }
}