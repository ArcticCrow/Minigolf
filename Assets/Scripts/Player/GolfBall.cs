using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GolfBall : MonoBehaviour {

	public float maxVelocity = 100f;

	private Rigidbody rb;

	private void Start ()
	{
		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = maxVelocity;
	}

	public void AddImpulse(Vector3 force)
	{
		rb.AddForce(force, ForceMode.Impulse);
	}

	public bool isMoving()
	{
		return (rb.velocity.magnitude > 0);
	}
}
