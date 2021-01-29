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
    public Modular3DText HighScoresText;
    public GameObject HighScoreboard;
    public GameObject CurrentScoreboard;
    public bool ResetToStartOnDeath = true;
    public bool DebugPlay;
    public LaserBeam ActiveLaserBeam {get; set;}
    public int Wave { get; set; } = 0;
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
    public int ComboMultiplier = 0;
    public int Score = 0;

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

        if (DebugPlay)
        {
            StartGame();
        }
        else
        {
            StartCoroutine(ShowScores());
        }
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
        if (_waveWarnsPlayed == 0 && _msSinceEnemySpawn >= (60 / AudioManager.Instance.BPM * 11)) //on 11th beat
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
                if (Math.IEEERemainder(_enemiesToSpawn - 5, 2) == 0)
                {
                    Mothership mothership = Instantiate(EnemyMothershipSource);
                    mothership.gameObject.SetActive(true);
                    mothership.gameObject.transform.position = WaveAudioSource.gameObject.transform.position;
                }
            }

            _enemiesToSpawn++;
            
            AudioManager.Instance.PlayWaveNo();
            Wave++;
            StartCoroutine(ShowText("Wave " + Wave.ToString(), TextType.PlayerInfo));
        }
        else
        {
            _msSinceEnemySpawn += Time.deltaTime * GameManager.Instance.Speed;
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
        else
        {
            ComboMultiplier = 0;
        }

        if (beamDirection != Vector3.zero)
        {
            ActiveLaserBeam.transform.rotation = Quaternion.LookRotation(beamDirection);
        }
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

    public void IncreaseScore(int score, GameObject enemy)
    {
        ComboMultiplier++;
        if (Math.IEEERemainder(ComboMultiplier, 5) == 0)
        {
            StartCoroutine(ShowText(ComboMultiplier.ToString() + "X Multiplier!", TextType.PlayerInfo));
        }
        score = score * ComboMultiplier;
        Score += score;

        StartCoroutine(ShowText(score.ToString(), TextType.EnemyScore, enemy));
        ScoreboardText.UpdateText(Score);
    }

    public void StartGame()
    {
        if (!IsStarted)
        {
            CurrentScoreboard.SetActive(true);
            HighScoreboard.SetActive(false);
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

                    _destroyer = null;
                }

                ProjectileRenderer.Instance.ClearProjectiles();
            }
            catch (Exception)
            {
                //sometimes the _destroyer is already destroyed, shrug
            }

            AudioManager.Instance.MusicSource.Play();
        }
    }

    public void StopGame()
    {
        if (!DebugPlay && IsStarted)
        {
            CurrentScoreboard.SetActive(false);
            HighScoreboard.SetActive(true);
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

            SubmitScore();
            StartCoroutine(ShowScores());
        }
    }

    private void SubmitScore()
    {
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

public enum TextType
{
    PlayerInfo,
    EnemyScore
}