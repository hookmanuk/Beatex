using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType Type;
    private float _msSinceShot = 0;

    private Bullet _bulletSource;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {        
        if (GameManager.Instance.IsStarted)
        {
            if (GameManager.Instance.ActiveLaserBeam != null && GameManager.Instance.ActiveLaserBeam.isActiveAndEnabled)
            {
                var diff = transform.position - GameManager.Instance.ActiveLaserBeam.transform.position;

                //Debug.Log(diff.magnitude);
                if (diff.magnitude < 0.1)
                {
                    Explode();
                }
            }

            if (_msSinceShot >= (60 / AudioManager.Instance.BPM * 1)) //every 1 beats
            {
                _msSinceShot = 0;
                Bullet bullet = null;
                switch (Type)
                {
                    case EnemyType.Green:
                        while(bullet?.gameObject.activeSelf??true)
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
            }
            else
            {
                _msSinceShot += Time.deltaTime;
            }

            switch (Type)
            {
                case EnemyType.Green:
                    var vector = Quaternion.AngleAxis(-80, Vector3.up) * (GameManager.Instance.UFO.transform.position - transform.position);
                    this.transform.position += vector.normalized * 0.5f * Time.deltaTime;
                    break;
                case EnemyType.Red:
                    this.transform.position += (GameManager.Instance.UFO.transform.position - transform.position).normalized * 0.2f * Time.deltaTime;
                    break;
                case EnemyType.Blue:
                    break;
                default:
                    break;
            }
        }
    }

    public void Explode()
    {
        GameObject.Destroy(this.gameObject);
    }
}

public enum EnemyType
{
    Green,
    Red,
    Blue
}