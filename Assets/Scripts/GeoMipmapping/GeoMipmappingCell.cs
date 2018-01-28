using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeoMipmappingCell
{

    public int CurrentLod
    {
        get { return m_CurrentLod; }
        set
        {
            if (m_CurrentLod == value)
                return;
            m_CurrentLod = value;
            m_IsMeshChanged = true;
        }
    }

    public Vector3 Center
    {
        get { return new Vector3(m_CenterX, m_MinY + (m_MaxY - m_MinY)*0.5f, m_CenterZ); }
    }

    private float m_CellSize;
    private int m_CurrentLod;

    private Mesh m_Mesh;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;

    private List<Vector3> m_VertexList;
    private List<int> m_IndexList;

    private bool m_IsMeshChanged;

    private float m_CenterX;
    private float m_CenterZ;
    private float m_MinY;
    private float m_MaxY;

    private float[,] m_Heights; 

    public GeoMipmappingCell(Transform parent, Material material, int cellX, int cellY, float cellSize)
    {
        this.m_CurrentLod = 0;
        this.m_CellSize = cellSize;

        GameObject m = new GameObject();
        m.transform.SetParent(parent);
        m.hideFlags = HideFlags.HideInHierarchy;
        m.transform.localPosition = new Vector3(cellX*cellSize, 0, cellY*cellSize);

        m_VertexList = new List<Vector3>();
        m_IndexList = new List<int>();

        m_MeshFilter = m.AddComponent<MeshFilter>();
        m_MeshRenderer = m.AddComponent<MeshRenderer>();
        m_MeshRenderer.sharedMaterial = material;

        m_Mesh = new Mesh();
        m_Mesh.MarkDynamic();

        m_MeshFilter.sharedMesh = m_Mesh;

        m_CenterX = cellX*cellSize + cellSize*0.5f;
        m_CenterZ = cellY*cellSize + cellSize*0.5f;
        m_MinY = Mathf.Infinity;
        m_MaxY = -Mathf.Infinity;

        m_IsMeshChanged = true;

        m_Heights = new float[17, 17];

    }

    public void Release()
    {
        if (m_Mesh)
            Object.Destroy(m_Mesh);
        m_Mesh = null;
    }

    public void CalculateLod(Vector3 position)
    {
        float dis = Vector3.Distance(position, Center);
        int l = (int) Mathf.Clamp(dis/m_CellSize, 0, 4);
        CurrentLod = l;
    }

    public void SetHeight(float height, int i, int j)
    {
        if (m_MinY > height)
            m_MinY = height;
        if (m_MaxY < height)
            m_MaxY = height;
        m_Heights[i, j] = height;
    }

    public void BuildMesh(int leftLod, int rightLod, int upLod, int downLod)
    {
        if (!m_IsMeshChanged)
            return;
        m_IsMeshChanged = false;
        m_VertexList.Clear();
        m_IndexList.Clear();

        int xw = (int)Mathf.Pow(2, CurrentLod);
        int yw = xw;
        UpdateMesh_InternalLod(xw, yw, leftLod, rightLod, upLod, downLod);

        m_Mesh.Clear();
        m_Mesh.SetVertices(m_VertexList);
        m_Mesh.SetTriangles(m_IndexList, 0);
    }

    private void UpdateMesh_InternalLod(int xwidth, int ywidth, int leftLod, int rightLod, int upLod, int downLod)
    {
        int index = 0;
        if (CurrentLod == 0)
        {
            m_VertexList.Add(new Vector3(0, GetHeight(0,0), 0));
            m_VertexList.Add(new Vector3(m_CellSize, GetHeight(16, 0), 0));
            m_VertexList.Add(new Vector3(0, GetHeight(0, 16), m_CellSize));
            m_VertexList.Add(new Vector3(m_CellSize, GetHeight(16, 16), m_CellSize));

            m_IndexList.Add(index + 0);
            m_IndexList.Add(index + 2);
            m_IndexList.Add(index + 1);

            m_IndexList.Add(index + 1);
            m_IndexList.Add(index + 2);
            m_IndexList.Add(index + 3);

            index += 4;
            return;
        }
        else if (CurrentLod == 1)
        {
            m_VertexList.Add(new Vector3(0.5f * m_CellSize, GetHeight(8, 8), 0.5f * m_CellSize));
            index += 1;
        }
        else
        {
            for (int i = 1; i < ywidth; i++)
            {
                for (int j = 1; j < xwidth; j++)
                {
                    float px = ((float) j)/xwidth;
                    float py = ((float) i)/ywidth;
                    float x = px * m_CellSize;
                    float z = py * m_CellSize;
                    int hx = (int) (px*16);
                    int hy = (int) (py*16);
                    Vector3 pos = new Vector3(x, GetHeight(hx, hy), z);
                    m_VertexList.Add(pos);
                    if (j != xwidth - 1 && i != ywidth - 1)
                    {
                        m_IndexList.Add(index + (i - 1) * (xwidth - 1) + j - 1);
                        m_IndexList.Add(index + (i) * (xwidth - 1) + j - 1);
                        m_IndexList.Add(index + (i - 1) * (xwidth - 1) + j);

                        m_IndexList.Add(index + (i - 1) * (xwidth - 1) + j);
                        m_IndexList.Add(index + (i) * (xwidth - 1) + j - 1);
                        m_IndexList.Add(index + (i) * (xwidth - 1) + j);
                    }
                }
            }
            index += (ywidth - 1) * (xwidth - 1);
        }

        m_VertexList.Add(new Vector3(0, GetHeight(0, 0), 0));
        m_VertexList.Add(new Vector3(m_CellSize, GetHeight(16, 0), 0));
        m_VertexList.Add(new Vector3(0, GetHeight(0, 16), m_CellSize));
        m_VertexList.Add(new Vector3(m_CellSize, GetHeight(16, 16), m_CellSize));

        int lbindex = index;
        int rbindex = index + 1;
        int luindex = index + 2;
        int ruindex = index + 3;

        index += 4;

        UpdateMeshHorizontalEdge(0, 0, xwidth, downLod, lbindex, rbindex, 0, true, ref index);
        UpdateMeshHorizontalEdge(m_CellSize, 16, xwidth, upLod, luindex, ruindex, (xwidth - 1) * (ywidth - 2), false, ref index);
        UpdateMeshVerticalEdge(0, 0, xwidth, ywidth, leftLod, lbindex, luindex, 0, true, ref index);
        UpdateMeshVerticalEdge(m_CellSize, 16, xwidth, ywidth, rightLod, rbindex, ruindex, xwidth - 2, false, ref index);
    }

    private void UpdateMeshHorizontalEdge(float z, int hz, int edgeWidth, int neighborLod,
        int leftIndex, int rightIndex, int firstIndex, bool clockWise, ref int index)
    {
        int deltaLod = Mathf.Max(0, CurrentLod - neighborLod);
        int step = (int)Mathf.Pow(2, deltaLod);
        int sp = deltaLod * (deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int)Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= edgeWidth; i += step)
        {
            int ind = i / step;
            if (i != 0 && i != edgeWidth)
            {
                float px = ((float) i)/edgeWidth;
                float x = px * m_CellSize;
                int hx = (int) (px*16);
                m_VertexList.Add(new Vector3(x, GetHeight(hx, hz), z));
            }
            if (i != edgeWidth)
            {
                if (i == 0)
                    m_IndexList.Add(leftIndex);
                else
                    m_IndexList.Add(index + ind - 1);
                if (clockWise)
                {
                    if (i == edgeWidth - 1)
                        m_IndexList.Add(firstIndex + edgeWidth - 2);
                    else
                        m_IndexList.Add(firstIndex + i + offset);
                    if (i == edgeWidth - step)
                        m_IndexList.Add(rightIndex);
                    else
                        m_IndexList.Add(index + ind + 1 - 1);
                }
                else
                {
                    if (i == edgeWidth - step)
                        m_IndexList.Add(rightIndex);
                    else
                        m_IndexList.Add(index + ind + 1 - 1);
                    if (i == edgeWidth - 1)
                        m_IndexList.Add(firstIndex + edgeWidth - 2);
                    else
                        m_IndexList.Add(firstIndex + i + offset);
                }
            }
            if (i > 0 && i <= edgeWidth - step)
            {
                if (deltaLod != 0 || i != edgeWidth - 1)
                {
                    m_IndexList.Add(index + ind - 1);
                    if (clockWise)
                    {
                        m_IndexList.Add(firstIndex + i - 1);
                        m_IndexList.Add(firstIndex + i);
                    }
                    else
                    {
                        m_IndexList.Add(firstIndex + i);
                        m_IndexList.Add(firstIndex + i - 1);
                    }
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < edgeWidth - step)
                {
                    if (clockWise)
                    {
                        m_IndexList.Add(firstIndex + i + sp);
                        m_IndexList.Add(firstIndex + i + sp + 1);
                    }
                    else
                    {
                        m_IndexList.Add(firstIndex + i + sp + 1);
                        m_IndexList.Add(firstIndex + i + sp);
                    }
                    m_IndexList.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= edgeWidth - step)
                {
                    int bindex = i == 0 ? leftIndex : (index + ind - 1);
                    int eindex = i == edgeWidth - step ? rightIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            m_IndexList.Add(bindex);
                        else
                            m_IndexList.Add(eindex);
                        if (clockWise)
                        {
                            m_IndexList.Add(firstIndex + i + j);
                            m_IndexList.Add(firstIndex + i + j + 1);
                        }
                        else
                        {
                            m_IndexList.Add(firstIndex + i + j + 1);
                            m_IndexList.Add(firstIndex + i + j);
                        }
                    }
                }
            }
        }
        index += deltaLod == 0 ? (edgeWidth - 1) : (edgeWidth - 2) / step;
    }

    private void UpdateMeshVerticalEdge(float x, int hx, int xwidth, int ywidth, int neighborLod,
        int bottomIndex, int upIndex, int firstIndex, bool clockWise, ref int index)
    {
        int deltaLod = Mathf.Max(0, CurrentLod - neighborLod);
        int step = (int)Mathf.Pow(2, deltaLod);
        int sp = deltaLod * (deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int)Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= ywidth; i += step)
        {
            int ind = i / step;
            if (i != 0 && i != ywidth)
            {
                float pz = ((float) i)/ywidth;
                float z = pz * m_CellSize;
                int hz = (int) (pz*16);
                m_VertexList.Add(new Vector3(x, GetHeight(hx, hz), z));
            }
            if (i != ywidth)
            {
                if (i == 0)
                    m_IndexList.Add(bottomIndex);
                else
                    m_IndexList.Add(index + ind - 1);
                if (clockWise)
                {
                    if (i == ywidth - step)
                        m_IndexList.Add(upIndex);
                    else
                        m_IndexList.Add(index + ind + 1 - 1);
                    if (i == ywidth - 1)
                        m_IndexList.Add(firstIndex + (ywidth - 2) * (xwidth - 1));
                    else
                        m_IndexList.Add(firstIndex + (i + offset) * (xwidth - 1));
                }
                else
                {
                    if (i == ywidth - 1)
                        m_IndexList.Add(firstIndex + (ywidth - 2) * (xwidth - 1));
                    else
                        m_IndexList.Add(firstIndex + (i + offset) * (xwidth - 1));

                    if (i == ywidth - step)
                        m_IndexList.Add(upIndex);
                    else
                        m_IndexList.Add(index + ind + 1 - 1);
                }
            }
            if (i > 0 && i <= ywidth - step)
            {
                if (deltaLod != 0 || i != ywidth - 1)
                {
                    m_IndexList.Add(index + ind - 1);
                    if (clockWise)
                    {
                        m_IndexList.Add(firstIndex + (i) * (xwidth - 1));
                        m_IndexList.Add(firstIndex + (i - 1) * (xwidth - 1));
                    }
                    else
                    {
                        m_IndexList.Add(firstIndex + (i - 1) * (xwidth - 1));
                        m_IndexList.Add(firstIndex + (i) * (xwidth - 1));
                    }
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < ywidth - step)
                {
                    if (clockWise)
                    {
                        m_IndexList.Add(firstIndex + (i + sp + 1) * (xwidth - 1));
                        m_IndexList.Add(firstIndex + (i + sp) * (xwidth - 1));
                    }
                    else
                    {
                        m_IndexList.Add(firstIndex + (i + sp) * (xwidth - 1));
                        m_IndexList.Add(firstIndex + (i + sp + 1) * (xwidth - 1));
                    }
                    m_IndexList.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= ywidth - step)
                {
                    int bindex = i == 0 ? bottomIndex : (index + ind - 1);
                    int eindex = i == ywidth - step ? upIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            m_IndexList.Add(bindex);
                        else
                            m_IndexList.Add(eindex);
                        if (clockWise)
                        {
                            m_IndexList.Add(firstIndex + (i + j + 1) * (xwidth - 1));
                            m_IndexList.Add(firstIndex + (i + j) * (xwidth - 1));
                        }
                        else
                        {
                            m_IndexList.Add(firstIndex + (i + j) * (xwidth - 1));
                            m_IndexList.Add(firstIndex + (i + j + 1) * (xwidth - 1));
                        }
                    }

                }
            }
        }
        index += deltaLod == 0 ? (ywidth - 1) : (ywidth - 2) / step;
    }

    private float GetHeight(int i, int j)
    {
        return m_Heights[i, j];
    }
}
