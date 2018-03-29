using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 四叉树Mesh
    /// </summary>
    [System.Serializable]
    internal class QuadTreeMesh : IMeshGenerator
    {
        /// <summary>
        /// 四叉树深度
        /// </summary>
        public int depth = 0;
        public float widthX;
        public float widthZ;
        
        public int samples = 2;
        public float uvDir;

        public void DrawGUI()
        {
            widthX = Mathf.Max(0.01f, EditorGUILayout.FloatField("Width", widthX));
            widthZ = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height", widthZ));
            depth = EditorGUILayout.IntSlider("深度", depth, 0, 10);
            uvDir = EditorGUILayout.Slider("UV水平方向", uvDir, 0, 360);
            samples = EditorGUILayout.IntSlider("不可见三角剔除采样", samples, 1, 4);
        }

        public void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight)
        {
            UnlitWaterHandles.DrawUnlitWaterArea(
                    target.transform.position + new Vector3(offset.x, 0, offset.y),
                    Quaternion.Euler(0, rotY, 0), new Vector2(widthX, widthZ),
                    new Vector2(minHeight, maxHeight), Color.green);

            int cellsize = (int)Mathf.Pow(2, depth);

            UnlitWaterHandles.DrawUnlitWaterLodCells(
                    target.transform.position + new Vector3(offset.x, 0, offset.y),
                    Quaternion.Euler(0, rotY, 0), new Vector2(widthX, widthZ), cellsize, cellsize, 0);

            float sz = Mathf.Max(widthX, widthZ) / 10;
            UnlitWaterHandles.DrawDirArrow(
                target.transform.position + new Vector3(offset.x, 0, offset.y), uvDir, sz,
                Color.cyan);
        }

        public Mesh GenerateMesh(Texture2D texture)
        {
            int cellSize = (int)Mathf.Pow(2, depth);
            if (widthX <= 0 || widthZ <= 0 || samples < 1)
                return null;
            QuadTreeMeshNode[,] cells = new QuadTreeMeshNode[cellSize, cellSize];

            //根据贴图尺寸和单元格数量，计算分配给单个单元格的像素宽高
            int w = texture.width / cellSize;
            int h = texture.height / cellSize;

            //计算Lod
            for (int i = 0; i < cellSize; i++)
            {
                for (int j = 0; j < cellSize; j++)
                {
                    var cell = new QuadTreeMeshLeaf(-widthX, -widthZ, i, j, widthX * 2 / cellSize,
                        widthZ * 2 / cellSize);
                    //为单元格分配指定区域的像素并计算极差和平均值
                    cell.Calculate(texture, i * w, j * h, w, h);
                    cells[i, j] = cell;
                }
            }
            
            
            float dtx = widthX * 2 / cellSize;
            float dty = widthZ * 2 / cellSize;

            MeshVertexData cache = new MeshVertexData(cellSize, cellSize, dtx, dty, -widthX, -widthZ);

            while (cellSize > 1)
            {
                cellSize = cellSize / 2;
                QuadTreeMeshNode[,] nodes = new QuadTreeMeshNode[cellSize, cellSize];
                for (int i = 0; i < cellSize; i++)
                {
                    for (int j = 0; j < cellSize; j++)
                    {
                        QuadTreeMeshNode lb = cells[i * 2, j * 2];
                        QuadTreeMeshNode rb = cells[i * 2 + 1, j * 2];
                        QuadTreeMeshNode lt = cells[i * 2, j * 2 + 1];
                        QuadTreeMeshNode rt = cells[i * 2 + 1, j * 2 + 1];
                        QuadTreeMeshNode node = new QuadTreeMeshNode(lt, lb, rt, rb, -widthX, -widthZ, i, j, widthX * 2 / cellSize,
                        widthZ * 2 / cellSize);
                        nodes[i, j] = node;
                    }
                }

                for (int i = 0; i < cellSize; i++)
                {
                    for (int j = 0; j < cellSize; j++)
                    {
                        var left = i != 0 ? nodes[i - 1, j] : null;
                        var right = i != nodes.GetLength(0) - 1 ? nodes[i + 1, j] : null;
                        var down = j != 0 ? nodes[i, j - 1] : null;
                        var up = j != nodes.GetLength(1) - 1 ? nodes[i, j + 1] : null;
                        nodes[i, j].SetNeighbor(left, right, up, down);
                    }
                }

                cells = nodes;
            }

            for (int i = 0; i < cellSize; i++)
            {
                for (int j = 0; j < cellSize; j++)
                {
                    cells[i, j].UpdateMesh(cache);
                }
            }

            return cache.Apply(texture, uvDir, samples);
        }

        public Vector2 GetSize()
        {
            return new Vector2(widthX, widthZ);
        }

        public void SetSize(Vector2 size)
        {
            widthX = size.x;
            widthZ = size.y;
        }
    }
}