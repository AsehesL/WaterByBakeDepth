using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LodMeshCell
{
    public int CurrentLod { get; set; }


    private int m_MinLod;
    private int m_MaxLod;
    private int m_CellX;
    private int m_CellY;
    private float m_Scale;

    public LodMeshCell(int minLod, int maxlod, int cellX, int cellY, float scale)
    {
        this.m_MinLod = minLod;
        this.m_MaxLod = maxlod;
        this.CurrentLod = minLod;
        this.m_CellX = cellX;
        this.m_CellY = cellY;
        this.m_Scale = scale;
    }

    public void UpdateMesh(List<Vector3> vlist, List<int> ilist, int leftLod, int rightLod, int upLod, int downLod,
        ref int index)
    {
        int xw = (int) Mathf.Pow(2, CurrentLod);
        int yw = xw;
        //if (CurrentLod <= 1)
        //    UpdateMesh_Internal(vlist, ilist, xw, yw, ref index);
        //else
            UpdateMesh_InternalLod(vlist, ilist, xw, yw, leftLod, rightLod, upLod, downLod, ref index);
    }

    private void UpdateMesh_Internal(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, ref int index)
    {
        for (int i = 0; i <= ywidth; i++)
        {
            for (int j = 0; j <= xwidth; j++)
            {
                float x = ((float) j)/xwidth*m_Scale + m_CellX*m_Scale;
                float z = ((float) i)/ywidth*m_Scale + m_CellY*m_Scale;
                Vector3 pos = new Vector3(x, 0, z);
                vlist.Add(pos);
                if (j != xwidth && i != ywidth)
                {
                    ilist.Add(index + i*(xwidth + 1) + j);
                    ilist.Add(index + (i + 1)*(xwidth + 1) + j);
                    ilist.Add(index + i*(xwidth + 1) + j + 1);

                    ilist.Add(index + i*(xwidth + 1) + j + 1);
                    ilist.Add(index + (i + 1)*(xwidth + 1) + j);
                    ilist.Add(index + (i + 1)*(xwidth + 1) + j + 1);


                }
            }
        }
        index += (ywidth + 1)*(xwidth + 1);
    }

    private void UpdateMesh_InternalLod(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, int leftLod,
        int rightLod, int upLod, int downLod, ref int index)
    {
        int firstIndex = index;
        if (CurrentLod == 0)
        {
            vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale + m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale + m_Scale));

            ilist.Add(index + 0);
            ilist.Add(index + 2);
            ilist.Add(index + 1);

            ilist.Add(index + 1);
            ilist.Add(index + 2);
            ilist.Add(index + 3);

            index += 4;
            return;
        }
        else if (CurrentLod == 1)
        {
            vlist.Add(new Vector3(m_CellX*m_Scale + 0.5f*m_Scale, 0, m_CellY*m_Scale + 0.5f*m_Scale));
            index += 1;
        }
        else
        {
            for (int i = 1; i < ywidth; i++)
            {
                for (int j = 1; j < xwidth; j++)
                {
                    float x = ((float) j)/xwidth*m_Scale + m_CellX*m_Scale;
                    float z = ((float) i)/ywidth*m_Scale + m_CellY*m_Scale;
                    Vector3 pos = new Vector3(x, 0, z);
                    vlist.Add(pos);
                    if (j != xwidth - 1 && i != ywidth - 1)
                    {
                        ilist.Add(index + (i - 1)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i - 1)*(xwidth - 1) + j);

                        ilist.Add(index + (i - 1)*(xwidth - 1) + j);
                        ilist.Add(index + (i)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i)*(xwidth - 1) + j);
                    }
                }
            }
            index += (ywidth - 1)*(xwidth - 1);
        }

        vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale + m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale + m_Scale));

        int lbindex = index;
        int rbindex = index + 1;
        int luindex = index + 2;
        int ruindex = index + 3;

        index += 4;

        UpdateMeshBottomEdge(vlist, ilist, xwidth, downLod, lbindex, rbindex, firstIndex, ref index);
        UpdateMeshUpEdge(vlist, ilist, xwidth, ywidth, upLod, luindex, ruindex, firstIndex, ref index);
        UpdateMeshLeftEdge(vlist, ilist, xwidth, ywidth, leftLod, lbindex, luindex, firstIndex, ref index);
        UpdateMeshRightEdge(vlist, ilist, xwidth, ywidth, rightLod, rbindex, ruindex, firstIndex, ref index);
    }

    private void UpdateMeshBottomEdge(List<Vector3> vlist, List<int> ilist, int xwidth, int downLod,
        int leftIndex, int rightIndex, int firstIndex, ref int index)
    {
        float z = m_CellY*m_Scale;
        int deltaLod = Mathf.Max(0, CurrentLod - downLod);
        int step = (int) Mathf.Pow(2, deltaLod);
        int sp = deltaLod*(deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int) Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= xwidth; i += step)
        {
            int ind = i/step;
            if (i != 0 && i != xwidth)
            {
                float x = ((float) i)/xwidth*m_Scale + m_CellX*m_Scale;
                vlist.Add(new Vector3(x, 0, z));
            }
            if (i != xwidth)
            {
                if (i == 0)
                    ilist.Add(leftIndex);
                else
                    ilist.Add(index + ind - 1);
                if (i == xwidth - 1)
                    ilist.Add(firstIndex + xwidth - 2);
                else
                    ilist.Add(firstIndex + i + offset);
                if (i == xwidth - step)
                    ilist.Add(rightIndex);
                else
                    ilist.Add(index + ind + 1 - 1);
            }
            if (i > 0 && i <= xwidth - step)
            {
                if (deltaLod != 0 || i != xwidth - 1)
                {
                    ilist.Add(index + ind - 1);
                    ilist.Add(firstIndex + i - 1);
                    ilist.Add(firstIndex + i);
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < xwidth - step)
                {
                    ilist.Add(firstIndex + i + sp);
                    ilist.Add(firstIndex + i + sp + 1);
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= xwidth - step)
                {
                    int bindex = i == 0 ? leftIndex : (index + ind - 1);
                    int eindex = i == xwidth - step ? rightIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        ilist.Add(firstIndex + i + j);
                        ilist.Add(firstIndex + i + j + 1);
                    }

                }
            }
        }
        index += deltaLod == 0 ? (xwidth - 1) : (xwidth - 2)/step;
    }

    private void UpdateMeshUpEdge(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, int upLod,
        int leftIndex, int rightIndex, int firstIndex, ref int index)
    {
        float z = m_CellY*m_Scale + m_Scale;
        int deltaLod = CurrentLod - upLod;
        int step = (int) Mathf.Pow(2, deltaLod);
        int sp = deltaLod*(deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int) Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= xwidth; i += step)
        {
            int ind = i/step;
            if (i != 0 && i != xwidth)
            {
                float x = ((float) i)/xwidth*m_Scale + m_CellX*m_Scale;
                vlist.Add(new Vector3(x, 0, z));
            }
            if (i != xwidth)
            {
                if (i == 0)
                    ilist.Add(leftIndex);
                else
                    ilist.Add(index + ind - 1);
                if (i == xwidth - step)
                    ilist.Add(rightIndex);
                else
                    ilist.Add(index + ind + 1 - 1);
                if (i == xwidth - 1)
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + xwidth - 2);
                else
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i + offset);
            }
            if (i > 0 && i <= xwidth - step)
            {
                if (deltaLod != 0 || i != xwidth - 1)
                {
                    ilist.Add(index + ind - 1);
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i);
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i - 1);
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < xwidth - step)
                {
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i + sp + 1);
                    ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i + sp);
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= xwidth - step)
                {
                    int bindex = i == 0 ? leftIndex : (index + ind - 1);
                    int eindex = i == xwidth - step ? rightIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i + j + 1);
                        ilist.Add(firstIndex + (xwidth - 1)*(ywidth - 2) + i + j);

                    }

                }
            }
        }
        index += deltaLod == 0 ? (xwidth - 1) : (xwidth - 2)/step;
    }

    private void UpdateMeshLeftEdge(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, int leftLod,
        int bottomIndex, int upIndex, int firstIndex, ref int index)
    {
        float x = m_CellX * m_Scale;
        if (CurrentLod <= leftLod)
        {
            for (int i = 0; i <= ywidth; i += 1)
            {
                if (i != 0 && i != ywidth)
                {
                    float z = ((float)i) / ywidth * m_Scale + m_CellY * m_Scale;
                    vlist.Add(new Vector3(x, 0, z));
                }
                if (i < ywidth - 1)
                {
                    if (i == 0)
                        ilist.Add(bottomIndex);
                    else
                        ilist.Add(index + i - 1);
                    ilist.Add(index + i + 1 - 1);
                    ilist.Add(firstIndex + i * (xwidth - 1));
                }
                if (i == ywidth - 1)
                {
                    ilist.Add(index + i - 1);
                    ilist.Add(upIndex);
                    ilist.Add(firstIndex + (i -1) * (xwidth - 1));
                }
                if (i > 0 && i < ywidth - 1)
                {
                    ilist.Add(index + i - 1);
                    ilist.Add(firstIndex + i * (xwidth - 1));
                    ilist.Add(firstIndex + (i - 1) * (xwidth - 1));
                }

            }
            index += (ywidth - 1);
        }
        else
        {
            int deltaLod = CurrentLod - leftLod;
            int step = (int)Mathf.Pow(2, deltaLod);
            int sp = deltaLod * (deltaLod - 1);
            int offset = (int)Mathf.Pow(2, deltaLod - 1) - 1;
            for (int i = 0; i <= ywidth; i += step)
            {
                int ind = i / step;
                if (i != 0 && i != ywidth)
                {
                    float z = ((float)i) / ywidth * m_Scale + m_CellY * m_Scale;
                    vlist.Add(new Vector3(x, 0, z));
                }
                if (i != ywidth)
                {
                    if (i == 0)
                        ilist.Add(bottomIndex);
                    else
                        ilist.Add(index + ind - 1);
                    if (i == ywidth - step)
                        ilist.Add(upIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                    ilist.Add(firstIndex + (i + offset) * (xwidth - 1));
                }
                if (i > 0 && i <= ywidth - step)
                {
                    ilist.Add(index + ind - 1);
                    ilist.Add(firstIndex + (i) * (xwidth - 1));
                    ilist.Add(firstIndex + (i - 1) * (xwidth - 1));
                }
                if (i >= 0 && i < ywidth - step)
                {
                    ilist.Add(firstIndex + (i+sp+1) * (xwidth - 1));
                    ilist.Add(firstIndex + (i+sp) * (xwidth - 1));
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= ywidth - step)
                {
                    int bindex = i == 0 ? bottomIndex : (index + ind - 1);
                    int eindex = i == ywidth - step ? upIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        ilist.Add(firstIndex + (i + j+ 1) * (xwidth - 1));
                        ilist.Add(firstIndex + (i + j) * (xwidth - 1));
                    }

                }
            }
            index += (ywidth - 2) / step;
        }
    }

    private void UpdateMeshRightEdge(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, int rightLod,
        int bottomIndex, int upIndex, int firstIndex, ref int index)
    {
        float x = m_CellX * m_Scale+m_Scale;
        if (CurrentLod <= rightLod)
        {
            for (int i = 0; i <= ywidth; i += 1)
            {
                if (i != 0 && i != ywidth)
                {
                    float z = ((float)i) / ywidth * m_Scale + m_CellY * m_Scale;
                    vlist.Add(new Vector3(x, 0, z));
                }
                if (i < ywidth - 1)
                {
                    if (i == 0)
                        ilist.Add(bottomIndex);
                    else
                        ilist.Add(index + i - 1);
                    ilist.Add(firstIndex + i * (xwidth - 1) + xwidth - 2);
                    ilist.Add(index + i + 1 - 1);
                }
                if (i == ywidth - 1)
                {
                    ilist.Add(index + i - 1);
                    ilist.Add(firstIndex + (i-1) * (xwidth - 1) + xwidth - 2);
                    ilist.Add(upIndex);
                }
                if (i > 0 && i < ywidth - 1)
                {
                    ilist.Add(index + i - 1);
                    ilist.Add(firstIndex + (i - 1) * (xwidth - 1) + xwidth - 2);
                    ilist.Add(firstIndex + (i) * (xwidth - 1) + xwidth - 2);
                }

            }
            index += (ywidth - 1);
        }
        else
        {
            int deltaLod = CurrentLod - rightLod;
            int step = (int)Mathf.Pow(2, deltaLod);
            int sp = deltaLod * (deltaLod - 1);
            int offset = (int)Mathf.Pow(2, deltaLod - 1) - 1;
            for (int i = 0; i <= ywidth; i += step)
            {
                int ind = i / step;
                if (i != 0 && i != ywidth)
                {
                    float z = ((float)i) / ywidth * m_Scale + m_CellY * m_Scale;
                    vlist.Add(new Vector3(x, 0, z));
                }
                if (i != ywidth)
                {
                    if (i == 0)
                        ilist.Add(bottomIndex);
                    else
                        ilist.Add(index + ind - 1);
                    ilist.Add(firstIndex + (i + offset) * (xwidth - 1) + xwidth - 2);
                    if (i == ywidth - step)
                        ilist.Add(upIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                }
                if (i > 0 && i <= ywidth - step)
                {
                    ilist.Add(index + ind - 1);
                    ilist.Add(firstIndex + (i - 1) * (xwidth - 1) + xwidth - 2);
                    ilist.Add(firstIndex + (i) * (xwidth - 1) + xwidth - 2);
                }
                if (i >= 0 && i < ywidth - step)
                {
                    ilist.Add(firstIndex + (i + sp) * (xwidth - 1) + xwidth - 2);
                    ilist.Add(firstIndex + (i + sp + 1) * (xwidth - 1) + xwidth - 2);
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= ywidth - step)
                {
                    int bindex = i == 0 ? bottomIndex : (index + ind - 1);
                    int eindex = i == ywidth - step ? upIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        ilist.Add(firstIndex + (i + j) * (xwidth - 1) + xwidth - 2);
                        ilist.Add(firstIndex + (i + j + 1) * (xwidth - 1) + xwidth - 2);
                    }

                }
            }
            index += (ywidth - 2) / step;
        }
    }
}
