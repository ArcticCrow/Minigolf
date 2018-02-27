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

		CreateVertices();
		CreateTriangles();
        CreateColliders();
    }

    private void CreateColliders()
    {
        //throw new NotImplementedException();
    }

	private void CreateVertices ()
	{
		WaitForSeconds wait = new WaitForSeconds(.05f);

		int cornerVertices = 4 * layout.positionCount;
		int edgeVertices = layout.positionCount * ((ySize - 1) * 2 + xSize - 1) +
			4 * (layout.positionCount - 1) * (zSize - 1) +
			(xSize - 1) * 2;
		int faceVertices = (layout.positionCount - 1) * (2 * (ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1)) +
			2 * (xSize - 1) * (ySize - 1);
		vertices = new Vector3 [cornerVertices + edgeVertices + faceVertices];
		normals = new Vector3 [vertices.Length];
		//Debug.Log("corner verts: " + cornerVertices + "; edge verts: " + edgeVertices + "; face verts: " + faceVertices + "; total verts: " + vertices.Length);


		int v = 0;

		// Set wall face, edge and corner vertices
		for (int y = 0; y <= ySize; y++)
		{
			for (int x = 0; x <= xSize; x++)
			{
				SetVertex(v++, x, y, 0, 0, 1);

			}
			for (int pos = 0; pos < layout.positionCount - 1; pos++)
			{
				//Debug.Log("Position " + pos);
				for (int z = 1; z <= zSize; z++)
				{
					SetVertex(v++, xSize, y, z, pos, pos + 1);
				}
			}

			for (int x = xSize - 1; x >= 0; x--)
			{
				SetVertex(v++, x, y, zSize, layout.positionCount - 2, layout.positionCount - 1);

			}
			for (int pos = layout.positionCount - 1; pos > 0; pos--)
			{
				//Debug.Log("Position " + pos + " (reverse)");
				for (int z = zSize - 1; z >= 0; z--)
				{
					if (pos > 1 || z != 0)
					{
						SetVertex(v++, 0, y, z, pos - 1, pos);
					}

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

		Vector3 pivot = new Vector3();
		Vector3 rotation = new Vector3();

		if (z == 0 && pos > 0)
		{
			pivot = layout.GetPosition(pos);
			Vector3 prev = Vector3.Scale(layout.GetPosition(pos) - layout.GetPosition(pos - 1), new Vector3(1, 0, 1)), 
				next = Vector3.Scale(layout.GetPosition(nextPos) - layout.GetPosition(pos), new Vector3(1, 0, 1));
			rotation = (Quaternion.LookRotation(prev) * Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.FromToRotation(prev, next), 0.5f)).eulerAngles;
		}
		else if (z == zSize && nextPos < layout.positionCount - 1)
		{
			pivot = layout.GetPosition(nextPos);
			Vector3 prev = Vector3.Scale(layout.GetPosition(nextPos) - layout.GetPosition(pos), new Vector3(1, 0, 1)),
				next = Vector3.Scale(layout.GetPosition(nextPos + 1) - layout.GetPosition(nextPos), new Vector3(1, 0, 1));
			rotation = (Quaternion.LookRotation(prev) * Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.FromToRotation(prev, next), 0.5f)).eulerAngles;
		}
		else
		{
			pivot = Vector3.Lerp(layout.GetPosition(pos), layout.GetPosition(nextPos), (float) z / (float) zSize);
			rotation = Vector3.Scale(layout.GetPosition(nextPos) - layout.GetPosition(pos), new Vector3(1, 0, 1));
			rotation = Quaternion.LookRotation(rotation).eulerAngles;
		}
		vertices [i] = RotatePointAroundPivot(vertices [i], pivot, rotation);

		normals [i] = new Vector3();

		//Debug.Log((i + 1) + ". vertex set: " + vertices [i] + "; pivot: " + pivot + " with rotation " + rotation);// + "; normal: " + normals [i]);
		//cubeUV [i] = new Color32((byte) x, (byte) y, (byte) z, 0);
	}

	private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler(angles) * dir;
		point = dir + pivot;
		return point;
	}

	private void CreateTriangles ()
	{
		WaitForSeconds wait = new WaitForSeconds(.5f);

		int [ ] trianglesXZ = new int [(xSize * ySize) * 12 + (zSize * ySize * layout.positionCount) * 12];
		int [ ] trianglesY = new int [(xSize * zSize * layout.positionCount) * 6];

		int ring = 2 * (xSize + zSize * (layout.positionCount - 1));

		int tXZ = 0, tY = 0, v = 0;


		for (int y = 0; y < ySize; y++, v++)
		{
			for (int q = 0; q < ring - 1; q++, v++)
			{
				tXZ = SetQuad(trianglesXZ, tXZ, v, v + 1, v + ring, v + ring + 1);
				mesh.SetTriangles(trianglesXZ, 0);
			}
			tXZ = SetQuad(trianglesXZ, tXZ, v, v - ring + 1, v + ring, v + 1);
		}


		mesh.subMeshCount = 2;
		mesh.SetTriangles(trianglesXZ, 0);
		mesh.SetTriangles(trianglesY, 1);
	}

	private int CreateTopFace (int [ ] triangles, int t, int ring)
	{
		int v = ring * ySize;
		for (int x = 0; x < xSize - 1; x++, v++)
		{
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (ySize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
		{
			// Set the first top face quad in a row
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
			// Set the center top face quads in a row
			for (int x = 1; x < xSize - 1; x++, vMid++)
			{
				t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
			}
			// Set the last top face quad in a row
			t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	private static int SetQuad (int [ ] triangles, int i, int v00, int v10, int v01, int v11)
	{
		triangles [i] = v00;
		triangles [i + 1] = triangles [i + 4] = v01;
		triangles [i + 2] = triangles [i + 3] = v10;
		triangles [i + 5] = v11;

		return i + 6;
	}

	private void OnDrawGizmos ()
	{
		if (vertices == null)
			return;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices [i] == Vector3.zero)
				continue;
			Gizmos.color = Color.black;
			Gizmos.DrawCube(vertices [i], Vector3.one);
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(vertices [i], normals [i]);
		}
	}
}
