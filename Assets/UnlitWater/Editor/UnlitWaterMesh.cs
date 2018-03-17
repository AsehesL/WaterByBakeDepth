using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UnlitWater自动网格生成器
/// </summary>
public static class UnlitWaterMeshGenerator
{
    /// <summary>
    /// 生成网格
    /// </summary>
    /// <param name="texture">参照纹理</param>
    /// <param name="xCells">x方向单元格数量</param>
    /// <param name="zCells">z方向单元格数量</param>
    /// <param name="xWidth">x方向宽度</param>
    /// <param name="zWidth">z方向宽度</param>
    /// <param name="offsetX">x方向偏移</param>
    /// <param name="offsetZ">z方向偏移</param>
    /// <param name="maxLod">最大lod</param>
    /// <returns></returns>
    public static Mesh GenerateMesh(Texture2D texture, int xCells, int zCells, float xWidth, float zWidth, float offsetX, float offsetZ, int maxLod)
    {
        //构建单元格数组
        UnlitWaterMeshCell[,] cells = new UnlitWaterMeshCell[xCells, zCells];

        //根据贴图尺寸和单元格数量，计算分配给单个单元格的像素宽高
        int w = texture.width/xCells;
        int h = texture.height/zCells;

        //计算Lod
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < zCells; j++)
            {
                cells[i, j] = new UnlitWaterMeshCell(offsetX, offsetZ, i, j, xWidth/xCells,
                    zWidth / zCells);
                //为单元格分配指定区域的像素并计算极差和平均值
                cells[i, j].Calculate(texture, i*w, j*h, w, h);
                if (cells[i, j].average < 0.01f)
                {
                    cells[i, j].lod = -1;//如果单元格像素颜色平均值小于0.01，则判定该单元格基本上位于非水域内，则lod设置为-1，将不参与水网格的构建
                    continue;
                }
                if (cells[i, j].range > 0.4f)//如果极差超过0.4，则判定该单元格同时包含水域和陆地，即岸边区域，应该给予最大lod
                    cells[i, j].lod = maxLod;
            }
        }

        //根据上一步计算的结果，将最大lod单元格边上的格子设置lod递减
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < zCells; j++)
            {
                UnlitWaterMeshCell cell = cells[i, j];
                if (cell.lod == -1)
                    continue;
                if (cell.lod != maxLod)
                    continue;
                for (int lx = maxLod - 1, ly = 0; lx >= 0; lx--, ly++)
                {
                    for (int lk = 0; lk <= ly; lk++)
                    {
                        if (lk == 0 && lx == 0)
                            continue;
                        int clod = maxLod - lx - lk;
                        SetNeighborLOD(i - lx, j - lk, xCells, zCells, clod, cells);
                        SetNeighborLOD(i + lx, j - lk, xCells, zCells, clod, cells);
                        SetNeighborLOD(i - lx, j + lk, xCells, zCells, clod, cells);
                        SetNeighborLOD(i + lx, j + lk, xCells, zCells, clod, cells);
                    }
                }
            }
        }

        //根据Lod生成Mesh

        float p = Mathf.Pow(2, maxLod);
        float dtx = xWidth / xCells / p;
        float dty = xWidth / zCells / p;

        UnlitWaterMeshVertexCache cache = new UnlitWaterMeshVertexCache(xCells*(int)p+1, zCells*(int)p+1, dtx, dty, offsetX, offsetZ);
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < zCells; j++)
            {
                UnlitWaterMeshCell cell = cells[i, j];
                if (cell.lod == -1)
                    continue;
                int leftLod = i == 0 ? -1 : cells[i - 1, j].lod;
                int rightLod = i == cells.GetLength(0) - 1 ? -1 : cells[i + 1, j].lod;
                int downLod = j == 0 ? -1 : cells[i, j - 1].lod;
                int upLod = j == cells.GetLength(1) - 1 ? -1 : cells[i, j + 1].lod;
                cell.UpdateMesh(cache, leftLod, rightLod, upLod, downLod);
            }
        }

        Mesh mesh = cache.Apply(texture);
        return mesh;
    }

    /// <summary>
    /// 设置相邻网格的Lod
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="cellx"></param>
    /// <param name="celly"></param>
    /// <param name="lod"></param>
    /// <param name="cells"></param>
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
        if (cells[i, j].lod < 0)
            return;
        if (lod <= cells[i, j].lod)
            return;
        cells[i, j].lod = lod;
    }

    /// <summary>
    /// UnlitWater网格顶点缓存
    /// </summary>
    private class UnlitWaterMeshVertexCache
    {
        /// <summary>
        /// 顶点数据
        /// </summary>
        private class VertexData
        {
            public Vector3 vertex;
            public Vector2 uv;
            public int index;

            public VertexData(Vector3 vertex, float beginx, float beginy, int width, int height, float cellSizeX, float cellSizeY)
            {
                this.vertex = vertex;
                this.index = -1;

                uv = new Vector2();
                uv.x = (vertex.x - beginx) / ((width - 1) * cellSizeX);
                uv.y = (vertex.z - beginy) / ((height - 1) * cellSizeY);
            }

            public bool IsVisible(Texture2D tex, float compare)
            {
                int x = (int)(uv.x * tex.width);
                int y = (int)(uv.y * tex.height);
                Color col = tex.GetPixel(x, y);
                if (col.g < compare)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// 当前索引
        /// </summary>
        public int index = 0;

        private List<Vector3> m_VertexList;

        private List<int> m_IndexList;

        /// <summary>
        /// 缓存相同key的顶点数据
        /// </summary>
        private Dictionary<int, VertexData> m_Vertexs;

        private int m_Width;
        private int m_Height;
        private float m_CellSizeX;
        private float m_CellSizeY;
        private float m_BeginX;
        private float m_BeginY;

        //private int m_Count = 0;

        public UnlitWaterMeshVertexCache(int width, int height, float cellSizeX, float cellSizeY, float beginX, float beginY)
        {
            m_VertexList = new List<Vector3>();
            m_IndexList = new List<int>();
            m_Vertexs = new Dictionary<int, VertexData>();

            m_Width = width;
            m_Height = height;
            m_CellSizeX = cellSizeX;
            m_CellSizeY = cellSizeY;
            m_BeginX = beginX;
            m_BeginY = beginY;
        }

        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="vertex"></param>
        public void AddVertex(Vector3 vertex)
        {
            m_VertexList.Add(vertex);
            int k = GetVertexKey(vertex);
            if (!m_Vertexs.ContainsKey(k))//确保不记录相同k的顶点
            {
                m_Vertexs.Add(k, new VertexData(vertex, m_BeginX, m_BeginY, m_Width, m_Height, m_CellSizeX, m_CellSizeY));
                //m_Count += 1;
            }
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="index"></param>
        public void AddIndex(int index)
        {
            m_IndexList.Add(index);
        }

        /// <summary>
        /// 应用网格（产生没有重合顶点的mesh）
        /// </summary>
        /// <returns></returns>
        public Mesh Apply(Texture2D texture)
        {
            List<Vector3> vlist = new List<Vector3>();
            List<Vector2> ulist = new List<Vector2>();
            List<int> ilist = new List<int>();

            foreach (var dt in m_Vertexs)
            {
                Vector2 uv = new Vector2();
                uv.x = (dt.Value.vertex.x - m_BeginX)/((m_Width - 1)*m_CellSizeX);
                uv.y = (dt.Value.vertex.z - m_BeginY) / ((m_Height - 1) * m_CellSizeY);

                bool isVisible = dt.Value.IsVisible(texture, 0.1f);
                if (!isVisible)
                {
                    continue;
                }
                dt.Value.index = vlist.Count;

                vlist.Add(dt.Value.vertex);
                ulist.Add(uv);

                //vlist[dt.Value.index] = dt.Value.vertex;
                //ulist[dt.Value.index] = uv;
            }

            for (int i = 0; i < m_IndexList.Count; i+=3)
            {
                Vector3 vertex0 = m_VertexList[m_IndexList[i]];
                Vector3 vertex1 = m_VertexList[m_IndexList[i+1]];
                Vector3 vertex2 = m_VertexList[m_IndexList[i+2]];
                int k0 = GetVertexKey(vertex0);
                int k1 = GetVertexKey(vertex1);
                int k2 = GetVertexKey(vertex2);
                if (!m_Vertexs.ContainsKey(k0) || !m_Vertexs.ContainsKey(k1) || !m_Vertexs.ContainsKey(k2))
                    continue;
                //if (!m_Vertexs.ContainsKey(k))
                //    continue;
                //var dt = m_Vertexs[k];
                //if (dt.index < 0)
                //    continue;
                var dt0 = m_Vertexs[k0];
                var dt1 = m_Vertexs[k1];
                var dt2 = m_Vertexs[k2];
                if (dt0.index < 0 || dt1.index < 0 || dt2.index < 0)
                    continue;

                var uv0 = ulist[dt0.index];
                var uv1 = ulist[dt1.index];
                var uv2 = ulist[dt2.index];
                
                //if (!IsVisible(texture, uv0, 0.1f))
                //    continue;
                //if (!IsVisible(texture, uv1, 0.1f))
                //    continue;
                //if (!IsVisible(texture, uv2, 0.1f))
                //    continue;

                //ilist[i] = dt.index;
                ilist.Add(dt0.index);
                ilist.Add(dt1.index);
                ilist.Add(dt2.index);
                //ilist.Add(dt.index);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vlist);
            mesh.SetUVs(0, ulist);
            mesh.SetTriangles(ilist, 0);
            //mesh.SetVertices(m_VertexList);
            //mesh.SetTriangles(m_IndexList, 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        private int GetVertexKey(Vector3 vertex)
        {
            int x = Mathf.FloorToInt((vertex.x - m_BeginX - m_CellSizeX*0.5f) / m_CellSizeX);
            int y = Mathf.FloorToInt((vertex.z - m_BeginY - m_CellSizeY *0.5f) / m_CellSizeY);
            int k = y * m_Width + x;
            return k;
        }
    }

    /// <summary>
    /// UnlitWater自动网格单元格
    /// </summary>
    private class UnlitWaterMeshCell
    {
        public int lod { get; set; }

        /// <summary>
        /// 极差
        /// </summary>
        public float range { get; private set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public float average { get; private set; }

        private float m_OffsetX;
        private float m_OffsetY;
        private int m_CellX;
        private int m_CellY;

        private float m_CellWidth;
        private float m_CellHeight;

        /// <summary>
        /// 构造单元格
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="cellWidth"></param>
        /// <param name="cellHeight"></param>
        public UnlitWaterMeshCell(float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
        {
            this.m_OffsetX = offsetX;
            this.m_OffsetY = offsetY;
            this.m_CellX = cellX;
            this.m_CellY = cellY;
            this.m_CellWidth = cellWidth;
            this.m_CellHeight = cellHeight;
        }

        public void Calculate(Texture2D tex, int x, int y, int width, int height)
        {
            float min = 1f;
            float max = 0f;

            float sum = 0;

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    Color c = tex.GetPixel(i, j);
                    if (c.g < min)
                        min = c.g;
                    if (c.g > max)
                        max = c.g;

                    sum += c.g;
                }
            }

            this.range = max - min;

            average = sum / (width * height);

            //float sum = 0;
            //for (int i = x; i < x + width; i++)
            //{
            //    for (int j = y; j < y + height; j++)
            //    {
            //        Color c = tex.GetPixel(i, j);
            //        sum += c.g;
            //    }
            //}

            //float u = sum / (width * height);
            //if (u < 0.01f)
            //{
            //    this.lod = -1;
            //    return;
            //}

            //deviation = 0;

            //for (int i = x; i < x + width; i++)
            //{
            //    for (int j = y; j < y + height; j++)
            //    {
            //        Color c = tex.GetPixel(i, j);
            //        deviation += (c.g - u) * (c.g - u);
            //    }
            //}
            //deviation = deviation / (width * height);
            //deviation = Mathf.Sqrt(deviation);
        }

        public void UpdateMesh(UnlitWaterMeshVertexCache cache, int leftLod, int rightLod, int upLod, int downLod)
        {
            int xw = (int)Mathf.Pow(2, lod);
            int yw = xw;
            UpdateMesh_InternalLod(cache, xw, yw, leftLod, rightLod, upLod, downLod);
        }

        private void UpdateMesh_InternalLod(UnlitWaterMeshVertexCache cache, int xwidth, int ywidth, int leftLod,
            int rightLod, int upLod, int downLod)
        {
            int firstIndex = cache.index;
            if (lod == 0)
            {
                cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_OffsetY));
                cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_OffsetY));
                cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_CellHeight + m_OffsetY));
                cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_CellHeight + m_OffsetY));

                cache.AddIndex(cache.index + 0);
                cache.AddIndex(cache.index + 2);
                cache.AddIndex(cache.index + 1);

                cache.AddIndex(cache.index + 1);
                cache.AddIndex(cache.index + 2);
                cache.AddIndex(cache.index + 3);

                cache.index += 4;
                return;
            }
            else if (lod == 1)
            {
                cache.AddVertex(new Vector3(m_CellX * m_CellWidth + 0.5f * m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + 0.5f * m_CellHeight + m_OffsetY));
                cache.index += 1;
            }
            else
            {
                for (int i = 1; i < ywidth; i++)
                {
                    for (int j = 1; j < xwidth; j++)
                    {
                        float x = ((float)j) / xwidth * m_CellWidth + m_CellX * m_CellWidth + m_OffsetX;
                        float z = ((float)i) / ywidth * m_CellHeight + m_CellY * m_CellHeight + m_OffsetY;
                        Vector3 pos = new Vector3(x, 0, z);
                        cache.AddVertex(pos);
                        if (j != xwidth - 1 && i != ywidth - 1)
                        {
                            cache.AddIndex(cache.index + (i - 1) * (xwidth - 1) + j - 1);
                            cache.AddIndex(cache.index + (i) * (xwidth - 1) + j - 1);
                            cache.AddIndex(cache.index + (i - 1) * (xwidth - 1) + j);

                            cache.AddIndex(cache.index + (i - 1) * (xwidth - 1) + j);
                            cache.AddIndex(cache.index + (i) * (xwidth - 1) + j - 1);
                            cache.AddIndex(cache.index + (i) * (xwidth - 1) + j);
                        }
                    }
                }
                cache.index += (ywidth - 1) * (xwidth - 1);
            }

            cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_OffsetY));
            cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_OffsetY));
            cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_CellHeight + m_OffsetY));
            cache.AddVertex(new Vector3(m_CellX * m_CellWidth + m_CellWidth + m_OffsetX, 0, m_CellY * m_CellHeight + m_CellHeight + m_OffsetY));

            int lbindex = cache.index;
            int rbindex = cache.index + 1;
            int luindex = cache.index + 2;
            int ruindex = cache.index + 3;

            cache.index += 4;

            UpdateMeshHorizontalEdge(cache, m_CellY * m_CellHeight + m_OffsetY, xwidth, downLod, lbindex, rbindex, firstIndex, true);
            UpdateMeshHorizontalEdge(cache, m_CellY * m_CellHeight + m_CellHeight + m_OffsetY, xwidth, upLod, luindex, ruindex, firstIndex + (xwidth - 1) * (ywidth - 2), false);
            UpdateMeshVerticalEdge(cache, m_CellX * m_CellWidth + m_OffsetX, xwidth, ywidth, leftLod, lbindex, luindex, firstIndex, true);
            UpdateMeshVerticalEdge(cache, m_CellX * m_CellWidth + m_CellWidth + m_OffsetX, xwidth, ywidth, rightLod, rbindex, ruindex, firstIndex + xwidth - 2, false);
        }

        private void UpdateMeshHorizontalEdge(UnlitWaterMeshVertexCache cache, float z, int edgeWidth, int neighborLod,
            int leftIndex, int rightIndex, int firstIndex, bool clockWise)
        {
            neighborLod = neighborLod < 0 ? lod : neighborLod;
            int deltaLod = Mathf.Max(0, lod - neighborLod);
            int step = (int)Mathf.Pow(2, deltaLod);
            int sp = deltaLod * (deltaLod - 1);
            int offset = deltaLod == 0 ? 0 : (int)Mathf.Pow(2, deltaLod - 1) - 1;
            for (int i = 0; i <= edgeWidth; i += step)
            {
                int ind = i / step;
                if (i != 0 && i != edgeWidth)
                {
                    float x = ((float)i) / edgeWidth * m_CellWidth + m_CellX * m_CellWidth + m_OffsetX;
                    cache.AddVertex(new Vector3(x, 0, z));
                }
                if (i != edgeWidth)
                {
                    if (i == 0)
                        cache.AddIndex(leftIndex);
                    else
                        cache.AddIndex(cache.index + ind - 1);
                    if (clockWise)
                    {
                        if (i == edgeWidth - 1)
                            cache.AddIndex(firstIndex + edgeWidth - 2);
                        else
                            cache.AddIndex(firstIndex + i + offset);
                        if (i == edgeWidth - step)
                            cache.AddIndex(rightIndex);
                        else
                            cache.AddIndex(cache.index + ind + 1 - 1);
                    }
                    else
                    {
                        if (i == edgeWidth - step)
                            cache.AddIndex(rightIndex);
                        else
                            cache.AddIndex(cache.index + ind + 1 - 1);
                        if (i == edgeWidth - 1)
                            cache.AddIndex(firstIndex + edgeWidth - 2);
                        else
                            cache.AddIndex(firstIndex + i + offset);
                    }
                }
                if (i > 0 && i <= edgeWidth - step)
                {
                    if (deltaLod != 0 || i != edgeWidth - 1)
                    {
                        cache.AddIndex(cache.index + ind - 1);
                        if (clockWise)
                        {
                            cache.AddIndex(firstIndex + i - 1);
                            cache.AddIndex(firstIndex + i);
                        }
                        else
                        {
                            cache.AddIndex(firstIndex + i);
                            cache.AddIndex(firstIndex + i - 1);
                        }
                    }
                }
                if (deltaLod != 0)
                {
                    if (i >= 0 && i < edgeWidth - step)
                    {
                        if (clockWise)
                        {
                            cache.AddIndex(firstIndex + i + sp);
                            cache.AddIndex(firstIndex + i + sp + 1);
                        }
                        else
                        {
                            cache.AddIndex(firstIndex + i + sp + 1);
                            cache.AddIndex(firstIndex + i + sp);
                        }
                        cache.AddIndex(cache.index + ind + 1 - 1);
                    }

                    if (i >= 0 && i <= edgeWidth - step)
                    {
                        int bindex = i == 0 ? leftIndex : (cache.index + ind - 1);
                        int eindex = i == edgeWidth - step ? rightIndex : (cache.index + ind);
                        for (int j = 0; j < step - 2; j++)
                        {
                            if (j < offset)
                                cache.AddIndex(bindex);
                            else
                                cache.AddIndex(eindex);
                            if (clockWise)
                            {
                                cache.AddIndex(firstIndex + i + j);
                                cache.AddIndex(firstIndex + i + j + 1);
                            }
                            else
                            {
                                cache.AddIndex(firstIndex + i + j + 1);
                                cache.AddIndex(firstIndex + i + j);
                            }

                        }

                    }
                }
            }
            cache.index += deltaLod == 0 ? (edgeWidth - 1) : (edgeWidth - 2) / step;
        }

        private void UpdateMeshVerticalEdge(UnlitWaterMeshVertexCache cache, float x, int xwidth, int ywidth, int neighborLod,
            int bottomIndex, int upIndex, int firstIndex, bool clockWise)
        {
            neighborLod = neighborLod < 0 ? lod : neighborLod;
            int deltaLod = Mathf.Max(0, lod - neighborLod);
            int step = (int)Mathf.Pow(2, deltaLod);
            int sp = deltaLod * (deltaLod - 1);
            int offset = deltaLod == 0 ? 0 : (int)Mathf.Pow(2, deltaLod - 1) - 1;
            for (int i = 0; i <= ywidth; i += step)
            {
                int ind = i / step;
                if (i != 0 && i != ywidth)
                {
                    float z = ((float)i) / ywidth * m_CellHeight + m_CellY * m_CellHeight + m_OffsetY;
                    cache.AddVertex(new Vector3(x, 0, z));
                }
                if (i != ywidth)
                {
                    if (i == 0)
                        cache.AddIndex(bottomIndex);
                    else
                        cache.AddIndex(cache.index + ind - 1);
                    if (clockWise)
                    {
                        if (i == ywidth - step)
                            cache.AddIndex(upIndex);
                        else
                            cache.AddIndex(cache.index + ind + 1 - 1);
                        if (i == ywidth - 1)
                            cache.AddIndex(firstIndex + (ywidth - 2) * (xwidth - 1));
                        else
                            cache.AddIndex(firstIndex + (i + offset) * (xwidth - 1));
                    }
                    else
                    {
                        if (i == ywidth - 1)
                            cache.AddIndex(firstIndex + (ywidth - 2) * (xwidth - 1));
                        else
                            cache.AddIndex(firstIndex + (i + offset) * (xwidth - 1));

                        if (i == ywidth - step)
                            cache.AddIndex(upIndex);
                        else
                            cache.AddIndex(cache.index + ind + 1 - 1);
                    }
                }
                if (i > 0 && i <= ywidth - step)
                {
                    if (deltaLod != 0 || i != ywidth - 1)
                    {
                        cache.AddIndex(cache.index + ind - 1);
                        if (clockWise)
                        {
                            cache.AddIndex(firstIndex + (i) * (xwidth - 1));
                            cache.AddIndex(firstIndex + (i - 1) * (xwidth - 1));
                        }
                        else
                        {
                            cache.AddIndex(firstIndex + (i - 1) * (xwidth - 1));
                            cache.AddIndex(firstIndex + (i) * (xwidth - 1));
                        }
                    }
                }
                if (deltaLod != 0)
                {
                    if (i >= 0 && i < ywidth - step)
                    {
                        if (clockWise)
                        {
                            cache.AddIndex(firstIndex + (i + sp + 1) * (xwidth - 1));
                            cache.AddIndex(firstIndex + (i + sp) * (xwidth - 1));
                        }
                        else
                        {
                            cache.AddIndex(firstIndex + (i + sp) * (xwidth - 1));
                            cache.AddIndex(firstIndex + (i + sp + 1) * (xwidth - 1));
                        }
                        cache.AddIndex(cache.index + ind + 1 - 1);
                    }

                    if (i >= 0 && i <= ywidth - step)
                    {
                        int bindex = i == 0 ? bottomIndex : (cache.index + ind - 1);
                        int eindex = i == ywidth - step ? upIndex : (cache.index + ind);
                        for (int j = 0; j < step - 2; j++)
                        {
                            if (j < offset)
                                cache.AddIndex(bindex);
                            else
                                cache.AddIndex(eindex);
                            if (clockWise)
                            {
                                cache.AddIndex(firstIndex + (i + j + 1) * (xwidth - 1));
                                cache.AddIndex(firstIndex + (i + j) * (xwidth - 1));
                            }
                            else
                            {
                                cache.AddIndex(firstIndex + (i + j) * (xwidth - 1));
                                cache.AddIndex(firstIndex + (i + j + 1) * (xwidth - 1));
                            }
                        }

                    }
                }
            }
            cache.index += deltaLod == 0 ? (ywidth - 1) : (ywidth - 2) / step;
        }
    }
}
