using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType Type;
    public ParticleSystem[] DeathParticleSystems;
    public ParticleSystem[] HitParticleSystems;
    private float _msSinceShot = 0;
    public bool IsHit = false;
    protected float _hitRate;
    protected int _health = 1;
    protected int _score = 10;

    private Bullet _bulletSource;


    // Start is called before the first frame update
    void Start()
    {
        switch (Type)
        {
            case EnemyType.Green:
                _hitRate = 2;
                _health = 1;
                _score = 10;
                break;
            case EnemyType.Red:
                _hitRate = 0;
                _health = 1;
                _score = 10;
                break;
            case EnemyType.Blue:
                _hitRate = 1f;
                _health = 2;
                _score = 20;
                break;
            case EnemyType.Mothership:
                _hitRate = 0;
                _health = 1;
                _score = 50;
                break;
            default:
                break;
        }

        //GetComponent<AudioSource>().PlayOneShot(AudioManager.Instance.EnemyBirth);
    }

    private void FixedUpdate()
    {        
        if (GameManager.Instance.IsStarted)
        {
            //if (GameManager.Instance.ActiveLaserBeam != null && GameManager.Instance.ActiveLaserBeam.isActiveAndEnabled && !_isHit)
            //{
            //    //this is wrong, only checking for middle of beam
            //    var diff = transform.position - GameManager.Instance.ActiveLaserBeam.transform.position;

            //    //Debug.Log(diff.magnitude);
            //    if (diff.magnitude < 0.1)
            //    {
            //        Explode();
            //    }
            //}

            if (_hitRate > 0)
            {
                if (_msSinceShot >= (60 / AudioManager.Instance.BPM * _hitRate)) //every 1 beats
                {
                    _msSinceShot = 0;
                    //Bullet bullet = null;
                    //int bulletsOutLoop = 0;
                    //switch (Type)
                    //{
                    //    case EnemyType.Green:                            
                    //        while (bullet?.gameObject.activeSelf ?? true)
                    //        {
                    //            bullet = Bullet.GreenBullets[Bullet.GreenBulletCount];
                    //            Bullet.GreenBulletCount++;
                    //            if (Bullet.GreenBulletCount >= Bullet.GreenBullets.Length - 1)
                    //            {
                    //                bulletsOutLoop++;
                    //                if (bulletsOutLoop == 2)
                    //                {
                    //                    break;
                    //                }
                    //                Bullet.GreenBulletCount = 0;
                    //            }
                    //        }

                    //        break;
                    //    case EnemyType.Red:
                    //        //while (bullet?.gameObject.activeSelf ?? true)
                    //        //{
                    //        //    bullet = Bullet.RedBullets[Bullet.RedBulletCount];
                    //        //    Bullet.RedBulletCount++;
                    //        //    if (Bullet.RedBulletCount > 999)
                    //        //    {
                    //        //        Bullet.RedBulletCount = 0;
                    //        //    }
                    //        //}
                    //        break;
                    //    case EnemyType.Blue:                            
                    //        while (bullet?.gameObject.activeSelf ?? true)
                    //        {
                    //            bullet = Bullet.BlueBullets[Bullet.BlueBulletCount];
                    //            Bullet.BlueBulletCount++;
                    //            if (Bullet.BlueBulletCount >= Bullet.BlueBullets.Length - 1)
                    //            {
                    //                bulletsOutLoop++;
                    //                if (bulletsOutLoop == 2)
                    //                {
                    //                    break;
                    //                }
                    //                Bullet.BlueBulletCount = 0;
                    //            }
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                    //if (bullet != null)
                    //{
                    //    bullet.gameObject.SetActive(true);
                    //    bullet.gameObject.transform.position = transform.position;
                    //    bullet.Direction = (GameManager.Instance.UFO.transform.position - transform.position).normalized;
                    //    bullet.gameObject.transform.rotation = Quaternion.LookRotation(bullet.Direction);
                    //}
                    ProjectileRenderer.Instance.SpawnProjectile(transform.position, Quaternion.LookRotation((GameManager.Instance.UFO.transform.position - transform.position).normalized), Type);                    
                }
                else
                {
                    _msSinceShot += Time.deltaTime * GameManager.Instance.Speed;
                }
            }

            if (GameManager.Instance.IsOnBeat)
            {                
                StartCoroutine(Move());
            }            
        }
    }    

    public void Hit(bool destroy = false)
    {
        _health -= 1;
        if (_health == 0 || destroy)
        { 
            IsHit = true;
            GameManager.Instance.IncreaseScore(_score, gameObject);
            StartCoroutine(HitDead());
        }
        else
        {
            //play some hit but not dead anim
            //GetComponent<AudioSource>().Play();
            StartCoroutine(HitNotDead());
        }
    }

    IEnumerator HitNotDead()
    {
        float t = 0;
        float intensity = 5f;

        GetComponent<AudioSource>().PlayOneShot(AudioManager.Instance.EnemyHit);

        if (Type == EnemyType.Mothership)
        {
            float fromDissolved = 0;
            float extraDissolved = 0;

            if (_health == 2)
            {
                fromDissolved = 0f;
                extraDissolved = 0.4f;
            }
            else if (_health == 1)
            {
                fromDissolved = 0.4f;
                extraDissolved = 0.1f;
            }

            if (HitParticleSystems != null)
            {
                foreach (var item in HitParticleSystems)
                {
                    item.Play();
                }
            }

            while (t < 0.1f)
            {
                ((Mothership)this).MothershipMesh.materials[1].SetFloat("DISSOLVED", fromDissolved + (extraDissolved / 0.1f * t));
                yield return new WaitForSeconds(0.01f);
                t += 0.01f;
            }
        }
        else
        {
            if (HitParticleSystems != null)
            {             
                foreach (var item in HitParticleSystems)
                {
                    item.Play();
                }
            }

            var material = GetComponentInChildren<MeshRenderer>().material;
            var col = material.GetColor("_EmissionColor");

            while (t < 0.1f)
            {
                intensity = (_health - 2) - t / 0.1f * 1;
                float factor = Mathf.Pow(2, intensity);
                material.SetColor("_EmissionColor", new Color(col.r * factor, col.g * factor, col.b * factor));
                yield return new WaitForSeconds(0.01f);
                t += 0.01f;
            }
        }
    }


    IEnumerator HitDead()
    {
        float t = 0;
        float intensity = 5f;

        GetComponent<AudioSource>().PlayOneShot(AudioManager.Instance.EnemyDeath);
        //var material = GetComponentInChildren<MeshRenderer>().material;
        //var col = material.GetColor("_EmissionColor");

        //while (t < 0.07f)
        //{
        //    intensity = t / 0.07f * 5;
        //    float factor = Mathf.Pow(2, intensity);
        //    //float factor = intensity;
        //    material.SetColor("_EmissionColor", new Color(col.r * factor, col.g * factor, col.b * factor));
        //    yield return new WaitForSeconds(0.01f);
        //    t += 0.01f;
        //}

        //while (t < 0.2f)
        //{
        //    intensity = 5 - t / 0.2f * 5;
        //    float factor = Mathf.Pow(2, intensity);
        //    material.SetColor("_EmissionColor", new Color(col.r * factor, col.g * factor, col.b * factor));
        //    yield return new WaitForSeconds(0.01f);
        //    t += 0.01f;
        //}
        if (DeathParticleSystems != null)
        {
            foreach (var item in DeathParticleSystems)
            {
                //item.Simulate(0,true,true);
                item.Play();
            }
        }
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        GameObject.Destroy(this.gameObject);        
    }

    IEnumerator Move()
    {
        Vector3 vector;
        float t = 0;

        switch (Type)
        {
            case EnemyType.Green:
                vector = Quaternion.AngleAxis(-80, Vector3.up) * (GameManager.Instance.UFO.transform.position - transform.position).normalized;                
                while (t < 0.1)
                {
                    transform.position += vector * 1.5f * Time.deltaTime;
                    t += Time.deltaTime;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case EnemyType.Red:
                vector = (GameManager.Instance.UFO.transform.position - transform.position).normalized;                
                while (t < 0.1)
                {
                    transform.position += vector * 1.5f * Time.deltaTime;
                    t += Time.deltaTime;
                    yield return new WaitForSeconds(0.01f);
                }
                break;
            case EnemyType.Blue:
                break;
            default:
                break;
        }
    }
}

public enum EnemyType
{
    Green,
    Red,
    Blue,
    Mothership
}