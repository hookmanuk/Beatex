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
        if (_bulletSource == null)
        {
            switch (Type)
            {
                case EnemyType.Green:
                    _bulletSource = GameManager.Instance.GreenBulletSource;
                    break;
                case EnemyType.Red:
                    _bulletSource = GameManager.Instance.RedBulletSource;
                    break;
                case EnemyType.Blue:
                    _bulletSource = GameManager.Instance.BlueBulletSource;
                    break;
                default:
                    break;
            }
        }

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
                Bullet bullet = GameObject.Instantiate<Bullet>(_bulletSource);
                bullet.gameObject.transform.position = transform.position;
                bullet.Direction = (GameManager.Instance.LeftController.transform.position - transform.position).normalized;
            }
            else
            {
                _msSinceShot += Time.deltaTime;
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