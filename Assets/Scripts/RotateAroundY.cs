using UnityEngine;
using System.Collections;

public class RotateAroundY : MonoBehaviour {

    public float Speed = 90f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * Speed);
    }
}
