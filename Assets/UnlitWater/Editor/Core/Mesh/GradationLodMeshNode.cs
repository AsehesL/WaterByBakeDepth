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

        protected GradationLodMeshNode()
        {
        }

        public GradationLodMeshNode(GradationLodMeshNode leftTop, GradationLodMeshNode leftBottom,
            GradationLodMeshNode rightTop, GradationLodMeshNode rightBottom, float offsetX, float offsetY, int cellX, int cellY, float cellWidth, float cellHeight)
        {
            if (leftTop.lod == 0 && leftBottom.lod == 0 && rightTop.lod == 0 && rightBottom.lod == 0)
            {
                this.lod = 0;
            }
            else
            {
                this.lod = 1;
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
        }

        public virtual void UpdateMesh(MeshVertexData cache)
        {
            if (lod == 0)
            {
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
            m_Cell.UpdateMesh(cache);
        }
    }
}