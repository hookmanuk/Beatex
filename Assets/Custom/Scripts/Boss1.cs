using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss1 : MonoBehaviour
{
    public float MovementX;
    public float Speed;
    public float MovementY;
    private List<GameObject> _bossTurrets;

    private Vector3 _startPosition;
    public int _movementPhase = 1;
    private Vector3 _sourcePosition = Vector3.zero;
    private Vector3 _targetPosition = Vector3.zero;
    private float t;
    private float _msSinceShot = 0;
    protected float _hitRate = 2f;

    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
        _bossTurrets = GetComponentsInChildren<BossTurret>().Select(bt => bt.gameObject).ToList();
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
                    _targetPosition = transform.localPosition + transform.right * MovementX;
                }                
                
                break;
            case 2:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition + transform.up * MovementY;
                }
                
                break;
            case 3:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition - transform.right * MovementX;
                }                

                break;
            case 4:
                if (_sourcePosition == Vector3.zero)
                {
                    t = 0;
                    _sourcePosition = transform.localPosition;
                    _targetPosition = transform.localPosition - transform.up * MovementY;
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

        if (_msSinceShot >= (60 / AudioManager.Instance.BPM * _hitRate))
        {
            _msSinceShot = 0;
            
            foreach (var item in _bossTurrets)
            {
                //this works when boss at zero rotation, suspect it breaks when it rotates though!
                ProjectileRenderer.Instance.SpawnProjectile(item.transform.position, Quaternion.LookRotation(transform.forward), EnemyType.Blue);
            }            
        }
        else
        {
            _msSinceShot += Time.deltaTime * GameManager.Instance.Speed;
        }
    }
}
