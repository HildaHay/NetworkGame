using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    float lifetime;

    // Start is called before the first frame update
    void Start()
    {
        lifetime = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
