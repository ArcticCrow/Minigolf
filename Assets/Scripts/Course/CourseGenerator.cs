using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseGenerator : MonoBehaviour {

	public int numberOfHoles = 2;	// How many holes are supposed to be generated
	public Vector3 maxAreaPerHole = new Vector3(10, 5, 10);	// How much area does one hole take
	public int basePointAmount = 3; // How many line segments is the generator supposed to start of with
	public float horizontalOffset = 5f;	// Horizontal offset between each area

	[Range(1f, 1.5f)]
	public float difficultyIncreasePower = 1.1f;	// The power that determines the increase of line segements per hole

	public bool useRandomSeed = true;	// Should the randomizer use a random or a given seed?
	public string seed = "";    // The seed to initialize the randomizer with


	[SerializeField]
	Dictionary<int, Mesh> courseMesh;

	[SerializeField]
	List<LineRenderer> lineList;

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

		// Init line renderers
		GenerateLineRenderers();

		// Iterate over position

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
		lineList = new List<LineRenderer>();
		for (int i = 0; i < numberOfHoles; i++)
		{
			GameObject newHole = new GameObject("Hole " + (i+1));
			newHole = Instantiate(newHole, transform);
			// Set and increment the number of points in line for hole i
			int points = currentPointAmount;
			currentPointAmount = Mathf.CeilToInt(Mathf.Pow(currentPointAmount, difficultyIncreasePower));
			//Debug.Log(i + ":" + segments + "->" + currentSegmentAmount);

			LineRenderer newLine = newHole.AddComponent<LineRenderer>();
			newLine.positionCount = points;

			for (int j = 0; j < points; j++)
			{
				Vector3 point = GetRandomVectorInArea(i);
				newLine.SetPosition(j, point);
			}

			lineList.Add(newLine);
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
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1))
		{
			Start();
		}
	}
}
