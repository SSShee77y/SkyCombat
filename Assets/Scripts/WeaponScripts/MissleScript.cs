using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleScript : MonoBehaviour
{   
    public float mslSpeed = 1200;
    private float dfMslSpeed;
    public float mslRotSpeed = 4;
    public float lifetime = 3000;
    public float damage = 80;
    private float maxLifetime;
    public bool missleActivated = false;
    private Rigidbody rb;
    private float previousDistance;
    private bool missed;
    private bool gaveDamage;
    private float speedModifier = 100f;

    public ParticleSystem particles;
    public ParticleSystem firePart;
    public GameObject target;
    public GameObject explosion;

    // Note - Add fire to end of missiles

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<Enemy>() != null && gaveDamage != true) {
            other.gameObject.GetComponent<Enemy>().setHP(-damage);
            gaveDamage = true;
        }
        else if (other.gameObject.GetComponent<PlaneScript>() != null && gaveDamage != true) {
            other.gameObject.GetComponent<PlaneScript>().setHP(-damage/3);
            gaveDamage = true;
            if (FindObjectOfType<PauseMenu>().getGamePaused() != true) FindObjectOfType<AudioManager>().PlayRepeatedly("PlaneExplosion");
        }
        detachParticles();
        Destroy(this.gameObject);
    }

    void OnDisable() {
        if(this.gameObject.scene.isLoaded) {
            Instantiate(explosion, this.transform.position, this.transform.rotation);
        }
    }

    void Start() {
        rb = this.GetComponent<Rigidbody>();
        previousDistance = 9999;
        particles.enableEmission = false;
        firePart.enableEmission = false;
        maxLifetime = lifetime;
        dfMslSpeed = mslSpeed;
    }

    void FixedUpdate() {
        if (missleActivated == true) {
            lifetime--;
            if (mslSpeed > dfMslSpeed) mslSpeed -= 2;
            if (rb.velocity.magnitude < mslSpeed) {
                rb.velocity += transform.forward * mslSpeed * 1.2f / speedModifier;
                if (speedModifier > 20) speedModifier--;
            }
            rb.drag = rb.velocity.magnitude/150f + 0f;
            if (rb.drag > 6) rb.drag = 6;
            if (maxLifetime - lifetime < 8) rb.drag = 0.01f;
            particles.enableEmission = true;
            firePart.enableEmission = true;
        }
    }

    void Update() {
        if (FindObjectOfType<PauseMenu>().getGamePaused() != true) {

        if (missleActivated == true && target != null) {
            if (missed != true) { // Missle Rotation
                float dFromTarget = Vector3.Distance(gameObject.transform.position, target.transform.position);
                float tFromTarget = 0;
                if (GetComponent<Rigidbody>().velocity.magnitude < mslSpeed/2) tFromTarget = (float) dFromTarget / (mslSpeed/2f);
                else tFromTarget = (float) dFromTarget / GetComponent<Rigidbody>().velocity.magnitude;
                Vector3 newDirection = (target.transform.position - transform.position).normalized;
                if (target.GetComponent<Rigidbody>() != null && GetComponent<Rigidbody>().velocity.magnitude >= mslSpeed/2) {
                    Vector3 predictedPosition = new Vector3(target.transform.position.x + (tFromTarget * target.GetComponent<Rigidbody>().velocity.x),
                                                target.transform.position.y + (tFromTarget * target.GetComponent<Rigidbody>().velocity.y),
                                                target.transform.position.z + (tFromTarget * target.GetComponent<Rigidbody>().velocity.z));
                    for (int i = 0; i < 4; i++) {
                        tFromTarget = (float) Vector3.Distance(gameObject.transform.position, predictedPosition) / GetComponent<Rigidbody>().velocity.magnitude;
                        predictedPosition = new Vector3(target.transform.position.x + (tFromTarget * target.GetComponent<Rigidbody>().velocity.x),
                                            target.transform.position.y + (tFromTarget * target.GetComponent<Rigidbody>().velocity.y),
                                            target.transform.position.z + (tFromTarget * target.GetComponent<Rigidbody>().velocity.z));
                    }
                    newDirection = (predictedPosition - transform.position).normalized;
                }
                Quaternion newRotation = Quaternion.LookRotation(newDirection);
                if (maxLifetime - lifetime < 10) transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * mslRotSpeed * ((maxLifetime-lifetime)/10));
                else transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * mslRotSpeed);
                
                // Missed detection
                if (Vector3.Distance(transform.position, target.transform.position) > previousDistance && Vector3.Angle(newDirection, transform.forward) > 45 && missed != true) {
                        missed = true;
                        if (lifetime > 120) lifetime = 120;
                        target = null;
                } if (target != null) previousDistance = Vector3.Distance(transform.position, target.transform.position);
            }
        }

        if (lifetime <= 0) {
            detachParticles();
            Destroy(this.gameObject);
        }
        }
    }

    public void ActivateMsl(Rigidbody plane, GameObject target) {
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(plane.velocity.x, plane.velocity.y, plane.velocity.z);
        missleActivated = true;
        this.target = target;
    }

    public void ActivateMsl(Rigidbody plane) {
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(plane.velocity.x, plane.velocity.y, plane.velocity.z);
        missleActivated = true;
        this.target = null;
    }

    public void ActivateMsl(GameObject target, float mS, float mRS) {
        mslSpeed = mS;
        mslRotSpeed = mRS;
        missleActivated = true;
        this.target = target;
    }

    public void setTarget(GameObject target) {
        this.target = target;
    }

    public GameObject getTarget() {
        return target;
    }

    public float getDamage() {
        return damage;
    }

    public void loseTarget() {
        missed = true;
        lifetime = 100;
        target = null;
    }
    
    private void detachParticles() {
        if(particles.enableEmission == true) {
            particles.Stop();
            particles.transform.parent = null;
            Destroy(particles.transform.gameObject, 5.0f);
        }
    }
}
