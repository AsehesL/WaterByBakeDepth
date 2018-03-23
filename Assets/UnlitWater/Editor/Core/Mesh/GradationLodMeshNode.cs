using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    internal class GradationLodMeshNode
    {
        public int lod;

        private GradationLodMeshNode m_LeftTopNode;
        private GradationLodMeshNode m_LeftBottomNode;
        private GradationLodMeshNode m_RightTopNode;
        private GradationLodMeshNode m_RightBottomNode;

        private int m_LeftLod;
        private int m_RightLod;
        private int m_UpLod;
        private int m_DownLod;

        private float m_OffsetX;
        private float m_OffsetY;
        private int m_CellX;
        private int m_CellY;

        private float m_CellWidth;
        private float m_CellHeight;

        private bool m_Combine;

        protected GradationLodMeshNode()
        {
        }

        public GradationLodMeshNode(GradationLodMeshNode leftTop, GradationLodMeshNode leftBottom,
            GradationLodMeshNode rightTop, GradationLodMeshNode rightBottom, float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
        {
            if (leftTop.lod == 0 && leftBottom.lod == 0 && rightTop.lod == 0 && rightBottom.lod == 0)
            {
                this.lod = 0;
                this.m_Combine = true;
            }
            else
            {
                this.lod = 1;
                this.m_Combine = false;
            }
            m_LeftBottomNode = leftBottom;
            m_LeftTopNode = leftTop;
            m_RightBottomNode = rightBottom;
            m_RightTopNode = rightTop;

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
            if (left != 0 || right != 0 || up != 0 || down != 0)
                lod = 1;
        }

        public virtual void UpdateMesh(MeshVertexData cache)
        {
            if (m_Combine)
            {
                if (m_LeftLod <= 0 && m_RightLod <= 0 && m_UpLod <= 0 && m_DownLod <= 0)
                {
                    cache.AddVertex(new Vector3(m_OffsetX + m_CellX*m_CellWidth, 0, m_OffsetY + m_CellY*m_CellHeight));
                    cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight+m_CellHeight));
                    cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth+m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight+m_CellHeight));
                    cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth+m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));

                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index+1);
                    cache.AddIndex(cache.index+2);
                    cache.AddIndex(cache.index);
                    cache.AddIndex(cache.index+2);
                    cache.AddIndex(cache.index+3);
                    cache.index += 4;
                }
                else
                {
                    if (m_LeftLod > 0)
                    {
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth*0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight*0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight+m_CellHeight*0.5f));

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
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_UpLod > 0)
                    {
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));

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
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_RightLod > 0)
                    {
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));

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
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }

                    if (m_DownLod > 0)
                    {
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight));

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
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth * 0.5f, 0, m_OffsetY + m_CellY * m_CellHeight + m_CellHeight * 0.5f));
                        cache.AddVertex(new Vector3(m_OffsetX + m_CellX * m_CellWidth + m_CellWidth, 0, m_OffsetY + m_CellY * m_CellHeight));

                        cache.AddIndex(cache.index);
                        cache.AddIndex(cache.index + 1);
                        cache.AddIndex(cache.index + 2);
                        cache.index += 3;
                    }
                }
            }
            else
            {
                m_LeftTopNode.UpdateMesh(cache);
                m_LeftBottomNode.UpdateMesh(cache);
                m_RightTopNode.UpdateMesh(cache);
                m_RightBottomNode.UpdateMesh(cache);
            }
        }

    }

    internal class GradationLodMeshLeaf : GradationLodMeshNode
    {

        private LodMeshCell m_Cell;

        public GradationLodMeshLeaf(LodMeshCell cell)
        {
            m_Cell = cell;
            lod = cell.lod;
        }

        public override void UpdateMesh(MeshVertexData cache)
        {
            if (m_Cell.lod >= 0)
                m_Cell.UpdateMesh(cache);
        }
    }
}