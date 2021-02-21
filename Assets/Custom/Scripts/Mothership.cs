using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mothership : Enemy
{
    public MeshRenderer MothershipMesh;
    private float lastRotationDegree = 0;
    // Start is called before the first frame update
    void Start()
    {
        _hitRate = 0;
        _health = 3;        
    }

    // Update is called once per frame
    void FixedUpdate()
    {                
        this.transform.Rotate(Vector3.up, Time.deltaTime*60,Space.Self);        

        if (lastRotationDegree - transform.rotation.eulerAngles.x > 359) //we've done a full rotation
        {
            //spawn enemy
            StartCoroutine(SpawnEnemy());
        }

        lastRotationDegree = transform.rotation.eulerAngles.x;        
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(0.7f);
        Enemy enemySpawn = GameObject.Instantiate(GameManager.Instance.EnemyRedSource);
        enemySpawn.gameObject.SetActive(true);
        enemySpawn.gameObject.transform.position = transform.position;
        enemySpawn.gameObject.transform.rotation = Quaternion.LookRotation((enemySpawn.gameObject.transform.position - GameManager.Instance.PlayerShip.transform.position).normalized); //rotate to look at UFO
    }
}
