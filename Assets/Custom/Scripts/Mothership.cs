using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mothership : Enemy
{
    private float lastRotationDegree = 0;
    // Start is called before the first frame update
    void Start()
    {
        _hitRate = 0;
        _health = 3;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Time.deltaTime * 60, 0, 0);

        if (lastRotationDegree - transform.rotation.eulerAngles.x > 300) //we've done a full rotation
        {            
            //spawn enemy
            Enemy enemySpawn = GameObject.Instantiate(GameManager.Instance.EnemyRedSource);
            enemySpawn.gameObject.SetActive(true);
            enemySpawn.gameObject.transform.position = transform.position;
        }

        lastRotationDegree = transform.rotation.eulerAngles.x;        
    }
}
