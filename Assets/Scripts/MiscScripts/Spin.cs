using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float SpinSpeed = 1f;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y+SpinSpeed, transform.eulerAngles.z);
    }
}
