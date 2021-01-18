using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float BPM { get; set; }

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
}
