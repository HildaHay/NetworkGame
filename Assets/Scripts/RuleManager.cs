using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class RuleManager : NetworkBehaviour
{
    private enum RoundStatus
    {
        Setup,
        InRound,
        PostRound
    }

    [SerializeField] GameObject networkManager;
    [SerializeField] GameObject scoreboardPlayerText;
    [SerializeField] GameObject scoreboardScoreText;
    [SerializeField] GameObject postRoundPlayerText;
    [SerializeField] GameObject postRoundScoreText;
    [SerializeField] GameObject winnerText;

    [SerializeField] GameObject startRoundButton;
    [SerializeField] GameObject pointsToWinInput;

    [SerializeField] GameObject[] respawnPoints;

    List<NetworkInstanceId> playerIDs;  // this also includes players who have disconnected previously
    List<PlayerServerStats> playerServerStats;  // Stored only on the server, this is used to track players' stats

    List<PlayerLocalStats> playerLocalStats;    // Stored on each client, this simply mirrors the values in playerServerStats for UI display

    List<PlayerIdentity> playerIdentities;

    int baseHealth = 1;
    int baseDamage = 1;
    float respawnTime = 2.0f;
    int pointsToWin = 1;

    //[SyncVar]
    //bool gameRunning;

    [SyncVar]
    RoundStatus roundStatus;

    float PostRoundTimer;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {

        } else
        {
            startRoundButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (isServer && gameRunning)
        if (isServer && roundStatus == RoundStatus.InRound)
        {
            UpdateRespawnTimers();
            CheckForWinner();
        } else if(isServer && roundStatus == RoundStatus.PostRound)
        {
            PostRoundTimer -= Time.deltaTime;
            if(PostRoundTimer < 0)
            {
                BeginSetup();
            }
        }
        DisplayScoreInRound();
    }
    
    List<GameObject> ConnectedPlayerList()
    {
        return networkManager.GetComponent<CustomNetworkManager>().GetPlayerList();
    }

    public void ServerSetup()
    {
        //gameRunning = false;
        BeginSetup();

        if (playerServerStats == null)
        {
            playerServerStats = new List<PlayerServerStats>();
        }

        playerIdentities = new List<PlayerIdentity>();
    }

    public void ServerShutdown()
    {
        roundStatus = RoundStatus.Setup;
        playerServerStats = null;
        playerLocalStats = null;
        winnerText.GetComponent<Text>().text = "";
        startRoundButton.SetActive(false);
    }

    public void ClientSetup()
    {

    }

    public void ClientShutdown()
    {
        playerLocalStats = null;
        winnerText.GetComponent<Text>().text = "";
    }

    string[] GetScoreboardText()
    {
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

        return new string[] { pt, st };
    }

    void DisplayScoreInRound()
    {
        if (NetworkClient.active)
        {
            //List<GameObject> player = ConnectedPlayerList();
            //string pt = "";
            //string st = "";
            //if (playerLocalStats != null)
            //{
            //    foreach (PlayerLocalStats ls in playerLocalStats)
            //    {
            //        pt += "Player " + ls.playerId.ToString() + "\n";
            //        st += ls.score + "\n";
            //    }
            //}

            //scoreboardPlayerText.GetComponent<Text>().text = pt;
            //scoreboardScoreText.GetComponent<Text>().text = st;

            string[] scoreboardText = GetScoreboardText();
            scoreboardPlayerText.GetComponent<Text>().text = scoreboardText[0];
            scoreboardScoreText.GetComponent<Text>().text = scoreboardText[1];
        } else
        {
            scoreboardPlayerText.GetComponent<Text>().text = "";
            scoreboardScoreText.GetComponent<Text>().text = "";
        }
    }

    void InitializeStats()
    {
        //if (playerServerStats == null)
        //{
            playerServerStats = new List<PlayerServerStats>();
        //}

        foreach(PlayerIdentity p in playerIdentities)
        {
            if(p != null)
            {
                PlayerServerStats s = new PlayerServerStats(p.gameObject);
                playerServerStats.Add(s);
                p.GetComponent<PlayerIdentity>().stats = s;

            }
        }

        foreach(PlayerIdentity p in playerIdentities)
        {
            if (p != null)
            {
                NetworkInstanceId[] playerIds = new NetworkInstanceId[playerServerStats.Count];
                int[] scores = new int[playerServerStats.Count];

                for (int i = 0; i < playerServerStats.Count; i++)
                {
                    playerIds[i] = playerServerStats[i].playerId;
                    scores[i] = playerServerStats[i].score;
                }

                RpcInitLocalStats(playerIds, scores);
            }
        }
    }

    // Called on server when a new player joins
    public void AddPlayer(GameObject p)
    {
        playerIdentities.Add(p.GetComponent<PlayerIdentity>());

        //PlayerServerStats s = new PlayerServerStats(p);
        //playerServerStats.Add(s);
        //p.GetComponent<PlayerIdentity>().stats = s;

        //NetworkInstanceId[] playerIds = new NetworkInstanceId[playerServerStats.Count];
        //int[] scores = new int[playerServerStats.Count];

        //for(int i = 0; i < playerServerStats.Count; i++)
        //{
        //    playerIds[i] = playerServerStats[i].playerId;
        //    scores[i] = playerServerStats[i].score;
        //}

        //RpcAddPlayer(playerIds, scores);
    }

    // this probably doesn't work
    //public void RemovePlayerById(NetworkInstanceId id)
    //{
    //    Debug.Log("Removing disconnected player");
    //    PlayerIdentity disconnectedPlayer = null;
    //    foreach(PlayerServerStats ps in playerServerStats)
    //    {

    //    }
    //    playerIdentities.Remove();
    //}

    [ClientRpc]
    public void RpcInitLocalStats(NetworkInstanceId[] ids, int[] scores)
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
                    if (s.player != null)
                    {
                        if (s.player.GetComponent<PlayerIdentity>().Character != null)
                        {
                            if (!s.player.GetComponent<PlayerIdentity>().Character.IsAlive())
                            {
                                s.player.GetComponent<PlayerIdentity>().Character.CmdRespawnPlayer(GetSpawnPoint());
                            }
                        }
                    }
                }
            }
        }
    }

    public bool GameRunning()
    {
        return roundStatus == RoundStatus.InRound;
    }

    void CheckForWinner()
    {
        PlayerServerStats winner = null;
        foreach(PlayerServerStats p in playerServerStats)
        {
            if (p.score >= pointsToWin)
            {
                winner = p;
            }
        }
        if(winner == null)
        {
            return;
        } else
        {
            EndRound(true, winner);
        }
    }

    void BeginSetup()
    {
        startRoundButton.SetActive(true);
        pointsToWinInput.SetActive(true);
        pointsToWinInput.GetComponent<TMP_InputField>().text = pointsToWin.ToString();

        roundStatus = RoundStatus.Setup;
        if (isServer)
        {
            //RpcHideWinnerText();
            RpcBeginSetup();
        }
    }

    public void CleanupDisconnectedPlayers()
    {
        // This removes players who have disconnected from the playerIdentities list
        playerIdentities.RemoveAll(item => item == null);
    }

    [ClientRpc]
    void RpcBeginSetup()
    {
        winnerText.GetComponent<Text>().text = "";
        postRoundPlayerText.GetComponent<TextMeshProUGUI>().text = "";
        postRoundScoreText.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void StartRound()
    {
        Debug.Log("Starting round");
        startRoundButton.SetActive(false);
        pointsToWinInput.SetActive(false);

        CleanupDisconnectedPlayers();   // Just in case there's a 

        int ptw = 10;    // default
        bool tryParsePtw = int.TryParse(pointsToWinInput.GetComponent<TMP_InputField>().text, out ptw);
        pointsToWin = ptw;

        roundStatus = RoundStatus.InRound;

        foreach(PlayerIdentity p in playerIdentities)
        {
            if (p != null)
            {
                p.StartRound();
            }
        }

        InitializeStats();
    }

    void EndRound(bool roundWon, PlayerServerStats winningPlayer)
    {
        Debug.Log("Ending game");
        //gameRunning = false;
        roundStatus = RoundStatus.PostRound;
        PostRoundTimer = 10.0f;
        if (roundWon)
        {
            RpcDisplayWinner(winningPlayer);
        } else
        {
            RpcDisplayNoWinner();
        }
        foreach(PlayerIdentity p in playerIdentities)
        {
            if (p != null)
            {
                p.CmdDestroyCharacterObject();
            }
        }

        RpcEndRound();
    }

    [ClientRpc]
    void RpcEndRound()
    {
        string[] scoreboardText = GetScoreboardText();
        postRoundPlayerText.GetComponent<TextMeshProUGUI>().text = scoreboardText[0];
        postRoundScoreText.GetComponent<TextMeshProUGUI>().text = scoreboardText[1];
    }

    public void AbortRound()
    {
        EndRound(false, null);
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

    [ClientRpc]
    public void RpcDisplayNoWinner()
    {
        winnerText.GetComponent<Text>().text = "No winner";
    }

    //[ClientRpc]
    //public void RpcHideWinnerText()
    //{
    //    winnerText.GetComponent<Text>().text = "";
    //}
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