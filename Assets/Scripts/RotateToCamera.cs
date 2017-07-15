using UnityEngine;
using System.Collections;

public class RotateToCamera : MonoBehaviour {

	void Update () {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
    }
}
