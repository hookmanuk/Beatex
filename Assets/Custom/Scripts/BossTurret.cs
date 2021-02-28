using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTurret : MonoBehaviour
{
    public EnemyType BulletType;
    private MeshRenderer _meshRenderer;
    private Material _blueMaterial;
    private Material _greenMaterial;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetBulletType(EnemyType bulletType)
    {
        if (bulletType != BulletType)
        {
            if (bulletType == EnemyType.Blue)
            {
                _meshRenderer.material = GetBlueMaterial();
            }
            else if (bulletType == EnemyType.Green)
            {
                _meshRenderer.material = GetGreenMaterial();
            }

            BulletType = bulletType;
        }
    }

    private Material GetBlueMaterial()
    {
        if (_blueMaterial == null)
        {
            _blueMaterial = GameManager.Instance.BlueBulletSource.GetComponent<MeshRenderer>().material;
        }

        return _blueMaterial;
    }

    private Material GetGreenMaterial()
    {
        if (_greenMaterial == null)
        {
            _greenMaterial = GameManager.Instance.GreenBulletSource.GetComponent<MeshRenderer>().material;
        }

        return _greenMaterial;
    }
}
