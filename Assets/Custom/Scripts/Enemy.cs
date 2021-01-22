using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType Type;
    private float _msSinceShot = 0;
    private bool _isHit = false;
    private float _hitRate;

    private Bullet _bulletSource;

    // Start is called before the first frame update
    void Start()
    {
        switch (Type)
        {
            case EnemyType.Green:
                _hitRate = 4;
                break;
            case EnemyType.Red:
                _hitRate = 0;
                break;
            case EnemyType.Blue:
                _hitRate = 2;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {        
        if (GameManager.Instance.IsStarted)
        {
            if (GameManager.Instance.ActiveLaserBeam != null && GameManager.Instance.ActiveLaserBeam.isActiveAndEnabled && !_isHit)
            {
                //this is wrong, only checking for middle of beam
                var diff = transform.position - GameManager.Instance.ActiveLaserBeam.transform.position;

                //Debug.Log(diff.magnitude);
                if (diff.magnitude < 0.1)
                {
                    Explode();
                }
            }

            if (_hitRate > 0)
            {
                if (_msSinceShot >= (60 / AudioManager.Instance.BPM * _hitRate)) //every 1 beats
                {
                    _msSinceShot = 0;
                    Bullet bullet = null;
                    switch (Type)
                    {
                        case EnemyType.Green:
                            while (bullet?.gameObject.activeSelf ?? true)
                            {
                                bullet = Bullet.GreenBullets[Bullet.GreenBulletCount];
                                Bullet.GreenBulletCount++;
                                if (Bullet.GreenBulletCount > 999)
                                {
                                    Bullet.GreenBulletCount = 0;
                                }
                            }

                            break;
                        case EnemyType.Red:
                            //while (bullet?.gameObject.activeSelf ?? true)
                            //{
                            //    bullet = Bullet.RedBullets[Bullet.RedBulletCount];
                            //    Bullet.RedBulletCount++;
                            //    if (Bullet.RedBulletCount > 999)
                            //    {
                            //        Bullet.RedBulletCount = 0;
                            //    }
                            //}
                            break;
                        case EnemyType.Blue:
                            while (bullet?.gameObject.activeSelf ?? true)
                            {
                                bullet = Bullet.BlueBullets[Bullet.BlueBulletCount];
                                Bullet.BlueBulletCount++;
                                if (Bullet.BlueBulletCount > 999)
                                {
                                    Bullet.BlueBulletCount = 0;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    bullet.gameObject.SetActive(true);
                    bullet.gameObject.transform.position = transform.position;
                    bullet.Direction = (GameManager.Instance.UFO.transform.position - transform.position).normalized;
                    bullet.gameObject.transform.rotation = Quaternion.LookRotation(bullet.Direction);
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

    private void Hit()
    {
        _isHit = true;
        GameManager.Instance.EnemiesHit.Add(this);
    }

    public void Explode()
    {
        GetComponent<AudioSource>().Play();
        StartCoroutine(Brighten());        
    }
    
    IEnumerator Brighten()
    {
        float t = 0;
        float intensity = 5f;

        var material = GetComponentInChildren<MeshRenderer>().material;
        var col = material.GetColor("_EmissionColor");

        while (t < 0.1f)
        {
            intensity = 5 + t / 0.1f * 3;
            float factor = Mathf.Pow(2, intensity);
            material.SetColor("_EmissionColor", new Color(col.r * factor, col.g * factor, col.b * factor));
            yield return new WaitForSeconds(0.01f);
            t += 0.01f;
        }

        while (t < 0.4f)
        {
            intensity = 8 - t / 0.4f * 8;
            float factor = Mathf.Pow(2, intensity);
            material.SetColor("_EmissionColor", new Color(col.r * factor, col.g * factor, col.b * factor));
            yield return new WaitForSeconds(0.01f);
            t += 0.01f;
        }

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
                    transform.position += vector * 0.5f * Time.deltaTime;
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
    Blue
}