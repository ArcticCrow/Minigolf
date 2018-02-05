﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class CourseGenerator : MonoBehaviour {

	public int numberOfHoles = 2;	// How many holes are supposed to be generated
	public Vector3 maxAreaPerHole = new Vector3(10, 5, 10);	// How much area does one hole take
	public int basePointAmount = 3; // How many line segments is the generator supposed to start of with
	public float horizontalOffset = 5f;	// Horizontal offset between each area

	[Range(1f, 1.5f)]
	public float difficultyIncreasePower = 1.1f;	// The power that determines the increase of line segements per hole

	public bool useRandomSeed = true;	// Should the randomizer use a random or a given seed?
	public string seed = "";    // The seed to initialize the randomizer with

	public bool areLinesVisible = true;

	public Material groundMaterial;
	public Material lineMaterial;


	[SerializeField]
	List<GameObject> holeList;

	[SerializeField]
	Vector3[] totalAreaBounds;

	[SerializeField]
	int currentPointAmount;

	// Use this for initialization
	void Start () {
		if (useRandomSeed)
		{
			seed = System.DateTime.Now.Ticks.ToString();
		}
		Random.InitState(seed.GetHashCode());

		currentPointAmount = basePointAmount;

		// Calculate CLOSE BOTTOM LEFT and FAR TOP RIGHT corner of total area
		CalculateTotalArea();

		// Create line renderers
		GenerateLineRenderers();

		// Create mesh for each line
		GenerateHoleMeshes();

		// Customize mesh appearence/collision
		AddMeshRenderers();
		AddMeshColliders();
	}

	private void AddMeshColliders ()
	{
		for (int i = 0; i < holeList.Count; i++)
		{
			GameObject hole = holeList [i];
			MeshCollider mc = hole.AddComponent<MeshCollider>();
		}
	}

	private void AddMeshRenderers ()
	{
		for (int i = 0; i < holeList.Count; i++)
		{
			GameObject hole = holeList [i];
			MeshRenderer mr = hole.AddComponent<MeshRenderer>();
			mr.material = groundMaterial;
		}
	}

	private void GenerateHoleMeshes ()
	{
		float trackPadding = 100f;

		for (int i = 0; i < holeList.Count; i++)
		{
			GameObject hole = holeList [i];
			MeshFilter mf = hole.AddComponent<MeshFilter>();
			Mesh newMesh = new Mesh();

			LineRenderer lr = hole.GetComponent<LineRenderer>();
			lr.widthMultiplier = trackPadding;
			lr.startColor = Color.blue;
			lr.endColor = Color.magenta;

			// Debugging
			GameObject dbo1 = new GameObject("Left Border");
			LineRenderer dbl1 = dbo1.AddComponent<LineRenderer>();
			dbo1.transform.SetParent(hole.transform);

			dbl1.positionCount = lr.positionCount * 2;
			dbl1.startColor = Color.green;
			dbl1.endColor = Color.red;
			dbl1.material = lineMaterial;
			dbl1.widthMultiplier = trackPadding/2f;


			GameObject dbo2 = new GameObject("Right Border");
			LineRenderer dbl2 = dbo2.AddComponent<LineRenderer>();
			dbo2.transform.SetParent(hole.transform);

			dbl2.positionCount = lr.positionCount * 2;
			dbl2.startColor = Color.cyan;
			dbl2.endColor = Color.yellow;
			dbl2.material = lineMaterial;
			dbl2.widthMultiplier = trackPadding/2f;

			// Setup line renderers
			for (int j = 0; j < lr.positionCount; j++)
			{
				Quaternion dir;
				if (j == lr.positionCount - 1)
				{
					
					dir = Quaternion.LookRotation(lr.GetPosition(j-1) - lr.GetPosition(j));
				}
				else if (j == 0)
				{
					dir = Quaternion.LookRotation(lr.GetPosition(j) - lr.GetPosition(j + 1));
				} else
				{
					Quaternion dirIn = Quaternion.LookRotation(lr.GetPosition(j) - lr.GetPosition(j + 1));
					Quaternion dirOut = Quaternion.LookRotation(lr.GetPosition(j - 1) - lr.GetPosition(j));
					dir = Quaternion.Slerp(dirIn, dirOut, 0.5f);
				}


				//Debug.Log(j + ":" + dir.eulerAngles);

				Vector3 bottomLeft, bottomRight, topLeft, topRight;
				bottomLeft = dir * new Vector3(-trackPadding, 0, -trackPadding) + lr.GetPosition(j);
				bottomRight = dir * new Vector3(trackPadding, 0, -trackPadding) + lr.GetPosition(j);
				topLeft = dir * new Vector3(-trackPadding, 0, trackPadding) + lr.GetPosition(j);
				topRight = dir * new Vector3(trackPadding, 0, trackPadding) + lr.GetPosition(j);

				dbl1.SetPosition(j*2, topLeft);
				dbl2.SetPosition(j*2, topRight);
				dbl1.SetPosition(j * 2 + 1, bottomLeft);
				dbl2.SetPosition(j * 2 + 1, bottomRight);

				//Debug.Log(j + ":(bl)" + bottomLeft + ";(br)" + bottomRight + ";(tl)" + topLeft + ";(tr)" + topRight);
			}

			// Create hole mesh
			int totalVertexCount = dbl1.positionCount + dbl2.positionCount;
			int vertexIndex = 0;
			Vector3 [ ] vertices = new Vector3 [totalVertexCount];
			Vector3 [ ] normals = new Vector3 [totalVertexCount];
			Vector2 [ ] uv = new Vector2 [totalVertexCount];

			int triIndex = 0;
			int [ ] tri = new int [Mathf.RoundToInt((float)totalVertexCount*3f - 6f)];

			for (int j = 0; j < dbl1.positionCount - 1; j++)
			{
				Vector3 topLeft = dbl1.GetPosition(j);
				Vector3 topRight = dbl2.GetPosition(j);
				Vector3 bottomLeft = dbl1.GetPosition(j+1);
				Vector3 bottomRight = dbl2.GetPosition(j+1);
				Vector3 up = -Vector3.up;

				vertexIndex = j * 2;
				triIndex = j * 6;

				Debug.Log(j + ":" + triIndex + " of " + Mathf.RoundToInt((float) totalVertexCount * 3f - 7f));

				// Vertices for track part j
				vertices [vertexIndex + 0] = bottomLeft;
				vertices [vertexIndex + 1] = bottomRight;
				vertices [vertexIndex + 2] = topLeft;
				vertices [vertexIndex + 3] = topRight;

				// Normals for vertices of track part j
				// FIXME wrong normals
				normals [vertexIndex + 0] = -up;
				normals [vertexIndex + 1] = -up;
				normals [vertexIndex + 2] = -up;
				normals [vertexIndex + 3] = -up;

				// UV Mapping for vertices of track part j
				uv [vertexIndex + 0] = new Vector2(0, 0);
				uv [vertexIndex + 1] = new Vector2(1, 0);
				uv [vertexIndex + 2] = new Vector2(0, 1);
				uv [vertexIndex + 3] = new Vector2(1, 1);

				// Triangles for track part j (clockwise)
				tri [triIndex + 0] = vertexIndex + 0;
				tri [triIndex + 1] = vertexIndex + 2;
				tri [triIndex + 2] = vertexIndex + 1;
				tri [triIndex + 3] = vertexIndex + 2;
				tri [triIndex + 4] = vertexIndex + 3;
				tri [triIndex + 5] = vertexIndex + 1;
			}

			newMesh.vertices = vertices;
			newMesh.triangles = tri;
			newMesh.normals = normals;
			newMesh.uv = uv;

			mf.mesh = newMesh;

			lr.enabled = areLinesVisible;
			dbl1.enabled = areLinesVisible;
			dbl2.enabled = areLinesVisible;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown(1))
		{
			ClearExistingCourse();
			Start();
		}
	}

	private void ClearExistingCourse ()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}

	void CalculateTotalArea()
	{
		Vector3 pos = transform.position;
		float xOffset = maxAreaPerHole.x * numberOfHoles / 2f + (horizontalOffset * (numberOfHoles / 2f - 1f)) + horizontalOffset/2f;
		float zOffset = maxAreaPerHole.z/2f;

		Vector3 closeBottomLeftCorner = new Vector3(pos.x - xOffset, pos.y, pos.z - zOffset);
		Vector3 farTopRightCorner = new Vector3(pos.x + xOffset, pos.y, pos.z + zOffset);

		totalAreaBounds = new Vector3 [2] {closeBottomLeftCorner, farTopRightCorner};
	}

	void GenerateLineRenderers()
	{

		// TODO Restrict angles for left/right and up/down
		// TODO Set minimum/maximum offset between each point
		holeList = new List<GameObject>();
		for (int i = 0; i < numberOfHoles; i++)
		{
			GameObject newHole = new GameObject("Hole " + (i+1));
			newHole.transform.SetParent(transform);
			//newHole.transform.position = GetAreaStart(i) + Vector3.Scale(maxAreaPerHole, new Vector3(0.5f, 1, 0.5f));

			// Set and increment the number of points in line for hole i
			int points = currentPointAmount;
			currentPointAmount = Mathf.CeilToInt(Mathf.Pow(currentPointAmount, difficultyIncreasePower));
			//Debug.Log(i + ":" + segments + "->" + currentSegmentAmount);

			LineRenderer newLine = newHole.AddComponent<LineRenderer>();
			newLine.positionCount = points;
			newLine.material = lineMaterial;

			for (int j = 0; j < points; j++)
			{
				Vector3 point = GetRandomVectorInArea(i);
				newLine.SetPosition(j, point);
			}

			holeList.Add(newHole);
		}
	}
	
	Vector3 GetRandomVectorInArea(int areaIndex)
	{
		// CLOSE BOTTOM LEFT corner
		Vector3 areaStart = GetAreaStart(areaIndex);
		//Debug.Log(areaIndex + ":" + areaStart.ToString());

		float x = Mathf.RoundToInt(Random.Range(0, maxAreaPerHole.x));
		float y = Mathf.RoundToInt(Random.Range(0, maxAreaPerHole.y));
		float z = Mathf.RoundToInt(Random.Range(0, maxAreaPerHole.z));

		Vector3 randomVector = new Vector3(areaStart.x + x, areaStart.y + y, areaStart.z + z);
		//Debug.Log(areaIndex + ":" + areaStart.ToString() + "->" + randomVector.ToString());

		return randomVector;
	}

	Vector3 GetAreaStart(int areaIndex)
	{
		Vector3 areaStart = totalAreaBounds [0] + new Vector3(maxAreaPerHole.x + horizontalOffset, 0, 0) * areaIndex;

		return areaStart;
	}
}
