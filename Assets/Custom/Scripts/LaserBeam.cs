using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color GreenPolarity;
    [ColorUsage(true, true)]
    public Color BluePolarity;
    public Material LaserBeamMaterial;

    // Start is called before the first frame update
    void Start()
    {        
    }

    public void SetPolarity(Polarity polarity)
    {
        if (polarity == Polarity.Blue)
        {
            LaserBeamMaterial.SetColor("_EmissionColor", BluePolarity);
        }
        else
        {
            LaserBeamMaterial.SetColor("_EmissionColor", GreenPolarity);
        }
    }
}
