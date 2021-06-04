﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    NetworkIdentity networkIdentity;

    public Vector2 virtualJoystick = new Vector2(0, 0);

    private bool facing_left = true;
    private SpriteRenderer m_sprite;

    public GameObject bulletPool;
    public GameObject ruleManager;
    public PlayerStats stats; // how is this handled on client?

    [SyncVar] int health;
    [SyncVar] bool alive;

    Vector2Int direction = new Vector2Int( 1, 0 );

    // Start is called before the first frame update
    void Start()
    {
        networkIdentity = this.GetComponent<NetworkIdentity>();
        fetchSpriteRenderer();

        if (NetworkServer.active)
        {
            //mdRespawnPlayer(ruleManager.GetComponent<RuleManagerScript>().GetSpawnPoint());
            InitHealth();
        }
    }

	void FixedUpdate () {
        if (alive)
        {
            if (virtualJoystick.x < 0)
            {
                facing_left = true;
            }
            else if (virtualJoystick.x > 0)
            {
                facing_left = false;
            }
            addForce(virtualJoystick.x, virtualJoystick.y);
        }
	}


    // Update is called once per frame
    void Update()
    {
        direction.x = 0;
        direction.y = 0;

        if (m_sprite == null)
        {
            fetchSpriteRenderer();
        }
        if (networkIdentity.isLocalPlayer && alive)
        {
            if (Input.GetKey("w"))
            {
                this.addForce(0.0f, 1.0f);
                direction.y += 1;
            }
            if (Input.GetKey("a"))
            {
                facing_left = true;
                this.addForce(-1.0f, 0.0f);
                direction.x -= 1;
            }
            if (Input.GetKey("s"))
            {
                this.addForce(0.0f, -1.0f);
                direction.y -= 1;
            }
            if (Input.GetKey("d"))
            {
                facing_left = false;
                this.addForce(1.0f, 0.0f);
                direction.x += 1;
            }

            if (Input.GetKeyDown("space"))
            {
                CmdFireBullet(direction.x, direction.y);
            }
        }

        m_sprite.flipX = facing_left;
    }

    public override void OnStartServer()
    {

    }

    public void addVirtualForce(float x_axis, float y_axis) {
        virtualJoystick = new Vector2(x_axis, y_axis);
    }

    public void addForce(float x_axis, float y_axis) {
        Vector3 movement = new Vector3(x_axis, y_axis, 0);

        if (movement.magnitude > 1.0)
        {
            movement.Normalize();
        }
        this.transform.position += movement * Time.deltaTime;
    }

    void fetchSpriteRenderer()
    {
        m_sprite = GetComponent<SpriteRenderer>();
    }

    void InitHealth()
    {
        health = ruleManager.GetComponent<RuleManagerScript>().GetBaseHealth();
        alive = true;
    }

    [Command]
    public void CmdRespawnPlayer(Vector2 loc)
    {
        InitHealth();
        RpcRespawnPlayer(loc);
    }

    [ClientRpc]
    void RpcRespawnPlayer(Vector2 loc)
    {
        if (hasAuthority)
        {
            this.transform.position = loc;
        }
    }

    [Command]
    void CmdFireBullet(int x, int y)
    {
        if (alive)
        {
            GameObject b = bulletPool.GetComponent<BulletPoolScript>().RetrieveBullet();
            if (b != null)
            {
                b.transform.position = this.transform.position;
                b.GetComponent<Rigidbody2D>().velocity = new Vector3(x * 10, y * 10, 0);
                b.GetComponent<BulletScript>().owner = this.gameObject;
                b.GetComponent<BulletScript>().ownerId = networkIdentity.netId;
            }
        }
    }

    [Command]
    public void CmdTakeDamageFromPlayer(int d, NetworkInstanceId attackerID)
    {
        Debug.Log("Took " + d + " damage from " + attackerID.ToString());
        this.health -= d;
        //ruleManager.GetComponent<RuleManagerScript>().CmdPlayerDamagedByPlayer(networkIdentity.netId, attackerID);
        if (health <= 0)
        {
            CmdDie(attackerID);
        }
    }

    [Command]
    void CmdDie(NetworkInstanceId attackerID)
    {
        if (alive)
        {
            alive = false;
            ruleManager.GetComponent<RuleManagerScript>().CmdPlayerKilled(networkIdentity.netId, attackerID);
            RpcDie();
        }
    }

    [ClientRpc]
    public void RpcDie()
    {
        if (hasAuthority)
        {
            transform.position = new Vector2(100, 100);
        }
    }

    public bool IsAlive()
    {
        return alive;
    }
}