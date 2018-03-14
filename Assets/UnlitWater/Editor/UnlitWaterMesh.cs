using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class UnlitWaterMesh
{

    public static Mesh GenerateMesh(Texture2D texture, int cellx, int celly, float width, float height, float offsetX, float offsetY, int lod)
    {
        UnlitWaterMeshCell[,] cells = new UnlitWaterMeshCell[cellx, celly];

        int w = texture.width/cellx;
        int h = texture.height/celly;

        for (int i = 0; i < cellx; i++)
        {
            for (int j = 0; j < celly; j++)
            {
                cells[i, j] = new UnlitWaterMeshCell(texture, i*w, j*h, w, h, offsetX, offsetY, i, j, width/cellx,
                    height/celly);
                if (cells[i, j].lod == -1)
                    continue;
                if (cells[i, j].deviation > 0.1f)
                    cells[i, j].lod = lod;
            }
        }

        for (int i = 0; i < cellx; i++)
        {
            for (int j = 0; j < celly; j++)
            {
                UnlitWaterMeshCell cell = cells[i, j];
                if (cell.lod == -1)
                    continue;
                if (cell.lod != lod)
                    continue;
                for (int lx = lod - 1, ly = 0; lx >= 0; lx--, ly++)
                {
                    for (int lk = 0; lk <= ly; lk++)
                    {
                        if (lk == 0 && lx == 0)
                            continue;
                        int clod = lod - lx - lk;
                        SetNeighborLOD(i - lx, j - lk, cellx, celly, clod, cells);
                        SetNeighborLOD(i + lx, j - lk, cellx, celly, clod, cells);
                        SetNeighborLOD(i - lx, j + lk, cellx, celly, clod, cells);
                        SetNeighborLOD(i + lx, j + lk, cellx, celly, clod, cells);
                    }
                }
                //int l = Mathf.FloorToInt((cell.variance - minvariance)/variancedelta);
                //if (l > lod)
                //    l = lod;
                //cell.lod = l;
            }
        }

        List<Vector3> vlist = new List<Vector3>();
        List<int> ilist = new List<int>();
        int index = 0;
        for (int i = 0; i < cellx; i++)
        {
            for (int j = 0; j < celly; j++)
            {
                UnlitWaterMeshCell cell = cells[i, j];
                if (cell.lod == -1)
                    continue;
                int leftLod = i == 0 ? -1 : cells[i - 1, j].lod;
                int rightLod = i == cells.GetLength(0) - 1 ? -1 : cells[i + 1, j].lod;
                int downLod = j == 0 ? -1 : cells[i, j - 1].lod;
                int upLod = j == cells.GetLength(1) - 1 ? -1 : cells[i, j + 1].lod;
                cell.UpdateMesh(vlist, ilist, leftLod, rightLod, upLod, downLod, ref index);
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vlist);
        mesh.SetTriangles(ilist, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void SetNeighborLOD(int i, int j, int cellx, int celly, int lod, UnlitWaterMeshCell[,] cells)
    {
        if (i < 0)
            return;
        if (i >= cellx)
            return;
        if (j < 0)
            return;
        if (j >= celly)
            return;
        if (lod < 0)
            return;
        if (lod <= cells[i, j].lod)
            return;
        cells[i, j].lod = lod;
    }
}
