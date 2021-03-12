using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMoveScript : MonoBehaviour
{
    NetworkIdentity networkIdentity;

    private bool facing_left = true;
    private SpriteRenderer m_sprite;
    // Start is called before the first frame update
    void Start()
    {
        networkIdentity = this.GetComponent<NetworkIdentity>();
        fetchSpriteRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_sprite == null)
        {
            fetchSpriteRenderer();
        }
        if (networkIdentity.isLocalPlayer)
        {
            if (Input.GetKey("w"))
            {
                this.transform.position += new Vector3(0.0f, 1.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("a"))
            {
                facing_left = true;
                this.transform.position += new Vector3(-1.0f, 0.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("s"))
            {
                this.transform.position += new Vector3(0.0f, -1.0f, 0.0f) * Time.deltaTime;
            }
            if (Input.GetKey("d"))
            {
                facing_left = false;
                this.transform.position += new Vector3(1.0f, 0.0f, 0.0f) * Time.deltaTime;
            }
        }

        m_sprite.flipX = facing_left;
    }

    void fetchSpriteRenderer()
    {
        m_sprite = GetComponent<SpriteRenderer>();
    }
}
