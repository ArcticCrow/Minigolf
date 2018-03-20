using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour {

	private void OnTriggerEnter (Collider other)
	{
		if (other.transform.CompareTag("Player"))
		{
			Debug.Log("Player reached hole");
		} else
		{
			Debug.Log("Something collided: " + other.gameObject.name);
		}
	}
}
