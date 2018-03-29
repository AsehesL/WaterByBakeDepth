using UnityEngine;
using System.Collections;


namespace ASL.UnlitWater
{
    /// <summary>
    /// 由于所有类型的网格生成器均实现IMeshGenerator接口，考虑到使用多态会导致序列化出现问题，即一旦项目中有重编的代码或导入新资源，会导致MeshGenerator编辑的参数丢失，因此采用以下方法，预先保存所有类型的Generator对象
    /// </summary>
    [System.Serializable]
    internal class MeshGeneratorFactory
    {
        [SerializeField] private LodMesh m_LodMesh;
        [SerializeField] private SimpleGridMesh m_SimpleGrid;
        [SerializeField] private ModelMesh m_ModelMesh;
        [SerializeField] private QuadTreeMesh m_QuadTreeMesh;

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
                case MeshGeneratorType.QuadTreeMesh:
                    if (m_QuadTreeMesh == null)
                        m_QuadTreeMesh = new QuadTreeMesh();
                    return m_QuadTreeMesh;
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