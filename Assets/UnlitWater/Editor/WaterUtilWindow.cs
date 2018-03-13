using UnityEngine;
using UnityEditor;
using System.Collections;

public class WaterUtilWindow : EditorWindow
{

    private enum PaintVertexType
    {
        点,//对单个顶点着色
        三角//对三角面着色
    }

    private enum PaintVertexChannel
    {
        R,
        G,
        B,
        A,
    }

    private bool m_IsPaintintVertex;

    private PaintVertexType m_PaintVertexType;
    private PaintVertexChannel m_PaintVertexChannel;

    private bool m_Lock;

    private MeshRenderer m_MeshRenderer;
    private Transform m_LightTransform;

    private bool m_PreviewVertexColor;

    private float m_PaintVertexAlpha;

    private bool m_IsMouseDragging;

    private Material m_BrushMaterial;

    [MenuItem("程序工具/图形/Shader/水材质编辑器")]
    static void InitWindow()
    {
        WaterUtilWindow window = WaterUtilWindow.GetWindow<WaterUtilWindow>();
        SceneView.onSceneGUIDelegate += window.OnSceneGUI;
    }

    void OnEnable()
    {
        if (m_BrushMaterial == null)
        {
            Shader m_TerrainBrushShader = Shader.Find("Hidden/MeshPainter/Editor/TerrainBrush");

            m_BrushMaterial = new Material(m_TerrainBrushShader);
            m_BrushMaterial.SetColor("_Color", new Color(0, 0.5f, 1, 0.5f));
            m_BrushMaterial.SetVector("_VertexMask", new Vector4(1, 0, 0, 0));
        }
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        m_LightTransform = null;
        m_MeshRenderer = null;
        if (m_BrushMaterial)
            DestroyImmediate(m_BrushMaterial);
    }

