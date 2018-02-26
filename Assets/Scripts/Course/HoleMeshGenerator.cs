using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HoleMeshGenerator : MonoBehaviour {

	int xSize = 1, ySize = 1, zSize = 1;

	private int groundHeight;
    private int width;
    private LineRenderer layout;

	Vector3[] vertices;
	Vector3[] normals;

	private Mesh mesh;

    public void Initialize(int groundLevel, int trackWidth)
    {
        groundHeight = groundLevel;
		width = trackWidth;
        layout = gameObject.GetComponent<LineRenderer>();

		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		StartCoroutine(
			CreateVertices()
		);
        CreateTriangles();
        CreateColliders();
    }

    private void CreateColliders()
    {
        //throw new NotImplementedException();
    }

    private void CreateTriangles()
    {
        //throw new NotImplementedException();
    }

	private IEnumerator CreateVertices ()
	{
		WaitForSeconds wait = new WaitForSeconds(.05f);

		//Vector3 [ ] dirs = new Vector3 [layout.positionCount];
		//for (int i = 0; i < dirs.Length; i++)
		//{
		//	if (i == 0)
		//	{
		//		dirs [i] = (layout.GetPosition(i + 1) - layout.GetPosition(i)).normalized;
		//	}
		//	else if (i == dirs.Length - 1)
		//	{
		//		dirs [i] = (layout.GetPosition(i) - layout.GetPosition(i - 1)).normalized;
		//	}
		//	else
		//	{
		//		Quaternion lastDir = Quaternion.Euler(layout.GetPosition(i) - layout.GetPosition(i - 1));
		//		Quaternion nextDir = Quaternion.Euler(layout.GetPosition(i + 1) - layout.GetPosition(i));
		//		dirs [i] = Quaternion.Lerp(lastDir, nextDir, 0.5f).eulerAngles.normalized;
		//	}
		//	//Debug.Log(i + ". Direction: " + dirs [i]);
		//}

		int cornerVertices = 4 * layout.positionCount;
		int edgeVertices = layout.positionCount * ((ySize - 1) * 2 + xSize - 1) +
			4 * (layout.positionCount - 1) * (zSize - 1) +
			(xSize - 1) * 2;
		int faceVertices = (layout.positionCount - 1) * (2 * (ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1)) +
			2 * (xSize - 1) * (ySize - 1);
		vertices = new Vector3 [cornerVertices + edgeVertices + faceVertices];
		normals = new Vector3 [vertices.Length];
		Debug.Log("corner verts: " + cornerVertices + "; edge verts: " + edgeVertices + "; face verts: " + faceVertices + "; total verts: " + vertices.Length);


		int v = 0;

		// Set wall face, edge and corner vertices
		for (int y = 0; y <= ySize; y++)
		{
			for (int x = 0; x <= xSize; x++)
			{
				SetVertex(v++, x, y, 0, 0, 1);
				yield return wait;

			}
			for (int pos = 0; pos < layout.positionCount - 1; pos++)
			{
				Debug.Log("Position " + pos);
				for (int z = 1; z <= zSize; z++)
				{
					SetVertex(v++, xSize, y, z, pos, pos + 1);
					yield return wait;
				}
			}

			for (int x = xSize - 1; x >= 0; x--)
			{
				SetVertex(v++, x, y, zSize, layout.positionCount - 2, layout.positionCount - 1);
				yield return wait;

			}
			for (int pos = layout.positionCount - 1; pos > 0; pos--)
			{
				Debug.Log("Position " + pos + " (reverse); next pos:" + (pos - 1));
				for (int z = zSize - 1; z >= 0; z--)
				{
					if (pos > 1 || z != 0)
					{
						SetVertex(v++, 0, y, z, pos - 1, pos);
					}
					yield return wait;

				}
			}
		}

		// Set top face vertices
		for (int pos = 0; pos < layout.positionCount - 1; pos++)
		{
			for (int z = 1; z <= zSize; z++)
			{
				if (z == zSize && pos == layout.positionCount - 2)
					break;
				for (int x = 1; x < xSize; x++)
				{
					SetVertex(v++, x, ySize, z, pos, pos + 1);
					yield return wait;

				}
			}
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void SetVertex (int i, int x, int y, int z, int pos, int nextPos)
	{
		vertices [i] = new Vector3
		{
			y = Mathf.Lerp(groundHeight, Mathf.Lerp(layout.GetPosition(pos).y, layout.GetPosition(nextPos).y, (float) z / (float) zSize), (float) y / (float) ySize),

			x = Mathf.Lerp(layout.GetPosition(pos).x, layout.GetPosition(nextPos).x, (float) z / (float) zSize) -
				((float) width / 2f - Mathf.Lerp(0, width, (float) x / (float) xSize)),

			z = Mathf.Lerp(layout.GetPosition(pos).z, layout.GetPosition(nextPos).z, (float) z / (float) zSize)
		};

		normals [i] = new Vector3(x,y,z).normalized;

		Debug.Log((i + 1) + ". vertex set: " + vertices [i]);// + "; normal: " + normals [i]);
		//cubeUV [i] = new Color32((byte) x, (byte) y, (byte) z, 0);
	}

	private void OnDrawGizmos ()
	{
		if (vertices == null)
			return;
		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawCube(vertices [i], Vector3.one);
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(vertices [i], normals [i]);
		}
	}
}
