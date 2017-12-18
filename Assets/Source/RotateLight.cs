using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLight : MonoBehaviour {

    public float rotationAmount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        float posMovement = Mathf.Cos(Time.time) * rotationAmount * Time.deltaTime;
        transform.Rotate(0, posMovement, 0);
    }
}
