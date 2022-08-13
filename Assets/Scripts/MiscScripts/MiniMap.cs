using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public GameObject player;
    private float zTransform;

    void Start() {
        zTransform = gameObject.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = new Vector3(player.transform.position.x+zTransform*Mathf.Sin(player.transform.eulerAngles.y*Mathf.Deg2Rad),
                                        transform.position.y, 
                                        player.transform.position.z+zTransform*Mathf.Cos(player.transform.eulerAngles.y*Mathf.Deg2Rad));
        gameObject.transform.eulerAngles = new Vector3(transform.eulerAngles.x, player.transform.eulerAngles.y, transform.eulerAngles.z);

    }
}
