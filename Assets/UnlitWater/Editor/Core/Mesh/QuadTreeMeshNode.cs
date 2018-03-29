using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    internal class QuadTreeMeshNode
    {
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

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 4);
                    cache.AddIndex(cache.index + 5);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 5);

                    cache.index += 6;
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_RightTopChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 4);
                    cache.AddIndex(cache.index + 5);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 5);

                    cache.index += 6;
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    m_RightBottomChild.isEdge)
                {
                    m_RightBottomChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 5);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 6;
                }
                else if (!m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftBottomChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.AddIndex(cache.index + 5);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 6;
                }
                else if (m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftTopChild.UpdateMesh(cache);
                    m_LeftBottomChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 4);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 5;
                }
                else if (m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    !m_RightBottomChild.isEdge)
                {
                    m_LeftTopChild.UpdateMesh(cache);
                    m_RightTopChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 4);

                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 5;
                }
                else if (!m_LeftTopChild.isEdge && !m_LeftBottomChild.isEdge && m_RightTopChild.isEdge &&
                    m_RightBottomChild.isEdge)
                {
                    m_RightTopChild.UpdateMesh(cache);
                    m_RightBottomChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 3);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 3);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 5;
                }
                else if (!m_LeftTopChild.isEdge && m_LeftBottomChild.isEdge && !m_RightTopChild.isEdge &&
                         m_RightBottomChild.isEdge)
                {
                    m_LeftBottomChild.UpdateMesh(cache);
                    m_RightBottomChild.UpdateMesh(cache);

                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                    cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 4);

                    cache.AddIndex(cache.index + 4);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 3);

                    cache.AddIndex(cache.index + 1);
                    cache.AddIndex(cache.index + 2);
                    cache.AddIndex(cache.index + 4);

                    cache.index += 5;
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
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 2);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 4;
                        }
                        else
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 3;
                        }

                        if (m_IsUpEdge)
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 2);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 4;
                        }
                        else
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 3;
                        }

                        if (m_IsRightEdge)
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.AddIndex(cache.index + 3);
                            cache.index += 4;
                        }
                        else
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 3;
                        }

                        if (m_IsDownEdge)
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 3);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 4;
                        }
                        else
                        {
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                            cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                            cache.AddIndex(cache.index);
                            cache.AddIndex(cache.index + 1);
                            cache.AddIndex(cache.index + 2);
                            cache.index += 3;
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
                else
                {
                    if (m_IsLeftEdge)
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 2);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 4;
                    }
                    else
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_IsUpEdge)
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 2);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 4;
                    }
                    else
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_IsRightEdge)
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.AddIndex(cache.index + 3);
                        cache.index += 4;
                    }
                    else
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight + cellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_IsDownEdge)
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 3);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 4;
                    }
                    else
                    {
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth, 0, offsetY + cellY * cellHeight));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth * 0.5f, 0, offsetY + cellY * cellHeight + cellHeight * 0.5f));
                        cache.AddVertex(new Vector3(offsetX + cellX * cellWidth + cellWidth, 0, offsetY + cellY * cellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }
                }
            }
        }
    }

    internal class QuadTreeMeshLeaf : QuadTreeMeshNode
    {
        /// <summary>
        /// 极差
        /// </summary>
        public float range { get; private set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public float average { get; private set; }

        public QuadTreeMeshLeaf(float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
            : base(offsetX, offsetY, cellX, cellY, cellWidth, cellHeight)
        {
            
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

        }

        public override void UpdateMesh(MeshVertexData cache)
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
    }
}