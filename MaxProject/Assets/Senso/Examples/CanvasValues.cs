using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasValues : MonoBehaviour
{

    public int midiValue;
    private bool seen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!seen)
        {
            GetComponent<Renderer>().enabled = false;

        }
        else if (!GetComponent<Renderer>().enabled) {
            GetComponent<Renderer>().enabled = true;
        }
    }
}
