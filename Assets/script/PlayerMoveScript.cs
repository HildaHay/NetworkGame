using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMoveScript : NetworkBehaviour
{
    NetworkIdentity networkIdentity;

    private bool facing_left = true;
    private SpriteRenderer m_sprite;

    public GameObject bullet;

    Vector2Int direction = new Vector2Int( 1, 0 );

    // Start is called before the first frame update
    void Start()
    {
        networkIdentity = this.GetComponent<NetworkIdentity>();
        fetchSpriteRenderer();
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
                this.transform.position += new Vector3(0.0f, 1.0f, 0.0f) * Time.deltaTime;
                direction.y += 1;
            }
            if (Input.GetKey("a"))
            {
                facing_left = true;
                this.transform.position += new Vector3(-1.0f, 0.0f, 0.0f) * Time.deltaTime;
                direction.x -= 1;
            }
            if (Input.GetKey("s"))
            {
                this.transform.position += new Vector3(0.0f, -1.0f, 0.0f) * Time.deltaTime;
                direction.y -= 1;
            }
            if (Input.GetKey("d"))
            {
                facing_left = false;
                this.transform.position += new Vector3(1.0f, 0.0f, 0.0f) * Time.deltaTime;
                direction.x += 1;
            }

            if (Input.GetKeyDown("space"))
            {
                CmdFireBullet(direction.x, direction.y);
            }
        }

        m_sprite.flipX = facing_left;
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
}