    void OnGUI()
    {
        GUI.BeginGroup(new Rect(10, 10, position.width - 20, position.height - 20));
        DrawRendererSetting();
        DrawMaterialSetting();
        DrawVertexSetting();
        GUI.EndGroup();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!m_IsPaintintVertex)
        {
            if (m_Lock)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            RaycastHit hit;
            if (m_PreviewVertexColor)
            {
                ShowVertexColorPreview();
            }
            if (RayCastInSceneView(out hit))
            {
                PaintVertexColor(hit.point, hit.triangleIndex);
            }
        }
    }

    void DrawRendererSetting()
    {
        GUI.enabled = !m_IsPaintintVertex;
        m_MeshRenderer =
            EditorGUILayout.ObjectField("水Renderer:", m_MeshRenderer, typeof(MeshRenderer), true) as MeshRenderer;
        GUI.enabled = true;
    }

    void DrawMaterialSetting()
    {
        EditorGUILayout.Space();
        GUI.enabled = m_MeshRenderer != null && !m_IsPaintintVertex;
        GUILayout.Label("深度烘焙", GUI.skin.FindStyle("BoldLabel"));
        if (GUILayout.Button("打开烘焙深度贴图窗口"))
        {
            MeshFilter mf = null;
            if (m_MeshRenderer)
                mf = m_MeshRenderer.GetComponent<MeshFilter>();
            if (mf)
            {
                m_Lock = true;
                DepthMapBaker.OpenDepthMapBaker(mf);
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("灯光", GUI.skin.FindStyle("BoldLabel"));
        Rect rect = EditorGUILayout.GetControlRect();
        m_LightTransform =
            (Transform)
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, 200, rect.height), m_LightTransform, typeof(Transform),
                    true);
        if (GUI.Button(new Rect(rect.x + 200, rect.y, rect.width - 200, rect.height), "烘焙光照方向"))
        {
            if (m_MeshRenderer)
                m_MeshRenderer.sharedMaterial.SetVector("_LightDir", m_LightTransform.forward);
        }
        GUI.enabled = true;
    }

    void DrawVertexSetting()
    {
        EditorGUILayout.Space();
        GUI.enabled = m_MeshRenderer != null;
        GUILayout.Label("顶点颜色", GUI.skin.FindStyle("BoldLabel"));
        if (GUILayout.Button(m_IsPaintintVertex ? "结束绘制" : "开始绘制"))
        {
            if (m_IsPaintintVertex)
            {
                m_IsPaintintVertex = false;
            }
            else
            {
                MeshFilter mf = m_MeshRenderer.GetComponent<MeshFilter>();
                if (mf)
                {
                    string meshPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
                    if (!meshPath.ToLower().EndsWith(".asset"))
                    {
                        if (EditorUtility.DisplayDialog("错误", "当前Mesh来源于模型文件，需要先创建副本才能编辑顶点色!是否立即创建Mesh拷贝？", "是", "否"))
                        {
                            if (CreateCopyMesh())
                                m_IsPaintintVertex = true;
                        }
                    }
                    else
                    {
                        m_IsPaintintVertex = true;
                    }
                }
            }
        }
        if (m_IsPaintintVertex)
        {
            GUILayout.Label("顶点绘制设置", GUI.skin.FindStyle("BoldLabel"));
            Rect m_Rect = EditorGUILayout.GetControlRect(GUILayout.Height(90));
            GUI.Box(m_Rect, "", GUI.skin.FindStyle("WindowBackground"));

            GUI.Label(new Rect(m_Rect.x + 5, m_Rect.y + 5, 100, 20), "选择绘制通道");

            EditorGUI.BeginChangeCheck();
            m_PaintVertexChannel = EditorGUI.Toggle(new Rect(m_Rect.width - 5 - 280, m_Rect.y + 5, 70, 20),
                m_PaintVertexChannel == PaintVertexChannel.R, GUI.skin.FindStyle("ButtonLeft"))
                ? PaintVertexChannel.R
                : m_PaintVertexChannel;
            GUI.Label(new Rect(m_Rect.width - 5 - 280, m_Rect.y + 5, 70, 20), "R:深度");

            m_PaintVertexChannel = EditorGUI.Toggle(new Rect(m_Rect.width - 5 - 210, m_Rect.y + 5, 70, 20),
                m_PaintVertexChannel == PaintVertexChannel.G, GUI.skin.FindStyle("ButtonMid"))
                ? PaintVertexChannel.G
                : m_PaintVertexChannel;
            GUI.Label(new Rect(m_Rect.width - 5 - 210, m_Rect.y + 5, 70, 20), "G:浪花透明");

            m_PaintVertexChannel = EditorGUI.Toggle(new Rect(m_Rect.width - 5 - 140, m_Rect.y + 5, 70, 20),
                m_PaintVertexChannel == PaintVertexChannel.B, GUI.skin.FindStyle("ButtonMid"))
                ? PaintVertexChannel.B
                : m_PaintVertexChannel;
            GUI.Label(new Rect(m_Rect.width - 5 - 140, m_Rect.y + 5, 70, 20), "B:海浪波纹透明");

            m_PaintVertexChannel = EditorGUI.Toggle(new Rect(m_Rect.width - 5 - 70, m_Rect.y + 5, 70, 20),
                m_PaintVertexChannel == PaintVertexChannel.A, GUI.skin.FindStyle("ButtonRight"))
                ? PaintVertexChannel.A
                : m_PaintVertexChannel;
            GUI.Label(new Rect(m_Rect.width - 5 - 70, m_Rect.y + 5, 70, 20), "A:整体透明");
            if (EditorGUI.EndChangeCheck())
            {
                Vector4 vertexMask = new Vector4(m_PaintVertexChannel == PaintVertexChannel.R ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.G ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.B ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.A ? 1 : 0);
                m_BrushMaterial.SetVector("_VertexMask", vertexMask);
            }

            m_PreviewVertexColor = EditorGUI.Toggle(new Rect(m_Rect.x + 5, m_Rect.y + 25, m_Rect.width - 10, 20),
                "预览顶点色",
                m_PreviewVertexColor);

            m_PaintVertexAlpha = EditorGUI.Slider(new Rect(m_Rect.x + 5, m_Rect.y + 45, m_Rect.width - 10, 20),
                "绘制透明度", m_PaintVertexAlpha, 0, 1);
            m_PaintVertexType =
                (PaintVertexType)
                    EditorGUI.EnumPopup(new Rect(m_Rect.x + 5, m_Rect.y + 65, m_Rect.width - 10, 20), "笔刷类型",
                        m_PaintVertexType);
        }
        GUI.enabled = true;
    }

    private bool RayCastInSceneView(out RaycastHit hit)
    {
        if (UnityEditor.Tools.viewTool != ViewTool.Pan)
        {
            hit = default(RaycastHit);
            return false;
        }
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        SceneView.RepaintAll();
        if (Physics.Raycast(ray, out hit))
        {
            if (m_MeshRenderer != null)
            {
                MeshCollider mc = m_MeshRenderer.GetComponent<MeshCollider>();
                if (mc && mc == hit.collider)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PaintVertexColor(Vector3 position, int index)
    {
        MeshFilter mf = m_MeshRenderer.GetComponent<MeshFilter>();
        if (mf)
        {
            if (m_PaintVertexType == PaintVertexType.点)
                ShowBrush(mf.sharedMesh, mf.transform.localToWorldMatrix, position, mf.sharedMesh.triangles[index * 3],
                    mf.sharedMesh.triangles[index * 3 + 1], mf.sharedMesh.triangles[index * 3 + 2]);
            else
                ShowBrush(mf.sharedMesh, mf.transform.localToWorldMatrix, mf.sharedMesh.triangles[index * 3],
                    mf.sharedMesh.triangles[index * 3 + 1], mf.sharedMesh.triangles[index * 3 + 2]);
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
                if (m_PaintVertexType == PaintVertexType.点)
                    PaintVertexColorAtPoint(mf.sharedMesh, index, position);
                else
                    PaintVertexColorAtTriangle(mf.sharedMesh, index);
            }
        }
    }

    private void ShowBrush(Mesh mesh, Matrix4x4 matrix, int index0, int index1, int index2)
    {
        m_BrushMaterial.SetVector("_VIndex", new Vector4(index0, index1, index2, 0));
        m_BrushMaterial.SetPass(2);
        Graphics.DrawMeshNow(mesh, matrix);
    }

    private void ShowVertexColorPreview()
    {
        MeshFilter mf = m_MeshRenderer.GetComponent<MeshFilter>();
        if (!mf)
            return;
        if (!mf.sharedMesh)
            return;
        m_BrushMaterial.SetPass(4);
        Graphics.DrawMeshNow(mf.sharedMesh, m_MeshRenderer.transform.localToWorldMatrix);
    }

    private void ShowBrush(Mesh mesh, Matrix4x4 matrix, Vector3 position, int index0, int index1, int index2)
    {
        float dis = Mathf.Infinity;
        int index = index0;
        position = m_MeshRenderer.transform.worldToLocalMatrix.MultiplyPoint(position);
        Vector3 p = mesh.vertices[index0];
        float tdis = Vector3.Distance(position, p);
        if (tdis < dis)
        {
            dis = tdis;
            index = index0;
        }
        p = mesh.vertices[index1];
        tdis = Vector3.Distance(position, p);
        if (tdis < dis)
        {
            dis = tdis;
            index = index1;
        }
        p = mesh.vertices[index2];
        tdis = Vector3.Distance(position, p);
        if (tdis < dis)
        {
            index = index2;
        }
        m_BrushMaterial.SetVector("_VIndex", new Vector4(index, 0, 0, 0));
        m_BrushMaterial.SetPass(3);
        Graphics.DrawMeshNow(mesh, matrix);
    }

    private void PaintVertexColorAtPoint(Mesh mesh, int index, Vector3 position)
    {
        float dis = Mathf.Infinity;
        int hitid = 0;
        position = m_MeshRenderer.transform.worldToLocalMatrix.MultiplyPoint(position);

        for (int i = 0; i < 3; i++)
        {
            int id = mesh.triangles[index * 3 + i];
            float distop = Vector3.Distance(position, mesh.vertices[id]);
            if (distop < dis)
            {
                dis = distop;
                hitid = index * 3 + i;
            }
        }
        Color[] ncl = mesh.colors;
        ncl[mesh.triangles[hitid]] = SetVertexColor(ncl[mesh.triangles[hitid]]);
        mesh.colors = ncl;
    }

    private void PaintVertexColorAtTriangle(Mesh mesh, int index)
    {
        Color[] ncl = mesh.colors;
        ncl[mesh.triangles[index * 3]] = SetVertexColor(ncl[mesh.triangles[index * 3]]);
        ncl[mesh.triangles[index * 3 + 1]] = SetVertexColor(ncl[mesh.triangles[index * 3 + 1]]);
        ncl[mesh.triangles[index * 3 + 2]] = SetVertexColor(ncl[mesh.triangles[index * 3 + 2]]);
        mesh.colors = ncl;
    }

    private Color SetVertexColor(Color color)
    {
        switch (m_PaintVertexChannel)
        {
            case PaintVertexChannel.R:
                color = new Color(m_PaintVertexAlpha, color.g, color.b, color.a);
                break;
            case PaintVertexChannel.G:
                color = new Color(color.r, m_PaintVertexAlpha, color.b, color.a);
                break;
            case PaintVertexChannel.B:
                color = new Color(color.r, color.g, m_PaintVertexAlpha, color.a);
                break;
            case PaintVertexChannel.A:
                color = new Color(color.r, color.g, color.b, m_PaintVertexAlpha);
                break;
        }
        return color;
    }

    private bool CreateCopyMesh()
    {
        MeshFilter mf = m_MeshRenderer.GetComponent<MeshFilter>();
        if (!mf)
            return false;
        if (!mf.sharedMesh)
            return false;
        string meshPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
        if (meshPath.ToLower().EndsWith(".asset"))
            return false;

        string savePath = EditorUtility.SaveFolderPanel("保存Mesh路径", "", "");
        if (string.IsNullOrEmpty(savePath))
            return false;
        savePath = FileUtil.GetProjectRelativePath(savePath);
        if (string.IsNullOrEmpty(savePath))
            return false;

        System.IO.FileInfo file = new System.IO.FileInfo(meshPath);
        string fileName = "New Terrain Mesh";
        if (!string.IsNullOrEmpty(file.Extension))
            fileName = file.Name.Replace(file.Extension, "");
        savePath = savePath + "/" + fileName + ".asset";

        Mesh mesh = new Mesh();
        mesh.vertices = mf.sharedMesh.vertices;
        mesh.colors = mf.sharedMesh.colors;
        mesh.normals = mf.sharedMesh.normals;
        mesh.tangents = mf.sharedMesh.tangents;
        mesh.triangles = mf.sharedMesh.triangles;
        mesh.uv = mf.sharedMesh.uv;
        mesh.uv2 = mf.sharedMesh.uv2;
        mesh.uv3 = mf.sharedMesh.uv3;
        mesh.uv4 = mf.sharedMesh.uv4;

        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
        AssetDatabase.CreateAsset(mesh, savePath);

        mf.sharedMesh = mesh;
        MeshCollider c = m_MeshRenderer.GetComponent<MeshCollider>();
        if (c)
            c.sharedMesh = mesh;
        return true;
    }
}
