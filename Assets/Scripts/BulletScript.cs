using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour
{
    bool isActive = false;
    float lifetime;

    public GameObject bulletPool;
    public GameObject owner;

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
        if(isServer)
        {
            if (collision.gameObject != owner && collision.gameObject.CompareTag("Player"))
            {
                CmdDespawnBullet();
            }
        }
    }

    [Command]
    void CmdDespawnBullet()
    {
        bulletPool.GetComponent<BulletPoolScript>().ReturnBullet(this.gameObject);
        isActive = false;
    }
}
