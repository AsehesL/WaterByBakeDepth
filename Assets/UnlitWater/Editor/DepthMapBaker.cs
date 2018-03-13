using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class DepthMapBaker : MDIEditorWindow
{

    private MeshFilter m_MeshFilter;

    private Texture2D m_Texture;

    private float m_RMaxDepth = 1;
    private float m_RDepthPower = 1;

    private float m_GMaxDepth = 1;
    private float m_GDepthPower = 1;

    private float m_BMaxDepth = 1;
    private float m_BDepthPower = 1;

    private float m_AMaxDepth = 1;
    private float m_ADepthPower = 1;

    private float m_Height = 1;

    private Vector2 m_Center;
    private Vector2 m_Size;

    private enum ApplyVertexChannel
    {
        R,
        G,
        B,
        Alpha
    }

    [MenuItem("程序工具/图形/工具/深度图烘焙")]
    static void OpenWin()
    {
        DepthMapBaker window = DepthMapBaker.CreateWindow<DepthMapBaker>();
        window.titleContent = new GUIContent("深度图烘焙");
    }

    public static void OpenDepthMapBaker(MeshFilter meshFilter)
    {
        DepthMapBaker window = DepthMapBaker.CreateWindow<DepthMapBaker>();
        window.titleContent = new GUIContent("深度图烘焙");
        window.m_MeshFilter = meshFilter;
        window.CheckMesh();
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
        if (m_MeshFilter)
        {
            Vector3 pos1 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(m_Size.x, 0, m_Size.y);
            Vector3 pos2 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(m_Size.x, 0, -m_Size.y);
            Vector3 pos3 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(-m_Size.x, 0, -m_Size.y);
            Vector3 pos4 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(-m_Size.x, 0, m_Size.y);

            Vector3 pos5 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(m_Size.x, -m_Height, m_Size.y);
            Vector3 pos6 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(m_Size.x, -m_Height, -m_Size.y);
            Vector3 pos7 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(-m_Size.x, -m_Height, -m_Size.y);
            Vector3 pos8 = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y) + new Vector3(-m_Size.x, -m_Height, m_Size.y);
            
            Handles.DrawLine(pos1, pos2);
            Handles.DrawLine(pos2, pos3);
            Handles.DrawLine(pos3, pos4);
            Handles.DrawLine(pos4, pos1);

            Handles.DrawLine(pos5, pos6);
            Handles.DrawLine(pos6, pos7);
            Handles.DrawLine(pos7, pos8);
            Handles.DrawLine(pos8, pos5);

            Handles.DrawLine(pos1, pos5);
            Handles.DrawLine(pos2, pos6);
            Handles.DrawLine(pos3, pos7);
            Handles.DrawLine(pos4, pos8);
        }
    }

    [EWSubWindow("预览", EWSubWindowIcon.Texture, true, SubWindowStyle.Preview, EWSubWindowToolbarType.Mini)]
    private void DrawPreview(Rect rect, Rect toolbar)
    {
        if (GUI.Button(new Rect(toolbar.x + 10, toolbar.y, 90, 15), "从文件加载深度图", GUIStyleCache.GetStyle("MiniToolBarButton")))
        {
            LoadTexture();
        }
        if (m_Texture)
        {
            Rect previewRect = DrawPreviewTexture(rect);
            GUI.DrawTexture(previewRect, m_Texture);
        }
    }

    private Rect DrawPreviewTexture(Rect rect)
    {
        float aspect = rect.width/rect.height;
        float textaspect = m_Texture.width/m_Texture.height;
        Rect previewRect = new Rect();
        if (aspect > textaspect)
        {
            previewRect.x = rect.x + (rect.width - textaspect*rect.height)/2;
            previewRect.y = rect.y;
            previewRect.width = textaspect*rect.height;
            previewRect.height = rect.height;
        }
        else
        {
            previewRect.x = rect.x;
            previewRect.y = rect.y + (rect.height - rect.width/textaspect)/2;
            previewRect.width = rect.width;
            previewRect.height = rect.width/textaspect;
        }
        return previewRect;
    }

    [EWSubWindow("设置", EWSubWindowIcon.Setting)]
    private void DrawSetting(Rect rect)
    {
        GUI.BeginGroup(new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20));

        GUI.Label(new Rect(0, 0, 110, 16), "目标网格:");
        EditorGUI.BeginChangeCheck();
        m_MeshFilter =
            (MeshFilter)
                EditorGUI.ObjectField(new Rect(110, 0, rect.width - 130, 16), m_MeshFilter, typeof(MeshFilter), true);
        if (EditorGUI.EndChangeCheck())
        {
            CheckMesh();
        }

        bool guienable = GUI.enabled;
        bool guienableNow = m_MeshFilter != null;
        GUI.enabled = guienableNow;

        EditorGUI.BeginChangeCheck();
        m_Height = Mathf.Max(0, EditorGUI.FloatField(new Rect(0, 20, rect.width - 20, 16),"高度:", m_Height));
        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();

        GUI.Label(new Rect(0, 40, 110, 16), "最大R深度:");
        m_RMaxDepth = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 40, rect.width - 130, 16), m_RMaxDepth));
        GUI.Label(new Rect(0, 60, 110, 16), "R深度强度:");
        m_RDepthPower = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 60, rect.width - 130, 16), m_RDepthPower));

        GUI.Label(new Rect(0, 80, 110, 16), "最大G深度:");
        m_GMaxDepth = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 80, rect.width - 130, 16), m_GMaxDepth));
        GUI.Label(new Rect(0, 100, 110, 16), "G深度强度:");
        m_GDepthPower = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 100, rect.width - 130, 16), m_GDepthPower));

        GUI.Label(new Rect(0, 120, 110, 16), "最大B深度:");
        m_BMaxDepth = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 120, rect.width - 130, 16), m_BMaxDepth));
        GUI.Label(new Rect(0, 140, 110, 16), "B深度强度:");
        m_BDepthPower = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 140, rect.width - 130, 16), m_BDepthPower));

        GUI.Label(new Rect(0, 160, 110, 16), "最大A深度:");
        m_AMaxDepth = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 160, rect.width - 130, 16), m_AMaxDepth));
        GUI.Label(new Rect(0, 180, 110, 16), "A深度强度:");
        m_ADepthPower = Mathf.Max(0, EditorGUI.FloatField(new Rect(110, 180, rect.width - 130, 16), m_ADepthPower));

        if (GUI.Button(new Rect(0, 200, (rect.width - 20) /3, 16), "渲染", GUIStyleCache.GetStyle("ButtonLeft")))
        {
            Render();
        }
        GUI.enabled = m_Texture != null && guienableNow;
        if (GUI.Button(new Rect((rect.width - 20) / 3, 200, (rect.width - 20) / 3, 16), "保存", GUIStyleCache.GetStyle("ButtonMid")))
        {
            SaveTexture();
        }
        GUI.enabled = guienableNow;
        if (GUI.Button(new Rect((rect.width - 20) *2 / 3, 200, (rect.width - 20) / 3, 16), "还原顶点色", GUIStyleCache.GetStyle("ButtonRight")))
        {
            RevertVertexColor(false);
        }
        GUI.enabled = m_Texture != null && guienableNow;
        if (GUI.Button(new Rect(0, 220, (rect.width - 20) / 4, 16), "应用到顶点R", GUIStyleCache.GetStyle("ButtonLeft")))
        {
            ApplyToVertex(ApplyVertexChannel.R);
        }
        if (GUI.Button(new Rect((rect.width - 20) / 4, 220, (rect.width - 20) / 4, 16), "应用到顶点G", GUIStyleCache.GetStyle("ButtonMid")))
        {
            ApplyToVertex(ApplyVertexChannel.G);
        }
        if (GUI.Button(new Rect((rect.width - 20) / 2, 220, (rect.width - 20) / 4, 16), "应用到顶点B", GUIStyleCache.GetStyle("ButtonMid")))
        {
            ApplyToVertex(ApplyVertexChannel.B);
        }
        if (GUI.Button(new Rect((rect.width - 20) *3/ 4, 220, (rect.width - 20) / 4, 16), "应用到顶点A", GUIStyleCache.GetStyle("ButtonRight")))
        {
            ApplyToVertex(ApplyVertexChannel.Alpha);
        }
        GUI.enabled = guienable;

        GUI.Label(new Rect(0, 240, rect.width - 20, 20), "Center:" + m_Center);
        GUI.Label(new Rect(0, 260, rect.width - 20, 20), "Size:" + m_Size);

        if (GUI.Button(new Rect(0, 280, rect.width, 20), "应用方向向量"))
        {
            ApplyToDir();
        }

        GUI.EndGroup();


    }

    void Render()
    {
        if (m_MeshFilter == null)
        {
            EditorUtility.DisplayDialog("错误", "请先设置目标网格", "确定");
            return;
        }
        Quaternion transRot = m_MeshFilter.transform.rotation * Quaternion.Euler(90, 0, 0);

        Camera newCam = new GameObject("[TestCamera]").AddComponent<Camera>();
        newCam.clearFlags = CameraClearFlags.SolidColor;
        newCam.backgroundColor = Color.black;
        newCam.orthographic = true;
        newCam.aspect = m_Size.x / m_Size.y;
        newCam.orthographicSize = m_Size.y;
        newCam.nearClipPlane = 0;
        newCam.farClipPlane = m_Height;
        newCam.transform.position = m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y);
        newCam.transform.rotation = transRot;
        newCam.enabled = false;

        RenderTexture rt = new RenderTexture(4096, 4096, 24);
        rt.hideFlags = HideFlags.HideAndDontSave;

        bool isMeshActive = m_MeshFilter.gameObject.active;
        m_MeshFilter.gameObject.SetActive(false);

        newCam.targetTexture = rt;
        Shader.SetGlobalFloat("depthR", m_RMaxDepth);
        Shader.SetGlobalFloat("powerR", m_RDepthPower);
        Shader.SetGlobalFloat("depthG", m_GMaxDepth);
        Shader.SetGlobalFloat("powerG", m_GDepthPower);
        Shader.SetGlobalFloat("depthB", m_BMaxDepth);
        Shader.SetGlobalFloat("powerB", m_BDepthPower);
        Shader.SetGlobalFloat("depthA", m_AMaxDepth);
        Shader.SetGlobalFloat("powerA", m_ADepthPower);
        newCam.RenderWithShader(Shader.Find("Hidden/DepthMapRenderer"), "RenderType");

        m_Texture = new Texture2D(rt.width, rt.height);

        RenderTexture tp = RenderTexture.active;
        RenderTexture.active = rt;
        m_Texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        m_Texture.Apply();
        RenderTexture.active = tp;

        DestroyImmediate(rt);
        DestroyImmediate(newCam.gameObject);

        m_MeshFilter.gameObject.SetActive(isMeshActive);
    }

    void ApplyToVertex(ApplyVertexChannel channel)
    {
        if (!RevertVertexColor(true))
            return;
        Color[] colors = m_MeshFilter.sharedMesh.colors;
        Matrix4x4 pj = GetUVProjMatrix();
        for (int i = 0; i < m_MeshFilter.sharedMesh.vertexCount; i++)
        {
            Vector3 uv = pj.MultiplyPoint(m_MeshFilter.sharedMesh.vertices[i]);
            Vector2 texUV = new Vector2(uv.x*0.5f + 0.5f, uv.y*0.5f + 0.5f);
            int x = (int) (texUV.x*m_Texture.width);
            int y = (int) (texUV.y*m_Texture.height);
            if (x < 0)
                x = 0;
            if (x >= m_Texture.width)
                x = m_Texture.width - 1;
            if (y < 0)
                y = 0;
            if (y >= m_Texture.height)
                y = m_Texture.height - 1;
            Color color = m_Texture.GetPixel(x, y);
            colors[i] = FilterColor(channel, colors[i], color);
        }

        m_MeshFilter.sharedMesh.colors = colors;
    }

    void ApplyToDir()
    {
        Matrix4x4 pj = GetUVProjMatrix();
        
        Vector2[] uv2 = new Vector2[m_MeshFilter.sharedMesh.vertexCount];
        for (int i = 0; i < m_MeshFilter.sharedMesh.vertexCount; i++)
        {
            Vector3 uv = pj.MultiplyPoint(m_MeshFilter.sharedMesh.vertices[i]);
            Vector2 texUV = new Vector2(uv.x * 0.5f + 0.5f, uv.y * 0.5f + 0.5f);
            int x = (int)(texUV.x * m_Texture.width);
            int y = (int)(texUV.y * m_Texture.height);
            uv2[i] = GetDir(m_Texture, x, y);
        }
        m_MeshFilter.sharedMesh.uv2 = uv2;
    }

    private static Vector2 GetDir(Texture2D tex, int i, int j)
    {
        float heightLeft = SafeGetPixel(tex, i - 1, j).r;
        float heightRight = SafeGetPixel(tex, i + 1, j).r;
        float heightBottom = SafeGetPixel(tex, i, j - 1).r;
        float heightTop = SafeGetPixel(tex, i, j + 1).r;

        Vector2 dir = new Vector2(heightRight - heightLeft, heightTop - heightBottom).normalized;
        return dir;
    }

    private static Color SafeGetPixel(Texture2D tex, int x, int y)
    {
        if (x < 0)
            x = 0;
        if (x >= tex.width)
            x = tex.width - 1;
        if (y < 0)
            y = 0;
        if (y >= tex.height)
            y = tex.height - 1;
        return tex.GetPixel(x, y);
    }

    bool RevertVertexColor(bool onlyRevertIfNotExist)
    {
        if (m_MeshFilter == null)
            return false;
        if (m_Texture == null)
            return false;
        if (m_MeshFilter.sharedMesh == null)
            return false;
        if (MeshFromFile(m_MeshFilter.sharedMesh))
        {
            if (EditorUtility.DisplayDialog("警告", "当前Mesh来源于模型文件，只有单独的Mesh Asset可以应用顶点，是否创建Mesh拷贝？", "是", "否"))
                if (!CreateCopyMesh())
                    return false;
        }
        bool notExist = false;
        if (m_MeshFilter.sharedMesh.colors.Length <= 0)
        {
            m_MeshFilter.sharedMesh.colors = new Color[m_MeshFilter.sharedMesh.vertexCount];
            notExist = true;
        }
        if (!notExist && onlyRevertIfNotExist)
            return true;
        Color[] colors = m_MeshFilter.sharedMesh.colors;
        for (int i = 0; i < m_MeshFilter.sharedMesh.vertexCount; i++)
        {
            colors[i] = Color.white;
        }
        m_MeshFilter.sharedMesh.colors = colors;
        return true;
    }

    private Color FilterColor(ApplyVertexChannel channel, Color originColor, Color sourceColor)
    {
        if (channel == ApplyVertexChannel.Alpha)
            return new Color(originColor.r, originColor.g, originColor.b, sourceColor.a);
        else if (channel == ApplyVertexChannel.R)
            return new Color(sourceColor.r, originColor.g, originColor.b, originColor.a);
        else if (channel == ApplyVertexChannel.G)
            return new Color(originColor.r, sourceColor.g, originColor.b, originColor.a);
        else if (channel == ApplyVertexChannel.B)
            return new Color(originColor.r, originColor.g, sourceColor.b, originColor.a);
        return originColor;
    }

    private void CheckMesh()
    {
        if (m_MeshFilter)
        {
            RefreshSetting();
        }
    }

    private void RefreshSetting()
    {
        if (m_MeshFilter.sharedMesh == null)
        {
            m_MeshFilter = null;
            return;
        }
        if (MeshFromFile(m_MeshFilter.sharedMesh))
        {
            if (EditorUtility.DisplayDialog("警告", "当前Mesh来源于模型文件，只有单独的Mesh Asset可以应用顶点，是否创建Mesh拷贝？", "是", "否"))
            {
                if (!CreateCopyMesh())
                {
                    m_MeshFilter = null;
                    return;
                }
            }
        }
        var vertexes = m_MeshFilter.sharedMesh.vertices;
        Vector2 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector2 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
        for (int i = 0; i < vertexes.Length; i++)
        {
            Vector3 pos = m_MeshFilter.transform.localToWorldMatrix.MultiplyPoint(vertexes[i]);
            if (min.x > pos.x)
                min.x = pos.x;
            if (min.y > pos.z)
                min.y = pos.z;
            if (max.x < pos.x)
                max.x = pos.x;
            if (max.y < pos.z)
                max.y = pos.z;
        }
        m_Center = min + (max - min)/2;
        m_Size = (max - min)/2;
        m_Center.x = m_Center.x - m_MeshFilter.transform.position.x;
        m_Center.y = m_Center.y - m_MeshFilter.transform.position.z;
    }

    bool CreateCopyMesh()
    {
        if (!m_MeshFilter)
            return false;
        if (!m_MeshFilter.sharedMesh)
            return false;
        string meshPath = AssetDatabase.GetAssetPath(m_MeshFilter.sharedMesh);
        if (meshPath.ToLower().EndsWith(".asset"))
            return false;

        string savePath = EditorUtility.SaveFilePanel("保存Mesh路径", "", "", "asset");
        savePath = FileUtil.GetProjectRelativePath(savePath);
        if (string.IsNullOrEmpty(savePath))
            return false;

        Mesh mesh = new Mesh();
        mesh.vertices = m_MeshFilter.sharedMesh.vertices;
        mesh.colors = m_MeshFilter.sharedMesh.colors;
        mesh.normals = m_MeshFilter.sharedMesh.normals;
        mesh.tangents = m_MeshFilter.sharedMesh.tangents;
        mesh.triangles = m_MeshFilter.sharedMesh.triangles;
        mesh.uv = m_MeshFilter.sharedMesh.uv;
        mesh.uv2 = m_MeshFilter.sharedMesh.uv2;
        mesh.uv3 = m_MeshFilter.sharedMesh.uv3;
        mesh.uv4 = m_MeshFilter.sharedMesh.uv4;

        AssetDatabase.CreateAsset(mesh, savePath);

        m_MeshFilter.sharedMesh = mesh;
        MeshCollider c = m_MeshFilter.GetComponent<MeshCollider>();
        if (c)
            c.sharedMesh = mesh;
        return true;
    }

    private bool MeshFromFile(Mesh mesh)
    {
        string path = AssetDatabase.GetAssetPath(mesh);
        if (path.ToLower().EndsWith(".asset"))
            return false;
        return true;
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

    private Matrix4x4 GetUVProjMatrix()
    {
        Matrix4x4 toWorld = m_MeshFilter.transform.localToWorldMatrix;

        Matrix4x4 toCam = Matrix4x4.TRS(m_MeshFilter.transform.position + new Vector3(m_Center.x, 0, m_Center.y), Quaternion.Euler(90, 0, 0),
            Vector3.one);

        Matrix4x4 toProj = new Matrix4x4();

        toProj.m00 = 1 / m_Size.x;
        toProj.m11 = 1 / m_Size.y;
        toProj.m22 = 2 / m_Height;
        toProj.m23 = -1;
        toProj.m33 = 1;

        return toProj * toCam.inverse * toWorld;
    }
}
