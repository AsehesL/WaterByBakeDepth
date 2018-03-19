using UnityEngine;
using System.Collections;

namespace ASL.UnlitWater
{
    /// <summary>
    /// Mesh生成器接口
    /// 通过抽象mesh生成器，负责实现UnlitWater Mesh的生成
    /// </summary>
    internal interface IMeshGenerator
    {
        Mesh GenerateMesh(Texture2D texture);
    }

}