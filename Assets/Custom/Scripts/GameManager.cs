using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    public float RefreshRate { get; set; }
    public LaserBeam LaserBeamSource;
    public Bullet GreenBulletSource;
    public Bullet RedBulletSource;
    public Bullet BlueBulletSource;
    public GameObject LeftController;
    public GameObject RightController;
    public GameObject SightLineSource;
    public GameObject UFO;
    public Enemy EnemyRedSource;
    public Enemy EnemyGreenSource;
    public Enemy EnemyBlueSource;
    public GameObject FloorPlane;
    public LaserBeam ActiveLaserBeam {get; set;}
    public float Speed = 1f;

    private float _msSinceBeam = 0;
    private float _msSinceEnemySpawn = 0;    
    public bool IsStarted { get; set; } = false;
    public bool IsOnBeat { get; set; } = false;
    private GameObject _sightLine;
    private object _destroyer = null;

    public List<Enemy> EnemiesHit = new List<Enemy>();

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

        for (int i = 0; i < 1000; i++)
        {
            Bullet.BlueBullets[i] = GameObject.Instantiate(GameManager.Instance.BlueBulletSource);
            Bullet.GreenBullets[i] = GameObject.Instantiate(GameManager.Instance.GreenBulletSource);
            Bullet.RedBullets[i] = GameObject.Instantiate(GameManager.Instance.RedBulletSource);
        }

        var col = FloorPlane.GetComponent<MeshRenderer>().material.color;
        float factor = Mathf.Pow(2, 0.5f);
        //float factor = intensity;
        FloorPlane.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(col.r * factor, col.g * factor, col.b * factor));
        
        //GetComponentInChildren<Camera>().
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
            Collider[] objectsHitPlayer = Physics.OverlapSphere(LeftController.transform.position, 0.04f);
            foreach (var item in objectsHitPlayer)
            {
                if (item.gameObject.GetComponentInParent<Enemy>() != null)
                {
                    //hit by enemy
                    _destroyer = item.gameObject.GetComponentInParent<Enemy>();                    
                }
        
                if (item.gameObject.GetComponent<Bullet>() != null)
                {
                    //hit by bullet
                    _destroyer = item.gameObject.GetComponent<Bullet>();                    
                }
            }
            if (_destroyer != null)
            {
                StopGame();
            }
            else
            {

                var lookDirection = (RightController.transform.position - LeftController.transform.position).normalized;

                if (_msSinceBeam >= (60 / AudioManager.Instance.BPM * 1)) //every 1 beats
                {
                    IsOnBeat = true;
                    _msSinceBeam = 0;
                    ActiveLaserBeam = GameObject.Instantiate(LaserBeamSource);
                    StartCoroutine(ShootBeam());
                }
                else
                {
                    IsOnBeat = false;
                    _msSinceBeam += Time.deltaTime;
                }

                if (_msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 2)) //every 2 beats
                {
                    _msSinceEnemySpawn = 0;
                    Enemy enemySpawn = null;
                    switch (UnityEngine.Random.Range((int)0, (int)3).ToString())
                    {
                        case "0":
                            enemySpawn = EnemyRedSource;
                            break;
                        case "1":
                            enemySpawn = EnemyGreenSource;
                            break;
                        case "2":
                            enemySpawn = EnemyBlueSource;
                            break;
                        default:
                            break;
                    }
                    enemySpawn = GameObject.Instantiate(enemySpawn);
                    enemySpawn.gameObject.SetActive(true);
                    enemySpawn.gameObject.transform.position = LeftController.transform.position + GetRandomPosition(1, 2);
                }
                else
                {
                    _msSinceEnemySpawn += Time.deltaTime * GameManager.Instance.Speed;
                }

                //_sightLine.transform.position = LeftController.transform.position;
                //_sightLine.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    IEnumerator ShootBeam()
    {
        ActiveLaserBeam.transform.position = LeftController.transform.position;
        var beamDirection = (RightController.transform.position - LeftController.transform.position).normalized;
        //yield return new WaitForSeconds(0.1f);
        
        GameObject firstHitObject = null;
        float radiusToCheck = 0.05f;        
        RaycastHit[] hitObjects;

        //check for objects, increasing radius as we go
        bool destoryAll = true; //first check destroys all in path, subsequent only destroys first hit, not all as radius too wide
        while (radiusToCheck < 1f)
        {
            Vector3 sphereRadiusLength = beamDirection * radiusToCheck;
            hitObjects = Physics.SphereCastAll(LeftController.transform.position + sphereRadiusLength, radiusToCheck, beamDirection, 10f);            
            foreach (var hitObject in hitObjects)
            {
                Enemy enemy = hitObject.collider.gameObject.GetComponentInParent<Enemy>();

                if (enemy != null && !enemy.IsHit)
                {
                    if (firstHitObject == null)
                    {
                        firstHitObject = enemy.gameObject;                        
                    }                    
                    enemy.Hit();
                    if (!destoryAll)
                    {
                        break; //we hit something using auto aim, stop checking other objects
                    }
                }
            }

            if (firstHitObject != null)
            {
                break; //we hit something, stop checking further away
            }

            destoryAll = false;
            radiusToCheck += 0.05f;
        }

        if (firstHitObject != null)
        {
            beamDirection = (firstHitObject.transform.position - LeftController.transform.position).normalized;
        }
        ActiveLaserBeam.transform.rotation = Quaternion.LookRotation(beamDirection);
        ActiveLaserBeam.gameObject.SetActive(true);

        while (_msSinceBeam < 0.15f)
        {
            ActiveLaserBeam.transform.position += beamDirection * 10 * Time.deltaTime;

            yield return null;
        }

        //yield return new WaitForSeconds(0.2f);

        if (ActiveLaserBeam.gameObject.activeSelf)
        {
            GameObject.Destroy(ActiveLaserBeam.gameObject);
        }
    }

    public void StartGame()
    {
        IsStarted = true;
        //_sightLine = GameObject.Instantiate(SightLineSource);
        //_sightLine.SetActive(true);

        if (_destroyer != null)
        {
            if (_destroyer.GetType() == typeof(Enemy))
            {
                Destroy(((Enemy)_destroyer).gameObject);
            }
            else if (_destroyer.GetType() == typeof(Bullet))
            {
                ((Bullet)_destroyer).gameObject.SetActive(false);
            }

            _destroyer = null;
        }
        AudioManager.Instance.AudioSource.Play();
    }

    public void StopGame()
    {
        IsStarted = false;
        AudioManager.Instance.AudioSource.Stop();
        
        foreach (var item in FindObjectsOfType<Enemy>())
        {
            if ((object)item != _destroyer)
            {
                Destroy(item.gameObject);
            }
        }

        foreach (var item in FindObjectsOfType<Bullet>())
        {
            if ((object)item != _destroyer)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    public static Vector3 GetRandomPosition(float minRange, float maxRange)
    {
        return new Vector3(GetRandomPositionComponent(minRange, maxRange, true), GetRandomPositionComponent(0.1f, 2f, false), GetRandomPositionComponent(minRange, maxRange, true));
    }

    public static float GetRandomPositionComponent(float minRange, float maxRange, bool blnAllowNegatives)
    {
        float number = UnityEngine.Random.Range(minRange, maxRange);
        if (blnAllowNegatives)
        {            
            number *= (UnityEngine.Random.Range((int)-1, (int)2) > 0 ? 1 : -1);            
        }
        return number;
    }
}
