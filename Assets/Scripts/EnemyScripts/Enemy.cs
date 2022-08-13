using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{   
    public float health = 100f;
    public int points = 100;
    public GameObject explosion;
    private UIScript ui;

    void OnDisable() {
        UIScript.instance.addToScore(points);
        if(this.gameObject.scene.isLoaded) {
            Instantiate(explosion, this.transform.position, this.transform.rotation);
        }
    }

    void Start() {

    }

    void OnParticleCollision(GameObject other) {
        if (other.name == "Ship MG") health -= 5;
    }

    void Update()
    {
        if (health <= 0f) {
            if (Vector3.Distance(FindObjectOfType<PlaneScript>().gameObject.transform.position, transform.position) < 1200) {
                FindObjectOfType<AudioManager>().PlayRepeatedly("Explosion");
            }
            Destroy(this.gameObject);
        }
    }

    public void setHP(float damage) {
        health += damage;
    }

    public int getPoints() {
        return points;
    }
}
