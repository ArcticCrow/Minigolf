using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour {

	public GameObject ball;
	public Vector3 forceMultiplier = new Vector3(1, 0, 1);
	//public float cameraDistance = 10f;

	LineRenderer forceLineRenderer;
	GolfBall golfBall;
	Vector3 mouseWorldPosition;

	private void Awake ()
	{
		if (ball == null)
			Debug.LogError("No ball is assigned to the player controller!");
	}

	// Use this for initialization
	void Start ()
	{
		golfBall = ball.GetComponent<GolfBall>();

		forceLineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!golfBall.isMoving())
		{
			if (Input.GetMouseButton(0))
			{
				DrawForceLine();
			}
			if (Input.GetMouseButtonUp(0))
			{
				golfBall.AddImpulse(Vector3.Scale((ball.transform.position - mouseWorldPosition), forceMultiplier));
				forceLineRenderer.positionCount = 0;
			}
		}
	}

	void DrawForceLine()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			mouseWorldPosition = hit.point;
			forceLineRenderer.positionCount = 2;
			forceLineRenderer.SetPosition(0, ball.transform.position);
			forceLineRenderer.SetPosition(1, ball.transform.position*2 - mouseWorldPosition);
			//Debug.Log(hit.point);
		}
	}
}
