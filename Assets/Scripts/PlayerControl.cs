using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

    public float MoveSpeed = 10;
    public float RotateSpeed = 60;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal") * RotateSpeed * Time.deltaTime;
        transform.Rotate(0, horizontal, 0);

        float vertical = Input.GetAxis("Vertical") * MoveSpeed * Time.deltaTime;
        transform.Translate(0, 0, -1 * vertical);
    }
}
