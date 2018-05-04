using UnityEngine;
using System.Collections;
using UnityEditor;


namespace ASL.UnlitWater
{
    internal static class UnlitWaterUtils
    {
        /// <summary>
        /// 生成网格
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tex"></param>
        /// <param name="size"></param>
        /// <param name="xCells"></param>
        /// <param name="zCells"></param>
        /// <param name="maxLod"></param>
        /// <param name="discardSamples"></param>
        public static void GenerateMesh(GameObject target, Texture2D tex, IMeshGenerator generator)
        {
            if (tex == null)
                return;
            if (target == null)
                return;
            if (generator == null)
                return;
            string savePath = EditorUtility.SaveFilePanel("保存Mesh路径", "Assets/", "New Water Mesh", "asset");
            if (string.IsNullOrEmpty(savePath))
                return;
            savePath = FileUtil.GetProjectRelativePath(savePath);
            if (string.IsNullOrEmpty(savePath))
                return;
            
            //var unlitMesh = new ASL.UnlitWater.LodMesh(xCells, zCells, size.x*2, size.y*2,
            //    -size.x, -size.y, maxLod, uvDir, discardSamples);
            Mesh mesh = generator.GenerateMesh(tex);
            if (!mesh)
                return;
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (!mf)
                mf = target.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            AssetDatabase.CreateAsset(mesh, savePath);

            MeshCollider mc = target.GetComponent<MeshCollider>();
            if (mc)
                mc.sharedMesh = mesh;
        }

        /// <summary>
        /// 创建拷贝Mesh
        /// </summary>
        /// <returns></returns>
        public static bool CreateCopyMesh(GameObject target)
        {
            if (!target)
                return false;
            MeshFilter mf = target.GetComponent<MeshFilter>();
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
            MeshCollider c = target.GetComponent<MeshCollider>();
            if (c)
                c.sharedMesh = mesh;
            return true;
        }

        /// <summary>
        /// 加载贴图
        /// </summary>
        /// <param name="tex"></param>
        public static void LoadTexture(ref Texture2D tex)
        {
            string path = EditorUtility.OpenFilePanel("读取深度图", "", "png");
            if (string.IsNullOrEmpty(path))
                return;
            byte[] buffer = System.IO.File.ReadAllBytes(path);
            tex = new Texture2D(1, 1);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.LoadImage(buffer);
            tex.Apply();
        }

        /// <summary>
        /// 保存贴图
        /// </summary>
        /// <param name="tex"></param>
        public static void SaveTexture(Texture2D tex)
        {
            if (tex == null)
                return;
            string path = EditorUtility.SaveFilePanel("保存", Application.dataPath, "", "png");
            if (!string.IsNullOrEmpty(path))
            {
                byte[] buffer = tex.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, buffer);
                AssetDatabase.Refresh();
            }
        }

        public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height);
            tex.hideFlags = HideFlags.HideAndDontSave;

            RenderTexture tp = RenderTexture.active;
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = tp;

            tex.wrapMode = TextureWrapMode.Clamp;

            return tex;
        }

        /// <summary>
        /// 设置光照方向
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public static void BakeLightDir(GameObject target, Vector3 dir)
        {
            if (!target)
                return;
            MeshRenderer mr = target.GetComponent<MeshRenderer>();
            if (!mr)
                return;
            if (!mr.sharedMaterial)
                return;
            mr.sharedMaterial.SetVector("_LightDir", dir);
        }

        /// <summary>
        /// 判断Mesh是否来自模型文件
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static bool IsMeshFromModelFile(Mesh mesh)
        {
            string meshPath = AssetDatabase.GetAssetPath(mesh);
            if (!meshPath.ToLower().EndsWith(".asset"))
                return true;
            return false;
        }

        /// <summary>
        /// 应用到顶点色
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tex"></param>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="minHeight"></param>
        /// <param name="maxHeight"></param>
        public static void ApplyToVertexColor(GameObject target, Texture2D tex, Vector2 center, Vector2 size, float minHeight, float maxHeight)
        {
            if (target == null)
                return;
            if (!tex)
                return;
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;
            if (meshFilter.sharedMesh == null)
                return;
            Color[] colors = new Color[meshFilter.sharedMesh.vertexCount];
            Matrix4x4 pj = GetUVProjMatrix(meshFilter.transform, center, size, -maxHeight, minHeight);
            for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                Vector3 uv = pj.MultiplyPoint(meshFilter.sharedMesh.vertices[i]);
                Vector2 texUV = new Vector2(uv.x * 0.5f + 0.5f, uv.y * 0.5f + 0.5f);
                int x = (int)(texUV.x * tex.width);
                int y = (int)(texUV.y * tex.height);
                if (x < 0)
                    x = 0;
                if (x >= tex.width)
                    x = tex.width - 1;
                if (y < 0)
                    y = 0;
                if (y >= tex.height)
                    y = tex.height - 1;
                Color color = tex.GetPixel(x, y);
                colors[i] = new Color(color.r, 1, 1, 1);
            }

            meshFilter.sharedMesh.colors = colors;
        }

        /// <summary>
        /// 计算区域信息
        /// </summary>
        /// <param name="target"></param>
        /// <param name="localCenter"></param>
        /// <param name="size"></param>
        public static void CalculateAreaInfo(GameObject target, bool maxSizeOnly, ref Vector2 localCenter, ref Vector2 size)
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
            if (maxSizeOnly)
            {
                localCenter = Vector2.zero;
                size.x = Mathf.Max(Mathf.Abs(min.x - meshFilter.transform.position.x), Mathf.Abs(max.x - meshFilter.transform.position.x));
                size.y = Mathf.Max(Mathf.Abs(min.y - meshFilter.transform.position.z), Mathf.Abs(max.y - meshFilter.transform.position.z));
            }
            else
            {
                localCenter = min + (max - min) / 2;
                size = (max - min) / 2;
                localCenter.x = localCenter.x - meshFilter.transform.position.x;
                localCenter.y = localCenter.y - meshFilter.transform.position.z;
            }
        }

        private static Matrix4x4 GetUVProjMatrix(Transform transform, Vector2 center, Vector2 size, float near, float far)
        {
            Matrix4x4 toWorld = transform.localToWorldMatrix;

            Matrix4x4 toCam = Matrix4x4.TRS(transform.position + new Vector3(center.x, 0, center.y), Quaternion.Euler(90, 0, 0),
                Vector3.one);

            Matrix4x4 toProj = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, near, far);

            return toProj * toCam.inverse * toWorld;
        }
    }
}