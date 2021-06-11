using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCharacter : NetworkBehaviour
{
    NetworkIdentity networkIdentity;

    PlayerIdentity player;

    private bool facing_left = true;
    private SpriteRenderer m_sprite;
    public GameObject bulletPool;

    [SyncVar] int health;
    [SyncVar] bool alive;

    Vector2Int direction = new Vector2Int(1, 0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
