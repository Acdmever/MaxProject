using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            down();
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            up();
        }
    }
    private void down()
    {
        transform.Translate(0.0f, -0.002f, 0.0f, Space.Self);
    }
    private void up()
    {
        transform.Translate(0.0f, 0.002f, 0.0f, Space.Self);
    }
}
