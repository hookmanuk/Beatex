using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SlowMotion : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public bool Exploding { get; set; } = false;
    private Volume _volume;

    // Start is called before the first frame update
    void Start()
    {
        _volume = GetComponentInChildren<Volume>();
    }

    public void Explode()
    {        
        Exploding = true;

        if (ParticleSystem != null)
        {
            ParticleSystem.Play();
        }

        //StartCoroutine(GameManager.Instance.SetTime(0.4f));
        GameManager.Instance.SetTimeAbsolute(0.75f);
        AudioManager.Instance.ChangePitch(0.75f);

        gameObject.GetComponent<MeshRenderer>().enabled = false;

        StartCoroutine(IncreaseSize());

        StartCoroutine(Deactivate());
    }

    IEnumerator IncreaseSize()
    {                
        float t = 0;
        
        while (t < 1)
        {
            _volume.blendDistance = t * 10;
            //transform.localScale += transform.localScale * (1 + 19f * t); //scale up 20x over 0.1 secs
            t += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }     
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(AudioManager.Instance.BPM / 60 * 4); //this is going to be the slowed time, not real time!
        GameManager.Instance.SetTimeAbsolute(1f);
        AudioManager.Instance.ChangePitch(1f);
        //StartCoroutine(GameManager.Instance.SetTime(1f));
        gameObject.SetActive(false);
    }
}
