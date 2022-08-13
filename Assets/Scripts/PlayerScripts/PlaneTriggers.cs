using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTriggers : MonoBehaviour
{   
    public bool liftFlapsDown = false;
    public bool groundWarning = false;

    private void OnTriggerEnter(Collider other) {
        if(gameObject.name == "IsPlayerTowardTerrain" && liftFlapsDown == false && other.gameObject.tag == "Terrain") groundWarning = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if(gameObject.name == "IsPlayerEnoughAbove" && other.gameObject.tag == "Terrain") liftFlapsDown = false;

        if(gameObject.name == "IsPlayerTowardTerrain" && other.gameObject.tag == "Terrain") groundWarning = false;
    }

    public bool getFlaps() {
        return liftFlapsDown;
    }

    public bool getGroundWarning() {
        return groundWarning;
    }
}
