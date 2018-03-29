using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.UnlitWater
{
    internal class MeshVertexData
    {
        /// <summary>
        /// 不可见颜色，小于该值的颜色判断为不可见
        /// </summary>
        public const float kInVisibleColor = 0.01f;
        /// <summary>
        /// 边缘极差，极差大于该值判断为处于边缘
        /// </summary>
        public const float kEdgeRange = 0.4f;

        /// <summary>
        /// 顶点数据
        /// </summary>
        private class VertexData
        {
            public Vector3 vertex;
            public Vector2 uv;
            public Color color;
            public int index;
            public bool visible;
            
            private int m_Width;

            public VertexData(Vector3 vertex, float beginx, float beginy, int width, int height, float cellSizeX, float cellSizeY)
            {
                this.vertex = vertex;
                this.index = -1;

                uv = new Vector2();
                uv.x = (vertex.x - beginx) / ((width - 1) * cellSizeX);
                uv.y = (vertex.z - beginy) / ((height - 1) * cellSizeY);
                
                m_Width = width;
            }

            public void Refresh(Texture2D tex, int samples)
            {
                if (tex == null)
                {
                    color = Color.white;
                    visible = true;
                    return;
                }
                Color col = GetColor(tex, uv);
                color = new Color(col.r, 1, 1, 1);

                visible = col.g >= kInVisibleColor;

                for (int i = 1; i < samples; i++)
                {
                    Vector2 spuv = new Vector2();

                    spuv.x = uv.x + ((float)i) / (m_Width - 1);
                    spuv.y = uv.y + ((float)i) / (m_Width - 1);
                    col = GetColor(tex, spuv);
                    if (col.g >= kInVisibleColor)
                        visible = true;

                    spuv.x = uv.x - ((float)i) / (m_Width - 1);
                    spuv.y = uv.y + ((float)i) / (m_Width - 1);
                    col = GetColor(tex, spuv);
                    if (col.g >= kInVisibleColor)
                        visible = true;

                    spuv.x = uv.x + ((float)i) / (m_Width - 1);
                    spuv.y = uv.y - ((float)i) / (m_Width - 1);
                    col = GetColor(tex, spuv);
                    if (col.g >= kInVisibleColor)
                        visible = true;

                    spuv.x = uv.x - ((float)i) / (m_Width - 1);
                    spuv.y = uv.y - ((float)i) / (m_Width - 1);
                    col = GetColor(tex, spuv);
                    if (col.g >= kInVisibleColor)
                        visible = true;
                }
            }

            private Color GetColor(Texture2D tex, Vector2 uv)
            {
                int x = (int)(uv.x * tex.width);
                int y = (int)(uv.y * tex.height);
                if (x < 0)
                    x = 0;
                if (x >= tex.width)
                    x = tex.width - 1;
                if (y < 0)
                    y = 0;
                if (y >= tex.height)
                    y = tex.height - 1;
                Color col = tex.GetPixel(x, y);
                return col;
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

        public MeshVertexData(int width, int height, float cellSizeX, float cellSizeY, float beginX, float beginY)
        {
            m_VertexList = new List<Vector3>();
            m_IndexList = new List<int>();
            m_Vertexs = new Dictionary<int, VertexData>();

            m_Width = width + 1;
            m_Height = height + 1;
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
        /// 添加三角形
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
        {
            this.AddVertex(vertex0);
            this.AddVertex(vertex1);
            this.AddVertex(vertex2);

            this.AddIndex(this.index);
            this.AddIndex(this.index + 1);
            this.AddIndex(this.index + 2);

            this.index += 3;
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
        public Mesh Apply(Texture2D texture, float uvDir, int samples)
        {
            List<Vector3> vlist = new List<Vector3>();
            List<Vector2> ulist = new List<Vector2>();
            List<Color> clist = new List<Color>();
            List<int> ilist = new List<int>();

            foreach (var dt in m_Vertexs)
            {

                dt.Value.Refresh(texture, samples);
                if (!dt.Value.visible)
                {
                    continue;
                }
                dt.Value.index = vlist.Count;

                vlist.Add(dt.Value.vertex);

                Vector2 uv = dt.Value.uv;
                float sinag = Mathf.Sin(Mathf.Deg2Rad*uvDir);
                float cosag = Mathf.Cos(Mathf.Deg2Rad*uvDir);
                uv = new Vector2(uv.x*cosag - uv.y*sinag, uv.x*sinag + uv.y*cosag);

                ulist.Add(uv);
                clist.Add(dt.Value.color);
            }

            for (int i = 0; i < m_IndexList.Count; i += 3)
            {
                Vector3 vertex0 = m_VertexList[m_IndexList[i]];
                Vector3 vertex1 = m_VertexList[m_IndexList[i + 1]];
                Vector3 vertex2 = m_VertexList[m_IndexList[i + 2]];
                int k0 = GetVertexKey(vertex0);
                int k1 = GetVertexKey(vertex1);
                int k2 = GetVertexKey(vertex2);
                if (!m_Vertexs.ContainsKey(k0) || !m_Vertexs.ContainsKey(k1) || !m_Vertexs.ContainsKey(k2))
                    continue;
                var dt0 = m_Vertexs[k0];
                var dt1 = m_Vertexs[k1];
                var dt2 = m_Vertexs[k2];
                if (dt0.index < 0 || dt1.index < 0 || dt2.index < 0)
                    continue;

                ilist.Add(dt0.index);
                ilist.Add(dt1.index);
                ilist.Add(dt2.index);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vlist);
            mesh.SetUVs(0, ulist);
            mesh.SetColors(clist);
            mesh.SetTriangles(ilist, 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        private int GetVertexKey(Vector3 vertex)
        {
            int x = Mathf.FloorToInt((vertex.x - m_BeginX - m_CellSizeX * 0.5f) / m_CellSizeX);
            int y = Mathf.FloorToInt((vertex.z - m_BeginY - m_CellSizeY * 0.5f) / m_CellSizeY);
            int k = y * m_Width + x;
            return k;
        }

    }
}