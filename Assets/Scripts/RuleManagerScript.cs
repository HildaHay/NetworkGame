using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RuleManagerScript : NetworkBehaviour
{
    [SerializeField] GameObject networkManager;
    [SerializeField] GameObject playerNameText;
    [SerializeField] GameObject scoreText;

    List<NetworkInstanceId> playerIDs;  // this also includes players who have disconnected previously
    List<PlayerStats> playerStats;

    int baseHealth = 10;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DisplayScore();
    }
    
    List<GameObject> ConnectedPlayerList()
    {
        return networkManager.GetComponent<CustomNetworkManager>().GetPlayerList();
    }

    public void ServerSetup()
    {
        if (playerStats == null)
        {
            playerStats = new List<PlayerStats>();
        }
    }

    public void ServerShutdown()
    {
        playerStats = null;
    }

    void DisplayScore()
    {
        if (NetworkServer.active || NetworkClient.active)
        {
            List<GameObject> player = ConnectedPlayerList();
            string pt = "";
            string st = "";
            if (playerStats != null)
            {
                foreach (PlayerStats s in playerStats)
                {
                    pt += "Player " + s.playerId.ToString() + "\n";
                    st += s.score + "\n";
                }
            }

            playerNameText.GetComponent<Text>().text = pt;
            scoreText.GetComponent<Text>().text = st;
        } else
        {
            playerNameText.GetComponent<Text>().text = "";
            scoreText.GetComponent<Text>().text = "";
        }
    }

    public void AddPlayer(GameObject p)
    {
        if(playerStats == null)
        {
            playerStats = new List<PlayerStats>();
        }
        PlayerStats s = new PlayerStats(p);
        playerStats.Add(s);
        p.GetComponent<Player>().stats = s;
    }

    PlayerStats FindStatsByID(NetworkInstanceId id)
    {
        foreach(PlayerStats s in playerStats)
        {
            if(s.playerId == id)
            {
                return s;
            }
        }
        return null;
    }

    public int GetBaseHealth()
    {
        return baseHealth;
    }

    public void PlayerDamagedByPlayer(NetworkInstanceId damagedPlayer, NetworkInstanceId attacker)
    {
        // does nothing... yet
    }

    public void PlayerKilled(NetworkInstanceId deadPlayer, NetworkInstanceId killer)
    {
        PlayerStats k = FindStatsByID(killer);
        k.score += 1;
    }
}

public class PlayerStats
{
    public GameObject player;   // This may be null if the player leaves the server
    public NetworkInstanceId playerId;
    bool connected;

    public int score;
    public int lives;

    public PlayerStats(GameObject p)
    {
        player = p;
        playerId = p.GetComponent<NetworkIdentity>().netId;
        connected = true;

        score = 0;
        lives = 0;
    }
}