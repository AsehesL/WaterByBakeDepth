using UnityEngine;
using System.Collections;

[System.Serializable]
public class GMHeightData
{
    [SerializeField]
    private int m_Width;

    [SerializeField]
    private int m_Height;

    [SerializeField]
    private float[] m_HeightDatas;

    public float this[int i, int j]
    {
        get { return m_HeightDatas[j*m_Width + i]; }
    }


}
