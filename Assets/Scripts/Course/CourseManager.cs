using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LineRendererHole
{
	public int holeNumber;
	public LineRenderer lineRenderer;
}


public class CourseManager : MonoBehaviour {

	// Takes a list of Line Renderers
	public LineRendererHole[] holeLayouts;

	public Material groundMaterial;
	public Material wallMaterial;

	// Amount of holes to be generated
	public int holeCount = 1;

	public float courseWidth = 2;
	public float groundHeight = 0;
	public float wallHeight = .5f;
	public float wallThickness = .5f;

	private Dictionary<int, LineRenderer> m_holesDict = new Dictionary<int, LineRenderer>();
	private Dictionary<int, Vector3[]> m_layoutDict = new Dictionary<int, Vector3[]>();

	private CapsuleCollider m_goalCol;
	private GameObject startPoint;

	private int currentHole = 0;

	private void Awake ()
	{
		GenerateCourse();
	}

    public void GenerateCourse()
    {
        InitializeLayoutDictionairy();
        GenerateMeshes();

		SetActiveHole(currentHole);
    }

    private void GenerateMeshes()
    {
		Material [ ] materials = new Material [ ]
		{
			wallMaterial,
			groundMaterial
		};
        for (int i = 0; i < m_holesDict.Count; i++)
        {
            Debug.Log("Has game object? " + m_holesDict[i]);
            m_holesDict[i].gameObject.AddComponent<CourseMeshGenerator>().Initialize(groundHeight, courseWidth, wallThickness, wallHeight, materials, m_layoutDict[i]);
        }
    }

    public static LineRenderer GenerateHoleLayout (int holeNumber, Transform parent)
	{
		GameObject newHole = new GameObject("Hole " + (holeNumber + 1));
		newHole.transform.SetParent(parent);
		return newHole.AddComponent<LineRenderer>();
	}

	private void InitializeLayoutDictionairy ()
	{
		m_holesDict = new Dictionary<int, LineRenderer>(holeCount);
		for (int i = 0; i < holeCount; i++)
		{
			LineRenderer lr = null;

			// Translate existing to dictionairy
			for (int j = 0; j < holeLayouts.Length; j++)
			{
				if (holeLayouts [j].holeNumber == i)
				{
					lr = holeLayouts [j].lineRenderer;
					break;
				}
			}
			if (lr == null)
			{
				Debug.LogWarning("Hole layout for number " + (i + 1) + " is missing and will be generated!");
                lr = GenerateHoleLayout(i, transform);
			}

			m_holesDict.Add(i, lr);

			Vector3 [ ] layout = new Vector3 [lr.positionCount];
			for (int j = 0; j < lr.positionCount; j++)
			{
				layout [j] = lr.GetPosition(j);
			}
			m_layoutDict.Add(i, layout);
		}
	}

	private void SetActiveHole (int holeID)
	{
		Vector3 [ ] layout = m_layoutDict [holeID];
		if (m_goalCol == null)
		{
			m_goalCol = gameObject.AddComponent<CapsuleCollider>();
			m_goalCol.isTrigger = true;
			m_goalCol.tag = "Finish";
		}
		m_goalCol.center = layout[layout.Length - 1];
		m_goalCol.radius = 1f;
		m_goalCol.height = 2f;

		if (startPoint == null)
		{
			startPoint = new GameObject("Current Spawnpoint");
			startPoint.transform.SetParent(transform);
			startPoint.AddComponent<NetworkStartPosition>();
		}

		startPoint.transform.position = layout [0] + Vector3.up;
	}

	private void OnTriggerEnter (Collider other)
	{
		Debug.Log("Collision!");

		if (other.gameObject.CompareTag("Player"))
		{
			Debug.Log("The player touched the goal");

			currentHole++;

			SetActiveHole(currentHole);

			other.transform.position = startPoint.transform.position;
			other.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}
}
