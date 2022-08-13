using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlaneScript : MonoBehaviour
{
    private Rigidbody rb;

    // Flap Movement
    public GameObject leftFlaperon;
    public GameObject rightFlaperon;
    private float leftFlaperonRot;
    private float rightFlaperonRot;
    public GameObject leftFlap;
    public GameObject rightFlap;
    private float leftFlapRot;
    private float rightFlapRot;
    public GameObject rudder;
    private float rudderRot;

    public GameObject planePathMark;

    // Camera Movement
    public ParentConstraint camCon;
    private Vector3 camConRotDef;
    public ParentConstraint camBaseCon;
    private float camRotVer; // For pitching
    private float camBaseRotZ; // For rolling
    private float camBasePosX; // For yawing

    // Radar Map
    public GameObject miniMap;
    public GameObject bigMap;
    private bool bigMapOn;

    // Movement stuff
    public float thrusterSpeed = 40f;
    public float liftspeed = 40f;
    public float pitchSpeed = 1f;
    public float rollSpeed = 1f;
    public float yawSpeed = 1f;
    public float gravityFactor = 1f;
    public float stallSpeed = 1f;

    private float globalTurnSpeed;
    public float speed;
    public float maxSpeed;
    public float throttle;

    private bool crashed;
    private bool autoPilot;
    private bool pullUp;
    private bool stalling;
    private float lockBuzzCD;
    
    public ParticleSystem engineSpeed;

    // Weapon Controls
    public ParticleSystem mgGun;
    public GameObject lMsl;
    private float lMslCD;
    public GameObject rMsl;
    private float rMslCD;
    private float fireMslCD;
    public float mslResetCD = 100f;
    public Rigidbody defaultMissle;
    private List<GameObject> enemyLocks = new List<GameObject>();
    private int lockIndex = 0;
    public ParticleSystem flareDeploy;

    public float FlrResetCD = 100f;
    private float flrCD;

    // Weapon Amounts
    public float gunAmt = 1800;
    public int mslAmt = 64;
    public int flrAmt = 4;

    // Health
    public float health = 100f;
    private bool death;
    private float deathWait = 100f;
    private bool gameOver;
    public float gameOverTimer = 100f;
    public GameObject explosion;
    
    // UI Elements
    public GameObject miniMapElement;
    private Quaternion cursorUIRot;

    // Flap Control
    public bool liftFlapsDown;


    // Locking of controls
    public bool lockedUpThrot = false;
    public bool lockedDownThrot = false;
    public bool lockedUp = false;
    public bool lockedDown = false;
    public bool lockedLeft = false;
    public bool lockedRight = false;
    public bool lockedFire = false;

    void Awake() {
        foreach (WheelCollider w in GetComponentsInChildren<WheelCollider>()) {
            w.motorTorque = 0.000001f;
        }
    }

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        camConRotDef = camCon.GetRotationOffset(0);
    }

    void OnCollisionEnter(Collision other) {
        if (liftFlapsDown != true) {
            if (other.gameObject.tag == "Terrain") {
                if (rb.transform.eulerAngles.x < 270) {
                    setHP(-rb.transform.eulerAngles.x*2f - (rb.velocity.magnitude/4));
                    rb.AddRelativeForce(Vector3.up*(rb.transform.eulerAngles.x/50));
                } 
                else {
                    setHP(-(360-rb.transform.eulerAngles.x)*2f - (rb.velocity.magnitude/4));
                    rb.AddRelativeForce(Vector3.up*((360-rb.transform.eulerAngles.x)/50));
                }
            }
        }
    
    }

    void OnParticleCollision(GameObject other) {
        if (other.name == "Enemy MG") {
            health -= 0.33f;
            if (FindObjectOfType<PauseMenu>().getGamePaused() != true) FindObjectOfType<AudioManager>().PlayRepeatedly(string.Format("BulletHit{0}", (int) Random.Range(1, 5.99f)));
        }
    }

    // FixedUpdate is for physics based updates
    void FixedUpdate() {
        if (death != true && FindObjectOfType<PauseMenu>().getGamePaused() != true) ProcessInput();

        // Friction
        Physics.gravity = new Vector3(0, -1.0f*gravityFactor, 0);
        rb.drag = (rb.velocity.magnitude+Mathf.Abs(rb.velocity.y))/40f + .1f;
        rb.angularDrag = (Mathf.Abs(rb.angularVelocity.magnitude)+rb.velocity.magnitude)/40f + .1f;
        if (rb.drag > 6) rb.drag = 6;
        else if (rb.drag < 0.5f) rb.drag = 0.5f;
        if (rb.angularDrag > 4) rb.angularDrag = 4;
        else if (rb.angularDrag < 0.5f) rb.angularDrag = 0.5f;
        
        // Lift Force
        liftFlapsDown = transform.Find("PlayerTriggers").transform.Find("IsPlayerEnoughAbove").GetComponent<PlaneTriggers>().getFlaps();
        if (liftFlapsDown == true) {
            if (rightFlapRot >= -25f) rightFlapRot -= 1.0f;
            if (leftFlapRot >= -25f) leftFlapRot -= 1.0f;
            rb.AddRelativeForce(Vector3.up*(rb.velocity.magnitude/100)*liftspeed);
            rb.AddRelativeTorque(Vector3.left * pitchSpeed * globalTurnSpeed/20);
            if (speed > 10) speed -= 0.5f;
            lockedDownThrot = true;
            lockedUp = true;
            lockedDown = true;
            lockedLeft = true;
            lockedRight = true;
        } else {
            rb.AddRelativeForce(Vector3.up*(rb.velocity.magnitude/400)*liftspeed);
            lockedDownThrot = false;
            lockedUp = false;
            lockedDown = false;
            lockedLeft = false;
            lockedRight = false;
        }

        // Foward Force
        if (throttle > 100) {
            maxSpeed = (Mathf.Pow((throttle-100)/18, 2f) + 1.0f) * thrusterSpeed;
        } else if (throttle >= 40) {
            maxSpeed = ((throttle-45)/65f) * thrusterSpeed;
        }
        
        if (maxSpeed < -200) maxSpeed = -200;
        if (maxSpeed > 4000) maxSpeed = 4000;

        // Plane AoA
        if (rb.transform.eulerAngles.x >= 270 && throttle > 35) {
            rb.AddRelativeForce(Vector3.forward * -(360-rb.transform.eulerAngles.x) * stallSpeed * (3000-speed) / 6000f);
            maxSpeed -= (360-rb.transform.eulerAngles.x)*3;
        }
        if (rb.transform.eulerAngles.x < 270 && throttle > 35) {
            rb.AddRelativeForce(Vector3.forward * -rb.velocity.y * stallSpeed * (3000-speed) / 3000f);
            maxSpeed += rb.transform.eulerAngles.x*3;
        }
        // Speed
        if (speed < maxSpeed) speed += 0.05f + (maxSpeed-speed)/800;
        if (speed > maxSpeed) speed -= 0.1f + (speed-maxSpeed)/400;
        rb.AddRelativeForce(Vector3.forward * speed);
        
        if (speed < 0) speed = 0;

        // Turn Speed Control
        if (rb.velocity.magnitude >= 200) globalTurnSpeed = 1f - Mathf.Pow((rb.velocity.magnitude-200)/200, 2f);
        else if (rb.velocity.magnitude < 200) globalTurnSpeed = 1f - Mathf.Pow((200-rb.velocity.magnitude)/200, 2f);
        if (globalTurnSpeed < 0.4f) globalTurnSpeed = 0.4f;
        else if (globalTurnSpeed > 1.8f) globalTurnSpeed = 1.8f;

        // For stalling rotation
        if (GetComponent<Rigidbody>().velocity.magnitude < 60 && maxSpeed < 180) {
            Vector3 newDirection = (new Vector3(transform.position.x, transform.position.y-100, transform.position.z) - transform.position).normalized;
            Quaternion newRotation = Quaternion.LookRotation(newDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, (60-GetComponent<Rigidbody>().velocity.magnitude)/250f);
        }
        /// End stalling rotation

        

        /* Left Missle Cooldown */
        if (lMslCD > 0) lMslCD--;
        if (lMslCD <= 30 && lMsl.GetComponent<Renderer>().enabled == false) {
            lMsl.GetComponent<Renderer>().enabled = true;
            if (FindObjectOfType<PauseMenu>().getGamePaused() != true) FindObjectOfType<AudioManager>().PlayRepeatedly("WeaponryReload");
        }
        
        /* Right Missle Cooldown */
        if (rMslCD > 0) rMslCD--;
        if (rMslCD <= 30 && rMsl.GetComponent<Renderer>().enabled == false) {
            rMsl.GetComponent<Renderer>().enabled = true;
            if (FindObjectOfType<PauseMenu>().getGamePaused() != true) FindObjectOfType<AudioManager>().PlayRepeatedly("WeaponryReload");
        }

        /* Fire Missles Cooldown */
        if (fireMslCD > 0) fireMslCD--;

        /* Flare Cooldown */
        if (flrCD > 0) flrCD--;

        if (engineSpeed != null) engineSpeed.emissionRate = (throttle-41) * 8;

    }

    void Update() {
        if (FindObjectOfType<PauseMenu>().getGamePaused() != true) {
        // Boundaries Check
        if (Mathf.Abs(gameObject.transform.position.x) > 31000 || Mathf.Abs(gameObject.transform.position.z) > 31000) {
            gameOver = true;
        }

        // [--- For Weaponry ---]
        /* MG Velocity */
        mgGun.startSpeed = 1000 + rb.velocity.magnitude;

        /* Refresh Locks */
         enemyLocks.Clear();
         foreach (GameObject lockOn in GameObject.FindGameObjectsWithTag("Enemy")) {
             if (lockOn.GetComponent<TargetLock>().isVisible()) enemyLocks.Add(lockOn);
         }

         enemyLocks.Sort(delegate(GameObject a, GameObject b) {
             return Vector3.Distance(new Vector3(0,0,0), a.GetComponent<TargetLock>().BoundingBoxLoc()).CompareTo(
                    Vector3.Distance(new Vector3(0,0,0), b.GetComponent<TargetLock>().BoundingBoxLoc()));
             }
         );

        /* End Refresh Locks*/

        if (death != true) ProcessWeaponry();

        // [--- For Flaps ---]
        leftFlaperon.transform.localRotation = Quaternion.Euler(-90+leftFlaperonRot, 0, 0);
        rightFlaperon.transform.localRotation = Quaternion.Euler(-90+rightFlaperonRot, 0, 0);
        leftFlap.transform.localRotation = Quaternion.Euler(-90+leftFlapRot, 0, 0);
        rightFlap.transform.localRotation = Quaternion.Euler(-90+rightFlapRot, 0, 0);
        rudder.transform.localRotation = Quaternion.Euler(0, rudderRot, 0);

        // [--- For Camera ---]
        camCon.SetTranslationOffset(0, new Vector3(camCon.GetTranslationOffset(0).x, camCon.GetTranslationOffset(0).y, -14-(throttle/50f)));
        camCon.SetRotationOffset(0, new Vector3(camConRotDef.x + camRotVer, 0, 0));

        // [--- For Camera Base ---]
        camBaseCon.SetTranslationOffset(0, new Vector3(camBasePosX, camRotVer/5f, 0));
        camBaseCon.SetRotationOffset(0, new Vector3(0, 0, 0+camBaseRotZ));

        // [--- For Flight Path Marker ---]
        if (liftFlapsDown == false || rb.velocity.magnitude > 2) planePathMark.transform.eulerAngles =
            new Vector3(-Mathf.Atan2(rb.velocity.y, Mathf.Sqrt(Mathf.Pow(rb.velocity.z,2) + Mathf.Pow(rb.velocity.x,2))) * Mathf.Rad2Deg,
                        Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg,
                        rb.transform.eulerAngles.z);

        if (health <= 0 && death != true) ProcessDeath();
        else if (health <= 0 && death == true) health = 0;
        if (death == true && gameOverTimer > 0) gameOverTimer--;
        else if (gameOverTimer <= 0 && gameOver != true) gameOver = true;
        if (gameOver == true && FindObjectOfType<PauseMenu>().gameOverBool != true) FindObjectOfType<PauseMenu>().gameOver();

        if (FindObjectOfType<PauseMenu>() != null) PlayLoops();
        }
    }

    void LateUpdate() {
        miniMapElement.transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, 0);
    }

    void PlayLoops() {
        if (death != true && lockBuzzCD > 0) lockBuzzCD--;

        if (GetComponent<Rigidbody>().velocity.magnitude < 90 && maxSpeed < 180) {
            FindObjectOfType<AudioManager>().Play("StallWarning");
            stalling = true;
        } else {
            FindObjectOfType<AudioManager>().Stop("StallWarning");
            stalling = false;
        }

        if (stalling == false && FindObjectOfType<PlaneTriggers>().getGroundWarning() == true) {
            FindObjectOfType<AudioManager>().Play("GroundWarning");
            pullUp = true;
        } else {
            FindObjectOfType<AudioManager>().Stop("GroundWarning");
            pullUp = false;
        }
        
        if (death != true && isLockedOn() == true && lockBuzzCD <= 1) {
            FindObjectOfType<AudioManager>().PlayRepeatedly("MissileLockBuzz");
        }

        if (death != true && isLockedOn() == true) {
            FindObjectOfType<AudioManager>().Play("MissileLockVoice");
        }

        FindObjectOfType<AudioManager>().Play("JetEngine");
        FindObjectOfType<AudioManager>().SetVolume("JetEngine", (throttle-30)/100f);
        if (death == true) {
            FindObjectOfType<AudioManager>().Stop("JetEngine");
            FindObjectOfType<AudioManager>().Stop("GAU12");
            FindObjectOfType<AudioManager>().Stop("StallWarning");
            FindObjectOfType<AudioManager>().Stop("GroundWarning");
        }
    }

    void ProcessDeath() {
        health = 0;
        throttle = 0;
        death = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Instantiate(explosion, this.transform.position, this.transform.rotation);
        FindObjectOfType<AudioManager>().Play("WreckExplosion");
        transform.Find("F16Reskin").gameObject.SetActive(false);
        transform.Find("WeaponControls").gameObject.SetActive(false);
        lockedUpThrot = true;
        lockedDownThrot = true;
        lockedUp = true;
        lockedDown = true;
        lockedLeft = true;
        lockedRight = true;
        lockedFire = true;
    }

    void ProcessWeaponry() {
        if(Input.GetKeyDown(KeyCode.Tab) && enemyLocks.Count > 1)
        {
            for(int i = 0; i < enemyLocks.Count; i++) {
                enemyLocks[i].GetComponent<TargetLock>().Locked(false);
            }

            if (lockIndex < enemyLocks.Count-1) lockIndex++;
            else lockIndex = 0;
            if (lockIndex > 3) lockIndex = 0;
            enemyLocks[lockIndex].GetComponent<TargetLock>().Locked(true);
            FindObjectOfType<AudioManager>().PlayRepeatedly("IFFChange");
        } else if (enemyLocks.Count > 0) {
            bool tempLock = false;
            int tempIndex = -1;
            for(int i = 0; i < enemyLocks.Count; i++) {
                if (enemyLocks[i].GetComponent<TargetLock>().getLocked() == true) {
                    tempLock = true;
                    tempIndex = i;
                    break;
                }
            }
            if (tempLock == false || lockIndex != tempIndex || lockIndex > 3) {
                
                if (lockIndex > 3) for(int i = 0; i < enemyLocks.Count; i++) {
                    enemyLocks[i].GetComponent<TargetLock>().Locked(false);
                }
                if (lockIndex != tempIndex && tempIndex > -1) lockIndex = tempIndex;
                else lockIndex = 0;
                enemyLocks[lockIndex].GetComponent<TargetLock>().Locked(true);
            }
        }

        //print(enemyLocks.Count + " | " + lockIndex );
        
        if(Input.GetKey(KeyCode.Mouse0) && gunAmt > 0 && lockedFire == false)
        {
            //gunAmt -= mgGun.emissionRate/120;
            gunAmt -= 1;
            mgGun.enableEmission = true;
            FindObjectOfType<AudioManager>().Play("GAU12");
        } else if (mgGun.enableEmission == true) {
            mgGun.enableEmission = false;
        }
        if (mgGun.enableEmission == false && FindObjectOfType<AudioManager>().IsPlaying("GAU12")) {
            FindObjectOfType<AudioManager>().Stop("GAU12");
        }
        if (gunAmt < 0) gunAmt = 0;

        if((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1)) && mslAmt > 0 && fireMslCD <= 0 && lockedFire == false)
        {
            if (lMslCD == 0) {
                lMsl.GetComponent<Renderer>().enabled = false;

                mslAmt--;
                if (enemyLocks.Count > 0) FireMissile(defaultMissle, rb.transform.Find("WeaponControls").transform.Find("Left MSL"), enemyLocks[lockIndex]);
                else FireMissile(defaultMissle, rb.transform.Find("WeaponControls").transform.Find("Left MSL"));

                lMslCD = mslResetCD;
                fireMslCD = 100;
                FindObjectOfType<AudioManager>().PlayRepeatedly("MissileLaunch");
            }
            else if (rMslCD == 0) {
                rMsl.GetComponent<Renderer>().enabled = false;

                mslAmt--;
                if (enemyLocks.Count > 0) FireMissile(defaultMissle, rb.transform.Find("WeaponControls").transform.Find("Right MSL"), enemyLocks[lockIndex]);
                else FireMissile(defaultMissle, rb.transform.Find("WeaponControls").transform.Find("Right MSL"));

                rMslCD = mslResetCD;
                fireMslCD = 100;
                FindObjectOfType<AudioManager>().PlayRepeatedly("MissileLaunch");
            } else {
                FindObjectOfType<AudioManager>().Play("AmmoZero");
            }
        }

        if(Input.GetKeyDown(KeyCode.F) && flrCD <= 0 && flrAmt > 0) {
            flareDeploy.Play();
            flrCD = FlrResetCD;
            flrAmt--;
            FindObjectOfType<AudioManager>().PlayRepeatedly("FlareDeployment");
            foreach (MissleScript missile in GameObject.FindObjectsOfType<MissleScript>()) {
                if (missile.getTarget() == gameObject && Vector3.Distance(gameObject.transform.position, missile.gameObject.transform.position) < 1000) {
                    missile.loseTarget();
                }
            }
        } else if(Input.GetKeyDown(KeyCode.F) && flrAmt <= 0) {
            FindObjectOfType<AudioManager>().Play("AmmoZero");
        }

        if(Input.GetKeyDown(KeyCode.R)) {
            bigMapOn = !bigMapOn;
            miniMap.SetActive(!bigMapOn);
            bigMap.SetActive(bigMapOn);
            if (!bigMapOn) miniMapElement.transform.localScale = new Vector3(500, 500, 0);
            else miniMapElement.transform.localScale = new Vector3(4000, 4000, 0);
        }
    }

    void ProcessInput() {

        if(Input.GetKey(KeyCode.LeftShift) && lockedUpThrot == false)
        {
            if (throttle < 110) throttle += 0.4f;
        }

        if(Input.GetKey(KeyCode.LeftControl) && lockedDownThrot == false)
        {
            if (throttle > 40) throttle -= 0.4f;
        }

        if (throttle > 0 && throttle < 40) throttle += 0.4f;
        if (!Input.GetKey(KeyCode.LeftShift) && (!Input.GetKey(KeyCode.LeftControl) || lockedDownThrot == true) && throttle < 70 && throttle >= 40) throttle += 0.4f;
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl) && throttle > 70) throttle -= 0.4f;

        // Autopilot [Level Out]
        if(Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E) && lockedLeft == false && lockedRight == false)
        {
            autoPilot = true;
            if (rb.transform.eulerAngles.z > 0.5 && rb.transform.eulerAngles.z <= 180) {
                // Same as pressing D-key
                rb.AddRelativeTorque(Vector3.back * rollSpeed);
                if (camBaseRotZ <= 8) camBaseRotZ += .4f;

                if (rightFlaperonRot <= 10f && (!Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))) rightFlaperonRot += 1.0f;
                if (leftFlaperonRot >= -10f && (!Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))) leftFlaperonRot -= 1.0f;

                if (rightFlapRot <= 10f) rightFlapRot += 1.0f;
                if (leftFlapRot >= -10f) leftFlapRot -= 1.0f;
            }
            else if (rb.transform.eulerAngles.z < 359.5 && rb.transform.eulerAngles.z > 180) {
                // Same as pressing A-key
                rb.AddRelativeTorque(Vector3.forward * rollSpeed);
                if (camBaseRotZ >= -8) camBaseRotZ -= .4f;

                if (leftFlaperonRot <= 10f && (!Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))) leftFlaperonRot += 1.0f;
                if (rightFlaperonRot >= -10f && (!Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))) rightFlaperonRot -= 1.0f;
                
                if (leftFlapRot <= 10f) leftFlapRot += 1.0f;
                if (rightFlapRot >= -10f) rightFlapRot -= 1.0f;
            }
            // After Rolling, pitching to level
            if (rb.transform.eulerAngles.z <= 2 || rb.transform.eulerAngles.z >= 358) {
                if (rb.transform.eulerAngles.x <= 90 && rb.transform.eulerAngles.x > 0.5) {
                    // Same as pressing S-key
                    rb.AddRelativeTorque(Vector3.left * pitchSpeed * globalTurnSpeed);
                    if (camRotVer <= 2f) camRotVer += 0.1f;
                    if (speed > 40) speed -= 0.01f + (speed)/1000;

                    if (rightFlaperonRot <= 10f && (!Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) rightFlaperonRot += 1.0f;
                    if (leftFlaperonRot <= 10f && (!Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))) leftFlaperonRot += 1.0f;
                }
                else if (rb.transform.eulerAngles.x >= 270 && rb.transform.eulerAngles.x < 359.5) {
                    // Same as pressing W-key
                    rb.AddRelativeTorque(Vector3.right * pitchSpeed * globalTurnSpeed);
                    if (camRotVer >= -2f) camRotVer -= 0.1f;
                    if (speed > 40) speed -= 0.01f + (speed)/1000;
                    
                    if (leftFlaperonRot >= -10f && (!Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) leftFlaperonRot -= 1.0f;
                    if (rightFlaperonRot >= -10f && (!Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))) rightFlaperonRot -= 1.0f;
                } else {
                    rb.transform.eulerAngles = new Vector3 (0, rb.transform.eulerAngles.y, 0);
                }
            }
        } else if (autoPilot == true) {
            autoPilot = false;
        }

        // Roll Left
        if(Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && lockedLeft == false && autoPilot == false)
        {
            rb.AddRelativeTorque(Vector3.forward * rollSpeed);
            if (camBaseRotZ >= -8) camBaseRotZ -= .4f;

            if (leftFlaperonRot <= 10f && (!Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))) leftFlaperonRot += 1.0f;
            if (rightFlaperonRot >= -10f && (!Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))) rightFlaperonRot -= 1.0f;
            
            if (leftFlapRot <= 10f) leftFlapRot += 1.0f;
            if (rightFlapRot >= -10f) rightFlapRot -= 1.0f;
        }
        
        // Roll Right
        if(Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && lockedRight == false && autoPilot == false)
        {
            rb.AddRelativeTorque(Vector3.back * rollSpeed);
            if (camBaseRotZ <= 8) camBaseRotZ += .4f;

            if (rightFlaperonRot <= 10f && (!Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))) rightFlaperonRot += 1.0f;
            if (leftFlaperonRot >= -10f && (!Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))) leftFlaperonRot -= 1.0f;

            if (rightFlapRot <= 10f) rightFlapRot += 1.0f;
            if (leftFlapRot >= -10f) leftFlapRot -= 1.0f;
        }

        // Pitch Down
        if(Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && lockedUp == false && autoPilot == false)
        {
            rb.AddRelativeTorque(Vector3.right * pitchSpeed * globalTurnSpeed);
            if (camRotVer >= -2f) camRotVer -= 0.1f;
            if (speed > 40) speed -= 0.01f + (speed)/1000;
            
            if (leftFlaperonRot >= -10f && (!Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) leftFlaperonRot -= 1.0f;
            if (rightFlaperonRot >= -10f && (!Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))) rightFlaperonRot -= 1.0f;
        }

        // Pitch Up
        if(Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) && lockedDown == false && autoPilot == false)
        {
            rb.AddRelativeTorque(Vector3.left * pitchSpeed * globalTurnSpeed);
            if (camRotVer <= 2f) camRotVer += 0.1f;
            if (speed > 40) speed -= 0.01f + (speed)/1000;
            
            if (rightFlaperonRot <= 10f && (!Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) rightFlaperonRot += 1.0f;
            if (leftFlaperonRot <= 10f && (!Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))) leftFlaperonRot += 1.0f;
        }

        // Yaw Left
        if(Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E) && lockedLeft == false && autoPilot == false)
        {
            if (rb.velocity.magnitude >= 5) rb.AddRelativeTorque(Vector3.down * yawSpeed * globalTurnSpeed);
            if (camBasePosX >= -.5f) camBasePosX -= 0.01f;

            if (rudderRot <= 25f) rudderRot += 1.0f;
        }

        // Yaw Right
        if(Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q) && lockedRight == false && autoPilot == false)
        {
            if (rb.velocity.magnitude >= 5) rb.AddRelativeTorque(Vector3.up * yawSpeed * globalTurnSpeed);
            if (camBasePosX <= .5f) camBasePosX += 0.01f;

            if (rudderRot >= -25f) rudderRot -= 1.0f;
        }
        
        if (leftFlaperonRot > 0.5f) leftFlaperonRot -= 0.50f;
        if (rightFlaperonRot > 0.5f) rightFlaperonRot -= 0.50f;
        if (leftFlaperonRot < -0.5f) leftFlaperonRot += 0.50f;
        if (rightFlaperonRot < -0.5f) rightFlaperonRot += 0.50f;

        if (leftFlapRot > 0.5f) leftFlapRot -= 0.50f;
        if (rightFlapRot > 0.5f) rightFlapRot -= 0.50f;
        if (leftFlapRot < -0.5f) leftFlapRot += 0.50f;
        if (rightFlapRot < -0.5f) rightFlapRot += 0.50f;

        if (rudderRot > 0.5f) rudderRot -= 0.50f;
        if (rudderRot < -0.5f) rudderRot += 0.50f;

        // [--- Begin of Camera Movements ---]
        if ((!Input.GetKey(KeyCode.A) || autoPilot == true) && camBaseRotZ < -0.2f) camBaseRotZ += 0.4f;
        if ((!Input.GetKey(KeyCode.D) || autoPilot == true) && camBaseRotZ > 0.2f) camBaseRotZ -= 0.4f;
        if (camBaseRotZ <= 0.2f && camBaseRotZ >= -0.2f) camBaseRotZ = 0;
        
        if ((!Input.GetKey(KeyCode.W) || autoPilot == true) && camRotVer < -0.05f) camRotVer += 0.1f;
        if ((!Input.GetKey(KeyCode.S) || autoPilot == true) && camRotVer > 0.05f) camRotVer -= 0.1f;
        if (camRotVer <= 0.05f && camRotVer >= -0.05f) camRotVer = 0;

        if ((!Input.GetKey(KeyCode.Q) || autoPilot == true) && camBasePosX < -0.005f) camBasePosX += 0.01f;
        if ((!Input.GetKey(KeyCode.E) || autoPilot == true) && camBasePosX > 0.005f) camBasePosX -= 0.01f;
        if (camBasePosX <= 0.005f && camBasePosX >= -0.005f) camBasePosX = 0;
        // [--- End of Camera Movements ---]
    }

    public void FireMissile(Rigidbody projectile, Transform from, GameObject target) {
        Rigidbody projectileClone = (Rigidbody) Instantiate(projectile, from.transform.position, rb.rotation);
        foreach(Collider c in GetComponents<Collider> ()) {
            Physics.IgnoreCollision(projectileClone.GetComponent<Collider>(), c, true);
        }
        projectileClone.GetComponent<MissleScript>().ActivateMsl(rb, target);
    }

    public void FireMissile(Rigidbody projectile, Transform from) {
        Rigidbody projectileClone = (Rigidbody) Instantiate(projectile, from.transform.position, rb.rotation);
        foreach(Collider c in GetComponents<Collider> ()) {
            Physics.IgnoreCollision(projectileClone.GetComponent<Collider>(), c, true);
        }
        projectileClone.GetComponent<MissleScript>().ActivateMsl(rb);
    }

    public float GetThrottle() {
        return throttle;
    }

    public float GetGunCount() {
        return gunAmt;
    }

    public int GetMslCount() {
        return mslAmt;
    }

    public int GetFlrCount() {
        return flrAmt;
    }

    public float GetHealth() {
        return health;
    }

    public void setHP(float damage) {
        health += damage;
    }

    public float GetLMslCD() {
        return lMslCD;
    }

    public float GetRMslCD() {
        return rMslCD;
    }

    public float GetMslResetCD() {
        return mslResetCD;
    }

    public void setLockIndex(int i) {
        lockIndex = i;
    }

    public bool isLockedOn() {
        bool lockedOn = false;
        foreach (MissleScript missile in GameObject.FindObjectsOfType<MissleScript>()) {
            if (missile.getTarget() == gameObject) {
                if (Vector3.Distance(gameObject.transform.position, missile.gameObject.transform.position) > 2000 && (lockBuzzCD <= 0 || lockBuzzCD > 100)) lockBuzzCD = 100;
                else if (Vector3.Distance(gameObject.transform.position, missile.gameObject.transform.position) > 1000 && (lockBuzzCD <= 0 || lockBuzzCD > 40)) lockBuzzCD = 40;
                else if (Vector3.Distance(gameObject.transform.position, missile.gameObject.transform.position) <= 1000 && (lockBuzzCD <= 0 || lockBuzzCD > 10)) lockBuzzCD = 10;
                lockedOn = true;
            }
        }
        return lockedOn;
    }
}
