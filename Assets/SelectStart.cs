using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectStart : MonoBehaviour
{
    public string GameTypeCode;
    public GameType GameType
    {
        get
        {
            switch (GameTypeCode)
            {
                case "Arcade":
                    return GameType.Arcade;                    
                case "Challenge":
                    return GameType.Challenge;
                case "Pacifism":
                    return GameType.Pacifism;
                default:
                    return GameType.Challenge;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "PlayerShip")
        {
            GameManager.Instance.GameType = GameType;
            GameManager.Instance.StartGame();
        }
    }
}

public enum GameType
{
    Arcade,
    Challenge,
    Pacifism
}