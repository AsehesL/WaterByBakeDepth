using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoMipmappingMesh : MonoBehaviour
{
    public int width;
    public int height;
    public float cellSize;
    public float heightScale;

    public Texture2D heightMap;

    public Material material;

    private GeoMipmappingCell[,] m_Cells;

    private int m_X;
    private int m_Y;


    void Start()
    {
        m_Cells = new GeoMipmappingCell[width, height];

        Vector3 cpos = transform.worldToLocalMatrix.MultiplyPoint(Camera.main.transform.position);
        m_X = (int) (cpos.x/cellSize);
        m_Y = (int) (cpos.z/cellSize);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {

                m_Cells[i, j] = new GeoMipmappingCell(transform, material, i, j, cellSize);
                //m_Cells[i, j].CurrentLod = 4;
                for (int x = 0; x <= 16; x++)
                {
                    for (int y = 0; y <= 16; y++)
                    {
                        float px = ((float) (i*16 + x))/(width*16);
                        float py = ((float) (j*16 + y))/(height*16);
                        float h = heightMap == null
                            ? 0
                            : heightMap.GetPixel((int) (px*heightMap.width), (int) (py*heightMap.height)).r;
                        m_Cells[i, j].SetHeight(h*heightScale, x, y);
                    }
                }
                if (Mathf.Abs(m_X - i) <= 4 || Mathf.Abs(m_Y - j) <= 4)
                    m_Cells[i, j].CalculateLod(cpos);
                //m_Cells[i, j].BuildMesh(GetLod(i-1,j), GetLod(i+1,j), GetLod(i,j+1), GetLod(i,j-1));
            }
        }
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {

                m_Cells[i, j].BuildMesh(GetLod(i - 1, j), GetLod(i + 1, j), GetLod(i, j + 1), GetLod(i, j - 1));
            }
        }

    }

    int GetLod(int i, int j)
    {
        if (i < 0)
            return -1;
        if (i >= width)
            return -1;
        if (j < 0)
            return -1;
        if (j >= height)
            return -1;
        return m_Cells[i, j].CurrentLod;
    }

    void OnRenderObject()
    {
        Vector3 cpos = transform.worldToLocalMatrix.MultiplyPoint(Camera.main.transform.position);
        int x = (int) (cpos.x/cellSize);
        int y = (int) (cpos.z/cellSize);
        if (x != m_X || y != m_Y)
        {
            int bx = Mathf.Max(0, Mathf.Min(m_X - 4, x - 4));
            int ex = Mathf.Min(width - 1, Mathf.Max(m_X + 4, x + 4));
            int by = Mathf.Max(0, Mathf.Min(m_Y - 4, y - 4));
            int ey = Mathf.Min(height - 1, Mathf.Max(m_Y + 4, y + 4));

            for (int j = by; j <= ey; j++)
            {
                for (int i = bx; i <= ex; i++)
                {
                    if (i == bx && j == by)
                    {
                        m_Cells[i,j].CalculateLod(cpos);
                    }
                    if (j == by && i != ex)
                    {
                        m_Cells[i + 1,j].CalculateLod(cpos);
                    }
                    if (j < ey)
                    {
                        m_Cells[i,j + 1].CalculateLod(cpos);
                    }
                    m_Cells[i,j].BuildMesh(GetLod(i - 1, j), GetLod(i + 1, j), GetLod(i, j + 1), GetLod(i, j - 1));
                }
            }

            //for (int j = by; j <= ey; j++)
            //{
            //    for (int i = bx; i <= ex; i++)
            //    {
            //        m_Cells[i, j].BuildMesh(GetLod(i - 1, j), GetLod(i + 1, j), GetLod(i, j + 1), GetLod(i, j - 1));
            //    }
            //}

            m_X = x;
            m_Y = y;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i <= width; i++)
        {
            Vector3 from = new Vector3(i * cellSize, 0, 0);
            Vector3 to = new Vector3(i * cellSize, 0, height * cellSize);
            Gizmos.DrawLine(from, to);
        }
        for (int j = 0; j <= height; j++)
        {
            Vector3 from = new Vector3(0, 0, j * cellSize);
            Vector3 to = new Vector3(width * cellSize, 0, j * cellSize);
            Gizmos.DrawLine(from, to);
        }


    }
}
