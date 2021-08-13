using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMenuHandler : MonoBehaviour
{
    /// <summary>
    /// The NetworkManager associated with this HUD.
    /// </summary>
    public UnityEngine.Networking.NetworkManager manager;
    // properties
    /// <summary>
    /// The main menu for Multiplayer
    /// </summary>
    public CanvasGroup mainMenu;
    /// <summary>
    /// The LAN menu for Multiplayer.
    /// </summary>
    public CanvasGroup lanMenu;
    /// <summary>
    /// The Match Making menu for Multiplayer.
    /// </summary>
    public CanvasGroup matchMakingMenu;

    private CanvasGroup activeGroup;

    // Start is called before the first frame update
    void Start()
    {
        ShowMenu(mainMenu);
        activeGroup = mainMenu;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (UnityEngine.Networking.NetworkServer.active)
        {
            if (leaveMatch())
            {
                if (manager.IsClientConnected())
                    manager.StopHost();
                else
                    manager.StopServer();
                ShowMenu(activeGroup);
            }
        }
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallMultiplayerMainMenu() {
        HideMenu(activeGroup);
        ShowMenu(mainMenu);
        activeGroup = mainMenu;
    }

    /// Match Making Handlers ////

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallMatchMakingMenu() {
        HideMenu(activeGroup);
        ShowMenu(matchMakingMenu);
        activeGroup = matchMakingMenu;
        manager.StartMatchMaker();
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallRoomCreateHost(string roomName) {
        HideMenu(activeGroup);
        manager.matchMaker.CreateMatch(roomName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallFindMatch() {
        // HideMenu(activeGroup);
        // manager.matchMaker.ListMatches(0, 20, "", false, 0, 0, manager.OnMatchList);
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void SetRoomName(string roomName) {
        manager.matchName = roomName;
        Debug.Log("Picked Room name changed: " + roomName);
        // HideMenu(activeGroup);
        // manager.matchMaker.ListMatches(0, 20, "", false, 0, 0, manager.OnMatchList);
    }

    /// LAN Handlers ////

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallLANMenu() {
        HideMenu(activeGroup);
        ShowMenu(lanMenu);
        activeGroup = lanMenu;
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallLANHost() {
        HideMenu(activeGroup);
        manager.StartHost();
    }

    /// <summary>
    /// caller for starting the LAN specific menu.
    /// </summary>
    public void CallLANConnect() {
        HideMenu(activeGroup);
        manager.StartClient();
    }

    private void ShowMenu(CanvasGroup group) {
        group.alpha = 1.0f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void HideMenu(CanvasGroup group) {
        group.alpha = 0.0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private bool leaveMatch() {
        return Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape);
    }
}
