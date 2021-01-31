using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotion : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public bool Exploding { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        
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

        StartCoroutine(Deactivate());
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
