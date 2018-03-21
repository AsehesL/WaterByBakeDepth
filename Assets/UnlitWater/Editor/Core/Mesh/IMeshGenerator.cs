using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    public enum MeshGeneratorType
    {
        /// <summary>
        /// 从模型文件
        /// </summary>
        ModelFile,
        /// <summary>
        /// Lod网格
        /// </summary>
        LodMesh,
        /// <summary>
        /// 简单格子
        /// </summary>
        SimpleGrid,
    }
    /// <summary>
    /// Mesh生成器接口
    /// 通过抽象mesh生成器，负责实现UnlitWater Mesh的生成
    /// </summary>
    internal interface IMeshGenerator
    {
        Mesh GenerateMesh(Texture2D texture);

        void SetSize(Vector2 size);

        Vector2 GetSize();

        void DrawGUI();

        void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight);
    }

    /// <summary>
    /// 由于所有类型的网格生成器均实现IMeshGenerator接口，考虑到序列化会出现问题，即一旦项目中有重编的代码或导入新资源，会导致MeshGenerator编辑的参数丢失，因此采用以下方法
    /// </summary>
    [System.Serializable]
    internal class MeshGeneratorFactory
    {
        [SerializeField] private LodMesh m_LodMesh;
        [SerializeField] private SimpleGridMesh m_SimpleGrid;
        [SerializeField] private ModelMesh m_ModelMesh;

        public IMeshGenerator GetGenerator(MeshGeneratorType type)
        {
            switch (type)
            {
                case MeshGeneratorType.SimpleGrid:
                    if (m_SimpleGrid == null)
                        m_SimpleGrid = new SimpleGridMesh();
                    return m_SimpleGrid;
                case MeshGeneratorType.LodMesh:
                    if (m_LodMesh == null)
                        m_LodMesh = new LodMesh();
                    return m_LodMesh;
                case MeshGeneratorType.ModelFile:
                default:
                    if (m_ModelMesh == null)
                        m_ModelMesh = new ModelMesh();
                    return m_ModelMesh;
            }
        }

        public void SetSize(MeshGeneratorType type, Vector2 size)
        {
            IMeshGenerator generator = GetGenerator(type);
            if (generator != null)
                generator.SetSize(size);
        }

        public Vector2 GetSize(MeshGeneratorType type)
        {
            IMeshGenerator generator = GetGenerator(type);
            if (generator != null)
                return generator.GetSize();
            return default(Vector2);
        }
        
    }
}