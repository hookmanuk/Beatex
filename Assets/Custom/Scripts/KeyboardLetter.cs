using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyboardLetter : MonoBehaviour
{
    public Material ScoreboardText;
    public Material ScoreboardTextSelected;
    public bool IsDel;
    public bool IsOK;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<XRSimpleInteractable>().onSelectEntered.AddListener(Selected);
        GetComponent<XRSimpleInteractable>().onSelectExited.AddListener(Unselected);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Selected(XRBaseInteractor interactor)
    {
        if (IsDel)
        {
            foreach (var item in GetComponentsInChildren<MeshRenderer>())
            {
                item.material = ScoreboardTextSelected;
            }
            GameManager.Instance.SelectedLetter = "del";
        }
        else if (IsOK)
        {
            foreach (var item in GetComponentsInChildren<MeshRenderer>())
            {
                item.material = ScoreboardTextSelected;
            }            
            GameManager.Instance.SelectedLetter = "ok";
        }
        else
        {
            this.GetComponent<MeshRenderer>().material = ScoreboardTextSelected;
            GameManager.Instance.SelectedLetter = this.name.Substring(0, 1);
        }        
    }

    public void Unselected(XRBaseInteractor interactor)
    {
        if (IsDel)
        {
            foreach (var item in GetComponentsInChildren<MeshRenderer>())
            {
                item.material = ScoreboardText;
            }
        }
        else if (IsOK)
        {
            foreach (var item in GetComponentsInChildren<MeshRenderer>())
            {
                item.material = ScoreboardText;
            }
        }
        else
        {
            this.GetComponent<MeshRenderer>().material = ScoreboardText;
        }
    }    
}
