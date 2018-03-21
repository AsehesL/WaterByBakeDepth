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

        public void DrawGUI()
        {
            widthX = Mathf.Max(0.01f, EditorGUILayout.FloatField("Width", widthX));
            widthZ = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height", widthZ));
            cellSizeX = Mathf.Max(1, EditorGUILayout.IntField("CellWidth", cellSizeX));
            cellSizeZ = Mathf.Max(1, EditorGUILayout.IntField("CellHeight", cellSizeZ));
            uvDir = EditorGUILayout.Slider("UV水平方向", uvDir, 0, 360);
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
            List<Vector3> vlist = new List<Vector3>();
            List<Vector2> ulist = new List<Vector2>();
            List<int> ilist = new List<int>();
            List<Color> clist = new List<Color>();

            float deltaX = widthX*2/cellSizeX;
            float deltaY = widthZ*2/cellSizeZ;

            float udeltax = 1.0f/cellSizeX;
            float udeltay = 1.0f/cellSizeZ;

            for (int i = 0; i <= cellSizeZ; i++)
            {
                for (int j = 0; j <= cellSizeX; j++)
                {
                    Vector3 p = new Vector3(-widthX + j*deltaX, 0, -widthZ + i*deltaY);
                    Vector2 uv = new Vector2(j*udeltax, i*udeltay);

                    Color col = GetColor(texture, uv);

                    float sinag = Mathf.Sin(Mathf.Deg2Rad * uvDir);
                    float cosag = Mathf.Cos(Mathf.Deg2Rad * uvDir);

                    uv = new Vector2(uv.x * cosag - uv.y * sinag, uv.x * sinag + uv.y * cosag);

                    vlist.Add(p);
                    ulist.Add(uv);
                    clist.Add(col);
                    if (i != cellSizeZ && j != cellSizeX)
                    {
                        ilist.Add((i*(cellSizeX + 1))+j);
                        ilist.Add(((i+1)*(cellSizeX + 1))+j+1);
                        ilist.Add((i*(cellSizeX + 1))+j+1);

                        ilist.Add((i*(cellSizeX + 1))+j);
                        ilist.Add(((i+1)*(cellSizeX + 1))+j);
                        ilist.Add(((i+1)*(cellSizeX + 1))+j+1);
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vlist);
            mesh.SetUVs(0, ulist);
            mesh.SetTriangles(ilist, 0);
            mesh.SetColors(clist);
            mesh.RecalculateNormals();

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