using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float BPM { get; set; }
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public AudioClip EnemyBirth;
    public AudioClip EnemyHit;
    public AudioClip EnemyDeath;
    public AudioClip WaveWarn;
    public AudioClip WaveStart;
    public AudioClip[] Waves;

    private static AudioManager _instance;
    public static AudioManager Instance {
        get {
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
        BPM = 120;
    }
   
    public void PlayWaveNo()
    {
        if (GameManager.Instance.Wave < 15)
        {
            SFXSource.PlayOneShot(Waves[GameManager.Instance.Wave]);
        }        
    }

    public void PlayWaveWarn(AudioSource enemySource)
    {        
        enemySource.PlayOneShot(WaveWarn);        
    }

    public void PlayWaveStart(AudioSource enemySource)
    {
        enemySource.PlayOneShot(WaveStart);
    }
}
