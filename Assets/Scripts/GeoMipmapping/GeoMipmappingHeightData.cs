using UnityEngine;
using System.Collections;

public class GeoMipmappingHeightData
{
    private float[,] m_Heights;

    public GeoMipmappingHeightData(int width, int height)
    {
        this.m_Heights = new float[width, height];
    }
}
