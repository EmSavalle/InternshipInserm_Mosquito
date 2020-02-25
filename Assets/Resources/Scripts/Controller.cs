using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mvmController : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 1;
    void Start()
    {
        gameObject.GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.Z))
        {
            transform.Translate(new Vector3(0,0, Time.deltaTime * speed));
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, 0,-Time.deltaTime * speed));
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(new Vector3(-Time.deltaTime * speed, 0, 0));
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(Time.deltaTime * speed, 0, 0));
        }

        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(new Vector3(0,-Time.deltaTime * speed, 0));
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(new Vector3(0,Time.deltaTime * speed, 0));
        }
    }
}
