using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class Python : MonoBehaviour
{

    private OSCReciever reciever1, reciever2;
    public float valence, arousal;
    public int motorIm;
    public int port1 = 5555, port2=5556;
    private int c,fps=100;
    // Start is called before the first frame update
    void Start()
    {
        reciever1 = new OSCReciever();
        reciever1.Open(port1);
        reciever2 = new OSCReciever();
        reciever2.Open(port2);
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
        
        if (reciever1.hasWaitingMessages()) // Valence-Arousal values
        {
            msg = reciever2.getNextMessage();
            object[] m = msg.Data.ToArray();
            valence =(float) m[0];
            arousal = (float)m[1];
            Debug.Log("Valence: "+valence+"\t Arousal: "+arousal);
        }
        if (reciever2.hasWaitingMessages()) // Motor Imagery values
        {
            msg = reciever2.getNextMessage();
            object[] m = msg.Data.ToArray();
            motorIm = Mathf.RoundToInt((float)m[0]);
            switch (motorIm) {
                case 1:
                    Debug.Log("Right\n");
                    break;
                case -1:
                    Debug.Log("Left\n");
                    break;
                default:
                    Debug.Log("Neutral\n");
                    break;


            }
        }

    }
}
