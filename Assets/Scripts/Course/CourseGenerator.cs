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

	// Amount of holes to be generated
	public int holeCount = 1;

	public int groundWidth = 10;
	public int groundHeight = 1;
	public int wallThickness = 2;

	private Dictionary<int, LineRenderer> holesDict = new Dictionary<int, LineRenderer>();

	private void Awake ()
	{
		GenerateCourse();
	}

	public static LineRenderer GenerateHoleLayout (int holeNumber, Transform parent)
	{
		GameObject newHole = new GameObject("Hole " + (holeNumber + 1));
		newHole.transform.SetParent(parent);
		return newHole.AddComponent<LineRenderer>();
	}

	public void GenerateCourse ()
	{
		InitializeLayoutDictionairy();
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
				GenerateHoleLayout(i, transform);
			}

			holesDict.Add(i, lr);
		}
	}
}
