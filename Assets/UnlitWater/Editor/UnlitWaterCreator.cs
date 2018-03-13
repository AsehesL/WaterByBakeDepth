using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitWaterCreator : EditorWindow
{

    private bool m_CreateMeshFromDepthTex;
    private bool m_AutoCreateMaterial;
    private Mesh m_Mesh;
    private Material m_Material;

    [MenuItem("GameObject/UnlitWater/Create UnlitWater", false, 2500)]
    static void CreateUnlitWater()
    {
        UnlitWaterCreator win = UnlitWaterCreator.GetWindow<UnlitWaterCreator>();
        win.titleContent = new GUIContent("WaterCreator");
    }

    void OnGUI()
    {
        GUI.Box(new Rect(5, 5, position.width - 10, position.height - 10), string.Empty);

        DrawSetting(new Rect(10, 10, position.width - 20, position.height - 20));
    }

    void DrawSetting(Rect rect)
    {
        m_CreateMeshFromDepthTex = GUI.Toggle(new Rect(rect.x, rect.y, rect.width, 17), m_CreateMeshFromDepthTex, "是否从深度图生成Mesh");
        m_Mesh = EditorGUI.ObjectField(new Rect(rect.x,rect.y+20,rect.width,17),"目标Mesh",m_Mesh, typeof(Mesh), true) as Mesh;
        m_AutoCreateMaterial = GUI.Toggle(new Rect(rect.x, rect.y + 40, rect.width, 17), m_AutoCreateMaterial, "自动生成材质");
        GUI.enabled = !m_AutoCreateMaterial;
        m_Material = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 60, rect.width, 17), "Material", m_Material, typeof(Material), false) as Material;
        GUI.enabled = true;

        if (GUI.Button(new Rect(rect.x, rect.y + 80, rect.width, 17), "创建"))
        {
            Create();
        }
    }

    void Create()
    {
        string filename = GetMeshName(m_Mesh);
        if (string.IsNullOrEmpty(filename))
            return;
        if (IsMeshFromModelFile(m_Mesh))
        {
            if (EditorUtility.DisplayDialog("错误", "当前Mesh来源于模型文件，需要先创建副本才能编辑顶点色!是否立即创建Mesh拷贝？", "是", "否"))
            {
                m_Mesh = CopyMesh(m_Mesh, filename);
            }
        }
        if (m_Mesh)
        {
            GameObject go = new GameObject("[UnlitWater]");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = m_Mesh;
            if (m_AutoCreateMaterial)
                mr.sharedMaterial = CreateMaterial(filename);
            else
                mr.sharedMaterial = m_Material;
        }
    }

    public static bool IsMeshFromModelFile(Mesh mesh)
    {
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (!meshPath.ToLower().EndsWith(".asset"))
            return true;
        return false;
    }

    public static string GetMeshName(Mesh mesh)
    {
        if (!mesh)
            return null;
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        System.IO.FileInfo file = new System.IO.FileInfo(meshPath);
        string fileName = file.Name.Replace(file.Extension, "");
        return fileName;
    }

    public static Mesh CopyMesh(Mesh targetmesh, string fileName)
    {
        string savePath = "Assets/UnlitWater/Meshes/" + fileName + ".asset";
        System.IO.FileInfo fi = new System.IO.FileInfo(savePath);
        if (fi.Directory.Exists == false)
            fi.Directory.Create();

        Mesh mesh = new Mesh();
        mesh.vertices = targetmesh.vertices;
        mesh.colors = targetmesh.colors;
        mesh.normals = targetmesh.normals;
        mesh.tangents = targetmesh.tangents;
        mesh.triangles = targetmesh.triangles;
        mesh.uv = targetmesh.uv;
        mesh.uv2 = targetmesh.uv2;
        mesh.uv3 = targetmesh.uv3;
        mesh.uv4 = targetmesh.uv4;

        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
        AssetDatabase.CreateAsset(mesh, savePath);
        
        return mesh;
    }

    public static Material CreateMaterial(string fileName)
    {
        string savePath = "Assets/UnlitWater/Materials/" + fileName + ".mat";
        System.IO.FileInfo fi = new System.IO.FileInfo(savePath);
        if (fi.Directory.Exists == false)
            fi.Directory.Create();
        Material mat = new Material(Shader.Find("Diffuse"));
        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
        AssetDatabase.CreateAsset(mat, savePath);
        return mat;
    }
}
