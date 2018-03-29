using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    /// <summary>
    /// UnlitWater LOD网格单元格
    /// </summary>
    internal class LodMeshCell
    {
        /// <summary>
        /// lod
        /// </summary>
        public int lod { get; set; }

        private float m_OffsetX;
        private float m_OffsetY;
        private int m_CellX;
        private int m_CellY;

        private float m_CellWidth;
        private float m_CellHeight;

        private int m_LeftLod;
        private int m_RightLod;
        private int m_UpLod;
        private int m_DownLod;

        /// <summary>
        /// 构造单元格
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="cellWidth"></param>
        /// <param name="cellHeight"></param>
        public LodMeshCell(float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
        {
            this.m_OffsetX = offsetX;
            this.m_OffsetY = offsetY;
            this.m_CellX = cellX;
            this.m_CellY = cellY;
            this.m_CellWidth = cellWidth;
            this.m_CellHeight = cellHeight;
        }

        public void SetNeighborLOD(int left, int right, int up, int down)
        {
            m_LeftLod = left;
            m_RightLod = right;
            m_UpLod = up;
            m_DownLod = down;
        }

        public void Calculate(Texture2D tex, int x, int y, int width, int height, int maxlod)
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

            float average = sum / (width * height);

            if (average < MeshVertexData.kInVisibleColor)
            {
                lod = -1;//如果单元格像素颜色平均值小于0.01，则判定该单元格基本上位于非水域内，则lod设置为-1，将不参与水网格的构建
                return;
            }

            float range = max - min;

            if (range > MeshVertexData.kEdgeRange)
            {
                lod = maxlod;//如果极差超过0.4，则判定该单元格同时包含水域和陆地，即岸边区域，应该给予最大lod
            }

        }

        public void UpdateMesh(MeshVertexData cache)
        {
            int xw = (int)Mathf.Pow(2, lod);
            int yw = xw;
            UpdateMesh_InternalLod(cache, xw, yw, m_LeftLod, m_RightLod, m_UpLod, m_DownLod);
        }

        private void UpdateMesh_InternalLod(MeshVertexData cache, int xwidth, int ywidth, int leftLod,
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

        private void UpdateMeshHorizontalEdge(MeshVertexData cache, float z, int edgeWidth, int neighborLod,
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

        private void UpdateMeshVerticalEdge(MeshVertexData cache, float x, int xwidth, int ywidth, int neighborLod,
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