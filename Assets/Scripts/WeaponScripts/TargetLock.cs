using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetLock : MonoBehaviour
{   
    private GameObject player;
    public GameObject boundingBox;

    public Text nameText;
    public Text distanceText;

    public GameObject lockedImage;
    public bool locked;
    
    void Start() {
        player = FindObjectOfType<PlaneScript>().gameObject;
    }

    void OnDisable() {
        Locked(false);
    }

    void Update()
    {
        boundingBox.transform.position = new Vector3(Camera.main.WorldToScreenPoint(this.transform.position).x, Camera.main.WorldToScreenPoint(this.transform.position).y, 0);

        if (isVisible()) {
            boundingBox.SetActive(true);
        } else {
            Locked(false);
            boundingBox.SetActive(false);
        }

        nameText.text = this.name.ToUpper();
        distanceText.text = string.Format("{0:0.}", Vector3.Distance(this.transform.position, player.transform.position)); 
    }

    void setNewTarget() {
        player.GetComponent<PlaneScript>().setLockIndex(0);
        Locked(false);
    }

    public void Locked(bool b) {
        locked = b;
        if (locked == true) {
            boundingBox.GetComponent<Image>().color = new Color(1f, 0f, 0f);
            lockedImage.SetActive(true);
        } else {
            boundingBox.GetComponent<Image>().color = new Color(0f, 1f, 0f);
            lockedImage.SetActive(false);
        }
    }

    public bool getLocked() {
        return locked;
    }

    public bool isVisible() {
        return (this.GetComponent<Renderer>().isVisible && Vector3.Distance(this.transform.position, player.transform.position) <= 6000);
    }

    public Vector2 BoundingBoxLoc() {
        return boundingBox.GetComponent<RectTransform>().localPosition;
    }

    
}
