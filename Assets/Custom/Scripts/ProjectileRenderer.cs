using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileRenderer : MonoBehaviour
{
    [Header("Data")]    
    public Mesh mesh;
    public Material BlueBulletMaterial;
    public Material GreenBulletMaterial;
    public float speed;
    public float damage;

    [Header("Instances")]
    public Dictionary<EnemyType, List<ProjectileData>> projectiles = new Dictionary<EnemyType, List<ProjectileData>>();    

    //Working values
    private RaycastHit[] rayHitBuffer = new RaycastHit[1];
    private Vector3 worldPoint;
    private Vector3 transPoint;
    private List<Matrix4x4[]> bufferedData = new List<Matrix4x4[]>();
    private GameObject _UFO;

    private static ProjectileRenderer _instance;
    public static ProjectileRenderer Instance
    {
        get
        {
            return _instance;
        }
    }

    public void ClearProjectiles()
    {
        foreach (var projectileListToDestroy in projectiles)
        {
            projectileListToDestroy.Value.Clear();
        }
    }

    private void Start()
    {
        _instance = this;
        projectiles.Add(EnemyType.Blue, new List<ProjectileData>());
        projectiles.Add(EnemyType.Green, new List<ProjectileData>());
        _UFO = GameManager.Instance.UFO.gameObject;
    }

    public void SpawnProjectile(Vector3 position, Quaternion rotation, EnemyType enemyType)
    {
        ProjectileData n = new ProjectileData();
        n.pos = position;
        n.rot = rotation;
        n.scale = Vector3.one * 0.008f;                
        n.damage = damage;
        n.enemyType = enemyType;

        projectiles[enemyType].Add(n);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsOnBeat)
        { 
            UpdateProjectiles(Time.deltaTime);            
        }
        BatchAndRender();
    }

    private void BatchAndRender()
    {
        bool blnQuitRendering = false;
        //If we dont have projectiles to render then just get out
        if (projectiles.Count <= 0)
            return;        

        foreach (var projectileList in projectiles)
        {
            //Clear the batch buffer
            bufferedData.Clear();

            Material material = null;
            switch (projectileList.Key)
            {
                case EnemyType.Green:
                    material = GreenBulletMaterial;
                    break;                    
                case EnemyType.Red:
                    break;
                case EnemyType.Blue:
                    material = BlueBulletMaterial;
                    break;
                case EnemyType.Mothership:
                    break;
                default:
                    break;
            }
           
            int count = projectileList.Value.Count;
            for (int i = 0; i < count; i += 1023)
            {
                int batchLimit = 1023;

                if (i + 1023 >= count)
                {
                    batchLimit = count - i;
                }                
                Matrix4x4[] tBuffer = new Matrix4x4[batchLimit];
                for (int ii = 0; ii < batchLimit; ii++)
                {
                    //check if projectile has hit the player
                    if ((projectileList.Value[i + ii].pos - _UFO.transform.position).sqrMagnitude < 0.01f)
                    {
                        var projectileHit = projectileList.Value[i + ii];
                        //destroy all other projectiles!
                        foreach (var projectileListToDestroy in projectiles)
                        {
                            projectileListToDestroy.Value.RemoveAll(p => p != projectileHit);
                        }
                        GameManager.Instance.StopGame();
                        blnQuitRendering = true;
                        break;
                    }
                    else
                    {
                        tBuffer[ii] = projectileList.Value[i + ii].renderData;
                    }
                }

                if (blnQuitRendering)
                {
                    break;
                }
                bufferedData.Add(tBuffer);
            }
            if (blnQuitRendering)
            {
                break;
            }

            //Draw each batch
            foreach (var batch in bufferedData)
                Graphics.DrawMeshInstanced(mesh, 0, material, batch, batch.Length);            
        }
    }

    private void UpdateProjectiles(float tick)
    {
        StartCoroutine(Move());
        //foreach (var projectileList in projectiles)
        //{ 
        //    foreach (var projectile in projectileList.Value)
        //    {
        //        //Sort out the projectiles 'forward' direction
        //        transPoint = projectile.rot * Vector3.forward;
        //        //See if its going to hit something and if so handle that
        //        //if (Physics.RaycastNonAlloc(projectile.pos, transPoint, rayHitBuffer, speed * tick) > 0)
        //        if (1 == 9)
        //        {
        //            //projectile.experation = -1;
        //            //worldPoint = rayHitBuffer[0].point;                    
        //            //SpawnSplash(worldPoint);
        //            //ConquestShipCombatController target = rayHitBuffer[0].rigidbody.GetComponent<ConquestShipCombatController>();
        //            //if (target.teamId != projectile.team)
        //            //{
        //            //    target.ApplyDamage(projectile.damage * projectile.damageScale, worldPoint);
        //            //}
        //        }
        //        else
        //        {
        //            //This project wont be hitting anything this tick so just move it forward
        //            projectile.pos += transPoint * speed;
        //        }
        //    }
        //}
        //Remove all the projectiles that are too far away
        //projectiles.RemoveAll(p => p.expiration <= 0);
    }

    IEnumerator Move()
    {
        float t = 0;
        while (t < 0.1)
        {
            foreach (var projectileList in projectiles)
            {
                foreach (var projectile in projectileList.Value)
                {
                    //Sort out the projectiles 'forward' direction
                    transPoint = projectile.rot * Vector3.forward;
                    projectile.pos += transPoint * speed * t;
                }
            }             
            t += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }

        foreach (var projectileList in projectiles)
        {
            projectileList.Value.RemoveAll(p => p.pos.y < 0 || Math.Abs(p.pos.x) > 4 || Math.Abs(p.pos.z) > 4);
        }        
    }

    private void SpawnSplash(Vector3 worlPoint)
    {
        //TODO: implament spawning of your splash effect e.g. the visual effect of a projectile hitting something
    }
}

public class ProjectileData
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public float expiration;    
    public float damage;
    public EnemyType enemyType;

    public Matrix4x4 renderData
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }
}
