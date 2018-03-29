using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    [System.Serializable]
    internal class QuadTreeMesh : IMeshGenerator
    {
        public int gradation = 0;
        public float widthX;
        public float widthZ;
        
        public int samples = 2;
        public float uvDir;

        public void DrawGUI()
        {
            widthX = Mathf.Max(0.01f, EditorGUILayout.FloatField("Width", widthX));
            widthZ = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height", widthZ));
            gradation = EditorGUILayout.IntSlider("层次", gradation, 0, 10);
            uvDir = EditorGUILayout.Slider("UV水平方向", uvDir, 0, 360);
            samples = EditorGUILayout.IntSlider("不可见三角剔除采样", samples, 1, 4);
        }

        public void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight)
        {
            UnlitWaterHandles.DrawUnlitWaterArea(
                    target.transform.position + new Vector3(offset.x, 0, offset.y),
                    Quaternion.Euler(0, rotY, 0), new Vector2(widthX, widthZ),
                    new Vector2(minHeight, maxHeight), Color.green);

            int cellsize = (int)Mathf.Pow(2, gradation);

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
            int cellSize = (int)Mathf.Pow(2, gradation);
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
                    if (cell.range > LodMesh.kEdgeRange) //如果极差超过0.4，则判定该单元格同时包含水域和陆地，即岸边区域，应该给予最大lod
                        cell.isEdge = true;
                    else
                        cell.isEdge = false;
                    cells[i, j] = cell;
                }
            }

            ////根据上一步计算的结果，将最大lod单元格边上的格子设置lod递减
            //for (int i = 0; i < cellSize; i++)
            //{
            //    for (int j = 0; j < cellSize; j++)
            //    {
            //        QuadTreeMeshNode cell = cells[i, j];
            //        if (cell.lod == -1)
            //            continue;
            //        if (cell.lod != maxLod)
            //            continue;
            //        for (int lx = maxLod - 1, ly = 0; lx >= 0; lx--, ly++)
            //        {
            //            for (int lk = 0; lk <= ly; lk++)
            //            {
            //                if (lk == 0 && lx == 0)
            //                    continue;
            //                int clod = maxLod - lx - lk;
            //                //从最大lod处往外递减lod
            //                SetNeighborLOD(i - lx, j - lk, cellSize, cellSize, clod, cells);
            //                SetNeighborLOD(i + lx, j - lk, cellSize, cellSize, clod, cells);
            //                SetNeighborLOD(i - lx, j + lk, cellSize, cellSize, clod, cells);
            //                SetNeighborLOD(i + lx, j + lk, cellSize, cellSize, clod, cells);
            //            }
            //        }
            //    }
            //}
            
            
            float dtx = widthX * 2 / cellSize;
            float dty = widthZ * 2 / cellSize;

            MeshVertexData cache = new MeshVertexData(cellSize + 1, cellSize + 1, dtx, dty, -widthX, -widthZ);

            //for (int i = 0; i < cellSize; i++)
            //{
            //    for (int j = 0; j < cellSize; j++)
            //    {
            //        LodMeshCell cell = cells[i, j];

            //        lastnodes[i, j] = new GradationLodMeshLeaf(cell);
            //        if (cell.lod == -1)
            //            continue;
            //        int leftLod = i == 0 ? -1 : cells[i - 1, j].lod;
            //        int rightLod = i == cells.GetLength(0) - 1 ? -1 : cells[i + 1, j].lod;
            //        int downLod = j == 0 ? -1 : cells[i, j - 1].lod;
            //        int upLod = j == cells.GetLength(1) - 1 ? -1 : cells[i, j + 1].lod;
            //        cell.SetNeighborLOD(leftLod, rightLod, upLod, downLod);
            //    }
            //}

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