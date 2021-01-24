using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float BPM { get; set; }
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public AudioClip EnemyBirth;
    public AudioClip EnemyDeath;
    public AudioClip WaveWarn;
    public AudioClip WaveStart;

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

    // Update is called once per frame
    void Update()
    {
        
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
