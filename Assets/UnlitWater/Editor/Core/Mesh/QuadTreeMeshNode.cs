using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 四叉树Mesh节点
    /// </summary>
    internal class QuadTreeMeshNode
    {
        /// <summary>
        /// 是否为边缘交界处的节点
        /// </summary>
        public bool isEdge;

        private QuadTreeMeshNode m_LeftTopChild;
        private QuadTreeMeshNode m_LeftBottomChild;
        private QuadTreeMeshNode m_RightTopChild;
        private QuadTreeMeshNode m_RightBottomChild;

        private bool m_IsLeftEdge;
        private bool m_IsRightEdge;
        private bool m_IsUpEdge;
        private bool m_IsDownEdge;

        protected float offsetX;
        protected float offsetY;
        protected int cellX;
        protected int cellY;

        protected float cellWidth;
        protected float cellHeight;

        protected QuadTreeMeshNode(float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.cellX = cellX;
            this.cellY = cellY;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
        }

        public QuadTreeMeshNode(QuadTreeMeshNode leftTop, QuadTreeMeshNode leftBottom,
            QuadTreeMeshNode rightTop, QuadTreeMeshNode rightBottom, float offsetX, float offsetY, int cellX, int cellY,
            float cellWidth, float cellHeight) : this(offsetX, offsetY, cellX, cellY, cellWidth, cellHeight)
        {
            if (leftTop.isEdge || leftBottom.isEdge || rightTop.isEdge || rightBottom.isEdge)
            {
                this.isEdge = true;
            }
            else
            {
                this.isEdge = false;
            }
            m_LeftBottomChild = leftBottom;
            m_LeftTopChild = leftTop;
            m_RightBottomChild = rightBottom;
            m_RightTopChild = rightTop;
        }

        public void SetNeighbor(QuadTreeMeshNode left, QuadTreeMeshNode right, QuadTreeMeshNode up, QuadTreeMeshNode down)
        {
            m_IsLeftEdge = left != null && left.isEdge && (left.m_RightBottomChild.isEdge || left.m_RightTopChild.isEdge);
            m_IsRightEdge = right != null && right.isEdge && (right.m_LeftBottomChild.isEdge || right.m_LeftTopChild.isEdge);
            m_IsUpEdge = up != null && up.isEdge && (up.m_RightBottomChild.isEdge || up.m_LeftBottomChild.isEdge);
            m_IsDownEdge = down != null && down.isEdge && (down.m_LeftTopChild.isEdge || down.m_RightTopChild.isEdge);
            if (m_IsLeftEdge || m_IsRightEdge || m_IsUpEdge || m_IsDownEdge)
                isEdge = true;
            //if (left != 0 || right != 0 || up != 0 || down != 0)
            //    combinelod += 1;
        }

        public virtual void UpdateMesh(MeshVertexData cache)
        {
            if (isEdge)
            {
                if (m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftTopChild.UpdateMesh(cache);

                    if (m_IsRightEdge && !m_IsDownEdge)
                    {
                        AddTopRightTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else if (!m_IsRightEdge && m_IsDownEdge)
                    {
                        AddTopRightTriangle(cache);
                        AddRightTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else if (m_IsRightEdge && m_IsDownEdge)
                    {
                        AddTopRightTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else
                    {
                        AddBottomTriangle(cache);
                        AddRightTriangle(cache);
                        AddLeftBottomTriangle(cache);
                        AddTopRightTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_RightTopChild.UpdateMesh(cache);

                    if (m_IsLeftEdge && !m_IsDownEdge)
                    {
                        AddTopLeftTriangle(cache);
                        AddLeftTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                        AddBottomTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                    else if (!m_IsLeftEdge && m_IsDownEdge)
                    {
                        AddTopLeftTriangle(cache);
                        AddLeftTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                    else if (m_IsLeftEdge && m_IsDownEdge)
                    {
                        AddTopLeftTriangle(cache);
                        AddLeftTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                    else
                    {
                        AddLeftTriangle(cache);
                        AddBottomTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    m_RightBottomChild.isEdge)
                {
                    m_RightBottomChild.UpdateMesh(cache);

                    if (m_IsUpEdge && !m_IsLeftEdge)
                    {
                        AddLeftTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddRightTopTriangle(cache);
                    }
                    else if (!m_IsUpEdge && m_IsLeftEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddRightTopTriangle(cache);
                    }
                    else if (m_IsUpEdge && m_IsLeftEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else
                    {

                        AddLeftTriangle(cache);
                        AddTopTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddRightTopTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftBottomChild.UpdateMesh(cache);

                    if (!m_IsUpEdge && m_IsRightEdge)
                    {
                        AddTopTriangle(cache);
                        AddLeftTopTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                    else if (m_IsUpEdge && !m_IsRightEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                        AddRightTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                    else if (m_IsUpEdge && m_IsRightEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                    else
                    {

                        AddTopTriangle(cache);
                        AddRightTriangle(cache);
                        AddLeftTopTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                }
                else if (m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftTopChild.UpdateMesh(cache);
                    m_LeftBottomChild.UpdateMesh(cache);

                    if (m_IsRightEdge)
                    {
                        AddTopRightTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                    else
                    {
                        AddTopRightTriangle(cache);
                        AddBottomRightTriangle(cache);
                        AddRightTriangle(cache);
                    }
                }
                else if (m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftTopChild.UpdateMesh(cache);
                    m_RightTopChild.UpdateMesh(cache);

                    if (m_IsDownEdge)
                    {
                        AddLeftBottomTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                    else
                    {
                        AddLeftBottomTriangle(cache);
                        AddRightBottomTriangle(cache);
                        AddBottomTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    m_RightBottomChild.isEdge)
                {
                    m_RightTopChild.UpdateMesh(cache);
                    m_RightBottomChild.UpdateMesh(cache);

                    if (m_IsLeftEdge)
                    {
                        AddTopLeftTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddLeftTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else
                    {
                        AddTopLeftTriangle(cache);
                        AddBottomLeftTriangle(cache);
                        AddLeftTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                         m_RightBottomChild.isEdge)
                {
                    m_LeftBottomChild.UpdateMesh(cache);
                    m_RightBottomChild.UpdateMesh(cache);

                    if (m_IsUpEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                    }
                    else
                    {
                        AddLeftTopTriangle(cache);
                        AddRightTopTriangle(cache);
                        AddTopTriangle(cache);
                    }
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                         !m_RightBottomChild.isEdge)
                {
                    if (!m_IsLeftEdge && !m_IsRightEdge && !m_IsUpEdge && !m_IsDownEdge)
                    {
                        m_LeftBottomChild.UpdateMesh(cache);
                        m_LeftTopChild.UpdateMesh(cache);
                        m_RightBottomChild.UpdateMesh(cache);
                        m_RightTopChild.UpdateMesh(cache);
                    }
                    else
                    {
                        if (m_IsLeftEdge)
                        {
                            AddLeftTopTriangle(cache);
                            AddLeftBottomTriangle(cache);
                        }
                        else
                        {
                            AddLeftTriangle(cache);
                        }

                        if (m_IsUpEdge)
                        {
                            AddTopLeftTriangle(cache);
                            AddTopRightTriangle(cache);
                        }
                        else
                        {
                            AddTopTriangle(cache);
                        }

                        if (m_IsRightEdge)
                        {
                            AddRightTopTriangle(cache);
                            AddRightBottomTriangle(cache);
                        }
                        else
                        {
                            AddRightTriangle(cache);
                        }

                        if (m_IsDownEdge)
                        {
                            AddBottomLeftTriangle(cache);
                            AddBottomRightTriangle(cache);
                        }
                        else
                        {
                            AddBottomTriangle(cache);
                        }
                    }
                }
                else
                {

                    m_LeftBottomChild.UpdateMesh(cache);
                    m_LeftTopChild.UpdateMesh(cache);
                    m_RightBottomChild.UpdateMesh(cache);
                    m_RightTopChild.UpdateMesh(cache);
                }
            }
            else
            {
                if (!m_IsLeftEdge && !m_IsRightEdge && !m_IsUpEdge && !m_IsDownEdge)
                {
                    AddQuad(cache);
                }
                else
                {
                    if (m_IsLeftEdge)
                    {
                        AddLeftTopTriangle(cache);
                        AddLeftBottomTriangle(cache);
                    }
                    else
                    {
                        AddLeftTriangle(cache);
                    }

                    if (m_IsUpEdge)
                    {
                        AddTopLeftTriangle(cache);
                        AddTopRightTriangle(cache);
                    }
                    else
                    {
                        AddTopTriangle(cache);
                    }

                    if (m_IsRightEdge)
                    {
                        AddRightTopTriangle(cache);
                        AddRightBottomTriangle(cache);
                    }
                    else
                    {
                        AddRightTriangle(cache);
                    }

                    if (m_IsDownEdge)
                    {
                        AddBottomLeftTriangle(cache);
                        AddBottomRightTriangle(cache);
                    }
                    else
                    {
                        AddBottomTriangle(cache);
                    }
                }
            }
        }

        protected void AddQuad(MeshVertexData cache)
        {
            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

            cache.AddIndex(cache.index);
            cache.AddIndex(cache.index + 1);
            cache.AddIndex(cache.index + 2);
            cache.AddIndex(cache.index);
            cache.AddIndex(cache.index + 2);
            cache.AddIndex(cache.index + 3);
            cache.index += 4;
        }

        protected void AddBottomLeftTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX*cellWidth, 0, offsetY + cellY*cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddBottomRightTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddRightBottomTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddRightTopTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddTopLeftTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddTopRightTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddLeftBottomTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddLeftTopTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddLeftTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddRightTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddBottomTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight);

            cache.AddTriangle(v0, v1, v2);
        }

        protected void AddTopTriangle(MeshVertexData cache)
        {
            Vector3 v0 = new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v1 = new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight);
            Vector3 v2 = new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f);

            cache.AddTriangle(v0, v1, v2);
        }
    }

    internal class QuadTreeMeshLeaf : QuadTreeMeshNode
    {

        public QuadTreeMeshLeaf(float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
            : base(offsetX, offsetY, cellX, cellY, cellWidth, cellHeight)
        {
            
        }

        public void Calculate(Texture2D tex, int x, int y, int width, int height)
        {
            float min = 1f;
            float max = 0f;

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    Color c = tex.GetPixel(i, j);
                    if (c.g < min)
                        min = c.g;
                    if (c.g > max)
                        max = c.g;
                }
            }

            float range = max - min;

            if (range > MeshVertexData.kEdgeRange) //如果极差超过0.4，则判定该单元格同时包含水域和陆地，标记为边
                isEdge = true;
            else
                isEdge = false;
        }

        public override void UpdateMesh(MeshVertexData cache)
        {
            this.AddQuad(cache);
        }
    }
}