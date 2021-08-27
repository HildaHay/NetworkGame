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

    // Game logic objects
    [SerializeField] GameObject networkManager;
    
    // HUD objects
    [SerializeField] GameObject scoreboardPlayerText;
    [SerializeField] GameObject scoreboardScoreText;
    [SerializeField] GameObject postRoundPlayerText;
    [SerializeField] GameObject postRoundScoreText;
    [SerializeField] GameObject winnerText;
    [SerializeField] GameObject playerListText;

    // Round setup menu objects
    [SerializeField] GameObject rulesMenu;  // Parent object for rules settings
    [SerializeField] GameObject startRoundButton;
    [SerializeField] GameObject killsToWinInput;
    [SerializeField] GameObject hitPointsInput;
    [SerializeField] GameObject damageInput;
    [SerializeField] GameObject respawnTimeInput;

    // Game objects
    [SerializeField] GameObject[] respawnPoints;

    // Player objects
    List<NetworkInstanceId> playerIDs;  // this also includes players who have disconnected previously
    List<PlayerSoul> playerSouls;
    List<PlayerServerStats> playerServerStats;  // Stored only on the server, this is used to track players' stats

    List<PlayerLocalStats> playerLocalStats;    // Stored on each client, this simply mirrors the values in playerServerStats for UI display
    List<string> playerNames;                   // A list of the connected players' names, for pre-round UI. Updated every second
    float nameListUpdateTimer;


    [SerializeField] float PostRoundScoreboardTime = 8.0f;

    //[SerializeField] int baseHealth = 3;
    //[SerializeField] int baseDamage = 1;
    //[SerializeField] float respawnTime = 3.0f;
    //[SerializeField] int killsToWin = 10;

    [SerializeField] int baseHitPoints = 3;
    [SerializeField] int baseDamage = 1;
    [SerializeField] float baseRespawnTime = 3.0f;
    [SerializeField] int baseKillsToWin = 10;

    bool gameRulesInitialized = false;
    int hitPoints;
    int damage;
    float respawnTime;
    int killsToWin;

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
            nameListUpdateTimer = 1.0f;
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
        } else if(isServer && roundStatus == RoundStatus.Setup)
        {
            nameListUpdateTimer -= Time.deltaTime;
            if(nameListUpdateTimer <= 0.0f)
            {
                Debug.Log("Updating player name list");
                Debug.Log(GetPlayerNameList());
                nameListUpdateTimer = 1.0f;
                RpcSendPlayerNames(GetPlayerNameList());
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

        playerSouls = new List<PlayerSoul>();
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

    string GetPlayerNameList()  // For use in round setup
    {
        string t = "";

        foreach (PlayerSoul p in playerSouls)
        {
            if (p != null)
            {
                t += "Player " + p.GetComponent<NetworkIdentity>().netId.ToString() + "\n";
            }
        }

        return t;
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

        foreach(PlayerSoul p in playerSouls)
        {
            if(p != null)
            {
                PlayerServerStats s = new PlayerServerStats(p.gameObject);
                playerServerStats.Add(s);
                p.GetComponent<PlayerSoul>().stats = s;

            }
        }

        foreach(PlayerSoul p in playerSouls)
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
        playerSouls.Add(p.GetComponent<PlayerSoul>());

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

        //RpcAddPlayer();
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
    public void RpcSendPlayerNames(string pn)
    {
        if(roundStatus == RoundStatus.Setup)
        {
            playerListText.GetComponent<TMP_Text>().text = pn;
        } else
        {
            playerListText.GetComponent<TMP_Text>().text = "";
        }
    }

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
                        if (s.player.GetComponent<PlayerSoul>().Character != null)
                        {
                            if (!s.player.GetComponent<PlayerSoul>().Character.IsAlive())
                            {
                                s.player.GetComponent<PlayerSoul>().Character.CmdRespawnPlayer(GetSpawnPoint());
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
            if (p.score >= GetKillsToWin())
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

        // Open the game rules menu
        if (!gameRulesInitialized) LoadBaseGameRules();
        rulesMenu.SetActive(true);
        killsToWinInput.GetComponent<TMP_InputField>().text = killsToWin.ToString();
        hitPointsInput.GetComponent<TMP_InputField>().text = hitPoints.ToString();
        respawnTimeInput.GetComponent<TMP_InputField>().text = respawnTime.ToString();
        damageInput.GetComponent<TMP_InputField>().text = damage.ToString();

        roundStatus = RoundStatus.Setup;
        if (isServer)
        {
            //RpcHideWinnerText();
            RpcBeginSetup();
        }
    }

    public void CleanupDisconnectedPlayers()
    {
        // This removes players who have disconnected from the players list
        playerSouls.RemoveAll(item => item == null);
    }

    [ClientRpc]
    void RpcBeginSetup()
    {
        // Note: This will NOT be called on the server's client when the server is first started, only on subsequent rounds
        // Probably because the client isn't created until after BeginSetup() has already run

        winnerText.GetComponent<Text>().text = "";
        postRoundPlayerText.GetComponent<TextMeshProUGUI>().text = "";
        postRoundScoreText.GetComponent<TextMeshProUGUI>().text = "";
    }

    void LoadBaseGameRules()
    {
        if (!gameRulesInitialized)
        {
            hitPoints = baseHitPoints;
            damage = baseDamage;
            respawnTime = baseRespawnTime;
            killsToWin = baseKillsToWin;

            gameRulesInitialized = true;
        }
    }

    public void StartRound()
    {
        Debug.Log("Starting round");
        startRoundButton.SetActive(false);
        rulesMenu.SetActive(false);

        CleanupDisconnectedPlayers();   // Just in case there's a 

        // Load game rules from input fields

        // Kills to win
        int ktw = killsToWin;
        bool tryParseKtw = int.TryParse(killsToWinInput.GetComponent<TMP_InputField>().text, out ktw);
        killsToWin = ktw;

        // Hit points
        int hp = hitPoints;
        bool tryParseHP = int.TryParse(hitPointsInput.GetComponent<TMP_InputField>().text, out hp);
        hitPoints = hp;

        // Damage
        int d = damage;
        bool tryParseDamage = int.TryParse(damageInput.GetComponent<TMP_InputField>().text, out d);
        damage = d;

        // Respawn Time
        float rt = respawnTime;
        bool tryParseRT = float.TryParse(respawnTimeInput.GetComponent<TMP_InputField>().text, out rt);
        respawnTime = rt;

        roundStatus = RoundStatus.InRound;

        foreach(PlayerSoul p in playerSouls)
        {
            if (p != null)
            {
                p.StartRound();
            }
        }

        InitializeStats();

        RpcStartRound();
    }

    [ClientRpc]
    void RpcStartRound()
    {
        playerListText.GetComponent<TMP_Text>().text = "";
    }

    void EndRound(bool roundWon, PlayerServerStats winningPlayer)
    {
        Debug.Log("Ending game");
        //gameRunning = false;
        roundStatus = RoundStatus.PostRound;
        PostRoundTimer = PostRoundScoreboardTime;
        if (roundWon)
        {
            RpcDisplayWinner(winningPlayer);
        } else
        {
            RpcDisplayNoWinner();
        }
        foreach(PlayerSoul p in playerSouls)
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

    public int GetHitPoints()
    {
        return hitPoints;
    }

    public int GetWeaponDamage()
    {
        return damage;
    }

    public float GetRespawnTime()
    {
        return respawnTime;
    }

    public int GetKillsToWin()
    {
        return killsToWin;
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

        d.respawnTimer = GetRespawnTime();
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

    // This function is called on a client when it disconnects from a server, to reset the UI to its normal state
    // NOTE: This is called through OnClientDisconnect, which doesn't get called when the client initiates the disconnect. (Todo: Check if
    // this problem is specific to localhost) Thus, this isn't always called properly, but it's Unity's fault, not mine :P
    // Should probably report this bug and/or find a workaround but i'm lazy
    public void CleanupUIOnDisconnect()
    {
        Debug.Log("Cleaning up UI");
        playerListText.GetComponent<TMP_Text>().text = "";
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