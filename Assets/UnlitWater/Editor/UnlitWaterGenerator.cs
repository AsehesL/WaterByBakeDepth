using System.Collections;
using System.Collections.Generic;
using ASL.UnlitWater;
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
    

    private GameObject m_TargetGameObject;

    private TargetErrorDef m_TargetErrorDef;

    private Vector2 m_LocalCenter;
    //private Vector2 m_Size;

    

    private float m_RotY = 0;  

    private float m_MaxHeight = 1;
    private float m_MinHeight = 1;

    private Texture2D m_Texture;

    private Transform m_LightTransform;

    private UnlitWaterPainter m_Painter;

    private IMeshGenerator meshGenerator
    {
        get
        {
            if (m_MeshGeneratorFactory != null)
                return m_MeshGeneratorFactory.GetGenerator(m_MeshGeneratorType);
            return null;
        }   
    }

    private TextureRenderer textureRenderer
    {
        get
        {
            if (m_TextureRendererFactory != null)
                return m_TextureRendererFactory.GetRenderer(m_TextureRendererType);
            return null;
        }
    }

    private MeshGeneratorType m_MeshGeneratorType;
    private TextureRendererType m_TextureRendererType;

    private MeshGeneratorFactory m_MeshGeneratorFactory;
    private TextureRendererFactory m_TextureRendererFactory;


    [MenuItem("GameObject/UnlitWater/Create UnlitWater", false, 2500)]
    static void InitWindow()
    {
        UnlitWaterGenerator win = UnlitWaterGenerator.CreateWindow<UnlitWaterGenerator>();
        win.titleContent = new GUIContent("Water编辑器");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_MeshGeneratorFactory == null)
            m_MeshGeneratorFactory = new MeshGeneratorFactory();
        if (m_TextureRendererFactory == null)
            m_TextureRendererFactory = new TextureRendererFactory();
        if(m_Painter == null)
            m_Painter = new UnlitWaterPainter();
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

            if (meshGenerator != null && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None)
                meshGenerator.DrawSceneGUI(m_TargetGameObject, m_LocalCenter, m_RotY, m_MinHeight, m_MaxHeight);

            m_Painter.DrawSceneGUI(m_TargetGameObject);
        }
    }

    [EWSubWindow("烘焙预览", EWSubWindowIcon.Texture, true, SubWindowStyle.Preview, EWSubWindowToolbarType.Mini)]
    private void DrawPreview(Rect rect, Rect toolbar)
    {
        if (GUI.Button(new Rect(toolbar.x + 10, toolbar.y, 90, 15), "从文件加载深度图", GUIStyleCache.GetStyle("MiniToolBarButton")))
        {
            UnlitWaterUtils.LoadTexture(ref m_Texture);
        }
        if (GUI.Button(new Rect(toolbar.x + 100, toolbar.y, 90, 15), "保存深度图", GUIStyleCache.GetStyle("MiniToolBarButton")))
        {
            UnlitWaterUtils.SaveTexture(m_Texture);
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
        GUI.enabled = guienable && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));
        EditorGUI.BeginChangeCheck();
        m_TargetGameObject =
            EditorGUI.ObjectField(new Rect(0, 0, rect.width - 10, 17), "载体目标", m_TargetGameObject, typeof (GameObject), true) as
                GameObject;
        m_MeshGeneratorType = (MeshGeneratorType)EditorGUI.EnumPopup(new Rect(0, 20, rect.width - 10, 17), "Mesh生成器类型", m_MeshGeneratorType);
        if (EditorGUI.EndChangeCheck())
        {
            CheckTargetCorrectness();
            Vector2 size = default(Vector2);
            UnlitWaterUtils.CalculateAreaInfo(m_TargetGameObject, m_MeshGeneratorType != MeshGeneratorType.ModelFile, ref m_LocalCenter, ref size);
            m_MeshGeneratorFactory.SetSize(m_MeshGeneratorType, size);
        }
        

        GUI.Box(new Rect(3, 45, rect.width - 16, rect.height - 140), "", GUI.skin.FindStyle("WindowBackground"));

        GUILayout.BeginArea(new Rect(5, 48, rect.width - 20, rect.height - 146));

        if (meshGenerator != null)
            meshGenerator.DrawGUI();
        GUILayout.EndArea();
        

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
        GUI.enabled = guienable && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

        GUI.enabled = (int) m_TargetErrorDef <= (int) TargetErrorDef.WillReplaceMesh && m_TargetGameObject && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None && guienable;
        GUI.Label(new Rect(0, 0, position.width - 10, 17), "区域设置");
        m_MaxHeight = Mathf.Max(0,
            EditorGUI.FloatField(new Rect(0, 20, rect.width - 10, 17),
                new GUIContent("上方高度", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整上方高度直到超过所有物体"),
                m_MaxHeight));
        m_MinHeight = Mathf.Max(0,
            EditorGUI.FloatField(new Rect(0, 40, rect.width - 10, 17),
                new GUIContent("下方高度", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整下方高度直到刚好超过水底最深处"),
                m_MinHeight));

        GUI.enabled = (int)m_TargetErrorDef <= (int)TargetErrorDef.WillReplaceMesh && m_TargetGameObject && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None && guienable && m_MeshGeneratorType == MeshGeneratorType.ModelFile;
        m_LocalCenter = EditorGUI.Vector2Field(new Rect(0, 60, rect.width - 10, 40),
            new GUIContent("位置偏移", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的坐标偏移"),
            m_LocalCenter);

        
        GUI.enabled = (int)m_TargetErrorDef <= (int)TargetErrorDef.WillReplaceMesh && m_TargetGameObject && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None && guienable;
        m_RotY = EditorGUI.FloatField(new Rect(0, 100, rect.width - 10, 17),
            new GUIContent("Y轴旋转", EditorGUIUtility.FindTexture("console.erroricon.inactive.sml"), "调整渲染区域的Y轴旋转"),
            m_RotY);

        GUI.Label(new Rect(0, 120, position.width - 10, 17), "渲染设置");
        m_TextureRendererType = (TextureRendererType)EditorGUI.EnumPopup(new Rect(0, 140, rect.width - 10, 17), "贴图渲染器类型", m_TextureRendererType);


        GUILayout.BeginArea(new Rect(0, 160, rect.width - 10, rect.height - 160));

        TextureRenderer renderer = textureRenderer;
        if (renderer != null)
            renderer.DrawGUI();

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("渲染", GUIStyleCache.GetStyle("ButtonLeft"), GUILayout.Width(rect.width*0.5f-5)))
        {
            if (renderer != null)
            {
                Vector2 size = m_MeshGeneratorFactory.GetSize(m_MeshGeneratorType);
                renderer.RenderDepthTexture(m_TargetGameObject, m_LocalCenter, size,
                    Quaternion.Euler(90, m_RotY, 0), m_MaxHeight, m_MinHeight, ref m_Texture);
            }
        }
        GUI.enabled = m_Texture != null && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None && guienable;
        if (m_MeshGeneratorType != MeshGeneratorType.ModelFile)
        {
            if (GUILayout.Button("生成Mesh", GUIStyleCache.GetStyle("ButtonRight")))
            {
                UnlitWaterUtils.GenerateMesh(m_TargetGameObject, m_Texture, meshGenerator);
            }
        }
        else
        {
            if (GUILayout.Button("应用到顶点色", GUIStyleCache.GetStyle("ButtonRight")))
            {
                UnlitWaterUtils.ApplyToVertexColor(m_TargetGameObject, m_Texture, m_LocalCenter, m_MeshGeneratorFactory.GetSize(m_MeshGeneratorType), m_MinHeight, m_MaxHeight);
            }
        }
        GUILayout.EndHorizontal();

        GUI.enabled = guienable;

        GUILayout.EndArea();
        
        GUI.EndGroup();
    }

    [EWSubWindow("材质设置", EWSubWindowIcon.Material)]
    private void DrawSetting(Rect rect)
    {
        bool guienable = true;
        GUI.enabled = (int)m_TargetErrorDef <= (int)TargetErrorDef.WillReplaceMesh && m_TargetGameObject && guienable && m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None;
        GUI.BeginGroup(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

        GUI.Label(new Rect(0,0,rect.width-10,17), "灯光");
        m_LightTransform =
            (Transform)
                EditorGUI.ObjectField(new Rect(0, 20, (rect.width-10)*0.8f, 17),"平行光", m_LightTransform, typeof(Transform),
                    true);
        if (GUI.Button(new Rect((rect.width-10)*0.8f, 20, (rect.width - 10)*0.2f, 17), "设置光照方向"))
        {
            if (m_LightTransform)
                UnlitWaterUtils.BakeLightDir(m_TargetGameObject, m_LightTransform.forward);
        }

        GUI.enabled = (int) m_TargetErrorDef <= (int) TargetErrorDef.WillReplaceMesh && m_TargetGameObject && guienable;

        GUI.Label(new Rect(0, 40, rect.width - 10, 17), "顶点绘制");
        

        GUI.Box(new Rect(5, 60, rect.width - 20, rect.height-75), "", GUI.skin.FindStyle("WindowBackground"));

        GUILayout.BeginArea(new Rect(10, 65, rect.width - 30, rect.height - 85));

        GUILayout.Label("选择绘制通道");

        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        m_Painter.paintVertexChannel = GUILayout.Toggle(m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.None, "不绘制",
             GUI.skin.FindStyle("ButtonLeft"))
            ? UnlitWaterPainter.Channel.None
            : m_Painter.paintVertexChannel;

        m_Painter.paintVertexChannel = GUILayout.Toggle(m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.R,"R:深度",
             GUI.skin.FindStyle("ButtonMid"))
            ? UnlitWaterPainter.Channel.R
            : m_Painter.paintVertexChannel;

        m_Painter.paintVertexChannel = GUILayout.Toggle(m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.G,"G:浪花透明", GUI.skin.FindStyle("ButtonMid"))
            ? UnlitWaterPainter.Channel.G
            : m_Painter.paintVertexChannel;

        m_Painter.paintVertexChannel = GUILayout.Toggle(m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.B, "B:海浪波纹透明", GUI.skin.FindStyle("ButtonMid"))
            ? UnlitWaterPainter.Channel.B
            : m_Painter.paintVertexChannel;

        m_Painter.paintVertexChannel = GUILayout.Toggle(m_Painter.paintVertexChannel == UnlitWaterPainter.Channel.A, "A:整体透明", GUI.skin.FindStyle("ButtonRight"))
            ? UnlitWaterPainter.Channel.A
            : m_Painter.paintVertexChannel;

        if (EditorGUI.EndChangeCheck())
        {
            m_Painter.ResetChannel();
        }

        GUILayout.EndHorizontal();

        m_Painter.paintVertexType =
    (UnlitWaterPainter.BrushType)
        EditorGUILayout.EnumPopup("笔刷类型", m_Painter.paintVertexType);

        m_Painter.paintVertexAlpha = EditorGUILayout.Slider("绘制强度", m_Painter.paintVertexAlpha, 0, 1);

        m_Painter.previewVertexColor = EditorGUILayout.Toggle("预览顶点色",
            m_Painter.previewVertexColor);


        GUILayout.EndArea();

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
                    if (UnlitWaterUtils.CreateCopyMesh(m_TargetGameObject))
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
        if (mf && mf.sharedMesh && UnlitWaterUtils.IsMeshFromModelFile(mf.sharedMesh))
        {
            m_TargetErrorDef = TargetErrorDef.MeshFromModel;
            return;
        }
        if (m_MeshGeneratorType != MeshGeneratorType.ModelFile)
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
    
}
