using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(LineRenderer))]
public class PlayerController : NetworkBehaviour {

	public Vector3 forceMultiplier = new Vector3(1, 0, 1);

	public float maxForce = 10f;
	public float maxVelocity = 100f;

	public Canvas PlayerUI;
	public Image ForceMeterDisplay;
	public float maxLineDistance = 10f;

	private Rigidbody rb;

	float startMouseY, releaseMouseY;

	float appliedForce;

	public override void OnStartLocalPlayer ()
	{
		GetComponent<Renderer>().material.SetColor("_Color", Color.blue);

		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = maxVelocity;

		// enable player ui
		PlayerUI.enabled = true;
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
		{
			return;
		}

		if (!IsMoving())
		{
			if (Input.GetMouseButton(0))
			{
				releaseMouseY = Input.mousePosition.y;
				CalculateAppliedForce();
				DrawUILine();
			}
			if (Input.GetMouseButtonDown(0))
			{
				startMouseY = Input.mousePosition.y;
			}
			if (Input.GetMouseButtonUp(0))
			{
				ApplyHit();
			}
		}
	}

	void CalculateAppliedForce ()
	{
		float maxForceDistance = (float) Screen.height * .5f;
		float distance = startMouseY - releaseMouseY;

		appliedForce = Mathf.Clamp(distance / maxForceDistance, 0f, 1f) * maxForce;
	}

	void ApplyHit()
	{
		if (appliedForce > 0)
		{
			AddImpulse(Vector3.Scale(forceMultiplier, Camera.main.transform.forward) * appliedForce * rb.mass);
		} else
		{

		}
	}

	public void AddImpulse (Vector3 force)
	{
		rb.AddForce(force, ForceMode.Impulse);
	}

	public bool IsMoving ()
	{
		return (rb.velocity.magnitude > 0);
	}

	void DrawUILine()
	{
		ForceMeterDisplay.fillAmount = appliedForce / maxForce;
	}
}
