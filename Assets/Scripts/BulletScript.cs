using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour
{
    float lifetime;

    public GameObject bulletPool;

    // Start is called before the first frame update
    void Start()
    {
        //lifetime = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0)
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
    }

    [Command]
    void CmdDespawnBullet()
    {
        bulletPool.GetComponent<BulletPoolScript>().ReturnBullet(this.gameObject);
    }
}
