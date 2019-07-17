using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class Python : MonoBehaviour
{

    private OSCReciever reciever;
    public float valence, arousal;
    public int port = 5555;
    private int c,fps=100;
    // Start is called before the first frame update
    void Start()
    {
        reciever = new OSCReciever();
        reciever.Open(port);

    }

    // Update is called once per frame
    void Update()
    {
        if (c % fps == 0) {
            getValues();
        }
        c++;
    }
    private void getValues() {
        OSCMessage msg;
        
        if (reciever.hasWaitingMessages()) // FFT values
        {
            msg = reciever.getNextMessage();
            object[] m = msg.Data.ToArray();
            valence =(float) m[0];
            arousal = (float)m[1];
            Debug.Log("Valence: "+valence+"\t Arousal: "+arousal);
        }
  
    }
}
