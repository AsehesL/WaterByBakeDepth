using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    /// <summary>
    /// Mesh生成器类型
    /// </summary>
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
        /// 四叉树网格
        /// </summary>
        QuadTreeMesh,
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
        /// <summary>
        /// 根据贴图生成mesh
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        Mesh GenerateMesh(Texture2D texture);

        void SetSize(Vector2 size);

        Vector2 GetSize();

        void DrawGUI();

        void DrawSceneGUI(GameObject target, Vector2 offset, float rotY, float minHeight, float maxHeight);
    }
}