using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;
using UnityEngine.InputSystem.XR;

public class GameManager : MonoBehaviour
{
    public float RefreshRate { get; set; }
    public LaserBeam LaserBeamSource;
    public Bullet GreenBulletSource;
    public Bullet RedBulletSource;
    public Bullet BlueBulletSource;
    public GameObject LeftController;
    public GameObject RightController;
    public GameObject SightLine;
    public LaserBeam ActiveLaserBeam {get; set;}

    private float _msSinceBeam = 0;
    public bool IsStarted { get; set; } = false;    

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {        
        if (XRDevice.refreshRate != RefreshRate)
        {
            RefreshRate = XRDevice.refreshRate;
            Time.fixedDeltaTime = (float)Math.Round(1 / XRDevice.refreshRate, 8);
        }

        if (GameManager.Instance.IsStarted)
        {
            var lookDirection = (RightController.transform.position - LeftController.transform.position).normalized;

            if (_msSinceBeam >= (60 / AudioManager.Instance.BPM * 1)) //every 1 beats
            {
                _msSinceBeam = 0;
                ActiveLaserBeam = GameObject.Instantiate(LaserBeamSource);
                StartCoroutine(ShootBeam());
            }
            else
            {
                _msSinceBeam += Time.deltaTime;
            }

            SightLine.transform.position = LeftController.transform.position;        
            SightLine.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    IEnumerator ShootBeam()
    {
        ActiveLaserBeam.transform.position = LeftController.transform.position;
        var beamDirection = (RightController.transform.position - LeftController.transform.position).normalized;
        ActiveLaserBeam.transform.rotation = Quaternion.LookRotation(beamDirection);
        ActiveLaserBeam.gameObject.SetActive(true);

        while (_msSinceBeam < 0.4)
        {
            ActiveLaserBeam.transform.position += beamDirection * 10 * Time.deltaTime;
            
            yield return null;
        }

        GameObject.Destroy(ActiveLaserBeam.gameObject);
    }

    public void StartGame()
    {
        IsStarted = true;
    }
}
