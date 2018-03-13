using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnlitWater  {

    [MenuItem("Test/PickDepthMap")]
    static void PickDepthMap()
    {
        string file = EditorUtility.OpenFilePanel("", "", "png");
        byte[] buffer = System.IO.File.ReadAllBytes(file);

        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(buffer);
        tex.Apply();

        Texture2D target = new Texture2D(tex.width, tex.height);

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                Color col = GetDir(tex, i, j);
                target.SetPixel(i, j, col);
            }
        }
        target.Apply();

        string savepath = EditorUtility.SaveFilePanel("", "", "", "png");
        buffer = target.EncodeToPNG();
        System.IO.File.WriteAllBytes(savepath, buffer);
    }

    private static Color GetDir(Texture2D tex, int i, int j)
    {
        float heightLeft = SafeGetPixel(tex, i - 1, j).r;
        float heightRight = SafeGetPixel(tex, i + 1, j).r;
        float heightBottom = SafeGetPixel(tex, i, j - 1).r;
        float heightTop = SafeGetPixel(tex, i, j + 1).r;

        Vector2 dir = new Vector2(heightRight - heightLeft, heightTop - heightBottom).normalized;
        return new Color(dir.x*0.5f + 0.5f, dir.y*0.5f + 0.5f, 1);
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
}
