using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Cage : MonoBehaviour
{
    public Volume BoxVolume;

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        BoxVolume.transform.position = pos;
    }
}
