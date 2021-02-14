using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class UFO : MonoBehaviour
{
    public GameObject Gun;
    public GameObject Body;
    public bool GunVisible { get; set; } = true;

    private Vector3 _gunPosition;

    // Start is called before the first frame update
    void Start()
    {
        //_gunPosition = Gun.transform.localPosition;
        GetComponent<XRGrabInteractable>().onSelectEntered.AddListener(Selected);
        GetComponent<XRGrabInteractable>().onSelectExited.AddListener(Unselected);
    }

    public void Selected(XRBaseInteractor interactor)
    {
        GameManager.Instance.SetPrimaryController(interactor.gameObject);
    }

    public void Unselected(XRBaseInteractor interactor)
    {        
        if (GameManager.Instance.SecondaryHand != null)
        {
            GameManager.Instance.SecondaryHand.SetActive(true);
        }
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

    
}
