using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMoveScript : MonoBehaviour
{
    NetworkIdentity networkIdentity;

    // Start is called before the first frame update
    void Start()
    {
        networkIdentity = this.GetComponent<NetworkIdentity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkIdentity.isLocalPlayer)
        {
            if (Input.GetKey("w"))
            {
                this.transform.position += new Vector3(0.0f, 1.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("a"))
            {
                this.transform.position += new Vector3(-1.0f, 0.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("s"))
            {
                this.transform.position += new Vector3(0.0f, -1.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("d"))
            {
                this.transform.position += new Vector3(1.0f, 0.0f, 0.0f) * Time.deltaTime;
            }
        }
    }
}
