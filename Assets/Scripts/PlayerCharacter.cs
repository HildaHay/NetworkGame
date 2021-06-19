using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCharacter : NetworkBehaviour
{
    private const float MAX_SPEED = 5.0f;
    private enum PlayerState
    {
        Standing,               // Player is currently not moving
        Moving,                 // Player is currently Moving
        Dead                    // Player is dead
    }
    private enum WeaponState
    {
        Shooting,   // Player's weapon is currently shooting
        Reloading,  // Player is currently reloading
        Broken      // Player cannot shoot or reload
    }

    NetworkIdentity networkIdentity;

    public Vector2 virtualJoystick = new Vector2(0, 0);

    private bool facing_left = true;
    private SpriteRenderer m_sprite;

    public GameObject bulletPool;
    public RuleManager ruleManager;
    //public PlayerServerStats stats; // how is this handled on client?

    [SyncVar] int health;
    [SyncVar] bool alive;

    // private float health = 1000.0f;         // in ml of blood

    [TooltipAttribute("Health Multiplyer")]
    public float health_mult = 1.0f;

    [Tooltip("Health regen amount (in ml of blood)")]
    public float health_gen_amount = 10;   // in ml of blood
    private float health_gen_period = 1;    // in seconds

    [TooltipAttribute("Speed Multiplyer")]
    public float speed_mult = 10.0f;
    private int death_count = 0;

    private PlayerState current_player_state;
    private PlayerState last_player_state;
    private float player_state_updated_at;

    private WeaponState current_weapon_state;
    private WeaponState last_weapon_state;
    private float weapon_state_updated_at;
    private float nextActionTime = 0.0f;

    Vector2Int direction = new Vector2Int( 1, 0 );
    private Vector3 velocity = new Vector3();
    private Rigidbody2D rigidbody;
    private float horizontal;
    private float vertical;
    private float moveLimiter = 0.7f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody2D>();
        last_player_state = current_player_state = PlayerState.Standing;
        // health = 100.0f;
        networkIdentity = this.GetComponent<NetworkIdentity>();
        fetchSpriteRenderer();

        ruleManager = GameObject.Find("RuleManager").GetComponent<RuleManager>();

        bulletPool = GameObject.Find("BulletPool");

        if (NetworkServer.active)
        {
            InitHealth();
        }
    }

	void FixedUpdate () {
        if (alive && ruleManager.GameRunning())
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
        addForce(virtualJoystick.x, virtualJoystick.y);

        if (horizontal != 0 && vertical != 0) { // Check for diagonal movement
            // limit movement speed diagonally, so you move at 70% speed
            horizontal *= moveLimiter;
            vertical *= moveLimiter;
        }
        this.addForce(horizontal, vertical);
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
        if (networkIdentity.isLocalPlayer && alive && ruleManager.GameRunning())
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            direction.x += (int) horizontal;
            vertical = Input.GetAxisRaw("Vertical");
            direction.y += (int) vertical;
            facing_left = horizontal < 0;

            if (Input.GetKeyDown("space"))
            {
                CmdFireBullet(direction.x, direction.y);
            }
        }
        m_sprite.flipX = facing_left;

        // Handle health gen
            if (health < playerMaxHealth()) {
                if (Time.time > nextActionTime ) {
                    nextActionTime = Time.time + health_gen_period;
            
                    // health += health_gen_amount;
                    // if (health >= playerMaxHealth()) {
                    //     health = 1000.0f;
                    // }
            }
        }
    }

    private void LateUpdate() {
        // Don't go too fast!
        if (rigidbody.velocity.magnitude > MAX_SPEED)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * MAX_SPEED;
        }
    }

    public override void OnStartServer()
    {

    }

    public void addVirtualForce(float x_axis, float y_axis) {
        virtualJoystick = new Vector2(x_axis, y_axis);
    }

    public void addForce(float x_axis, float y_axis) {
        Vector3 movement = new Vector3(x_axis, y_axis, 0);

        last_player_state = current_player_state;
        current_player_state = PlayerState.Moving;
        if (last_player_state != current_player_state)
        {
            player_state_updated_at = Time.time;
        }

        if (movement.magnitude > 1.0)
        {
            movement.Normalize();
        }
        velocity = movement * speed_mult;
        this.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public void addHit(float dmg) {
        // health -= dmg;

        if (health < 0)
        {
            death_count++;
            last_player_state = current_player_state;
            current_player_state = PlayerState.Dead;
            player_state_updated_at = Time.time;
        }
    }

    void fetchSpriteRenderer()
    {
        m_sprite = GetComponent<SpriteRenderer>();
    }

    void InitHealth()
    {
        health = ruleManager.GetBaseHealth();
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
            Debug.Log("attempt fire!");
            GameObject b = bulletPool.GetComponent<BulletPoolScript>().RetrieveBullet();
            if (b != null)
            {
                Debug.Log("firing bullet!");
                b.transform.position = this.transform.position;
                b.GetComponent<Rigidbody2D>().velocity = new Vector3(velocity.x + x * 10, velocity.y + y * 10, 0);
                b.GetComponent<BulletScript>().owner = this.gameObject;
                b.GetComponent<BulletScript>().ownerId = networkIdentity.netId;
            }
        }
    }

    [Command]
    public void CmdTakeDamageFromPlayer(int d, NetworkInstanceId attackerID)
    {
        this.health -= d;
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
            ruleManager.CmdPlayerKilled(networkIdentity.netId, attackerID);
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

    private float healthGenRate() {
        return health_gen_amount / health_gen_period;
    }

    private float playerMaxHealth() {
        return health / health_mult;
    }
}
