using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitWaterGenerator : MDIEditorWindow
{
    /// <summary>
    /// 目标对象错误类型
    /// </summary>
    enum TargetErrorDef
    {
        None = 0,
        //覆盖Mesh警告
        WillReplaceMesh,
        //Mesh来自模型文件警告
        MeshFromModel,
        //没有Mesh警告
        NoMesh,
    }

    private enum PaintVertexType
    {
        点,//对单个顶点着色
        三角//对三角面着色
    }

    private enum PaintVertexChannel
    {
        None,
        R,
        G,
        B,
        A,
    }

    private GameObject m_TargetGameObject;
    private bool m_AutoGenerateMesh;

    private TargetErrorDef m_TargetErrorDef;

    private int m_CellSizeX = 1;
    private int m_CellSizeZ = 1;
    private int m_MaxLod;

    private Vector2 m_LocalCenter;
    private Vector2 m_Size;

    private float m_MaxDepth = 1;
    private float m_DepthPower = 1;

    private float m_RotY = 0;

    private float m_MaxHeight = 1;
    private float m_MinHeight = 1;

    private PaintVertexType m_PaintVertexType;
    private PaintVertexChannel m_PaintVertexChannel;

    private Texture2D m_Texture;

    private Transform m_LightTransform;

    private bool m_PreviewVertexColor;

    private float m_PaintVertexAlpha;

    [MenuItem("GameObject/UnlitWater/Create UnlitWater", false, 2500)]
    static void Init()
    {
        UnlitWaterGenerator win = UnlitWaterGenerator.CreateWindow<UnlitWaterGenerator>();
        win.titleContent = new GUIContent("Water编辑器");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if ((int)m_TargetErrorDef <= (int)TargetErrorDef.WillReplaceMesh && m_TargetGameObject)
        {
            Quaternion rot = Quaternion.Euler(0, m_RotY, 0);
            Vector3 pos1 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +rot* new Vector3(m_Size.x, 0, m_Size.y);
            Vector3 pos2 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(m_Size.x, 0, -m_Size.y);
            Vector3 pos3 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, 0, -m_Size.y);
            Vector3 pos4 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, 0, m_Size.y);

            Vector3 pos5 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(m_Size.x, -m_MinHeight, m_Size.y);
            Vector3 pos6 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(m_Size.x, -m_MinHeight, -m_Size.y);
            Vector3 pos7 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, -m_MinHeight, -m_Size.y);
            Vector3 pos8 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, -m_MinHeight, m_Size.y);

            Vector3 pos9 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(m_Size.x, m_MaxHeight, m_Size.y);
            Vector3 pos10 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(m_Size.x, m_MaxHeight, -m_Size.y);
            Vector3 pos11 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, m_MaxHeight, -m_Size.y);
            Vector3 pos12 = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) + rot * new Vector3(-m_Size.x, m_MaxHeight, m_Size.y);

            Handles.color = Color.green;

            Handles.DrawLine(pos1, pos2);
            Handles.DrawLine(pos2, pos3);
            Handles.DrawLine(pos3, pos4);
            Handles.DrawLine(pos4, pos1);

            Handles.DrawLine(pos5, pos6);
            Handles.DrawLine(pos6, pos7);
            Handles.DrawLine(pos7, pos8);
            Handles.DrawLine(pos8, pos5);

            Handles.DrawLine(pos9, pos10);
            Handles.DrawLine(pos10, pos11);
            Handles.DrawLine(pos11, pos12);
            Handles.DrawLine(pos12, pos9);

            Handles.DrawLine(pos9, pos5);
            Handles.DrawLine(pos10, pos6);
            Handles.DrawLine(pos11, pos7);
            Handles.DrawLine(pos12, pos8);

            if (m_AutoGenerateMesh)
            {
                

                float deltax = m_Size.x*2/m_CellSizeX;
                float deltaz = m_Size.y*2/m_CellSizeZ;

                int dt = (int)Mathf.Pow(2, m_MaxLod);
                float loddeltax = deltax / dt;
                float loddeltaz = deltaz / dt;

                for (int i = 0; i < m_CellSizeX; i++)
                {
                    if (i > 0)
                    {
                        Handles.color = Color.blue;
                        Vector3 posb = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot*new Vector3(-m_Size.x + i*deltax, 0, m_Size.y);
                        Vector3 posf = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot*new Vector3(-m_Size.x + i*deltax, 0, -m_Size.y);
                        Handles.DrawLine(posb, posf);
                    }
                    Handles.color = Color.yellow;
                    for (int j = 1; j < dt; j++)
                    {
                        Vector3 posb = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot * new Vector3(-m_Size.x + i * deltax + j * loddeltax, 0, m_Size.y);
                        Vector3 posf = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot * new Vector3(-m_Size.x + i * deltax + j * loddeltax, 0, -m_Size.y);
                        Handles.DrawLine(posb, posf);
                    }
                }

                for (int i = 0; i < m_CellSizeZ; i++)
                {
                    if (i > 0)
                    {
                        Handles.color = Color.blue;
                        Vector3 posb = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot*new Vector3(-m_Size.x, 0, -m_Size.y + i*deltaz);
                        Vector3 posf = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot*new Vector3(m_Size.x, 0, -m_Size.y + i*deltaz);
                        Handles.DrawLine(posb, posf);
                    }
                    Handles.color = Color.yellow;
                    for (int j = 1; j < dt; j++)
                    {
                        Vector3 posb = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot * new Vector3(-m_Size.x, 0, -m_Size.y + i * deltaz + j * loddeltaz);
                        Vector3 posf = m_TargetGameObject.transform.position +
                                       new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y) +
                                       rot * new Vector3(m_Size.x, 0, -m_Size.y + i * deltaz + j * loddeltaz);
                        Handles.DrawLine(posb, posf);
                    }
                }
            }
        }
    }

    [EWSubWindow("烘焙预览", EWSubWindowIcon.Texture, true, SubWindowStyle.Preview, EWSubWindowToolbarType.Mini)]
    private void DrawPreview(Rect rect, Rect toolbar)
    {
        if (GUI.Button(new Rect(toolbar.x + 10, toolbar.y, 90, 15), "从文件加载深度图", GUIStyleCache.GetStyle("MiniToolBarButton")))
        {
            LoadTexture();
        }
        if (GUI.Button(new Rect(toolbar.x + 100, toolbar.y, 90, 15), "保存深度图", GUIStyleCache.GetStyle("MiniToolBarButton")))
        {
            SaveTexture();
        }
        if (m_Texture)
        {
            Rect previewRect = DrawPreviewTexture(rect);
            GUI.DrawTexture(previewRect, m_Texture);
        }
    }

    [EWSubWindow("Mesh设置", EWSubWindowIcon.MeshRenderer)]
    private void DrawMeshSetting(Rect rect)
    {
        bool guienable = GUI.enabled;
        GUI.enabled = guienable && m_PaintVertexChannel == PaintVertexChannel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));
        EditorGUI.BeginChangeCheck();
        m_TargetGameObject =
            EditorGUI.ObjectField(new Rect(0, 0, rect.width - 10, 17), "载体目标", m_TargetGameObject, typeof (GameObject), true) as
                GameObject;
        m_AutoGenerateMesh = EditorGUI.Toggle(new Rect(0, 20, rect.width - 10, 17), "是否自动生成Mesh", m_AutoGenerateMesh);
        if (EditorGUI.EndChangeCheck())
        {
            CheckTargetCorrectness();
            CalculateAreaInfo(m_TargetGameObject, ref m_LocalCenter, ref m_Size);
        }

        GUI.enabled = guienable && m_AutoGenerateMesh;
        m_Size.x = Mathf.Max(0.01f, EditorGUI.FloatField(new Rect(0, 40, rect.width - 10, 17), "Width", m_Size.x));
        m_Size.y = Mathf.Max(0.01f, EditorGUI.FloatField(new Rect(0, 60, rect.width - 10, 17), "Height", m_Size.y));
        m_CellSizeX = Mathf.Max(1, EditorGUI.IntField(new Rect(0, 80, rect.width - 10, 17), "CellWidth", m_CellSizeX));
        m_CellSizeZ = Mathf.Max(1, EditorGUI.IntField(new Rect(0, 100, rect.width - 10, 17), "CellHeight", m_CellSizeZ));
        m_MaxLod = EditorGUI.IntSlider(new Rect(0, 120, rect.width - 10, 17), "最大Lod", m_MaxLod, 0, 8);

        GUI.enabled = guienable;

        if (m_TargetErrorDef != TargetErrorDef.None)
        {
            DrawTargetErrorHelpBox(new Rect(0, rect.height - 90, rect.width - 10, 80));
        }
        GUI.EndGroup();
    }

    [EWSubWindow("烘焙设置", EWSubWindowIcon.Setting)]
    private void DrawBakeSetting(Rect rect)
    {
        bool guienable = GUI.enabled;
        GUI.enabled = guienable && m_PaintVertexChannel == PaintVertexChannel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

        GUI.enabled = (int) m_TargetErrorDef <= (int) TargetErrorDef.WillReplaceMesh && m_TargetGameObject && guienable;
        GUI.Label(new Rect(0, 0, position.width - 10, 17), "区域设置");
        m_MaxHeight = Mathf.Max(0,
            EditorGUI.FloatField(new Rect(0, 20, rect.width - 10, 17),
                new GUIContent("上方高度", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整上方高度直到超过所有物体"),
                m_MaxHeight));
        m_MinHeight = Mathf.Max(0,
            EditorGUI.FloatField(new Rect(0, 40, rect.width - 10, 17),
                new GUIContent("下方高度", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整下方高度直到刚好超过水底最深处"),
                m_MinHeight));

        m_LocalCenter = EditorGUI.Vector2Field(new Rect(0, 60, rect.width - 10, 40),
            new GUIContent("位置偏移", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的坐标偏移"),
            m_LocalCenter);
        m_Size = EditorGUI.Vector2Field(new Rect(0, 100, rect.width - 10, 40),
          new GUIContent("区域大小", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的区域大小"),
          m_Size);
        m_RotY = EditorGUI.FloatField(new Rect(0, 140, rect.width - 10, 17),
            new GUIContent("Y轴旋转", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的Y轴旋转"),
            m_RotY);

        GUI.Label(new Rect(0, 160, position.width - 10, 17), "渲染设置");
        m_MaxDepth = Mathf.Max(0,
           EditorGUI.FloatField(new Rect(0, 180, rect.width - 10, 17),
               new GUIContent("最大深度范围", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制渲染的最大深度范围，默认为1"),
               m_MaxDepth));
        m_DepthPower = Mathf.Max(0, 
           EditorGUI.FloatField(new Rect(0, 200, rect.width - 10, 17),
               new GUIContent("深度增强", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "控制渲染深度的效果增强或减弱，默认为1表示不增强"),
               m_DepthPower));

        if (GUI.Button(new Rect(0, 220, (rect.width - 10) / 2, 16), "渲染", GUIStyleCache.GetStyle("ButtonLeft")))
        {
            Render();
        }
        GUI.enabled = m_Texture != null && guienable;
        if (m_AutoGenerateMesh)
        {
            if (GUI.Button(new Rect((rect.width - 10) / 2, 220, (rect.width - 10) / 2, 16), "生成Mesh",
                GUIStyleCache.GetStyle("ButtonRight")))
            {
                GenerateMesh();
            }
        }
        else
        {
            if (GUI.Button(new Rect((rect.width - 10)/2, 220, (rect.width - 10)/2, 16), "应用到顶点色",
                GUIStyleCache.GetStyle("ButtonRight")))
            {
                ApplyToVertex();
            }
        }

        GUI.enabled = guienable;
        GUI.EndGroup();
    }

    [EWSubWindow("材质设置", EWSubWindowIcon.Material)]
    private void DrawSetting(Rect rect)
    {
        bool guienable = GUI.enabled;
        GUI.enabled = (int)m_TargetErrorDef <= (int)TargetErrorDef.WillReplaceMesh && m_TargetGameObject && guienable && m_PaintVertexChannel == PaintVertexChannel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

        GUI.Label(new Rect(0,0,rect.width-10,17), "灯光");
        m_LightTransform =
            (Transform)
                EditorGUI.ObjectField(new Rect(0, 20, (rect.width-10)*0.8f, 17),"平行光", m_LightTransform, typeof(Transform),
                    true);
        if (GUI.Button(new Rect((rect.width-10)*0.8f, 20, (rect.width - 10)*0.2f, 17), "烘焙光照方向"))
        {
        }

        GUI.enabled = (int) m_TargetErrorDef <= (int) TargetErrorDef.WillReplaceMesh && m_TargetGameObject && guienable;

        GUI.Label(new Rect(0, 40, rect.width - 10, 17), "顶点绘制");

        GUI.Box(new Rect(0, 60, rect.width - 10, 90), "", GUI.skin.FindStyle("WindowBackground"));

        GUI.Label(new Rect(5, 65, 100, 20), "选择绘制通道");

        EditorGUI.BeginChangeCheck();
        m_PaintVertexChannel = EditorGUI.Toggle(new Rect(rect.width - 15 - 280, 65, 70, 20),
            m_PaintVertexChannel == PaintVertexChannel.R, GUI.skin.FindStyle("ButtonLeft"))
            ? PaintVertexChannel.R
            : m_PaintVertexChannel;
        GUI.Label(new Rect(rect.width - 15 - 280, 65, 70, 20), "R:深度");

        m_PaintVertexChannel = EditorGUI.Toggle(new Rect(rect.width - 15 - 210, 65, 70, 20),
            m_PaintVertexChannel == PaintVertexChannel.G, GUI.skin.FindStyle("ButtonMid"))
            ? PaintVertexChannel.G
            : m_PaintVertexChannel;
        GUI.Label(new Rect(rect.width - 15 - 210, 65, 70, 20), "G:浪花透明");

        m_PaintVertexChannel = EditorGUI.Toggle(new Rect(rect.width - 15 - 140, 65, 70, 20),
            m_PaintVertexChannel == PaintVertexChannel.B, GUI.skin.FindStyle("ButtonMid"))
            ? PaintVertexChannel.B
            : m_PaintVertexChannel;
        GUI.Label(new Rect(rect.width - 15 - 140, 65, 70, 20), "B:海浪波纹透明");

        m_PaintVertexChannel = EditorGUI.Toggle(new Rect(rect.width - 15 - 70, 65, 70, 20),
            m_PaintVertexChannel == PaintVertexChannel.A, GUI.skin.FindStyle("ButtonRight"))
            ? PaintVertexChannel.A
            : m_PaintVertexChannel;
        GUI.Label(new Rect(rect.width - 15 - 70, 65, 70, 20), "A:整体透明");
        if (EditorGUI.EndChangeCheck())
        {
            if (m_PaintVertexChannel != PaintVertexChannel.None)
            {
                Vector4 vertexMask = new Vector4(m_PaintVertexChannel == PaintVertexChannel.R ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.G ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.B ? 1 : 0,
                    m_PaintVertexChannel == PaintVertexChannel.A ? 1 : 0);
                //m_BrushMaterial.SetVector("_VertexMask", vertexMask);
            }
        }

        m_PreviewVertexColor = EditorGUI.Toggle(new Rect(5, 85, rect.width - 20, 20),
            "预览顶点色",
            m_PreviewVertexColor);

        m_PaintVertexAlpha = EditorGUI.Slider(new Rect(5, 105, rect.width - 20, 20),
            "绘制透明度", m_PaintVertexAlpha, 0, 1);
        m_PaintVertexType =
            (PaintVertexType)
                EditorGUI.EnumPopup(new Rect(5, 125, rect.width - 20, 20), "笔刷类型",
                    m_PaintVertexType);


        GUI.EndGroup();
        GUI.enabled = guienable;
    }

    private void DrawTargetErrorHelpBox(Rect rect)
    {
        switch (m_TargetErrorDef)
        {
            case TargetErrorDef.MeshFromModel:
                EditorGUI.HelpBox(rect, "提示，Mesh来自模型文件，需要生成拷贝Mesh！", MessageType.Info);
                if (GUI.Button(new Rect(rect.x + 30, rect.y + 30, rect.width - 60, 17), "生成拷贝"))
                {
                    if (CreateCopyMesh())
                        CheckTargetCorrectness();
                }
                break;
            case TargetErrorDef.NoMesh:
                EditorGUI.HelpBox(rect, "错误，目标对象没有MeshFilter或没有Mesh，请确认！", MessageType.Error);
                break;
            case TargetErrorDef.WillReplaceMesh:
                EditorGUI.HelpBox(rect, "提示，该目标对象已经存在Mesh，自动生成新Mesh将覆盖原Mesh！", MessageType.Info);
                break;
        }
    }

    private Rect DrawPreviewTexture(Rect rect)
    {
        float aspect = rect.width / rect.height;
        float textaspect = m_Texture.width / m_Texture.height;
        Rect previewRect = new Rect();
        if (aspect > textaspect)
        {
            previewRect.x = rect.x + (rect.width - textaspect * rect.height) / 2;
            previewRect.y = rect.y;
            previewRect.width = textaspect * rect.height;
            previewRect.height = rect.height;
        }
        else
        {
            previewRect.x = rect.x;
            previewRect.y = rect.y + (rect.height - rect.width / textaspect) / 2;
            previewRect.width = rect.width;
            previewRect.height = rect.width / textaspect;
        }
        return previewRect;
    }

    /// <summary>
    /// 检测目标载体的正确性
    /// </summary>
    private void CheckTargetCorrectness()
    {
        if (m_TargetGameObject == null)
        {
            m_TargetErrorDef = TargetErrorDef.None;
            return;
        }
        MeshFilter mf = m_TargetGameObject.GetComponent<MeshFilter>();
        if (mf && mf.sharedMesh && IsMeshFromModelFile(mf.sharedMesh))
        {
            m_TargetErrorDef = TargetErrorDef.MeshFromModel;
            return;
        }
        if (m_AutoGenerateMesh)
        {
            if (mf && mf.sharedMesh)
            {
                m_TargetErrorDef = TargetErrorDef.WillReplaceMesh;
                return;
            }
        }
        else
        {
            if (!mf || !mf.sharedMesh)
            {
                m_TargetErrorDef = TargetErrorDef.NoMesh;
                return;
            }
        }
        m_TargetErrorDef = TargetErrorDef.None;
    }

    /// <summary>
    /// 创建拷贝Mesh
    /// </summary>
    /// <returns></returns>
    private bool CreateCopyMesh()
    {
        MeshFilter mf = m_TargetGameObject.GetComponent<MeshFilter>();
        if (!mf)
            return false;
        if (!mf.sharedMesh)
            return false;
        string meshPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
        if (meshPath.ToLower().EndsWith(".asset"))
            return false;

        string savePath = EditorUtility.SaveFilePanel("保存Mesh路径", "Assets/", "New Water Mesh", "asset");
        if (string.IsNullOrEmpty(savePath))
            return false;
        savePath = FileUtil.GetProjectRelativePath(savePath);
        if (string.IsNullOrEmpty(savePath))
            return false;

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

        //savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
        AssetDatabase.CreateAsset(mesh, savePath);

        mf.sharedMesh = mesh;
        MeshCollider c = m_TargetGameObject.GetComponent<MeshCollider>();
        if (c)
            c.sharedMesh = mesh;
        return true;
    }

    void Render()
    {
        if (m_TargetGameObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先设置目标网格", "确定");
            return;
        }
        Quaternion transRot = Quaternion.Euler(90, m_RotY, 0);

        Camera newCam = new GameObject("[TestCamera]").AddComponent<Camera>();
        newCam.clearFlags = CameraClearFlags.SolidColor;
        newCam.backgroundColor = Color.black;
        newCam.orthographic = true;
        newCam.aspect = m_Size.x / m_Size.y;
        newCam.orthographicSize = m_Size.y;
        newCam.nearClipPlane = -m_MaxHeight;
        newCam.farClipPlane = m_MinHeight;
        newCam.transform.position = m_TargetGameObject.transform.position + new Vector3(m_LocalCenter.x, 0, m_LocalCenter.y);
        newCam.transform.rotation = transRot;
        newCam.enabled = false;

        RenderTexture rt = new RenderTexture(4096, 4096, 24);
        rt.hideFlags = HideFlags.HideAndDontSave;

        bool isMeshActive = m_TargetGameObject.gameObject.active;
        m_TargetGameObject.gameObject.SetActive(false);

        newCam.targetTexture = rt;
        Shader.SetGlobalFloat("depth", m_MaxDepth);
        Shader.SetGlobalFloat("power", m_DepthPower);
        Shader.SetGlobalFloat("height", m_TargetGameObject.transform.position.y);
        Shader.SetGlobalFloat("minheight", m_MinHeight);
        newCam.RenderWithShader(Shader.Find("Hidden/DepthMapRenderer"), "RenderType");

        m_Texture = new Texture2D(rt.width, rt.height);

        RenderTexture tp = RenderTexture.active;
        RenderTexture.active = rt;
        m_Texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        m_Texture.Apply();
        RenderTexture.active = tp;

        DestroyImmediate(rt);
        DestroyImmediate(newCam.gameObject);

        m_TargetGameObject.gameObject.SetActive(isMeshActive);
    }

    void LoadTexture()
    {
        string path = EditorUtility.OpenFilePanel("读取深度图", "", "png");
        if (string.IsNullOrEmpty(path))
            return;
        byte[] buffer = System.IO.File.ReadAllBytes(path);
        m_Texture = new Texture2D(1, 1);
        m_Texture.LoadImage(buffer);
        m_Texture.Apply();
    }

    void SaveTexture()
    {
        if (m_Texture == null)
            return;
        {
            string path = EditorUtility.SaveFilePanel("保存", Application.dataPath, "", "png");
            if (!string.IsNullOrEmpty(path))
            {
                byte[] buffer = m_Texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, buffer);
                AssetDatabase.Refresh();
            }
        }
    }

    void GenerateMesh()
    {
        string savePath = EditorUtility.SaveFilePanel("保存Mesh路径", "Assets/", "New Water Mesh", "asset");
        if (string.IsNullOrEmpty(savePath))
            return;
        savePath = FileUtil.GetProjectRelativePath(savePath);
        if (string.IsNullOrEmpty(savePath))
            return;
        if (m_Texture == null)
            return;
        if (m_TargetGameObject == null)
            return;
        Mesh mesh = UnlitWaterMeshGenerator.GenerateMesh(m_Texture, m_CellSizeX, m_CellSizeZ, m_Size.x*2, m_Size.y*2,
            - m_Size.x + m_LocalCenter.x, - m_Size.y + m_LocalCenter.y,
            m_MaxLod);
        MeshFilter mf = m_TargetGameObject.GetComponent<MeshFilter>();
        if (!mf)
            mf = m_TargetGameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        //savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
        AssetDatabase.CreateAsset(mesh, savePath);

        MeshCollider mc = m_TargetGameObject.GetComponent<MeshCollider>();
        if(mc)
            mc.sharedMesh = mesh;
    }

    void ApplyToVertex()
    {
        
    }

    /// <summary>
    /// 计算区域信息
    /// </summary>
    /// <param name="target"></param>
    /// <param name="localCenter"></param>
    /// <param name="size"></param>
    private static void CalculateAreaInfo(GameObject target, ref Vector2 localCenter, ref Vector2 size)
    {
        if (!target)
            return;
        var meshFilter = target.GetComponent<MeshFilter>();
        if (!meshFilter || !meshFilter.sharedMesh)
            return;
        var vertexes = meshFilter.sharedMesh.vertices;
        Vector2 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector2 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
        for (int i = 0; i < vertexes.Length; i++)
        {
            Vector3 pos = meshFilter.transform.localToWorldMatrix.MultiplyPoint(vertexes[i]);
            if (min.x > pos.x)
                min.x = pos.x;
            if (min.y > pos.z)
                min.y = pos.z;
            if (max.x < pos.x)
                max.x = pos.x;
            if (max.y < pos.z)
                max.y = pos.z;
        }
        localCenter = min + (max - min) / 2;
        size = (max - min) / 2;
        localCenter.x = localCenter.x - meshFilter.transform.position.x;
        localCenter.y = localCenter.y - meshFilter.transform.position.z;
    }

    /// <summary>
    /// Mesh是否来自模型文件
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    private static bool IsMeshFromModelFile(Mesh mesh)
    {
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (!meshPath.ToLower().EndsWith(".asset"))
            return true;
        return false;
    }
}
