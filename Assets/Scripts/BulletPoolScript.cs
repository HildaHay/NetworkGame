using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletPoolScript : NetworkBehaviour
{
    bool bulletPoolReady = false;

    public GameObject bulletPrefab;
    public int bulletCount;
    List<GameObject> bulletPool;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkServer.active && !bulletPoolReady)
        {
            InitializeBulletPool();
        } else if(!NetworkServer.active && bulletPoolReady)
        {
            ClearBulletPool();
        }
    }

    void InitializeBulletPool()
    {
        Debug.Log("foooo");

        // called when a server is created
        bulletPool = new List<GameObject>();
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject newBullet = Instantiate(bulletPrefab, this.transform.position, Quaternion.identity);
            NetworkServer.Spawn(newBullet);
            bulletPool.Add(newBullet);
            newBullet.GetComponent<BulletScript>().bulletPool = this.gameObject;
            newBullet.SetActive(false);
        }

        bulletPoolReady = true;
    }

    void ClearBulletPool()
    {
        // called when the server is shut down
        // shutting down the server already gets rid of all the bullets so don't need to bother deleting them
        bulletPool = null;
        bulletPoolReady = false;
    }

    public GameObject RetrieveBullet()
    {
        Debug.Log("retrieving bullet");

        if (bulletPool.Count >= 1)
        {
            GameObject b = bulletPool[0];
            bulletPool.RemoveAt(0);
            b.SetActive(true);
            b.GetComponent<BulletScript>().Fire();

            return b;
        }
        else
        {
            Debug.Log("No bullets in pool");
            return null;
        }
    }

    public void ReturnBullet(GameObject b)
    {
        b.SetActive(false);
        bulletPool.Add(b);
    }

    //[ClientRpc]
    //public GameObject RpcRetrieveBullet()
    //{
    //    Debug.Log("retrieving bullet");

    //    if (bulletPool.Count >= 1)
    //    {
    //        GameObject b = bulletPool[0];
    //        bulletPool.RemoveAt(0);
    //        b.SetActive(true);
    //        b.GetComponent<BulletScript>().Fire();

    //        return b;
    //    } else {
    //        Debug.Log("No bullets in pool");
    //        return null;
    //    }
    //}

    //[ClientRpc]
    //public void RpcReturnBullet(GameObject b)
    //{
    //    b.SetActive(false);
    //    bulletPool.Add(b);
    //}
}
