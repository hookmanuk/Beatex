using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerShip : MonoBehaviour
{
    public GameObject Gun;
    public GameObject Body;
    [ColorUsage(true, true)]
    public Color GreenPolarity;
    [ColorUsage(true, true)]
    public Color BluePolarity;
    public Material GunFillMaterial;
    public Material GunCapsuleMaterial;

    public bool GunVisible { get; set; } = true;

    private Vector3 _gunPosition;
    private Material _shipMaterial;

    // Start is called before the first frame update
    void Start()
    {
        //_gunPosition = Gun.transform.localPosition;
        GetComponent<XRGrabInteractable>().onSelectEntered.AddListener(Selected);
        GetComponent<XRGrabInteractable>().onSelectExited.AddListener(Unselected);

        _shipMaterial = Body.GetComponent<MeshRenderer>().material;
        FlipPolarity();
    }

    public void Selected(XRBaseInteractor interactor)
    {
        GameManager.Instance.SetPrimaryController(interactor.gameObject);               
    }

    public void Unselected(XRBaseInteractor interactor)
    {
        GameManager.Instance.UnselectControllers();        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GunVisible && GameManager.Instance.PrimaryController != null)
        {
            //Gun.transform.position = Body.transform.position + _gunPosition * 0.03f;

            var targetDirection = (GameManager.Instance.SecondaryController.transform.position - GameManager.Instance.PrimaryController.transform.position).normalized;

            if (targetDirection != Vector3.zero)
            {
                this.transform.rotation = Quaternion.LookRotation(targetDirection);
            }
            
        }
        else
        {
            //rotate based on movement direction instead?
        }
        //Body.transform.rotation = Quaternion.LookRotation(new Vector3(targetDirection.x, targetDirection.y, targetDirection.z));
    }

    public void ToggleGunVisibility(bool blnShowGun)
    {
        if (blnShowGun)
        {
            Gun.SetActive(true);
        }
        else
        {
            Gun.SetActive(false);
        }
    }

    public void FlipPolarity()
    {
        if (GameManager.Instance.Polarity == Polarity.Blue)
        {
            GameManager.Instance.Polarity = Polarity.Green;
            GunFillMaterial.SetColor("GradientColor", GreenPolarity);
            _shipMaterial.SetColor("_EmissionColor", GreenPolarity);
            GunCapsuleMaterial.SetColor("GradientColor", GreenPolarity * 0.8f);
        }
        else
        {
            GameManager.Instance.Polarity = Polarity.Blue;
            GunFillMaterial.SetColor("GradientColor", BluePolarity);
            _shipMaterial.SetColor("_EmissionColor", BluePolarity);
            GunCapsuleMaterial.SetColor("GradientColor", BluePolarity * 0.8f);
        }
        GameManager.Instance.LaserBeamSource.SetPolarity(GameManager.Instance.Polarity);
    }

    public void SetFillRate(float fillRate)
    {
        GunFillMaterial.SetFloat("GradientFillRate", fillRate);
    }

    public void SetCapsuleFillRate(float fillRate)
    {
        GunCapsuleMaterial.SetFloat("GradientFillRate", fillRate/10f + 0.43f);
    }
}
