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
    public GameObject SightLineSource;
    public GameObject UFO;
    public Enemy EnemyRedSource;
    public Enemy EnemyGreenSource;
    public Enemy EnemyBlueSource;
    public LaserBeam ActiveLaserBeam {get; set;}
    public float Speed = 1f;

    private float _msSinceBeam = 0;
    private float _msSinceEnemySpawn = 0;
    private float _msSinceShot = 0;
    public bool IsStarted { get; set; } = false;
    public bool IsOnBeat { get; set; } = false;
    private GameObject _sightLine;

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
                switch (UnityEngine.Random.Range((int)0,(int)3).ToString())
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

            if (_msSinceShot >= (60 / AudioManager.Instance.BPM * 0.25f)) //every half beat
            {
                _msSinceShot = 0;

                foreach (var item in EnemiesHit)
                {
                    item.Explode();
                }

                EnemiesHit.Clear();
            }
            else
            {
                _msSinceShot += Time.deltaTime;
            }

            _sightLine.transform.position = LeftController.transform.position;
            _sightLine.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    IEnumerator ShootBeam()
    {
        ActiveLaserBeam.transform.position = LeftController.transform.position;
        var beamDirection = (RightController.transform.position - LeftController.transform.position).normalized;
        ActiveLaserBeam.transform.rotation = Quaternion.LookRotation(beamDirection);
        ActiveLaserBeam.gameObject.SetActive(true);

        //yield return new WaitForSeconds(0.1f);

        //while (_msSinceBeam < 0.15f)
        //{
        //    ActiveLaserBeam.transform.position += beamDirection * 10 * Time.deltaTime;

        //    yield return null;
        //}

        yield return new WaitForSeconds(0.5f);

        GameObject.Destroy(ActiveLaserBeam.gameObject);
    }

    public void StartGame()
    {
        IsStarted = true;
        _sightLine = GameObject.Instantiate(SightLineSource);
        _sightLine.SetActive(true);

        AudioManager.Instance.Play();
    }

    public void StopGame()
    {
        IsStarted = false;
        Destroy(_sightLine);
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
            Debug.Log(number);
        }
        return number;
    }
}
