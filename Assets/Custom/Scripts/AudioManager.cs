using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float BPM { get; set; }
    public AudioSource AudioSource;

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

    public void Play()
    {
        StartCoroutine(WaitThenPlay(0.01f));        
    }

    IEnumerator WaitThenPlay(float waitSecs)
    {
        yield return new WaitForSeconds(waitSecs);
        AudioSource.Play();
    }    
}
