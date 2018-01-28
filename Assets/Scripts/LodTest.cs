using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LodTest : MonoBehaviour
{

    public Transform agent;

    public int xsize;
    public int zsize;
    public float scale;

    public bool debug;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;

    private LodMesh m_Mesh;

	void Start ()
	{
	    m_MeshFilter = gameObject.AddComponent<MeshFilter>();
	    m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

	    m_MeshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));

	    m_Mesh = new LodMesh(xsize, zsize, scale);

	    m_Mesh.BuildMesh();

	    m_MeshFilter.sharedMesh = m_Mesh.Mesh;

	}

    void OnDestroy()
    {
        m_Mesh.Release();
    }


    void OnDrawGizmos()
    {
        if (!debug)
            return;
        Gizmos.color = Color.green;

        for (int i = 0; i <= xsize; i++)
        {
            Vector3 from = new Vector3(i*scale, 0, 0);
            Vector3 to = new Vector3(i*scale, 0, zsize*scale);
            Gizmos.DrawLine(from, to);
        }
        for (int j = 0; j <= zsize; j++)
        {
            Vector3 from = new Vector3(0, 0, j*scale);
            Vector3 to = new Vector3(xsize*scale, 0, j * scale);
            Gizmos.DrawLine(from, to);
        }
    }
}
