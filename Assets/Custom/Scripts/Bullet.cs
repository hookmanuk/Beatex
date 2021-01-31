using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 Direction { get; set; }
    public static Bullet[] GreenBullets = new Bullet[10000];
    public static Bullet[] RedBullets = new Bullet[10000];
    public static Bullet[] BlueBullets = new Bullet[100000];
    public static int GreenBulletCount = 0;
    public static int RedBulletCount = 0;
    public static int BlueBulletCount = 0;
    private Rigidbody _rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {        
        if (GameManager.Instance.IsOnBeat)
        {
            //transform.position += Direction * 0.2f * Time.deltaTime * GameManager.Instance.Speed;            
            //transform.position += Direction * 0.1f * GameManager.Instance.Speed;
            StartCoroutine(Move());            
        }
    }

    IEnumerator Move()
    {
        float t = 0;        
        while (t < 0.1)
        {
            _rigidBody.position += Direction * 1f * Time.deltaTime;
            t += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }

        if (transform.position.y < 0)
        {
            this.gameObject.SetActive(false);
        }
        else if (Math.Abs(transform.position.x) > 4)
        {
            this.gameObject.SetActive(false);
        }
        else if (Math.Abs(transform.position.z) > 4)
        {
            this.gameObject.SetActive(false);
        }
    }
}