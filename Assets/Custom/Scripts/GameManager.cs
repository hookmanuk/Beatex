using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using MText;
using System.Linq;

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
    public UFO UFO;
    public Enemy EnemyRedSource;
    public Enemy EnemyGreenSource;
    public Enemy EnemyBlueSource;
    public Mothership EnemyMothershipSource;
    public GameObject FloorPlane;
    public GameObject Camera;
    public GameObject TestBehindArea;
    public VolumeProfile VolumeProfile;
    public AudioSource WaveAudioSource;
    public Modular3DText Modular3DText;
    public Modular3DText EnemyScore3DText;
    public Modular3DText ScoreboardText;
    public Modular3DText HighestScoreText;
    public Modular3DText LastScoreText;
    public Modular3DText HighScoresText;
    public GameObject HighScoreboard;
    public GameObject CurrentScoreboard;
    public SpawnParticles SpawnIn;
    public Nuke NukeSource;
    public SlowMotion SlowMotionSource;
    public Camera SpecatorCamera;
    public GameObject PlatformTopThirdPerson;
    public GameObject Platform;
    public GameObject Scoreboard;
    public GameType GameType;
    public SelectStart[] SelectStartTypes;
    public bool ThirdPersonMirror;
    public bool ResetToStartOnDeath = true;
    public bool DebugPlay;
    public float DayNightCycleSpeed;

    public LaserBeam ActiveLaserBeam {get; set;}
    public int Wave { get; set; } = 0;
    private int _enemiesToSpawn = 3;
    public float Speed = 1f;
    public bool NukeIsExploding { get; set; }

    private float _secsSinceBeat = 0;
    private float _secsSinceBeam = 0;
    private float _secsSinceEnemySpawn = 0;    
    private bool _isMovingSpectatorCam = false;
    public bool IsStarted { get; set; } = false;
    public bool IsOnBeat { get; set; } = false;
    private GameObject _sightLine;
    private object _destroyer = null;
    private Vignette _vignette;    
    private int _waveWarnsPlayed = 0;
    private Vector3[] _spawnPositions = null;
    private Vector3 _centralSpawnPoint;
    private SpawnParticles _spawnParticles;
    public int ComboMultiplier = 0;
    public int Score = 0;
    private float _startTime;
    public int _pacifyCount = 0;

    public List<Enemy> EnemiesHit = new List<Enemy>();
    dreamloLeaderBoard dl; //http://dreamlo.com/lb/3wAj4tobOEuqRbj6b88HjgrqDHY0wK_UCkp0R3ncu2vQ

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
        // get the reference here...
        this.dl = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
        CurrentScoreboard.SetActive(false);

        if (!ThirdPersonMirror)
        {
            PlatformTopThirdPerson.SetActive(false);
            SpecatorCamera.enabled = false;
        }

        //for (int i = 0; i < 1000; i++)
        //{
        //    Bullet.BlueBullets[i] = GameObject.Instantiate(GameManager.Instance.BlueBulletSource);
        //    Bullet.GreenBullets[i] = GameObject.Instantiate(GameManager.Instance.GreenBulletSource);
        //    Bullet.RedBullets[i] = GameObject.Instantiate(GameManager.Instance.RedBulletSource);
        //}

        var col = FloorPlane.GetComponent<MeshRenderer>().material.color;
        float factor = Mathf.Pow(2, 0.5f);
        //float factor = intensity;
        FloorPlane.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(col.r * factor, col.g * factor, col.b * factor));

        //GetComponentInChildren<Camera>().                

        

        if (!VolumeProfile.TryGet(out _vignette)) throw new System.NullReferenceException(nameof(_vignette));
        _vignette.intensity.Override(0);

        //List<InputDevice> inputDevices = new List<InputDevice>();        
        //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, inputDevices);
        //InputDevice hMDDevice = inputDevices[0];
        //XRInputSubsystem XRIS = hMDDevice.subsystem;
        //List<Vector3> boundaryPoints = new List<Vector3>();
        //XRIS.TryGetBoundaryPoints(boundaryPoints);

        List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(inputSubsystems);
        if (inputSubsystems.Count > 0)
        {
            List<Vector3> boundary = new List<Vector3>();
            if (inputSubsystems[0].TryGetBoundaryPoints(boundary))
            {
                // boundary is now filled with the current sequence of boundary points
                var bounds = GeometryUtility.CalculateBounds(boundary.ToArray(), transform.localToWorldMatrix);
                Platform.transform.localScale = bounds.extents + Vector3.up * 0.53f;
                Scoreboard.transform.position = new Vector3(0, 0.49f, bounds.extents.z / 2 + 1.1f);
                //bounds.size
            }
        }

        _startTime = Time.time;

        if (DebugPlay)
        {
            StartGame();
        }
        else
        {
            StartCoroutine(ShowScores());
        }
    }

    private void XRDevice_deviceLoaded(string obj)
    {
        throw new NotImplementedException();
    }

    //code for boundaries... untested, draw platform to size of boundaries
    //private void Awake()
    //{
    //    List<XRInputSubsystem> list = new List<XRInputSubsystem>();
    //    SubsystemManager.GetInstances<XRInputSubsystem>(list);
    //    foreach (var sSystem in list)
    //    {
    //        if (sSystem.running)
    //        {
    //            _inputSubSystem = sSystem;
    //            break;
    //        }
    //    }
    //    _inputSubSystem.boundaryChanged += RefreshBoundaries;
    //}

    //private void RefreshBoundaries(XRInputSubsystem inputSubsystem)
    //{
    //    List<Vector3> currentBoundaries = new List<Vector3>();
    //    //if (UnityEngine.Experimental.XR.Boundary.TryGetGeometry(currentBoundaries))
    //    if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries))
    //    {
    //        //got boundaries, keep only those which didn't change.
    //        if (currentBoundaries != null && (_boundaries != currentBoundaries || _boundaries.Count != currentBoundaries.Count))
    //            _boundaries = currentBoundaries;
    //        DrawWalls();
    //    }
    //} 

    private void FixedUpdate()
    {
        RefreshRateCheck();

        PollCameraCheck();

        if (IsStarted)
        {            
            if (EnemyHit())
            {
                StopGame();
            }
            else
            {
                RearVignetteCheck();

                BeatCheck();

                if (GameType == GameType.Challenge || GameType == GameType.Arcade)
                {
                    BeamShootCheck();
                }

                if (GameType == GameType.Challenge)
                {
                    ChallengeEnemySpawnCheck();
                }
                else if (GameType == GameType.Arcade)
                {
                    //Arcade Mode here!
                }
                else if (GameType == GameType.Pacifism)
                {
                    PacifismEnemySpawnCheck();
                }
            }
        }

        RenderSettings.skybox.SetFloat("_Rotation", ((Time.time - _startTime) * DayNightCycleSpeed + 210f));
    }

    private void RefreshRateCheck()
    {
        if (XRDevice.refreshRate != RefreshRate)
        {
            RefreshRate = XRDevice.refreshRate;
            Time.fixedDeltaTime = (float)Math.Round(1 / XRDevice.refreshRate, 8);
        }
    }

    public void SetTimeAbsolute(float timeScale)
    {
        Time.timeScale = timeScale;        
    }

    public IEnumerator SetTime(float timeScale)
    {
        float t = 0;
        float startTimescale = Time.timeScale;
        float diffTimescale = timeScale - startTimescale;

        while (t <= 1)
        {            
            Time.timeScale = startTimescale + (diffTimescale * t);
            yield return new WaitForSeconds(0.01f * Time.timeScale);
            t += Time.deltaTime / Time.timeScale;
        }        
    }

    private bool EnemyHit()
    {
        _destroyer = null;

        Collider[] objectsHitPlayer = Physics.OverlapSphere(UFO.transform.position, 0.04f);
        foreach (var item in objectsHitPlayer)
        {
            if (item.gameObject.GetComponentInParent<Enemy>() != null)
            {
                //hit by enemy
                _destroyer = item.gameObject.GetComponentInParent<Enemy>();
            }

            var nuke = item.gameObject.GetComponent<Nuke>();
            if (nuke != null && !nuke.Exploding)
            {
                item.gameObject.GetComponent<Nuke>().Explode();
            }

            var slowMotion = item.gameObject.GetComponent<SlowMotion>();
            if (slowMotion != null && !slowMotion.Exploding)
            {
                item.gameObject.GetComponent<SlowMotion>().Explode();
            }
        }

        return (_destroyer != null);
    }

    private void RearVignetteCheck()
    {
        Collider[] objectsBehind = Physics.OverlapBox(UFO.transform.position - Camera.transform.forward.normalized * 0.5f, new Vector3(0.75f, 0.45f, 0.45f), Quaternion.LookRotation(Camera.transform.forward, Vector3.up));

        float closestDistance = 999f;
        foreach (var item in objectsBehind)
        {
            if (item.gameObject.GetComponentInParent<Enemy>())
            {
                var itemDistance = (UFO.transform.position - item.ClosestPoint(UFO.transform.position)).magnitude;
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

    private void BeatCheck()
    {
        if (_secsSinceBeat >= (60 / AudioManager.Instance.BPM * 1)) //every 1 beats
        {
            IsOnBeat = true;
            _secsSinceBeat = 0;
        }
        else
        {
            IsOnBeat = false;
            _secsSinceBeat += Time.deltaTime;
        }
    }

    private void BeamShootCheck()
    {
        if (_secsSinceBeam >= (60 / AudioManager.Instance.BPM * 1)) //every 1 beats
        {            
            _secsSinceBeam = 0;
            ActiveLaserBeam = GameObject.Instantiate(LaserBeamSource);
            StartCoroutine(ShootBeam());
        }
        else
        {         
            _secsSinceBeam += Time.unscaledDeltaTime;
        }
    }

    private void PollCameraCheck()
    {
        if (!_isMovingSpectatorCam)
        {            
            StartCoroutine(PollMovementForSpectatorCamera());
        }        
    }

    private void ChallengeEnemySpawnCheck()
    {        
        if (_waveWarnsPlayed == 0 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 11)) //on 11th beat
        {
            //Calculate positions for spawns
            _spawnPositions = new Vector3[_enemiesToSpawn];
            _centralSpawnPoint = UFO.transform.position + GetRandomPosition(2,3);

            for (int i = 0; i < _enemiesToSpawn; i++)
            {
                _spawnPositions[i] = UnityEngine.Random.insideUnitSphere * 1.25f + _centralSpawnPoint;
            }

            CreateSpawnParticles();            

            //set the audio source to play where the wave will spawn
            WaveAudioSource.gameObject.transform.position = _centralSpawnPoint;
            
            _waveWarnsPlayed++;            
        }

        //delay half a beat??
        if (_waveWarnsPlayed == 1 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 11.5f))
        {
            AudioManager.Instance.PlayWaveWarn(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 2 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 12)) //on 12th beat
        {
            SpawnParticles(1);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 3 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 13.5f)) //on 13th beat
        {            
            AudioManager.Instance.PlayWaveWarn(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 4 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 14)) //on 14th beat
        {
            SpawnParticles(2);
            _waveWarnsPlayed++;
        }

        if (_waveWarnsPlayed == 5 && _secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 15.5f)) //on 15th beat
        {            
            AudioManager.Instance.PlayWaveStart(WaveAudioSource);
            _waveWarnsPlayed++;
        }

        if (_secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 16)) //every 16 beats
        {
            _secsSinceEnemySpawn = 0;
            _waveWarnsPlayed = 0;
            Enemy enemySpawn = null;

            SpawnParticles(3);

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
                enemySpawn.gameObject.transform.rotation = Quaternion.LookRotation((enemySpawn.gameObject.transform.position - UFO.transform.position).normalized); //rotate to look at UFO
            }

            if (_enemiesToSpawn >= 5)
            {
                if (Math.IEEERemainder(_enemiesToSpawn - 5, 2) == 0)
                {
                    Mothership mothership = Instantiate(EnemyMothershipSource);
                    mothership.gameObject.SetActive(true);
                    mothership.gameObject.transform.position = WaveAudioSource.gameObject.transform.position * 1.3f;
                    mothership.gameObject.transform.rotation = Quaternion.LookRotation((mothership.gameObject.transform.position - UFO.transform.position).normalized); //rotate to look at UFO
                }

                if (Math.IEEERemainder(_enemiesToSpawn - 7, 3) == 0)
                //if (Math.IEEERemainder(_enemiesToSpawn - 5, 1) == 0) //for testing
                {
                    int randomPowerupType = UnityEngine.Random.Range(1, 3);

                    if (randomPowerupType == 1)
                    {
                        Nuke nuke = Instantiate(NukeSource);
                        nuke.gameObject.SetActive(true);
                        nuke.gameObject.transform.position = GetRandomPosition(0.5f, 1f, 0.5f, 1.5f);
                    }
                    else if (randomPowerupType == 2)
                    {
                        SlowMotion slowMotion = Instantiate(SlowMotionSource);
                        slowMotion.gameObject.SetActive(true);
                        slowMotion.gameObject.transform.position = GetRandomPosition(0.5f, 1f, 0.5f, 1.5f);
                    }

                }
            }

            _enemiesToSpawn++;
            
            AudioManager.Instance.PlayWaveNo();
            Wave++;
            StartCoroutine(ShowText("Wave " + Wave.ToString(), TextType.PlayerInfo));
        }
        else
        {
            _secsSinceEnemySpawn += Time.deltaTime * GameManager.Instance.Speed;
        }
    }

    private void PacifismEnemySpawnCheck()
    {
        if (_secsSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 8)) //on 8th beat
        {
            //Calculate positions for spawns
            _spawnPositions = new Vector3[_enemiesToSpawn];
            _centralSpawnPoint = UFO.transform.position + GetRandomPosition(2, 3);

            for (int i = 0; i < _enemiesToSpawn; i++)
            {
                _spawnPositions[i] = UnityEngine.Random.insideUnitSphere * 1.25f + _centralSpawnPoint;
            }            

            //set the audio source to play where the wave will spawn
            WaveAudioSource.gameObject.transform.position = _centralSpawnPoint;
            AudioManager.Instance.PlayWaveStart(WaveAudioSource);

            _secsSinceEnemySpawn = 0;
            Enemy enemySpawn = null;

            //spawn enemies
            for (int i = 0; i < _enemiesToSpawn; i++)
            {
                switch (UnityEngine.Random.Range((int)0, (int)4).ToString())
                {
                    case "3":
                        enemySpawn = EnemyRedSource;
                        break;
                    case "0":
                    case "1":
                    case "2":
                        enemySpawn = EnemyGreenSource;
                        break;                    
                    default:
                        break;
                }

                enemySpawn = GameObject.Instantiate(enemySpawn);
                enemySpawn.gameObject.SetActive(true);
                enemySpawn.gameObject.transform.position = _spawnPositions[i];
                enemySpawn.gameObject.transform.rotation = Quaternion.LookRotation((enemySpawn.gameObject.transform.position - UFO.transform.position).normalized); //rotate to look at UFO
            }

            //if (Math.IEEERemainder(_pacifyCount, 4) == 1)
            //{
            //    Mothership mothership = Instantiate(EnemyMothershipSource);
            //    mothership.gameObject.SetActive(true);
            //    mothership.gameObject.transform.position = WaveAudioSource.gameObject.transform.position * 1.3f;
            //    mothership.gameObject.transform.rotation = Quaternion.LookRotation((mothership.gameObject.transform.position - UFO.transform.position).normalized); //rotate to look at UFO

            //    StartCoroutine(ShowText("Mothership Spawned", TextType.PlayerInfo));
            //}

            if (Math.IEEERemainder(_pacifyCount, 8) == 4)            
            {                
                Nuke nuke = Instantiate(NukeSource);
                nuke.gameObject.SetActive(true);
                nuke.gameObject.transform.position = GetRandomPosition(0.5f, 1f, 0.5f, 1.5f);

                StartCoroutine(ShowText("Nuke Deployed", TextType.PlayerInfo));
            }

            _pacifyCount++;
            _secsSinceEnemySpawn = 0;
        }
        else
        {
            _secsSinceEnemySpawn += Time.deltaTime * GameManager.Instance.Speed;
        }
    }

    private IEnumerator PollMovementForSpectatorCamera()
    {
        float t = 0;
        float fraction = 0;

        _isMovingSpectatorCam = true;
        //Vector3 endPosition = UFO.transform.position - (_centralSpawnPoint - UFO.transform.position).normalized * 1f + Vector3.up * 0.4f;
        //Quaternion endRotation = Quaternion.LookRotation(_centralSpawnPoint - SpecatorCamera.transform.position);
        //while (t < 3)
        //{
        //    fraction = t / 3f;
        //    SpecatorCamera.transform.position = Vector3.Lerp(SpecatorCamera.transform.position, endPosition, fraction);
        //    SpecatorCamera.transform.rotation = Quaternion.Lerp(SpecatorCamera.transform.rotation, Quaternion.LookRotation(_centralSpawnPoint - SpecatorCamera.transform.position), fraction);
        //    t += Time.deltaTime;
        //    yield return new WaitForSeconds(0.01f);
        //}        

        Vector3 endPosition = UFO.transform.position - (Camera.transform.forward) * 1f + Vector3.up * 0.4f;
        Quaternion endRotation = Quaternion.LookRotation(Camera.transform.forward);
        while (t < 0.3f)
        {
            fraction = t / 4f;
            SpecatorCamera.transform.position = Vector3.Lerp(SpecatorCamera.transform.position, endPosition, fraction);
            SpecatorCamera.transform.rotation = Quaternion.Lerp(SpecatorCamera.transform.rotation, endRotation, fraction);
            t += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }

        _isMovingSpectatorCam = false;
    }

    private void CreateSpawnParticles()
    {
        _spawnParticles = Instantiate(SpawnIn);
        _spawnParticles.transform.position = _centralSpawnPoint;
        _spawnParticles.transform.rotation = Quaternion.LookRotation(UFO.transform.position - _centralSpawnPoint);
        _spawnParticles.gameObject.SetActive(true);
    }

    private void SpawnParticles(int system)
    {
        if (system == 1)
        {
            _spawnParticles.SmallPulse.Play();
        }
        else if (system == 2)
        {
            _spawnParticles.SmallPulse.Play();
        }
        else if (system == 3)
        {
            _spawnParticles.BigPulse.Play();
            _spawnParticles.BigPulseSparks.Play();
        }
    }
   
    IEnumerator ShowText(string text, TextType textType, GameObject relatedObject = null)
    {
        float t = 0;
        Vector3 startPosition;
        Vector3 startForwardCamera;
        Modular3DText modular3DText;        

        switch (textType)
        {
            case TextType.PlayerInfo:
                modular3DText = Instantiate(Modular3DText);

                modular3DText.gameObject.SetActive(true);
                modular3DText.UpdateText(text);

                startForwardCamera = Camera.transform.forward.normalized * 2;
                startPosition = Camera.transform.position + startForwardCamera;
                modular3DText.transform.rotation = Quaternion.LookRotation(Camera.transform.forward);

                while (t < 1.5f)
                {
                    modular3DText.transform.position = startPosition - (startForwardCamera * t);
                    t += Time.deltaTime;
                    yield return new WaitForSeconds(0.01f);
                }

                modular3DText.gameObject.SetActive(false);

                break;
            case TextType.EnemyScore:
                modular3DText = Instantiate(EnemyScore3DText);

                modular3DText.gameObject.SetActive(true);
                modular3DText.UpdateText(text);

                startPosition = relatedObject.transform.position;
                modular3DText.transform.rotation = Quaternion.LookRotation(Camera.transform.forward);                

                while (t < 0.75f)
                {
                    modular3DText.transform.position = startPosition + (Vector3.up * (t/4f));
                    t += Time.deltaTime;
                    yield return new WaitForSeconds(0.01f);
                }

                modular3DText.gameObject.SetActive(false);

                break;                
            default:
                break;
        }                
    }

    IEnumerator ShootBeam()
    {
        ActiveLaserBeam.transform.position = UFO.transform.position;
        var beamDirection = (RightController.transform.position - UFO.transform.position).normalized;
        //yield return new WaitForSeconds(0.1f);
        
        GameObject firstHitObject = null;
        float radiusToCheck = 0.05f;        
        RaycastHit[] hitObjects;

        //check for objects, increasing radius as we go
        bool destoryAll = true; //first check destroys all in path, subsequent only destroys first hit, not all as radius too wide
        while (radiusToCheck < 1f)
        {
            Vector3 sphereRadiusLength = beamDirection * radiusToCheck;
            hitObjects = Physics.SphereCastAll(UFO.transform.position + sphereRadiusLength, radiusToCheck, beamDirection, 10f);            
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
            beamDirection = (firstHitObject.transform.position - UFO.transform.position).normalized;
        }
        else
        {
            ComboMultiplier = 0;
        }

        if (beamDirection != Vector3.zero)
        {
            ActiveLaserBeam.transform.rotation = Quaternion.LookRotation(beamDirection);
        }
        ActiveLaserBeam.gameObject.SetActive(true);

        while (_secsSinceBeam < 0.15f)
        {
            ActiveLaserBeam.transform.position += beamDirection * 20 * Time.unscaledDeltaTime;

            yield return null;
        }

        //yield return new WaitForSeconds(0.2f);

        if (ActiveLaserBeam.gameObject.activeSelf)
        {
            GameObject.Destroy(ActiveLaserBeam.gameObject);
        }
    }

    public void IncreaseScore(int score, GameObject enemy)
    {
        ComboMultiplier++;
        if (!NukeIsExploding && Math.IEEERemainder(ComboMultiplier, 5) == 0)
        {
            StartCoroutine(ShowText(ComboMultiplier.ToString() + "X Multiplier!", TextType.PlayerInfo));
        }
        score = score * ComboMultiplier;
        Score += score;

        StartCoroutine(ShowText(score.ToString(), TextType.EnemyScore, enemy));

        if (!NukeIsExploding)
        {
            UpdateScore();
        }        
    }

    public void UpdateScore()
    {
        ScoreboardText.UpdateText(Score);
    }

    public void StartGame()
    {
        if (!IsStarted)
        {
            DayNightCycleSpeed = 1;

            CurrentScoreboard.SetActive(true);
            HighScoreboard.SetActive(false);

            foreach (var item in SelectStartTypes)
            {
                item.gameObject.SetActive(false);
            }

            Score = 0;
            ComboMultiplier = 0;
            Wave = 0;
            IsStarted = true;
            ScoreboardText.UpdateText(Score);

            if (RightController.GetComponentInChildren<Controller>() != null)
            {
                RightController.GetComponentInChildren<Controller>().gameObject.SetActive(false);
            }

            //_sightLine = GameObject.Instantiate(SightLineSource);
            //_sightLine.SetActive(true);
            _secsSinceBeam = 0;
            _secsSinceEnemySpawn = 0;
            _waveWarnsPlayed = 0;
            if (ResetToStartOnDeath)
            {
                switch (GameType)
                {
                    case GameType.Arcade:
                        _enemiesToSpawn = 3;
                        UFO.ToggleGunVisibility(true);
                        break;
                    case GameType.Challenge:
                        _enemiesToSpawn = 3;
                        UFO.ToggleGunVisibility(true);
                        break;
                    case GameType.Pacifism:
                        _enemiesToSpawn = 1;
                        _pacifyCount = 0;
                        UFO.ToggleGunVisibility(false);
                        break;
                    default:
                        break;
                }                
            }

            try
            {
                if (_destroyer != null)
                {
                    if (_destroyer.GetType() == typeof(Enemy))
                    {
                        Destroy(((Enemy)_destroyer).gameObject);
                    }

                    _destroyer = null;
                }

                ProjectileRenderer.Instance.ClearProjectiles();
            }
            catch (Exception)
            {
                //sometimes the _destroyer is already destroyed, shrug
            }

            AudioManager.Instance?.MusicSource.Play();
        }
    }

    public void StopGame()
    {
        if (!DebugPlay && IsStarted)
        {
            CurrentScoreboard.SetActive(false);
            HighScoreboard.SetActive(true);

            UFO.gameObject.transform.position = new Vector3(0, 1.1f, 0);
            UFO.ToggleGunVisibility(false);

            foreach (var item in SelectStartTypes)
            {
                item.gameObject.SetActive(true);
            }            

            IsStarted = false;
            AudioManager.Instance.MusicSource.Stop();

            foreach (var item in FindObjectsOfType<Enemy>())
            {
                if ((object)item != _destroyer)
                {
                    Destroy(item.gameObject);
                }
                else
                {
                    //it is the destroyer, so clear all projectiles instead
                    ProjectileRenderer.Instance.ClearProjectiles();
                }
            }

            foreach (var item in FindObjectsOfType<Nuke>())
            {
                Destroy(item.gameObject);
            }

            SubmitScore();
            StartCoroutine(ShowScores());
        }
    }

    private void SubmitScore()
    {
        LastScoreText.UpdateText(Score);
        dl.AddScore(SystemInfo.deviceUniqueIdentifier.Substring(0,12), Score);
    }

    private IEnumerator ShowScores()
    {
        //Debug.Log("Getting Scores")
        dl.GetScores();
        List<dreamloLeaderBoard.Score> scoreList = dl.ToListHighToLow();        

        float maxTimeToWait = 5f;
        float t = 0;
        while (t < maxTimeToWait)
        {
            t += Time.deltaTime;
            if (scoreList.Count == 0)
            {
                //GUILayout.Label("(loading...)");
                HighScoresText.UpdateText("Loading...");
                scoreList = dl.ToListHighToLow();
            }
            else
            {
                int maxToDisplay = 10;
                int count = 0;
                string strScores = "";

                var currentPlayerScore = scoreList.Where(s => s.playerName.Substring(0, 12) == SystemInfo.deviceUniqueIdentifier.Substring(0, 12) && s.score < Score).FirstOrDefault();

                if (currentPlayerScore.score > 0)
                {
                    currentPlayerScore.score = Score;
                    scoreList = scoreList.OrderByDescending(s => s.score).ToList();
                }

                foreach (dreamloLeaderBoard.Score currentScore in scoreList)
                {
                    count++;

                    if (count == 1)
                    {
                        HighestScoreText.UpdateText(currentScore.score.ToString());
                    }
                    
                    strScores += currentScore.playerName + " - " + currentScore.score.ToString();
                    
                    //GUILayout.BeginHorizontal();
                    //GUILayout.Label(currentScore.playerName, width200);
                    //GUILayout.Label(currentScore.score.ToString(), width200);
                    //GUILayout.EndHorizontal();

                    if (count >= maxToDisplay) break;

                    strScores += Environment.NewLine;
                }

                HighScoresText.UpdateText(strScores);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public static Vector3 GetRandomPosition(float minRange, float maxRange, float ymin = 0.1f, float ymax = 2f)
    {
        return new Vector3(GetRandomPositionComponent(minRange, maxRange, true), GetRandomPositionComponent(ymin, ymax, false), GetRandomPositionComponent(minRange, maxRange, true));
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

public enum TextType
{
    PlayerInfo,
    EnemyScore
}