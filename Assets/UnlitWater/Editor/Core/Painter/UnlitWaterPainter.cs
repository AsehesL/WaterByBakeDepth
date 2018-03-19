using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    /// <summary>
    /// 网格顶点绘制器
    /// </summary>
    [System.Serializable]
    internal class UnlitWaterPainter
    {
        /// <summary>
        /// 笔刷模式
        /// </summary>
        public enum BrushType
        {
            点, //对单个顶点着色
            三角 //对三角面着色
        }

        /// <summary>
        /// 绘制通道
        /// </summary>
        public enum Channel
        {
            None,
            R,
            G,
            B,
            A,
        }

        private Material brushMaterial
        {
            get
            {
                if (m_BrushMaterial == null)
                {
                    Shader m_TerrainBrushShader = Shader.Find("Hidden/MeshPainter/Editor/TerrainBrush");

                    m_BrushMaterial = new Material(m_TerrainBrushShader);
                    m_BrushMaterial.SetColor("_Color", new Color(0, 0.5f, 1, 0.5f));
                    m_BrushMaterial.SetVector("_VertexMask", new Vector4(1, 0, 0, 0));
                }
                return m_BrushMaterial;
            }
        }

        private static System.Reflection.MethodInfo intersectRayMesh
        {
            get
            {
                if (m_IntersectRayMesh == null)
                {
                    var tp = typeof (HandleUtility);
                    m_IntersectRayMesh = tp.GetMethod("IntersectRayMesh",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                }
                return m_IntersectRayMesh;
            }
        }

        private static System.Reflection.MethodInfo m_IntersectRayMesh;

        [SerializeField] private Material m_BrushMaterial;

        public bool previewVertexColor;

        public float paintVertexAlpha;

        public BrushType paintVertexType;
        public Channel paintVertexChannel;

        private bool m_IsMouseDragging;

        public void DrawSceneGUI(GameObject target)
        {
            if (paintVertexChannel != UnlitWaterPainter.Channel.None)
            {
                if (previewVertexColor)
                {
                    ShowVertexColorPreview(target);
                }
                RaycastHit hit;
                if (RayCastInSceneView(target, out hit))
                {
                    RenderBrush(target, hit.point, hit.triangleIndex);
                    DrawVertexColor(target, hit.point, hit.triangleIndex);
                }
            }
        }

        public void ResetChannel()
        {
            if (paintVertexChannel == UnlitWaterPainter.Channel.None)
                return;
            Vector4 vertexMask = new Vector4(paintVertexChannel == UnlitWaterPainter.Channel.R ? 1 : 0,
                paintVertexChannel == UnlitWaterPainter.Channel.G ? 1 : 0,
                paintVertexChannel == UnlitWaterPainter.Channel.B ? 1 : 0,
                paintVertexChannel == UnlitWaterPainter.Channel.A ? 1 : 0);
            brushMaterial.SetVector("_VertexMask", vertexMask);
        }

        /// <summary>
        /// 显示顶点色预览
        /// </summary>
        /// <param name="target"></param>
        private void ShowVertexColorPreview(GameObject target)
        {
            if (!target)
                return;
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (!mf)
                return;
            if (!mf.sharedMesh)
                return;
            brushMaterial.SetPass(2);
            Graphics.DrawMeshNow(mf.sharedMesh, target.transform.localToWorldMatrix);
        }

        /// <summary>
        /// 渲染笔刷
        /// </summary>
        /// <param name="target"></param>
        /// <param name="point"></param>
        /// <param name="index"></param>
        private void RenderBrush(GameObject target, Vector3 point, int index)
        {
            if (!target)
                return;
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh)
            {
                if (paintVertexType == BrushType.点)
                    ShowBrush(mf, point, index);
                else
                    ShowBrush(mf, index);
            }
        }

        private void ShowBrush(MeshFilter meshFilter, int index)
        {
            if (!meshFilter || !meshFilter.sharedMesh)
                return;
            int index0 = meshFilter.sharedMesh.triangles[index*3];
            int index1 = meshFilter.sharedMesh.triangles[index*3 + 1];
            int index2 = meshFilter.sharedMesh.triangles[index*3 + 2];
            brushMaterial.SetVector("_VIndex", new Vector4(index0, index1, index2, 0));
            brushMaterial.SetPass(0);
            Graphics.DrawMeshNow(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix);
        }

        private void ShowBrush(MeshFilter meshFilter, Vector3 point, int index)
        {
            if (!meshFilter || !meshFilter.sharedMesh)
                return;
            float dis = Mathf.Infinity;
            int index0 = meshFilter.sharedMesh.triangles[index*3];
            int index1 = meshFilter.sharedMesh.triangles[index*3 + 1];
            int index2 = meshFilter.sharedMesh.triangles[index*3 + 2];
            index = index0;
            point = meshFilter.transform.worldToLocalMatrix.MultiplyPoint(point);
            Vector3 p = meshFilter.sharedMesh.vertices[index0];
            float tdis = Vector3.Distance(point, p);
            if (tdis < dis)
            {
                dis = tdis;
                index = index0;
            }
            p = meshFilter.sharedMesh.vertices[index1];
            tdis = Vector3.Distance(point, p);
            if (tdis < dis)
            {
                dis = tdis;
                index = index1;
            }
            p = meshFilter.sharedMesh.vertices[index2];
            tdis = Vector3.Distance(point, p);
            if (tdis < dis)
            {
                index = index2;
            }
            brushMaterial.SetVector("_VIndex", new Vector4(index, 0, 0, 0));
            brushMaterial.SetPass(1);
            Graphics.DrawMeshNow(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix);
        }

        private void DrawVertexColor(GameObject target, Vector3 point, int index)
        {
            if (!target)
                return;
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    m_IsMouseDragging = true;
                    if (mf.sharedMesh.colors.Length <= 0)
                    {

                        Color[] cl = new Color[mf.sharedMesh.vertexCount];
                        for (int i = 0; i < mf.sharedMesh.colors.Length; i++)
                        {
                            cl[i] = Color.white;
                        }
                        mf.sharedMesh.colors = cl;
                    }
                }
                if (Event.current.type == EventType.MouseUp)
                {
                    m_IsMouseDragging = false;
                }
                if (Event.current.type == EventType.MouseDrag && m_IsMouseDragging)
                {
                    if (paintVertexType == BrushType.点)
                        PaintVertexColorAtPoint(mf, index, point);
                    else
                        PaintVertexColorAtTriangle(mf, index);
                }
            }
        }

        private void PaintVertexColorAtPoint(MeshFilter meshFilter, int index, Vector3 position)
        {
            float dis = Mathf.Infinity;
            int hitid = 0;
            position = meshFilter.transform.worldToLocalMatrix.MultiplyPoint(position);

            for (int i = 0; i < 3; i++)
            {
                int id = meshFilter.sharedMesh.triangles[index*3 + i];
                float distop = Vector3.Distance(position, meshFilter.sharedMesh.vertices[id]);
                if (distop < dis)
                {
                    dis = distop;
                    hitid = index*3 + i;
                }
            }
            Color[] ncl = meshFilter.sharedMesh.colors;
            ncl[meshFilter.sharedMesh.triangles[hitid]] = SetVertexColor(paintVertexChannel, paintVertexAlpha,
                ncl[meshFilter.sharedMesh.triangles[hitid]]);
            meshFilter.sharedMesh.colors = ncl;
        }

        private void PaintVertexColorAtTriangle(MeshFilter meshFilter, int index)
        {
            Color[] ncl = meshFilter.sharedMesh.colors;
            ncl[meshFilter.sharedMesh.triangles[index*3]] = SetVertexColor(paintVertexChannel, paintVertexAlpha,
                ncl[meshFilter.sharedMesh.triangles[index*3]]);
            ncl[meshFilter.sharedMesh.triangles[index*3 + 1]] = SetVertexColor(paintVertexChannel, paintVertexAlpha,
                ncl[meshFilter.sharedMesh.triangles[index*3 + 1]]);
            ncl[meshFilter.sharedMesh.triangles[index*3 + 2]] = SetVertexColor(paintVertexChannel, paintVertexAlpha,
                ncl[meshFilter.sharedMesh.triangles[index*3 + 2]]);
            meshFilter.sharedMesh.colors = ncl;
        }

        private static Color SetVertexColor(Channel channel, float alpha, Color color)
        {
            switch (channel)
            {
                case Channel.R:
                    color = new Color(alpha, color.g, color.b, color.a);
                    break;
                case Channel.G:
                    color = new Color(color.r, alpha, color.b, color.a);
                    break;
                case Channel.B:
                    color = new Color(color.r, color.g, alpha, color.a);
                    break;
                case Channel.A:
                    color = new Color(color.r, color.g, color.b, alpha);
                    break;
            }
            return color;
        }

        private static bool RayCastInSceneView(GameObject target, out RaycastHit hit)
        {
            hit = default(RaycastHit);
            if (!target)
                return false;
            MeshFilter meshfilter = target.GetComponent<MeshFilter>();
            if (!meshfilter || !meshfilter.sharedMesh)
                return false;
            if (UnityEditor.Tools.viewTool != ViewTool.Pan)
                return false;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            SceneView.RepaintAll();

            if (RaycastMesh(ray, meshfilter, out hit))
                return true;
            return false;
        }

        private static bool RaycastMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
        {
            var parameters = new object[] {ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, null};
            bool result = (bool) intersectRayMesh.Invoke(null, parameters);
            hit = (RaycastHit) parameters[3];
            return result;
        }
    }
}