using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    /// <summary>
    /// Lod网格
    /// 该类型网格根据传入的纹理，将自动在水岸与陆地的交界处生成最密集的网格，越远离海岸，网格越稀疏
    /// </summary>
    internal class LodMesh : IMeshGenerator
    {
        /// <summary>
        /// 不可见颜色，小于该值的颜色判断为不可见
        /// </summary>
        public const float kInVisibleColor = 0.01f;
        /// <summary>
        /// 边缘极差，极差大于该值判断为处于边缘
        /// </summary>
        public const float kEdgeRange = 0.4f;

        private LodMeshCell[,] m_Cells;

        private int m_XCells;
        private int m_ZCells;
        private float m_XWidth;
        private float m_ZWidth;
        private float m_OffsetX;
        private float m_OffsetZ;
        private int m_MaxLod;
        private int m_Samples;
        private float m_UVDir;

        private bool m_Support;

        public LodMesh(int xCells, int zCells, float xWidth, float zWidth, float offsetX, float offsetZ, int maxLod, float uvDir, int samples)
        {
            m_Cells = new LodMeshCell[xCells, zCells];
            m_XCells = xCells;
            m_ZCells = zCells;
            m_XWidth = xWidth;
            m_ZWidth = zWidth;
            m_OffsetX = offsetX;
            m_OffsetZ = offsetZ;
            m_MaxLod = maxLod;
            m_Samples = samples;
            m_UVDir = uvDir;

            if (xCells > 0 && zCells > 0 && zWidth > 0 && xWidth > 0 && maxLod >= 0 && samples >= 1)
                m_Support = true;
        }

        public Mesh GenerateMesh(Texture2D texture)
        {
            if (!m_Support)
                return null;
            //根据贴图尺寸和单元格数量，计算分配给单个单元格的像素宽高
            int w = texture.width / m_XCells;
            int h = texture.height / m_ZCells;

            //计算Lod
            for (int i = 0; i < m_XCells; i++)
            {
                for (int j = 0; j < m_ZCells; j++)
                {
                    m_Cells[i, j] = new LodMeshCell(m_OffsetX, m_OffsetZ, i, j, m_XWidth / m_XCells,
                        m_ZWidth / m_ZCells);
                    //为单元格分配指定区域的像素并计算极差和平均值
                    m_Cells[i, j].Calculate(texture, i * w, j * h, w, h);
                    if (m_Cells[i, j].average < kInVisibleColor)
                    {
                        m_Cells[i, j].lod = -1;//如果单元格像素颜色平均值小于0.01，则判定该单元格基本上位于非水域内，则lod设置为-1，将不参与水网格的构建
                        continue;
                    }
                    if (m_Cells[i, j].range > kEdgeRange)//如果极差超过0.4，则判定该单元格同时包含水域和陆地，即岸边区域，应该给予最大lod
                        m_Cells[i, j].lod = m_MaxLod;
                }
            }

            //根据上一步计算的结果，将最大lod单元格边上的格子设置lod递减
            for (int i = 0; i < m_XCells; i++)
            {
                for (int j = 0; j < m_ZCells; j++)
                {
                    LodMeshCell cell = m_Cells[i, j];
                    if (cell.lod == -1)
                        continue;
                    if (cell.lod != m_MaxLod)
                        continue;
                    for (int lx = m_MaxLod - 1, ly = 0; lx >= 0; lx--, ly++)
                    {
                        for (int lk = 0; lk <= ly; lk++)
                        {
                            if (lk == 0 && lx == 0)
                                continue;
                            int clod = m_MaxLod - lx - lk;
                            //从最大lod处往外递减lod
                            SetNeighborLOD(i - lx, j - lk, m_XCells, m_ZCells, clod, m_Cells);
                            SetNeighborLOD(i + lx, j - lk, m_XCells, m_ZCells, clod, m_Cells);
                            SetNeighborLOD(i - lx, j + lk, m_XCells, m_ZCells, clod, m_Cells);
                            SetNeighborLOD(i + lx, j + lk, m_XCells, m_ZCells, clod, m_Cells);
                        }
                    }
                }
            }

            //根据Lod生成Mesh

            float p = Mathf.Pow(2, m_MaxLod);
            float dtx = m_XWidth / m_XCells / p;
            float dty = m_ZWidth / m_ZCells / p;

            MeshVertexData cache = new MeshVertexData(m_XCells * (int)p + 1, m_ZCells * (int)p + 1, dtx, dty, m_OffsetX, m_OffsetZ);
            for (int i = 0; i < m_XCells; i++)
            {
                for (int j = 0; j < m_ZCells; j++)
                {
                    LodMeshCell cell = m_Cells[i, j];
                    if (cell.lod == -1)
                        continue;
                    int leftLod = i == 0 ? -1 : m_Cells[i - 1, j].lod;
                    int rightLod = i == m_Cells.GetLength(0) - 1 ? -1 : m_Cells[i + 1, j].lod;
                    int downLod = j == 0 ? -1 : m_Cells[i, j - 1].lod;
                    int upLod = j == m_Cells.GetLength(1) - 1 ? -1 : m_Cells[i, j + 1].lod;
                    cell.UpdateMesh(cache, leftLod, rightLod, upLod, downLod);
                }
            }
            //生成网格
            Mesh mesh = cache.Apply(texture, m_UVDir, m_Samples);
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
        private void SetNeighborLOD(int i, int j, int cellx, int celly, int lod, LodMeshCell[,] cells)
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
    }

}