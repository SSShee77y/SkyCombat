using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapElement : MonoBehaviour
{
    public GameObject miniMapElement;

    void LateUpdate() {
        miniMapElement.transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, 0);
    }
}
