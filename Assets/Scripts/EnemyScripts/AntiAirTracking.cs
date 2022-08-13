using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiAirTracking : MonoBehaviour
{
    public GameObject turret;
    public GameObject guns;
    private Rigidbody player;
    private Rigidbody projectile;    
    public ParticleSystem mgGun;
    public float turretRotSpeed = 5f;
    public float lockTimeReset = 1000f;
    private float lockTime;
    private float dFromTarget;
    private float tFromTarget;

    void Start(){
        lockTime = lockTimeReset/10;
        player = player = FindObjectOfType<PlaneScript>().gameObject.GetComponent<Rigidbody>();
        if (gameObject.name == "SAM") projectile = FindObjectOfType<MissleScript>().gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (FindObjectOfType<PauseMenu>().getGamePaused() != true) {
        dFromTarget = Vector3.Distance(gameObject.transform.position, player.transform.position);
        if (mgGun != null) tFromTarget = (float) dFromTarget / mgGun.startSpeed;
        else tFromTarget = (float) dFromTarget / 600f;
        if (dFromTarget < 4500) {
            Vector3 predictedPosition = new Vector3(player.position.x + (tFromTarget * player.velocity.x), player.position.y + (tFromTarget * player.velocity.y), player.position.z + (tFromTarget * player.velocity.z));
            for (int i = 0; i < 2; i++) {
                if (mgGun != null) tFromTarget = (float) Vector3.Distance(gameObject.transform.position, predictedPosition) / mgGun.startSpeed;
                else tFromTarget = (float) Vector3.Distance(gameObject.transform.position, predictedPosition) / 600f;
                predictedPosition = new Vector3(player.transform.position.x + (tFromTarget * player.GetComponent<Rigidbody>().velocity.x),
                                    player.transform.position.y + (tFromTarget * player.GetComponent<Rigidbody>().velocity.y),
                                    player.transform.position.z + (tFromTarget * player.GetComponent<Rigidbody>().velocity.z));
            }
            Vector3 newDirection = (predictedPosition - guns.transform.position).normalized;
            if (projectile != null) newDirection = (player.transform.position - guns.transform.position).normalized;
            Quaternion newRotation = Quaternion.LookRotation(newDirection);
            guns.transform.rotation = Quaternion.Slerp(guns.transform.rotation, newRotation, turretRotSpeed);
            if (turret != null) turret.transform.eulerAngles = new Vector3(turret.transform.eulerAngles.x, guns.transform.eulerAngles.y, turret.transform.eulerAngles.z);

            //Debug.DrawRay(transform.position, newDirection, Color.red);
        }

        if (projectile != null) {
            if(guns.transform.eulerAngles.x < 355 && guns.transform.eulerAngles.x > 5) {
                if (dFromTarget < 4500) {
                    lockTime--;
                    if (lockTime < lockTimeReset/10 * 3 && lockTime >= 0) {
                        if (FindObjectOfType<PauseMenu>().getGamePaused() != true && FindObjectOfType<AudioManager>().IsPlaying("MissileLockVoice") == false) {
                            FindObjectOfType<AudioManager>().Play("RadarLockBuzz");
                            FindObjectOfType<AudioManager>().Play("RadarLockVoice");
                        } else if (FindObjectOfType<PauseMenu>().getGamePaused() != true) {
                            FindObjectOfType<AudioManager>().Stop("RadarLockBuzz");
                            FindObjectOfType<AudioManager>().Stop("RadarLockVoice");
                        }
                    }
                }
                else if (lockTime > lockTimeReset/10) lockTime--;
                if (lockTime <= 0) {
                    FireMissile(projectile, guns.transform, player.gameObject);
                    lockTime = lockTimeReset;
                }
            }
            else if (lockTime < lockTimeReset/10) {
                lockTime = lockTimeReset/10;
            }
        }

        if (mgGun != null) {
            if(guns.transform.eulerAngles.x < 359 && guns.transform.eulerAngles.x > 1 && dFromTarget < 3000) {
                mgGun.enableEmission = true;
                mgGun.startLifetime = tFromTarget + Random.Range(-0.1f - tFromTarget/10, 0.1f + tFromTarget/10);
            } else {
                mgGun.enableEmission = false;
            }
        }
        }
    }

    public void FireMissile(Rigidbody projectile, Transform from, GameObject target) {
        Rigidbody projectileClone = (Rigidbody) Instantiate(projectile, from.position, from.rotation);
        foreach(Collider c in GetComponents<Collider> ()) {
            Physics.IgnoreCollision(projectileClone.GetComponent<Collider>(), c, true);
        }
        projectileClone.GetComponent<MissleScript>().ActivateMsl(target, 600, 2.5f);
    }
}
