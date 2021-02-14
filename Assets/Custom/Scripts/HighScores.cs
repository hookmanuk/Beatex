using MText;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScores : MonoBehaviour
{
    public ScoreMode ScoreMode;
    public GameObject Display;
    public GameObject Entry;
    public Modular3DText Name;

    private float _secsSinceClick = 1;

    // Start is called before the first frame update
    void Start()
    {     
    }

    // Update is called once per frame
    void Update()
    {
        if (ScoreMode == ScoreMode.Entry)
        {
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

            bool triggerValue = false;
            rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue);

            if (triggerValue && _secsSinceClick > 0.4f)
            {
                _secsSinceClick = 0;

                if (GameManager.Instance.SelectedLetter == "del")
                {
                    if (Name.text.Length == 1)
                    {
                        Name.UpdateText("");
                    }
                    else if (Name.text.Length > 1)
                    {
                        Name.UpdateText(Name.text.Substring(0, Name.text.Length - 1));
                    }                    
                }
                else if (GameManager.Instance.SelectedLetter == "ok")
                {
                    StartCoroutine(SaveScore());
                }
                else
                {
                    Name.UpdateText(Name.text + GameManager.Instance.SelectedLetter);
                }
            }

            _secsSinceClick += Time.deltaTime;
        }
    }

    private IEnumerator SaveScore()
    {
        GameManager.Instance.SaveScore(Name.text);
        yield return new WaitForSeconds(0.5f);
        SetMode(ScoreMode.Display);
    }

    public void SetMode(ScoreMode scoreMode)
    {
        ScoreMode = scoreMode;

        switch (scoreMode)
        {
            case ScoreMode.Display:
                Display.SetActive(true);
                Entry.SetActive(false);
                StartCoroutine(GameManager.Instance.ShowScores());
                GameManager.Instance.ClearForScoreEntry(false);
                break;
            case ScoreMode.Entry:
                Display.SetActive(false);
                Entry.SetActive(true);
                GameManager.Instance.ClearForScoreEntry(true);
                break;
            default:
                break;
        }
    }
}

public enum ScoreMode
{
    Display,
    Entry        
}
