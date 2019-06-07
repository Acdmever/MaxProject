using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityOSC;
using System.Linq;

public class SendMax : MonoBehaviour
{
    private string outIP = "127.0.0.1"; //Local IP
    private int outPort = 12349, c , fps=160; // Port to send to Max Patch      Frame count     FPS of the HMD
    public double smrR1, smrR2, smrL1, smrL2, lowbetaR1, lowbetaR2, lowbetaL1, lowbetaL2, betaR1, betaR2, betaL1, betaL2, highbetaR1, highbetaR2, highbetaL1, highbetaL2; //All the wave values
    public int value,MIDICC=0; //Value between 0-127 and MIDI CC number
    public int[] invertCC = { 74,10 }; //List of MIDI CC messages that are more intuitive to control if the range would be inverted
    public bool ctrl = false, change=false,stop=false,key=false, key2=false, speed=false; // These check if the values are currently being changed
    private GameObject bci,rightGlove,leftGlove;
    private OpenBCIData bcidata;
    private SensoHandExample rGloveData, lGloveData;
    // Start is called before the first frame update
    void Start()
    {
        OSCHandler.Instance.Init();

        // Initialize OSC clients (transmitters)
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);
        initVals();

        //BCI object and script
        bci = GameObject.Find("OpenBCI");
        bcidata = bci.GetComponent<OpenBCIData>();

        //Only using right hand
        rightGlove = GameObject.Find("[CameraRig]/Right Hand Container");
        rGloveData = rightGlove.GetComponent<SensoHandExample>();
    }

    // Update is called once per frame
    void Update()
    {
        if (c == 0)
        {
            //We calculate wave values every second
            changeWaveVals();

        }
        if (c % 5==0) {//Glove values need to be updated more often than every second

            value = rGloveData.value;
            if (invertCC.Contains(MIDICC) || speed/*Check if it works*/) //If the MIDICC value would be in invertCC, we invert the control values
            {
                value = 127 - value;
            }
            ctrl = rGloveData.control; //Get whether the glove is in control position

            if (rGloveData.change) { //If user did the motion to change song
                change = true;
                rGloveData.change = false;
            }
            if (rGloveData.stop) //If user did the stop motion
            {
                stop = true;
                rGloveData.stop = false;
            }

            sendToMax();
        }
        c = (c + 1) % fps;
    }
    
    //This function just initializes all values to default values
    private void initVals() {
        c = 0;
        smrR1 = 0;
        smrR2 = 0;
        smrL1 = 0;
        smrL2 = 0;
        lowbetaR1 = 0;
        lowbetaR2 = 0;
        lowbetaL1 = 0;
        lowbetaL2 = 0;
        betaR1 = 0;
        betaR2 = 0;
        betaL1 = 0;
        betaL2 = 0;
        highbetaR1 = 0;
        highbetaR2 = 0;
        highbetaL1 = 0;
        highbetaL2 = 0;

        value = 127;
    }

    //Functions for changing the wave values
    private void changeWaveVals()
    {
        smrR1 = smrR2;
        smrR2 = bcidata.smrR;
        smrL1 = smrL2;
        smrL2 = bcidata.smrL;
        lowbetaR1 = lowbetaR2;
        lowbetaR2 = bcidata.lowbetaR;
        lowbetaL1 = lowbetaL2;
        lowbetaL2 = bcidata.lowbetaL;
        betaR1 = betaR2;
        betaR2 = bcidata.betaR;
        betaL1 = betaL2;
        betaL2 = bcidata.betaL;
        highbetaR1 = highbetaR2;
        highbetaR2 = bcidata.highbetaR;
        highbetaL1 = highbetaL2;
        highbetaL2 = bcidata.highbetaL;

        
    }

    //Data that we send to the Max patch
    private void sendToMax() {

        //Wave values smoothened, we do this since we calculate the values every second, and we need updated values more often that every second
        List<object> msg = new List<object>();
        msg.Add((float)(smrR1 + (smrR2 - smrR1) * (double)c / (double)fps));
        msg.Add((float)(smrL1 + (smrL2 - smrL1) * (double)c / (double)fps));
        msg.Add((float)(lowbetaR1 + (lowbetaR2 - lowbetaR1) * (double)c / (double)fps));
        msg.Add((float)(lowbetaL1 + (lowbetaL2 - lowbetaL1) * (double)c / (double)fps));
        msg.Add((float)(betaR1 + (betaR2 - betaR1) * (double)c / (double)fps));
        msg.Add((float)(betaL1 + (betaL2 - betaL1) * (double)c / (double)fps));
        msg.Add((float)(highbetaR1 + (highbetaR2 - highbetaR1) * (double)c / (double)fps));
        msg.Add((float)(highbetaL1 + (highbetaL2 - highbetaL1) * (double)c / (double)fps));
        OSCHandler.Instance.SendMessageToClient("myClient", "/openbci", msg);

        //If the user is looking at one of the planes and intends to change a value(glove is in proper position), send a MIDI CC command to max
        if (MIDICC != 0 && ctrl)
        {

            msg = new List<object>();
            msg.Add(value);
            msg.Add(MIDICC);
            OSCHandler.Instance.SendMessageToClient("myClient", "/midicc", msg);
        }
        else if ((key || key2) && ctrl)//If user wantes to change the key (Not a MIDI CC command) and glove is in control position
        {
            
            msg = new List<object>();
            int k = (int)Mathf.Round((float)value * 2f / 127f) - 1;
            msg.Add(k);
            if (key)
            {
                msg.Add(1);
            }
            else {
                msg.Add(2);
            }
            //key = false; //CHCK IF IT WORKS
            //key2 = false;
            OSCHandler.Instance.SendMessageToClient("myClient", "/key", msg);
        }
        else if (speed && ctrl)
        {
            //speed = false;
            msg = new List<object>();
            int k = (int)Mathf.Round(value * 20f / 127f) + 10;
            msg.Add(k);
            OSCHandler.Instance.SendMessageToClient("myClient", "/speed", msg);
        }
        //If user wants to change song
        if (change) {
            change = false;
            msg = new List<object>();
            msg.Add("bang");
            OSCHandler.Instance.SendMessageToClient("myClient", "/change", msg);
        }

        //If user wants to stop current setting (manually assigning effects <-> OpenBCI waves)
        if (stop)
        {
            stop = false;
            msg = new List<object>();
            msg.Add("bang");
            OSCHandler.Instance.SendMessageToClient("myClient", "/stop", msg);
        }

        
        
    }
}
