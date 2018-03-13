using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class EditorStyleViewer : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;
    private string search = string.Empty;

    private List<IconData> m_IconList = new List<IconData>();

    private class IconData
    {
        public GUIContent content;
        public string name;

        public IconData(GUIContent content, string name)
        {
            this.content = content;
            this.name = name;
        }
    }

    private MethodInfo m_LoadIcon;

    private const string kInternalTexturePath = "Assets/Editor Default Resources/InternalIcon/Internal_Texture.txt";
    private const string kInternalIconPath = "Assets/Editor Default Resources/InternalIcon/Internal_Icon.txt";
    private const string kInternalLoadIconPath = "Assets/Editor Default Resources/InternalIcon/Internal_LoadIcon.txt";

    private enum ViewMode
    {
        InternalStyle,
        InternalIcon,
    }

    private ViewMode m_ViewMode;

    [MenuItem("程序工具/杂项/GUI样式查看器")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(EditorStyleViewer));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("单击示例将复制其名到剪贴板", "label");
        GUILayout.FlexibleSpace();
        GUILayout.Label("查找:");
        search = EditorGUILayout.TextField(search);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("HelpBox");
        if (GUILayout.Button("查看GUI样式"))
        {
            m_ViewMode = ViewMode.InternalStyle;
        }
        if (GUILayout.Button("查看内置Texture"))
        {
            LoadInternalTexture();
        }
        if (GUILayout.Button("查看内置Icon"))
        {
            LoadIcon();
        }
        GUILayout.EndHorizontal();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        switch (m_ViewMode)
        {
            case ViewMode.InternalStyle:
                DrawInternalStyle();
                break;
            case ViewMode.InternalIcon:
                DrawInternalIcon();
                break;
        }

        GUILayout.EndScrollView();
    }

    private void DrawInternalStyle()
    {
        foreach (GUIStyle style in GUI.skin)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                GUILayout.Space(7);
                if (GUILayout.Button(style.name, style))
                {
                    EditorGUIUtility.systemCopyBuffer = "\"" + style.name + "\"";
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel("\"" + style.name + "\"");
                GUILayout.EndHorizontal();
                GUILayout.Space(11);
            }
        }
    }

    private void DrawInternalIcon()
    {
        for (int i = 0; i < m_IconList.Count; i++)
        {
            if (m_IconList[i].content.text.ToLower().Contains(search.ToLower()))
            {
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                if (GUILayout.Button(m_IconList[i].content))
                {
                    EditorGUIUtility.systemCopyBuffer = "\"" + m_IconList[i].name + "\"";
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel("\"" + m_IconList[i].name + "\"");
                GUILayout.EndHorizontal();
                GUILayout.Space(11);
            }
        }
    }

    private void LoadInternalTexture()
    {
        if (!File.Exists(kInternalTexturePath))
            return;
        m_IconList.Clear();
        StreamReader sr = new StreamReader(kInternalTexturePath);
        string line = sr.ReadLine();
        while (line != null)
        {
            Texture2D tex = EditorGUIUtility.FindTexture(line);
            if (tex != null)
                m_IconList.Add(new IconData(new GUIContent(line, tex), line));
            line = sr.ReadLine();
        }
        sr.Close();
        m_ViewMode = ViewMode.InternalIcon;
    }

    private void LoadIcon()
    {
        if (!File.Exists(kInternalIconPath) && !File.Exists(kInternalLoadIconPath))
            return;
        m_IconList.Clear();
        LoadInternalIcon();
        LoadInternalIconByReflection();
        m_ViewMode = ViewMode.InternalIcon;
    }

    private void LoadInternalIcon()
    {
        if (!File.Exists(kInternalIconPath))
            return;
        StreamReader sr = new StreamReader(kInternalIconPath);
        string line = sr.ReadLine();
        while (line != null)
        {
            GUIContent content = EditorGUIUtility.IconContent(line);
            if(content != null)
                m_IconList.Add(new IconData(content, line));
            line = sr.ReadLine();
        }
        sr.Close();
    }

    private void LoadInternalIconByReflection()
    {
        if (!File.Exists(kInternalLoadIconPath))
            return;
        if (m_LoadIcon == null)
        {
            m_LoadIcon = typeof(EditorGUIUtility).GetMethod("LoadIcon",
                BindingFlags.NonPublic | BindingFlags.Static);
        }
        StreamReader sr = new StreamReader(kInternalLoadIconPath);
        string line = sr.ReadLine();
        while (line != null)
        {
            Texture2D tex = (Texture2D)m_LoadIcon.Invoke(null, new System.Object[] { line });
            if (tex != null)
                m_IconList.Add(new IconData(new GUIContent(line, tex), line));
            line = sr.ReadLine();
        }
        sr.Close();
    }
}
