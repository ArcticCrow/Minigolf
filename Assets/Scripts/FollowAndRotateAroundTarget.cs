﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FollowAndRotateAroundTarget : MonoBehaviour {

	public Transform target;

	public float zoomSpeed = 10f;
	public float distance = 10f;
	public float minDistance = 5f, maxDistance = 50f;

	public float turnSpeed = 2f;
	public float horizontalSensitivity = 2f;
	public float verticalSensitivity = 1f;

	public float minPitch = 0f, maxPitch = 55f;

	float rotYAxis = 0f;
	float rotXAxis = 0f;

	// Use this for initialization
	void Start () {
		rotYAxis = transform.eulerAngles.y;
		rotXAxis = transform.eulerAngles.x;
	}

	void LateUpdate ()
	{
		if (target)
		{
			// Lock camera rotation and zoom if trying to apply force
			if (Input.GetMouseButton(0) && target.gameObject.GetComponent<Rigidbody>().velocity.magnitude <= 0)
				return;

			rotYAxis += turnSpeed * horizontalSensitivity * Input.GetAxis("Mouse X");
			rotXAxis += turnSpeed * verticalSensitivity * Input.GetAxis("Mouse Y");

			rotXAxis = ClampAngle(rotXAxis, minPitch, maxPitch);

			Quaternion rotation = Quaternion.Euler(rotXAxis, rotYAxis, 0f);

			distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, minDistance, maxDistance);
			/*RaycastHit hit;
			if (Physics.Linecast(target.position, transform.position, out hit))
			{
				distance -= hit.distance;
			}*/
			Vector3 negDistance = new Vector3(0f, 0f, -distance);
			Vector3 position = rotation * negDistance + target.position;

			transform.rotation = rotation;
			transform.position = position;
		} else
		{
			GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");
			if (objs != null)
			{
				for (int i = 0; i < objs.Length; i++)
				{
					GameObject obj = objs[i];
					if (obj.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						target = obj.transform;
						return;
					}
				}
			}
		}
	}

	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360f)
			angle += 360f;
		if (angle > 360f)
			angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}
}
