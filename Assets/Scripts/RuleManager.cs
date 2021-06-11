using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RuleManager : NetworkBehaviour
{
    [SerializeField] GameObject networkManager;
    [SerializeField] GameObject playerNameText;
    [SerializeField] GameObject scoreText;
    [SerializeField] GameObject winnerText;

    [SerializeField] GameObject[] respawnPoints;

    List<NetworkInstanceId> playerIDs;  // this also includes players who have disconnected previously
    List<PlayerServerStats> playerServerStats;  // Stored only on the server, this is used to track players' stats

    List<PlayerLocalStats> playerLocalStats;    // Stored on each client, this simply mirrors the values in playerServerStats for UI display

    int baseHealth = 5;
    int baseDamage = 1;
    float respawnTime = 2.0f;
    int pointsToWin = 5;

    [SyncVar]
    bool gameRunning;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            //gameRunning = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer && gameRunning)
        {
            UpdateRespawnTimers();
            CheckForWinner();
        }
        DisplayScore();
    }
    
    List<GameObject> ConnectedPlayerList()
    {
        return networkManager.GetComponent<CustomNetworkManager>().GetPlayerList();
    }

    public void ServerSetup()
    {
        gameRunning = true;
        if (playerServerStats == null)
        {
            playerServerStats = new List<PlayerServerStats>();
        }
    }

    public void ServerShutdown()
    {
        gameRunning = false;
        playerServerStats = null;
        playerLocalStats = null;
        winnerText.GetComponent<Text>().text = "";
    }

    public void ClientSetup()
    {

    }

    public void ClientShutdown()
    {
        playerLocalStats = null;
        winnerText.GetComponent<Text>().text = "";
    }

    void DisplayScore()
    {
        if (NetworkClient.active)
        {
            List<GameObject> player = ConnectedPlayerList();
            string pt = "";
            string st = "";
            if (playerLocalStats != null)
            {
                foreach (PlayerLocalStats ls in playerLocalStats)
                {
                    pt += "Player " + ls.playerId.ToString() + "\n";
                    st += ls.score + "\n";
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

    // Called on server when a new player joins
    public void AddPlayer(GameObject p)
    {
        if(playerServerStats == null)
        {
            playerServerStats = new List<PlayerServerStats>();
        }
        PlayerServerStats s = new PlayerServerStats(p);
        playerServerStats.Add(s);
        p.GetComponent<PlayerIdentity>().stats = s;

        NetworkInstanceId[] playerIds = new NetworkInstanceId[playerServerStats.Count];
        int[] scores = new int[playerServerStats.Count];

        for(int i = 0; i < playerServerStats.Count; i++)
        {
            playerIds[i] = playerServerStats[i].playerId;
            scores[i] = playerServerStats[i].score;
        }

        RpcAddPlayer(playerIds, scores);
    }

    [ClientRpc]
    public void RpcAddPlayer(NetworkInstanceId[] ids, int[] scores)
    {
        playerLocalStats = new List<PlayerLocalStats>();

        for (int i = 0; i < ids.Length; i++)
        {
            playerLocalStats.Add(new PlayerLocalStats(ids[i], scores[i]));
        }
    }

    PlayerServerStats FindStatsByID(NetworkInstanceId id)
    {
        foreach(PlayerServerStats s in playerServerStats)
        {
            if(s.playerId == id)
            {
                return s;
            }
        }
        return null;
    }

    void UpdateRespawnTimers()
    {
        if(isServer)
        {
            foreach(PlayerServerStats s in playerServerStats)
            {
                // todo: check for if player has left server
                if (s.respawnTimer > 0)
                {
                    s.respawnTimer -= Time.deltaTime;
                } else
                {
                    if (!s.player.GetComponent<PlayerCharacter>().IsAlive())
                    {
                        s.player.GetComponent<PlayerCharacter>().CmdRespawnPlayer(GetSpawnPoint());
                    }
                }
            }
        }
    }

    public bool GameRunning()
    {
        return gameRunning;
    }

    void CheckForWinner()
    {
        PlayerServerStats winner = null;
        foreach(PlayerServerStats p in playerServerStats)
        {
            if(p.score >= pointsToWin)
            {
                Debug.Log("eeeee");
                winner = p;
            }
        }
        if(winner == null)
        {
            return;
        } else
        {
            EndGame(true, winner);
        }
    }

    void EndGame(bool gameWon, PlayerServerStats winningPlayer)
    {
        Debug.Log("Ending game");
        gameRunning = false;
        if(gameWon)
        {
            RpcDisplayWinner(winningPlayer);
        }
    }

    public int GetBaseHealth()
    {
        return baseHealth;
    }

    [Command]
    public void CmdPlayerDamagedByPlayer(NetworkInstanceId damagedPlayer, NetworkInstanceId attacker)
    {
        // does nothing... yet
    }

    [Command]
    public void CmdPlayerKilled(NetworkInstanceId deadPlayer, NetworkInstanceId killer)
    {
        PlayerServerStats k = FindStatsByID(killer);
        PlayerServerStats d = FindStatsByID(deadPlayer);
        k.score += 1;
        RpcUpdateScore(k.playerId, k.score);

        d.respawnTimer = respawnTime;
    }

    // This should be called whenever a player's score changes
    [ClientRpc]
    public void RpcUpdateScore(NetworkInstanceId playerId, int score)
    {
        foreach(PlayerLocalStats ls in playerLocalStats)
        {
            if(ls.playerId == playerId)
            {
                ls.score = score;
            }
        }
    }

    public Vector2 GetSpawnPoint()
    {
        int r = Random.Range(0, respawnPoints.Length);
        return respawnPoints[r].transform.position;
    }

    [ClientRpc]
    public void RpcDisplayWinner(PlayerServerStats winner)
    {
        winnerText.GetComponent<Text>().text = winner.playerId + " won!";
    }
}

public class PlayerServerStats
{
    public GameObject player;   // This may be null if the player leaves the server
    public NetworkInstanceId playerId;
    bool connected;

    public int score;
    public int lives;

    public float respawnTimer;

    public PlayerServerStats(GameObject p)
    {
        player = p;
        playerId = p.GetComponent<NetworkIdentity>().netId;
        connected = true;

        score = 0;
        lives = 0;
        respawnTimer = 0;
    }

    public PlayerServerStats()
    {
        player = null;
        connected = false;
        score = 0;
        lives = 0;
        respawnTimer = 0;
    }
}

public class PlayerLocalStats {
    public NetworkInstanceId playerId;
    public int score;

    public PlayerLocalStats(NetworkInstanceId id)
    {
        playerId = id;
        score = 0;
    }

    public PlayerLocalStats(NetworkInstanceId id, int s)
    {
        playerId = id;
        score = s;
    }
}