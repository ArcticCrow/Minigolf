using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshAnalyzer : MonoBehaviour {

    private Vector3[] m_vertices;
    private Vector3[] m_normals;
    private Vector4[] m_tangents;

    private Mesh m_mesh;

    // Use this for initialization
    void Awake () {
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_vertices = m_mesh.vertices;
        m_normals = m_mesh.normals;
        m_tangents = m_mesh.tangents;
        Debug.Log(m_mesh.name + " analyzer init; " + m_vertices.Length + "; " + m_normals.Length + "; " + m_tangents.Length);
	}

    void OnDrawGizmos()
    {
        if (m_vertices == null)
        {
            return;
        }
        for (int i = 0; i < m_vertices.Length; i++)
        {
            //Debug.Log(i + ". drawing Gizmos");
            if (m_vertices[i] == Vector3.zero)
                continue;
            Gizmos.color = Color.black;
            Gizmos.DrawCube(m_vertices[i], new Vector3(.01f, .01f, .01f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(m_vertices[i], m_normals[i]);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(m_vertices[i], m_tangents[i]);
        }
    }
}
