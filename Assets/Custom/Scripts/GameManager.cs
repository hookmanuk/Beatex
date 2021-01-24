using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    public Mothership EnemyMothershipSource;
    public GameObject FloorPlane;
    public GameObject Camera;
    public GameObject TestBehindArea;
    public VolumeProfile VolumeProfile;
    public AudioSource WaveAudioSource;
    public bool ResetToStartOnDeath;
    public LaserBeam ActiveLaserBeam {get; set;}
    private int _enemiesToSpawn = 3;
    public float Speed = 1f;

    private float _msSinceBeam = 0;
    private float _msSinceEnemySpawn = 0;    
    public bool IsStarted { get; set; } = false;
    public bool IsOnBeat { get; set; } = false;
    private GameObject _sightLine;
    private object _destroyer = null;
    private Vignette _vignette;
    private int _waveWarnsPlayed = 0;
    private Vector3[] _spawnPositions = null;

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

        if (!VolumeProfile.TryGet(out _vignette)) throw new System.NullReferenceException(nameof(_vignette));        
    }

    // Update is called once per frame
    void Update()
    {
        
    }  

    private void FixedUpdate()
    {
        RefreshRateCheck();

        if (IsStarted)
        {            
            if (EnemyHit())
            {
                StopGame();
            }
            else
            {
                RearVignetteCheck();

                BeamShootCheck();

                EnemySpawnCheck();
            }
        }
    }

    private void RefreshRateCheck()
    {
        if (XRDevice.refreshRate != RefreshRate)
        {
            RefreshRate = XRDevice.refreshRate;
            Time.fixedDeltaTime = (float)Math.Round(1 / XRDevice.refreshRate, 8);
        }
    }

    private bool EnemyHit()
    {
        _destroyer = null;

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

        return (_destroyer != null);
    }

    private void RearVignetteCheck()
    {
        Collider[] objectsBehind = Physics.OverlapBox(LeftController.transform.position - Camera.transform.forward.normalized * 0.5f, new Vector3(0.75f, 0.45f, 0.45f), Quaternion.LookRotation(Camera.transform.forward, Vector3.up));

        float closestDistance = 999f;
        foreach (var item in objectsBehind)
        {
            if (item.gameObject.GetComponentInParent<Enemy>())
            {
                var itemDistance = (LeftController.transform.position - item.ClosestPoint(LeftController.transform.position)).magnitude;
                //Debug.Log(item.name + " " + itemDistance);

                if (itemDistance < closestDistance)
                {
                    closestDistance = itemDistance;
                }
            }
        }

        float vignetteRatio = (closestDistance < 1 ? (1f - closestDistance) : 0);

        _vignette.intensity.Override(1f * vignetteRatio);
    }

    private void BeamShootCheck()
    {
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
    }

    private void EnemySpawnCheck()
    {
        if (_waveWarnsPlayed == 0 && _msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 11)) //on 16th beat
        {
            //Calculate positions for spawns
            _spawnPositions = new Vector3[_enemiesToSpawn];
            Vector3 centralSpawnPoint = LeftController.transform.position + GetRandomPosition(1,2);

            for (int i = 0; i < _enemiesToSpawn; i++)
            {
                _spawnPositions[i] = UnityEngine.Random.insideUnitSphere * 1.25f + centralSpawnPoint;
            }

            //set the audio source to play where the wave will spawn
            WaveAudioSource.gameObject.transform.position = centralSpawnPoint;

            AudioManager.Instance.PlayWaveWarn(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 1 && _msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 13)) //on 13th beat
        {
            AudioManager.Instance.PlayWaveWarn(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 2 && _msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 15)) //on 15th beat
        {

            AudioManager.Instance.PlayWaveStart(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 16)) //every 16 beats
        {
            _msSinceEnemySpawn = 0;
            _waveWarnsPlayed = 0;
            Enemy enemySpawn = null;

            //spawn enemies
            for (int i = 0; i < _enemiesToSpawn; i++)
            {
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
                enemySpawn.gameObject.transform.position = _spawnPositions[i];
            }

            if (_enemiesToSpawn >= 5)
            {
                Mothership mothership = Instantiate(EnemyMothershipSource);
                mothership.gameObject.SetActive(true);
                mothership.gameObject.transform.position = WaveAudioSource.gameObject.transform.position;
            }

            _enemiesToSpawn++;
        }
        else
        {
            _msSinceEnemySpawn += Time.deltaTime * GameManager.Instance.Speed;
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

                if (hitObject.collider.gameObject.GetComponentInParent<Mothership>() != null)
                {
                    enemy = hitObject.collider.gameObject.GetComponentInParent<Mothership>();
                }
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
            ActiveLaserBeam.transform.position += beamDirection * 20 * Time.deltaTime;

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
        _msSinceBeam = 0;
        _msSinceEnemySpawn = 0;
        if (ResetToStartOnDeath)
        {
            _enemiesToSpawn = 3;
        }

        try
        {
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
        }
        catch (Exception)
        {
            //sometimes the _destroyer is already destroyed, shrug
        }
        
        AudioManager.Instance.MusicSource.Play();
    }

    public void StopGame()
    {
        IsStarted = false;
        AudioManager.Instance.MusicSource.Stop();
        
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
