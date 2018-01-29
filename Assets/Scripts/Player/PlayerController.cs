using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public GameObject GolfBall;

	public float appliedForce = 20f;
	public float maxForce = 100f;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		if (GolfBall == null)
			return;

		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 dir = new Vector3();
		dir.z = Input.GetAxis("Vertical");
		dir.x = Input.GetAxis("Horizontal");

		rb.AddForce(dir * appliedForce);
	}

	private void LateUpdate ()
	{
		//transform.position = GolfBall.transform.position;
	}
}
