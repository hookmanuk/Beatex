using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuke : MonoBehaviour
{
    public bool Exploding { get; set; } = false;
    public ParticleSystem DeathParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Explode()
    {
        GameManager.Instance.NukeIsExploding = true;
        Exploding = true;

        if (DeathParticleSystem != null)
        {
            DeathParticleSystem.Play();
        }

        Collider[] objectsNukeKilled = Physics.OverlapSphere(transform.position, 3f);
        foreach (var item in objectsNukeKilled)
        {
            var enemy = item.gameObject.GetComponentInParent<Enemy>();
            if (enemy != null)
            {                
                enemy.Hit(true);
            }
            var nuke = item.gameObject.GetComponent<Nuke>();
            if (nuke != null && !nuke.Exploding)
            {
                nuke.Explode();
            }
        }

        foreach (var list in ProjectileRenderer.Instance.projectiles)
        {
            for (int i = 0; i < list.Value.Count; i++)
            {
                if ((transform.position - list.Value[i].pos).sqrMagnitude < 3*3f)
                {
                    list.Value.RemoveAt(i);
                    i--;
                }
            }            
        }
        
        StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        GameManager.Instance.NukeIsExploding = false;
        GameManager.Instance.UpdateScore();
    }
}
