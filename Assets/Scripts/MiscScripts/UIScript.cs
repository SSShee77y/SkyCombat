using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public static UIScript instance;
    
    public Rigidbody player;
    
    public Text speedometer;
    public Text altimeter;
    public Text throttle;
    public Text weaponStats;
    public Text timeScore;
    private float score;
    public float timer;
    private float secs;
    private int mins, hits;

    public Image planeSilhouette;
    public Image leftMissileFill;
    public Image rightMissileFill;

    
    public void Awake()
    {
        instance = this;
    }

    void Update()
    {
        speedometer.text = string.Format("SPEED\n{0:0.}", player.velocity.magnitude);
        altimeter.text = string.Format("ALT\n{0:0.}", player.transform.position.y);
        if (throttle != null) throttle.text = string.Format("THROTTLE\n{0:0.}%", player.GetComponent<PlaneScript>().GetThrottle());
        
        weaponStats.text = string.Format("{0:0.}\n{1:0.}\n{2:0.}\n{3:0.}%", player.GetComponent<PlaneScript>().GetGunCount(),
            player.GetComponent<PlaneScript>().GetMslCount(), player.GetComponent<PlaneScript>().GetFlrCount(), player.GetComponent<PlaneScript>().GetHealth());
        
        timer = Mathf.Round(Time.timeSinceLevelLoad * 100f) / 100f;
        mins = (int)(timer / 60);
        secs = timer % 60;
        bool hasLocks = false;
        foreach (GameObject lockOn in GameObject.FindGameObjectsWithTag("Enemy")) {
            if (lockOn.GetComponent<TargetLock>().getLocked()) {
                hasLocks = true;
                timeScore.text = string.Format("TIME {0:00.}:{1:00.00}\nSCORE {2:000000.}\nTARGET {3} +{4:0.}", mins, secs, score, lockOn.name, lockOn.GetComponent<Enemy>().getPoints());
            } 
        } if (hasLocks == false) timeScore.text = string.Format("TIME {0:00.}:{1:00.00}\nSCORE {2:000000.}", mins, secs, score);

        
        if (player.GetComponent<PlaneScript>().GetHealth() > 65) planeSilhouette.color = new Color(0, 255, 0);
        else if (player.GetComponent<PlaneScript>().GetHealth() > 30) planeSilhouette.color = new Color(255, 255, 0);
        else planeSilhouette.color = new Color(255, 0, 0);
        
        float MslResetNum = player.GetComponent<PlaneScript>().GetMslResetCD();
        leftMissileFill.fillAmount = (MslResetNum-player.GetComponent<PlaneScript>().GetLMslCD())/MslResetNum;
        rightMissileFill.fillAmount = (MslResetNum-player.GetComponent<PlaneScript>().GetRMslCD())/MslResetNum;

    }

    public void addToScore(float num) {
        score += num;
    }
}
