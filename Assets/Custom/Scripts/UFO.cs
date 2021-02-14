using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour
{
    public GameObject Gun;
    public GameObject Body;
    public bool GunVisible { get; set; } = false;

    private Vector3 _gunPosition;

    // Start is called before the first frame update
    void Start()
    {
        //_gunPosition = Gun.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.IsStarted && GunVisible)
        {
            //Gun.transform.position = Body.transform.position + _gunPosition * 0.03f;

            var targetDirection = (GameManager.Instance.RightController.transform.position - GameManager.Instance.LeftController.transform.position).normalized;

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
