using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    bool isActive = false;
    float lifetime;

    public GameObject bulletPool;
    public GameObject owner;
    public NetworkInstanceId ownerId;

    [TooltipAttribute("Speed Multiplier of the bullet (maybe based on bullet shape?)")]
    public float speed_mult = 10.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (isActive && lifetime < 0)
        {
            // Destroy(this.gameObject);
            if (isServer)
            {
                Debug.Log("NO HIT!");
                CmdDespawnBullet();
            }
        }
    }

    public void Fire()
    {
        lifetime = 1.0f;
        isActive = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.gameObject != owner && collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<PlayerCharacter>().CmdTakeDamageFromPlayer(owner.GetComponent<PlayerCharacter>().GetDamage(), ownerId);
                CmdDespawnBullet();
                // collision.gameObject.GetComponent<Player>().addHit(10.0f);
            }
        }
    }

    [Command]
    void CmdDespawnBullet()
    {
        bulletPool.GetComponent<BulletPool>().ReturnBullet(this.gameObject);
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
        isActive = false;
    }
}
