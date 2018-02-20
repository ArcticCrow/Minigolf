using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HoleMeshGenerator : MonoBehaviour {

	private float scale = 2f;

	private int groundHeight;
    private int width;
	private int length;
    private LineRenderer layout;

	Vector3[] vertices;
	Vector3[] normals;

	private Mesh mesh;

    private int xSize, ySize, zSize;

    public void Initialize(int groundLevel, int trackWidth, int segmentLength)
    {
        Debug.Log("groundLevel: " + groundLevel);
        groundHeight = groundLevel;
		width = trackWidth;
		length = segmentLength;
        layout = gameObject.GetComponent<LineRenderer>();
		//layout.enabled = false;

        CreateVertices();
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

	private void CreateVertices ()
	{
		xSize = 1;
		ySize = 1;
		zSize = 1;

		Vector3 [ ] dirs = new Vector3 [layout.positionCount];
		for (int i = 0; i < dirs.Length; i++)
		{
			if (i == 0)
			{
				dirs [i] = (layout.GetPosition(i + 1) - layout.GetPosition(i)).normalized;
			} else if (i == dirs.Length - 1)
			{
				dirs [i] = (layout.GetPosition(i) - layout.GetPosition(i - 1)).normalized;
			} else
			{
				Quaternion lastDir = Quaternion.Euler(layout.GetPosition(i) - layout.GetPosition(i - 1));
				Quaternion nextDir = Quaternion.Euler(layout.GetPosition(i + 1) - layout.GetPosition(i));
				dirs [i] = Quaternion.Lerp(lastDir, nextDir, 0.5f).eulerAngles.normalized;
			}
			Debug.Log(i + ". Direction: " + dirs [i]);
		}


		/* Calculating corner vertices
         * A cube has 8 corners (vertices)
         * + 4 corners for every extra section (corners, obstacles)
         * = 8 [base] + 4 * points [4 for each section]
         * */
		int cornerVertices = 8 + 4 * layout.positionCount;

		/* Calculating edge verticies
         * A cube has 4 time 3 edges each requireing x/y/z vertices depending on their local direction
         * - 3 for each already added corner vertice
         * + 1 x edge
         * + 4 z edges
         * + 2 y edges
         * = 4 * (xSize + ySize + zSize - 3) [base cube] + points * (2 * (2 * zSize + ySize - 3)  + xSize - 1)
         * */
		int edgeVertices = 4 * (xSize + ySize + zSize - 3) + layout.positionCount * (2 * (2 * zSize + ySize - 3) + xSize - 1);

		/* Calculating face vertices
		 * A cube has 2 times x - 1 * y - 1 for the start and end faces
		 * + z - 1 * y - 1 face * position count + 1 for all the side faces
		 * + x - 1 * z - 1 face * position count + 1 for all the top surfaces
		 * = 2 * (xSize - 1) * (ySize - 1) + (positions + 1) * ((ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1))
		 * */
		int faceVertices = 2 * (xSize - 1) * (ySize - 1) +
			(layout.positionCount + 1) * (
			(xSize - 1) * (zSize - 1) +
			(zSize - 1) * (ySize - 1));
		vertices = new Vector3 [cornerVertices + edgeVertices + faceVertices];
		normals = new Vector3 [vertices.Length];
		Debug.Log("corner verts: " + cornerVertices + "; edge verts: " + edgeVertices + "; face verts: " + faceVertices + "; total verts: " + vertices.Length);

		int v = 0;

		for (int y = 0; y <= ySize; y++)
		{
			for (int i = 0; i < layout.positionCount; i++)
			{
				Vector3 offset = layout.GetPosition(i) + new Vector3(-width / 2f * xSize, 0, -length / 2f * ySize) * scale;
				if (i == layout.positionCount - 1)
				{
					offset.z -= length * zSize;
				}

				if (i == 0)//|| y == ySize)
				{
					for (int x = 0; x <= xSize; x++)
					{
						SetVertex(v++, x * width, y, 0, offset, dirs [i]);
					}
					offset.z += length * zSize;
				}
				for (int z = 1; z <= zSize; z++)
				{
					SetVertex(v++, xSize * width, y, z, offset, dirs [i]);
				}

				if (i == layout.positionCount - 1)// || (i == 0 && y == ySize))
				{
					offset.z += length * zSize;
					for (int x = xSize; x >= 0; x--)
					{
						SetVertex(v++, x * width, y, zSize * length, offset, dirs [i]);
					}
				}
			}
		}
		for (int y = 0; y <= ySize; y++)
		{
			for (int i = layout.positionCount - 1; i >= 0; i--)
			{
				Vector3 offset = layout.GetPosition(i) + new Vector3(-width / 2f * xSize, 0, -length / 2f * ySize) * scale;
				for (int z = zSize; z >= 0; z--)
				{
					SetVertex(v++, 0, y, z, offset, dirs[i]);
				}
			}
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void SetVertex (int i, int x, int y, int z, Vector3 offset, Vector3 dir)
	{
		Vector3 inner = vertices [i] = new Vector3(x, y, z) * scale + offset;

		normals [i] = (vertices [i] - inner).normalized;
		vertices [i] = inner;

		Debug.Log(i + ". vertex set: " + vertices [i] + "; normal: " + normals [i]);
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
