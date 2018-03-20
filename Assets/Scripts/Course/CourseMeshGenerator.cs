using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CourseMeshGenerator : MonoBehaviour {

	enum PositionInTrack
	{
		OUTER_WALL = 0,
		TOP_WALL = 1,
		INNER_WALL = 2,
		TRACK = 3,
	}

	private float m_groundLevel, m_trackWidth, m_wallThickness, m_wallHeight;
	private Vector3[] m_layout;

	Vector3[] m_vertices;
	Vector3[] m_normals;
	Vector4[] m_tangents;
	Vector2[] m_uv;

	private Mesh m_mesh;

	public void Initialize(float groundLevel, float trackWidth, float wallThickness, float wallHeight, Material[] materials, Vector3[] layout)
	{
		m_groundLevel = groundLevel;
		m_trackWidth = trackWidth;
		m_wallThickness = wallThickness;
		m_wallHeight = wallHeight;

		m_layout = layout;

		GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
		m_mesh.name = gameObject.name + " (Generated Mesh)";

		GetComponent<MeshRenderer>().materials = materials;

		//StartCoroutine(CreateVerticesInterval());

		CreateVertices();

		//StartCoroutine(CreateTrianglesInterval());

		CreateTriangles();

		CreateColliders();

		GetComponent<LineRenderer>().enabled = true;
	}


	private IEnumerator CreateVerticesInterval ()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		int verticesCount = 16 * (m_layout.Length - 1) + 8;

		m_vertices = new Vector3 [verticesCount];
		m_normals = new Vector3 [verticesCount];
		m_tangents = new Vector4 [verticesCount];

		int v = 0;
		//int ring = m_layout.Length * 4;

		// Outer vertices
		for (int y = 0; y < 2; y++)
		{
			// front
			for (int x = 0; x < 2; x++)
			{
				SetVertex(v++, x, y, 0, 0);
				Debug.Log(v + ". Front");
				m_mesh.vertices = m_vertices;
				yield return wait;
			}
			// right side
			for (int pos = 0; pos < m_layout.Length - 1; pos++)
			{
				for (int z = 0; z < 2; z++)
				{
					SetVertex(v++, 1, y, z, pos);
					Debug.Log(v + ". Right");
					m_mesh.vertices = m_vertices;
					yield return wait;
				}
			}
			// back
			for (int x = 1; x >= 0; x--)
			{
				SetVertex(v++, x, y, 1, m_layout.Length - 2);
				Debug.Log(v + ". Back");
				m_mesh.vertices = m_vertices;
				yield return wait;
			}
			//left side
			for (int pos = m_layout.Length - 1; pos > 0; pos--)
			{
				for (int z = 1; z >= 0; z--)
				{
					SetVertex(v++, 0, y, z, pos - 1);
					Debug.Log(v + ". Left");
					m_mesh.vertices = m_vertices;
					yield return wait;
				}
			}
		}


		m_mesh.vertices = m_vertices;
	}

	private void CreateVertices ()
	{
		int verticesCount = 28 * (m_layout.Length - 1) + 24;

		m_vertices = new Vector3 [verticesCount];
		m_normals = new Vector3 [verticesCount];
		m_tangents = new Vector4 [verticesCount];
		m_uv = new Vector2 [verticesCount];

		int v = 0;
		//int ring = m_layout.Length * 4;

		// Outer vertices
		for (int y = 0; y < 2; y++)
		{
			// front
			for (int x = 0; x < 2; x++)
			{
				m_uv [v] = new Vector2(x, y);
				SetVertex(v++, x, y, 0, 0);
			}
			// right side
			for (int pos = 0; pos < m_layout.Length - 1; pos++)
			{
				for (int z = 0; z < 2; z++)
				{
					m_uv [v] = new Vector2(z, y);
					SetVertex(v++, 1, y, z, pos);
				}
			}
			// back
			for (int x = 1; x >= 0; x--)
			{
				m_uv [v] = new Vector2(x, y);
				SetVertex(v++, x, y, 1, m_layout.Length - 2);
			}
			//left side
			for (int pos = m_layout.Length - 1; pos > 0; pos--)
			{
				for (int z = 1; z >= 0; z--)
				{
					m_uv [v] = new Vector2(z, y);
					SetVertex(v++, 0, y, z, pos - 1);
				}
			}
		}


		m_mesh.vertices = m_vertices;
		m_mesh.uv = m_uv;
	}

	private void SetVertex (int i, int x, int y, int z, int pos, PositionInTrack posInTrack = PositionInTrack.OUTER_WALL)
	{
		switch (posInTrack)
		{
		case PositionInTrack.TRACK: // Position vertices based on Track Height
			m_vertices [i] = new Vector3
			{
				y = Mathf.Lerp(
					Mathf.Lerp(m_layout [pos].y, m_layout [pos + 1].y, (float) z),
					Mathf.Lerp(m_layout [pos].y, m_layout [pos + 1].y, (float) z) + m_wallHeight,
					(float) y),

				x = Mathf.Lerp(m_layout [pos].x, m_layout [pos + 1].x, (float) z)
				- ((float) m_trackWidth / 2f - Mathf.Lerp(0, m_trackWidth, (float) x)),

				z = Mathf.Lerp(m_layout [pos].z, m_layout [pos + 1].z, (float) z)
			};
			break;

		case PositionInTrack.OUTER_WALL: // Position vertices based on offset from wall and ground level
		default:
			m_vertices [i] = new Vector3
			{
				y = Mathf.Lerp(m_groundLevel, Mathf.Lerp(m_layout [pos].y, m_layout [pos + 1].y, (float) z) + m_wallHeight, (float) y),

				x = Mathf.Lerp(m_layout [pos].x, m_layout [pos + 1].x, (float) z) -
				((float) m_trackWidth / 2f - Mathf.Lerp(0, m_trackWidth, (float) x))
			};

			// TODO MOVE WALL FURTHER OUT
			if (pos == 0 || pos == m_layout.Length - 2)
			{
				m_vertices [i].z = Mathf.Lerp(m_layout [pos].z, m_layout [pos + 1].z, (float) z);
				break;
			}
			m_vertices [i].z = Mathf.Lerp(m_layout [pos].z, m_layout [pos + 1].z, (float) z);
			break;
		}
		SmoothRotateVertexAroundPos(i, z, pos);
	}

	private void SmoothRotateVertexAroundPos (int i, int z, int pos)
	{
		Vector3 pivot = new Vector3();
		Vector3 rotation = new Vector3();

		if (z == 0 && pos > 0)
		{
			pivot = m_layout [pos];
			Vector3 prev = Vector3.Scale(m_layout [pos] - m_layout [pos - 1], new Vector3(1, 0, 1)),
				next = Vector3.Scale(m_layout [pos + 1] - m_layout [pos], new Vector3(1, 0, 1));
			rotation = (Quaternion.LookRotation(prev) * Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.FromToRotation(prev, next), 0.5f)).eulerAngles;
		}
		else if (z == 1 && pos + 1 < m_layout.Length - 1)
		{
			pivot = m_layout [pos + 1];
			Vector3 prev = Vector3.Scale(m_layout [pos + 1] - m_layout [pos], new Vector3(1, 0, 1)),
				next = Vector3.Scale(m_layout [pos + 2] - m_layout [pos + 1], new Vector3(1, 0, 1));
			rotation = (Quaternion.LookRotation(prev) * Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.FromToRotation(prev, next), 0.5f)).eulerAngles;
		}
		else
		{
			pivot = Vector3.Lerp(m_layout [pos], m_layout [pos + 1], (float) z);
			rotation = Vector3.Scale(m_layout [pos + 1] - m_layout [pos], new Vector3(1f, 0, 1f));
			rotation = Quaternion.LookRotation(rotation).eulerAngles;
		}
		m_vertices [i] = RotatePointAroundPivot(m_vertices [i], pivot, rotation);
	}

	private static Vector3 RotatePointAroundPivot (Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler(angles) * dir;
		point = dir + pivot;
		return point;
	}

	private IEnumerator CreateTrianglesInterval ()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		int [ ] trianglesXZ = new int [12 * m_layout.Length];
		int [ ] trianglesY = new int [12 * m_layout.Length];
		int tXZ = 0, v = 0;

		int ring = 4 * m_layout.Length;

		m_mesh.subMeshCount = 2;

		for (int pos = 0; pos < m_layout.Length * 2; pos++, v += 2)
		{
			yield return wait;
			tXZ = SetQuad(trianglesXZ, tXZ, v, v + 1, v + ring, v + ring + 1);
			CalculateNormalAndTangent(v, v + 1, v + ring, v + ring + 1);
			Debug.Log(v + ". Vert");
			m_mesh.SetTriangles(trianglesXZ, 0);
			
		}

		m_mesh.normals = m_normals;
		m_mesh.tangents = m_tangents;

		
		
		m_mesh.SetTriangles(trianglesY, 1);
	}

	private void CreateTriangles ()
	{
		int [ ] trianglesXZ = new int [12 * m_layout.Length];
		int [ ] trianglesY = new int [12 * m_layout.Length];
		int tXZ = 0, v = 0;

		int ring = 4 * m_layout.Length;

		for (int pos = 0; pos < m_layout.Length * 2; pos++, v += 2)
		{
			tXZ = SetQuad(trianglesXZ, tXZ, v, v + 1, v + ring, v + ring + 1);
			CalculateNormalAndTangent(v, v + 1, v + ring, v + ring + 1);
		}

		m_mesh.normals = m_normals;
		m_mesh.tangents = m_tangents;

		m_mesh.subMeshCount = 2;
		m_mesh.SetTriangles(trianglesXZ, 0);
		m_mesh.SetTriangles(trianglesY, 1);
	}

	private static int SetQuad (int [ ] triangles, int i, int v00, int v10, int v01, int v11)
	{

		triangles [i] = v00;
		triangles [i + 1] = triangles [i + 4] = v01;
		triangles [i + 2] = triangles [i + 3] = v10;
		triangles [i + 5] = v11;

		return i + 6;
	}

	private void CalculateNormalAndTangent (int a, int b, int c, int d)
	{
		Vector3 normal = Vector3.Cross(m_vertices [a] - m_vertices [b], m_vertices [c] - m_vertices [b]).normalized;
		Vector4 right = (m_vertices [b] - m_vertices [a]).normalized;
		right.w = -1f;
		m_normals [a] = m_normals [b] = m_normals [c] = m_normals [d] = normal;
		m_tangents [a] = m_tangents [b] = m_tangents [c] = m_tangents [d] = right;

	}

	private void CreateColliders ()
	{
		//throw new NotImplementedException();
	}

	private void OnDrawGizmos ()
	{
		if (m_vertices == null)
			return;
		for (int i = 0; i < m_vertices.Length; i++)
		{
			if (m_vertices [i] == Vector3.zero)
				continue;
			Gizmos.color = Color.black;
			Gizmos.DrawCube(m_vertices [i], new Vector3(.25f, .25f, .25f));
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(m_vertices [i], m_normals [i]);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(m_vertices [i], m_tangents [i]);
		}

		/*Gizmos.color = Color.green;
		Gizmos.DrawCube(m_vertices [0], new Vector3(.5f, .5f, .5f));
		Gizmos.DrawCube(m_vertices [1], new Vector3(.5f, .5f, .5f));
		Gizmos.DrawCube(m_vertices [20], new Vector3(.5f, .5f, .5f));
		Gizmos.DrawCube(m_vertices [21], new Vector3(.5f, .5f, .5f));
		*/
	}
}
