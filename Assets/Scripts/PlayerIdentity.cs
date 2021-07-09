using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIdentity : NetworkBehaviour
{
    NetworkIdentity networkIdentity;

    public Vector2 virtualJoystick = new Vector2(0, 0);

    [SerializeField] GameObject CharacterPrefab;

    public PlayerCharacter Character;

    //private bool facing_left = true;
    //private SpriteRenderer m_sprite;

    //public GameObject bulletPool;
    public RuleManager ruleManager;
    public PlayerServerStats stats; // how is this handled on client?

    // Start is called before the first frame update
    void Start()
    {
        // health = 100.0f;
        networkIdentity = this.GetComponent<NetworkIdentity>();

        ruleManager = GameObject.Find("RuleManager").GetComponent<RuleManager>();
    }

    public override void OnStartAuthority()
    {
        networkIdentity = this.GetComponent<NetworkIdentity>();

        // StartRound();
    }

    public void StartRound()
    {
        //if (hasAuthority)
        //{
            CreateCharacterObject();
        //}
    }

    public void EndRound()
    {
        if(hasAuthority)
        {

        }
    }

    [Command]
    public void CmdDestroyCharacterObject()
    {
        if (Character != null)
        {
            if (Character.gameObject != null)
            {
                NetworkServer.Destroy(Character.gameObject);
            }
        }
    }

    void CreateCharacterObject()
    {
        CmdSpawnPlayerCharacter();
    }

    [Command]
    void CmdSpawnPlayerCharacter()
    {
        GameObject newCharacterObj = Instantiate(CharacterPrefab);
        NetworkServer.SpawnWithClientAuthority(newCharacterObj, networkIdentity.connectionToClient);
        RpcLinkCharacter(newCharacterObj);
    }

    [ClientRpc]
    void RpcLinkCharacter(GameObject characterObject)
    {
        Character = characterObject.GetComponent<PlayerCharacter>();
        Character.ConnectPlayerIdentity(this.gameObject);
    }

	void FixedUpdate () {
        if (ruleManager.GameRunning())
        {
            if (virtualJoystick.x < 0)
            {
                //facing_left = true;
            }
            else if (virtualJoystick.x > 0)
            {
                //facing_left = false;
            }
            //addForce(virtualJoystick.x, virtualJoystick.y);
        }
		//addForce(virtualJoystick.x, virtualJoystick.y);

        //if (horizontal != 0 && vertical != 0) { // Check for diagonal movement
        //    // limit movement speed diagonally, so you move at 70% speed
        //    horizontal *= moveLimiter;
        //    vertical *= moveLimiter;
        //}
        //this.addForce(horizontal, vertical);
	}


    // Update is called once per frame
    void Update()
    {
        //direction.x = 0;
        //direction.y = 0;

        //if (m_sprite == null)
        //{
        //    fetchSpriteRenderer();
        //}
        //if (networkIdentity.isLocalPlayer && alive && ruleManager.GameRunning())
        //{
        //    horizontal = Input.GetAxisRaw("Horizontal");
        //    direction.x += (int) horizontal;
        //    vertical = Input.GetAxisRaw("Vertical");
        //    direction.y += (int) vertical;
        //    facing_left = horizontal < 0;

        //    if (Input.GetKeyDown("space"))
        //    {
        //        CmdFireBullet(direction.x, direction.y);
        //    }
        //}
        //m_sprite.flipX = facing_left;

        // Handle health gen
        //    if (health < playerMaxHealth()) {
        //        if (Time.time > nextActionTime ) {
        //            nextActionTime = Time.time + health_gen_period;
            
        //            // health += health_gen_amount;
        //            // if (health >= playerMaxHealth()) {
        //            //     health = 1000.0f;
        //            // }
        //    }
        //}
    }

    //private void LateUpdate() {
    //    // Don't go too fast!
    //    if (GetComponent<Rigidbody>().velocity.magnitude > MAX_SPEED)
    //    {
    //        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * MAX_SPEED;
    //    }
    //}

    public override void OnStartServer()
    {

    }

    public void addVirtualForce(float x_axis, float y_axis) {
        virtualJoystick = new Vector2(x_axis, y_axis);
    }

    //public void addForce(float x_axis, float y_axis) {
    //    Vector3 movement = new Vector3(x_axis, y_axis, 0);

    //    last_player_state = current_player_state;
    //    current_player_state = PlayerState.Moving;
    //    if (last_player_state != current_player_state)
    //    {
    //        player_state_updated_at = Time.time;
    //    }

    //    if (movement.magnitude > 1.0)
    //    {
    //        movement.Normalize();
    //    }
    //    velocity = movement * speed_mult;
    //    this.GetComponent<Rigidbody2D>().velocity = velocity;
    //}

    //public void addHit(float dmg) {
    //    // health -= dmg;

    //    if (health < 0)
    //    {
    //        death_count++;
    //        last_player_state = current_player_state;
    //        current_player_state = PlayerState.Dead;
    //        player_state_updated_at = Time.time;
    //    }
    //}

    [Command]
    public void CmdRespawnPlayer(Vector2 loc)
    {
        //InitHealth();
        //RpcRespawnPlayer(loc);
    }

    [ClientRpc]
    void RpcRespawnPlayer(Vector2 loc)
    {
        //if (hasAuthority)
        //{
        //    this.transform.position = loc;
        //}
    }

    [Command]
    void CmdFireBullet(int x, int y)
    {
        //if (alive)
        //{
        //    Debug.Log("attempt fire!");
        //    GameObject b = bulletPool.GetComponent<BulletPool>().RetrieveBullet();
        //    if (b != null)
        //    {
        //        Debug.Log("firing bullet!");
        //        b.transform.position = this.transform.position;
        //        b.GetComponent<Rigidbody2D>().velocity = new Vector3(velocity.x + x * 10, velocity.y + y * 10, 0);
        //        b.GetComponent<Bullet>().owner = this.gameObject;
        //        b.GetComponent<Bullet>().ownerId = networkIdentity.netId;
        //    }
        //}
    }

    [Command]
    public void CmdTakeDamageFromPlayer(int d, NetworkInstanceId attackerID)
    {
        //this.health -= d;
        //if (health <= 0)
        //{
        //    CmdDie(attackerID);
        //}
    }

    [Command]
    void CmdDie(NetworkInstanceId attackerID)
    {
        //if (alive)
        //{
        //    alive = false;
        //    ruleManager.CmdPlayerKilled(networkIdentity.netId, attackerID);
        //    RpcDie();
        //}
    }

    [ClientRpc]
    public void RpcDie()
    {
        //if (hasAuthority)
        //{
        //    transform.position = new Vector2(100, 100);
        //}
    }

    public bool IsAlive()
    {
        return false;
        //return alive;
    }
}
