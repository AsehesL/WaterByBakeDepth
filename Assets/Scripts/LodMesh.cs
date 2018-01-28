using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LodMesh
{
    public Mesh Mesh { get { return m_Mesh; } }

    private Mesh m_Mesh;

    private List<Vector3> m_VertexList;

    private List<int> m_IndexList;

    LodMeshCell[,] m_Cells;

    public LodMesh(int xwidth, int ywidth, float scale)
    {
        m_Cells = new LodMeshCell[xwidth, ywidth];
        for (int i = 0; i < xwidth; i++)
        {
            for (int j = 0; j < ywidth; j++)
            {
                //int cx = Mathf.FloorToInt(pos.x/scale);
                //int cy = Mathf.FloorToInt(pos.z / scale);
                //int delta = Mathf.Clamp(Mathf.Abs(cx - i) + Mathf.Abs(cy - j), 0, 4);
                m_Cells[i, j] = new LodMeshCell(0, 4, i, j, scale);
                if (i == 3 && j == 3)
                    m_Cells[i, j].CurrentLod = 3;
                if (i == 2 && j == 3)
                    m_Cells[i, j].CurrentLod = 1;
                if (i == 4 && j == 3)
                    m_Cells[i, j].CurrentLod = 1;
                if (i == 3 && j == 2)
                    m_Cells[i, j].CurrentLod = 1;
                if (i == 3 && j == 4)
                    m_Cells[i, j].CurrentLod = 1;
                //m_Cells[i, j].CurrentLod = 4 - delta;
            }
        }

        m_VertexList = new List<Vector3>();
        m_IndexList = new List<int>();
    }

    public void BuildMesh()
    {
        if (m_Mesh == null)
            m_Mesh = new Mesh();
        m_Mesh.Clear();
        m_VertexList.Clear();
        m_IndexList.Clear();


        int index = 0;
        for (int i = 0; i < m_Cells.GetLength(0); i++)
        {
            for (int j = 0; j < m_Cells.GetLength(1); j++)
            {
                int leftLod = i == 0 ? -1 : m_Cells[i - 1, j].CurrentLod;
                int rightLod = i == m_Cells.GetLength(0) - 1 ? -1 : m_Cells[i + 1, j].CurrentLod;
                int downLod = j == 0 ? -1 : m_Cells[i, j - 1].CurrentLod;
                int upLod = j == m_Cells.GetLength(1) - 1 ? -1 : m_Cells[i, j + 1].CurrentLod;
                m_Cells[i, j].UpdateMesh(m_VertexList, m_IndexList, leftLod, rightLod, upLod, downLod, ref index);
            }
        }

        m_Mesh.SetVertices(m_VertexList);
        m_Mesh.SetTriangles(m_IndexList, 0);
    }

    public void Release()
    {
        if (m_Mesh)
            Object.Destroy(m_Mesh);
        m_Mesh = null;
    }
}
