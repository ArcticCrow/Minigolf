using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineRendererHole
{
	public int holeNumber;
	public LineRenderer lineRenderer;
}


public class CourseGenerator : MonoBehaviour {

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

	private Dictionary<int, LineRenderer> holesDict = new Dictionary<int, LineRenderer>();
	private Dictionary<int, Vector3[]> layoutDict = new Dictionary<int, Vector3[]>();

	private void Awake ()
	{
		GenerateCourse();
	}

    public void GenerateCourse()
    {
        InitializeLayoutDictionairy();
        GenerateMeshes();
    }

    private void GenerateMeshes()
    {
		Material [ ] materials = new Material [ ]
		{
			wallMaterial,
			groundMaterial
		};
        for (int i = 0; i < holesDict.Count; i++)
        {
            Debug.Log("Has game object? " + holesDict[i]);
            holesDict[i].gameObject.AddComponent<CourseMeshGenerator>().Initialize(groundHeight, courseWidth, wallThickness, wallHeight, materials, layoutDict[i]);
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
		holesDict = new Dictionary<int, LineRenderer>(holeCount);
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

			holesDict.Add(i, lr);

			Vector3 [ ] layout = new Vector3 [lr.positionCount];
			for (int j = 0; j < lr.positionCount; j++)
			{
				layout [j] = lr.GetPosition(j);
			}
			layoutDict.Add(i, layout);
		}
	}
}
