using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;


	// Use this for initialization
	void Start () {
		if (instance != null)
			Destroy(gameObject);
		else
			instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.LeftAlt)) {
			Cursor.lockState = CursorLockMode.None;
		} else if (Input.GetKeyUp(KeyCode.LeftAlt))
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
	}
}
