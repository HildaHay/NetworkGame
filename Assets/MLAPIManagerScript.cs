using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class MLAPIManagerScript : MonoBehaviour
{
    NetworkingManager networkingManager;

    // Start is called before the first frame update
    void Start()
    {
        networkingManager = this.GetComponent<NetworkingManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartHost()
    {
        networkingManager.StartHost();
    }

    public void StartServer()
    {
        networkingManager.StartServer();
    }
}
