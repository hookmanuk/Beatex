using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour
{
    public GameObject Gun;
    public GameObject Body;

    private Vector3 _gunPosition;

    // Start is called before the first frame update
    void Start()
    {
        //_gunPosition = Gun.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsStarted)
        {
            //Gun.transform.position = Body.transform.position + _gunPosition * 0.03f;

            var targetDirection = (GameManager.Instance.RightController.transform.position - GameManager.Instance.LeftController.transform.position).normalized;
            this.transform.rotation = Quaternion.LookRotation(new Vector3(targetDirection.x, targetDirection.y, targetDirection.z));
        }
        //Body.transform.rotation = Quaternion.LookRotation(new Vector3(targetDirection.x, targetDirection.y, targetDirection.z));
    }
}
