using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] List<GameObject> playerObjects;

    [SerializeField] GameObject ruleManager;
    [SerializeField] GameObject bulletPool;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //base.OnServerAddPlayer(conn, playerControllerId);
        var player = (GameObject)GameObject.Instantiate(playerPrefab, GetStartPosition().position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        InitializePlayer(player);
        if (playerObjects == null)
            playerObjects = new List<GameObject>();
        playerObjects.Add(player);
    }

    void InitializePlayer(GameObject p)
    {
        p.GetComponent<Player>().bulletPool = bulletPool;
        ruleManager.GetComponent<RuleManagerScript>().AddPlayer(p);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        if (player.gameObject != null)
            playerObjects.Remove(player.gameObject);
            NetworkServer.Destroy(player.gameObject);
    }

    public override void OnStartHost()
    {
        Debug.Log("Host has started");
    }

    public override void OnStopHost()
    {
        Debug.Log("Host has stopped");
    }

    public override void OnStartServer()
    {
        Debug.Log("Server has started");
        ruleManager.GetComponent<RuleManagerScript>().ServerSetup();
    }

    public override void OnStopServer()
    {
        Debug.Log("Server has stopped");
        ClearPlayerList();
        ruleManager.GetComponent<RuleManagerScript>().ServerShutdown();
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        ruleManager.GetComponent<RuleManagerScript>().ClientSetup();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        ruleManager.GetComponent<RuleManagerScript>().ClientShutdown();
    }

    void ClearPlayerList()
    {
        if (playerObjects != null)
        {
            playerObjects.Clear();
            playerObjects = null;
        }
    }

    public List<GameObject> GetPlayerList()
    {
        return playerObjects;
    }
}
