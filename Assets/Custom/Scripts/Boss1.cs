using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss1 : MonoBehaviour
{
    public float MovementX;
    public float Speed;
    public float MovementY;
    public float RotationSpeed;
    public BossTurret BlueTurret;
    public BossTurret GreenTurret;
    private List<BossTurret> _outerTurrets = new List<BossTurret>();
    private List<BossTurret> _innerTurrets = new List<BossTurret>();

    private Vector3 _startPosition;
    public int _movementPhase = 1;
    public int _shotPhase = 1;
    public int _lifeLeft = 3;
    private int _beatsInShotPhase = 0;
    private Vector3 _sourcePosition = Vector3.zero;
    private Vector3 _targetPosition = Vector3.zero;
    private float t;
    private float r;
    private float _msSinceShot = 0;
    protected float _hitRate = 2f;
    private Vector3 _startRight;
    private Vector3 _startUp;
    private EnemyType _phase1Type = EnemyType.Blue;
    private int _health = 3;

    public Vector3 centerPos = new Vector3(0, 0, 0);    //center of circle/elipsoid    
    public float radiusX, radiusY;                    //radii for each x,y axes, respectively
    public bool isCircular = false;                    //is the drawn shape a complete circle?
    public bool vertical = true;                    //is the drawb shape on the xy-plane?
    Vector3 pointPos;                                //position to place each prefab along the given circle/eliptoid
                                                     //*is set during each iteration of the loop



    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
        _startRight = new Vector3(transform.right.x, transform.right.y, transform.right.z);
        _startUp = new Vector3(transform.up.x, transform.up.y, transform.up.z);

        centerPos = _startPosition;

        //Outer circle
        CreateCircle(0.6f, _outerTurrets, 84);
        CreateCircle(0.1f, _innerTurrets, 7);
        CreateCircle(0.2f, _innerTurrets, 14);

        SetDissolved(-0.01f);

        //radiusX = 0.85f;
        //radiusY = 0.85f;

        //for (int i = 0; i < numPoints; i++)
        //{
        //    //multiply 'i' by '1.0f' to ensure the result is a fraction
        //    float pointNum = (i * 1.0f) / numPoints;

        //    //angle along the unit circle for placing points
        //    float angle = pointNum * Mathf.PI * 2;

        //    float x = Mathf.Sin(angle) * radiusX;
        //    float y = Mathf.Cos(angle) * radiusY;

        //    //position for the point prefab
        //    if (vertical)
        //        pointPos = new Vector3(x, y) + centerPos;
        //    else if (!vertical)
        //    {
        //        pointPos = new Vector3(x, 0, y) + centerPos;
        //    }

        //    //place the prefab at given position
        //    var gameObject = Instantiate(GreenTurret, pointPos, Quaternion.identity, transform);

        //    gameObject.transform.RotateAround(centerPos, new Vector3(0, 0, 1), transform.rotation.eulerAngles.z);
        //    gameObject.transform.RotateAround(centerPos, new Vector3(1, 0, 0), transform.rotation.eulerAngles.x);
        //    gameObject.transform.RotateAround(centerPos, new Vector3(0, 1, 0), transform.rotation.eulerAngles.y);

        //    _bossTurrets.Add(gameObject);
        //}
    }

    private void CreateCircle(float radius, List<BossTurret> turrets, int numPoints)
    {
        radiusX = radius;
        radiusY = radius;

        for (int i = 0; i < numPoints; i++)
        {
            //multiply 'i' by '1.0f' to ensure the result is a fraction
            float pointNum = (i * 1.0f) / numPoints;

            //angle along the unit circle for placing points
            float angle = pointNum * Mathf.PI * 2;

            float x = Mathf.Sin(angle) * radiusX;
            float y = Mathf.Cos(angle) * radiusY;

            //position for the point prefab
            if (vertical)
                pointPos = new Vector3(x, y) + centerPos;
            else if (!vertical)
            {
                pointPos = new Vector3(x, 0, y) + centerPos;
            }

            //place the prefab at given position
            var gameObject = Instantiate(BlueTurret, pointPos, Quaternion.identity, transform);

            gameObject.transform.RotateAround(centerPos, new Vector3(0, 0, 1), transform.rotation.eulerAngles.z);
            gameObject.transform.RotateAround(centerPos, new Vector3(1, 0, 0), transform.rotation.eulerAngles.x);
            gameObject.transform.RotateAround(centerPos, new Vector3(0, 1, 0), transform.rotation.eulerAngles.y);

            turrets.Add(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        switch (_movementPhase)
        {
            case 1:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition + _startRight * MovementX;
                }

                break;
            case 2:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition + _startUp * MovementY;
                }

                break;
            case 3:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition - _startRight * MovementX;
                }

                break;
            case 4:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition - _startUp * MovementY;
                }

                break;
            default:
                break;
        }

        t += Time.deltaTime * Speed;

        transform.localPosition = Vector3.Lerp(_sourcePosition, _targetPosition, t);

        if (transform.localPosition == _targetPosition)
        {
            _movementPhase++;
            if (_movementPhase > 4)
            {
                _movementPhase = 1;
            }
            _sourcePosition = Vector3.zero;
        }

        if (RotationSpeed > 0)
        {
            transform.Rotate(new Vector3(0, 0, 10f) * Time.deltaTime * RotationSpeed, Space.Self);
        }

        if (_msSinceShot >= (60 / AudioManager.Instance.BPM * _hitRate))
        {
            _msSinceShot = 0;
            int intTurret = 0;
            EnemyType enemyType = EnemyType.Blue;

            switch (_shotPhase)
            {
                case 1:
                    if (Math.IEEERemainder(_beatsInShotPhase, 2) == 0) //flip all bullets every 2 shots
                    {
                        if (_phase1Type == EnemyType.Blue)
                        {
                            _phase1Type = EnemyType.Green;
                        }
                        else if (_phase1Type == EnemyType.Green)
                        {
                            _phase1Type = EnemyType.Blue;
                        }                        
                    }
                    foreach (var item in _innerTurrets)
                    {
                        ProjectileRenderer.Instance.SpawnProjectile(item.transform.position, Quaternion.LookRotation(transform.forward), item.BulletType);

                        item.SetBulletType(_phase1Type);

                        intTurret++;
                    }
                    break;
                case 2:
                    foreach (var item in _outerTurrets)
                    {
                        ProjectileRenderer.Instance.SpawnProjectile(item.transform.position, Quaternion.LookRotation(transform.forward), item.BulletType);

                        if (Math.IEEERemainder(intTurret, 12) == 0) //flip the enemy every 12 turrets
                        {
                            if (enemyType == EnemyType.Blue)
                            {
                                enemyType = EnemyType.Green;
                            }
                            else
                            {
                                enemyType = EnemyType.Blue;
                            }
                        }
                        item.SetBulletType(enemyType);

                        intTurret++;
                    }

                    break;
                case 3:
                    if (_sourcePosition == Vector3.zero)
                    {
                        t = 0;
                        _sourcePosition = transform.localPosition;
                        _targetPosition = transform.localPosition - _startRight * MovementX;
                    }

                    break;
                case 4:
                    if (_sourcePosition == Vector3.zero)
                    {
                        t = 0;
                        _sourcePosition = transform.localPosition;
                        _targetPosition = transform.localPosition - _startUp * MovementY;
                    }

                    break;
                default:
                    break;
            }

            _beatsInShotPhase++;

            if (_beatsInShotPhase == 16)
            {
                _beatsInShotPhase = 0;
                _shotPhase++;

                if (_shotPhase > 2)
                {
                    _shotPhase = 1;
                }
            }

        }
        else
        {
            _msSinceShot += Time.deltaTime * GameManager.Instance.Speed;
        }
    }

    public void Hit()
    {        
        float extraDissolved = 0;
        _health--;

        if (_health == 2)
        {
            extraDissolved = 0f;
        }
        else if (_health == 1)
        {
            extraDissolved = 0.39f;
        }
        else if (_health == 0)
        {            
            StartCoroutine(HitDead());
        }

        //if (HitParticleSystems != null)
        //{
        //    foreach (var item in HitParticleSystems)
        //    {
        //        item.Play();
        //    }
        //}
        SetDissolved(extraDissolved);        
    }

    private void SetDissolved(float dissolved)
    {
        foreach (var item in ((Boss1)this).GetComponentsInChildren<MeshRenderer>())
        {
            item.materials[0].SetFloat("DISSOLVED", dissolved);
        }
    }

    private IEnumerator HitDead()
    {
        GetComponent<AudioSource>().PlayOneShot(AudioManager.Instance.EnemyDeath);
        GameManager.Instance.CurrentBoss = null;

        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        GameObject.Destroy(this.gameObject);
    }
}
