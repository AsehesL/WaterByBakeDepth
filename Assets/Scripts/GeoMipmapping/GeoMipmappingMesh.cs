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
    

	void Start ()
	{
	    m_Cells = new GeoMipmappingCell[width, height];
	    for (int i = 0; i < width; i++)
	    {
	        for (int j = 0; j < height; j++)
	        {
	            m_Cells[i, j] = new GeoMipmappingCell(transform, material, i, j, cellSize);
	            //m_Cells[i, j].CurrentLod = 4;
	            for (int x = 0; x <= 16; x++)
	            {
	                for (int y = 0; y <= 16; y++)
	                {
	                    float px = ((float)(i*16 + x))/(width*16);
                        float py = ((float)(j * 16 + y)) / (height * 16);
	                    float h = heightMap == null ? 0 : heightMap.GetPixel((int)(px*heightMap.width),(int)(py*heightMap.height)).r;
	                    m_Cells[i, j].SetHeight(h* heightScale, x, y);
	                }
	            }
	            m_Cells[i, j].BuildMesh(4, 4, 4, 4);
	        }
	    }

	}
	
	void OnRenderObject () {
		
	}
}
