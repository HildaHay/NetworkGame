using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
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

    public GameObject bullet;

    private float health = 1000.0f;         // in ml of blood
    private float health_mult = 1.0f;
    private float health_gen_amount = 10;   // in ml of blood
    private float health_gen_period = 1;    // in seconds
    private float speed_mult = 1.0f;
    private int death_count = 0;

    private PlayerState current_player_state;
    private PlayerState last_player_state;
    private float player_state_updated_at;

    private WeaponState current_weapon_state;
    private WeaponState last_weapon_state;
    private float weapon_state_updated_at;
    private float nextActionTime = 0.0f;

    Vector2Int direction = new Vector2Int( 1, 0 );

    // Start is called before the first frame update
    void Start()
    {
        last_player_state = current_player_state = PlayerState.Standing;
        health = 100.0f;
        networkIdentity = this.GetComponent<NetworkIdentity>();
        fetchSpriteRenderer();
    }

	void FixedUpdate () {
		if(virtualJoystick.x < 0) {
            facing_left = true;
		} else if(virtualJoystick.x > 0) {
            facing_left = false;
        }
		addForce(virtualJoystick.x, virtualJoystick.y);
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
        if (networkIdentity.isLocalPlayer)
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

        // Handle health gen
            if (health < playerMaxHealth()) {
                if (Time.time > nextActionTime ) {
                    nextActionTime = Time.time + health_gen_period;
            
                    health += health_gen_amount;
                    if (health >= playerMaxHealth()) {
                        health = 1000.0f;
                    }
            }
        }
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
        this.transform.position += movement * Time.deltaTime * speed_mult;
    }

    public void addHit(float dmg) {
        health -= dmg;

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

    [Command]
    void CmdFireBullet(int x, int y)
    {
        Debug.Log("shoot");
        GameObject newBullet = Instantiate(bullet, this.transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody2D>().velocity = new Vector3(x * 10, y * 10, 0);
        NetworkServer.Spawn(newBullet);
    }

    private float healthGenRate() {
        return health_gen_amount / health_gen_period;
    }

    private float playerMaxHealth() {
        return health / health_mult;
    }
}
