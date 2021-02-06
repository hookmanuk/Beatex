using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    public float Speed;
    public Light Light;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, Speed * Time.deltaTime);
        transform.LookAt(Vector3.zero);

        if (transform.position.y < -11f)
        {
            if (Light.enabled)
            {
                Light.enabled = false;
            }
        }
        else if (transform.position.y > 11f)
        {
            if (!Light.enabled)
            {
                Light.enabled = true;
            }
        }
    }
}
