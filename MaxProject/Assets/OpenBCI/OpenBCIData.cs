using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityOSC;

public class OpenBCIData : MonoBehaviour
{
    private OSCReciever reciever, reciever2; //1 is FFT Data, and 2 is wave band power from OpenBCI GUI
    private double[][] values = new double[16][], values2 = new double[16][]; //Arrays where wave values are stored divided by channels
    public int port = 12345, port2=12346; //Ports where the OpenBCI GUI sends data
    private int c,channels=16, fps=60; //Frame counter      Number of channels      Frames
    public double delta, theta, alpha, mu, smr, lowbeta, beta, highbeta, delta2,theta2,alpha2,beta2,gamma2, smrR, lowbetaR, betaR, highbetaR, smrL, lowbetaL, betaL, highbetaL; //Wave values (means, GUI, right,left means)
    public double deltaM, thetaM, alphaM, muM, smrM, lowbetaM, betaM, highbetaM ;//Waves values (max values)

    // Start is called before the first frame update
    void Start()
    {

        //Initialize all values and recievers
        c = 0;
        reciever = new OSCReciever();
        reciever2 = new OSCReciever();
        reciever.Open(port);
        reciever2.Open(port2);
        initWaves();
    }

    //This function just initializes all wave values
    private void initWaves()
    {
        delta = 0;
        theta = 0;
        alpha = 0;
        mu = 0;
        lowbeta = 0;
        beta = 0;
        delta2 = 0;
        theta2 = 0;
        alpha2 = 0;
        beta2 = 0;
        gamma2 = 0;
        smrR = 0;
        lowbetaR = 0;
        betaR = 0;
        highbetaR = 0;
        smrL = 0;
        lowbeta = 0;
        betaL = 0;
        highbetaL = 0;


    }

    // Update is called once per frame
    void Update()
    {


        if (c % 2 == 0)
        {
            //We get values constantly to keep the OSC Queue up to date
            getData();
        }
        if (c == 0)
        {
            //We calculate everything after a new cycle starts
            initWaves(); //Set all waves to 0
            setWaves(); //Update all waves
            setWaves2();
            //printWaves(); // Format print of all wave values

        }
        c = (c + 1) % fps;
        

    }

    //Recieve data as 2 double[] (FFT and Wave Band Length) and store them in values[](FFT) and values2[](Wave band)
    private void getData() {

        OSCMessage msg;
        try //Due to the sending rate, there somtimes happens not to be a new message after a new frame
        {
            if (reciever.hasWaitingMessages()) // FFT values
            {
                for (int x = 0; x < channels; x++)
                {
                    msg = reciever.getNextMessage();
                    object[] m = msg.Data.ToArray();
                    values[x] = Array.ConvertAll<object, double>(m, o => Convert.ToDouble(o));
                }
            }
            if (reciever2.hasWaitingMessages()) //Wave band values
            {
                for (int x = 0; x < channels; x++)
                {
                    msg = reciever2.getNextMessage();
                    object[] m = msg.Data.ToArray();
                    values2[x] = Array.ConvertAll<object, double>(m, o => Convert.ToDouble(o));
                }
            }
        }
        catch (InvalidOperationException e) {
        }

    }

    //Calculate wave values based on the FFT values sent by the GUI
    private void setWaves() {
        //Working with mean for each wave and max value

        //Delta waves 1-4 hz (Posterior part)   NOTE: You can also ignore 1hz freq values in order to ignore most blinking artifacts.
        int[] deltaCH = {5,6,7,8,15,16 }; //Channels where the waves are present
        double[] deltaW = new double[4 * deltaCH.Length]; //Create array to store all values relevant to the wave (# of freqs that the wave is present in * # of channels the wave is present in)
        int i=0;
        foreach (int chan in deltaCH) {

            //Save the wave values to the array
            //chan-1 because arrays start at 0
            // The reason we don't start with the [chan-1][0] postition is that, in that position, we have the channel number (chan), from [chan-1][1] onwards it is the amplitudes
            deltaW[i] = values[chan - 1][1];
            deltaW[i+1] = values[chan - 1][2];
            deltaW[i+2] = values[chan - 1][3];
            deltaW[i+3] = values[chan - 1][4];

            i = i + 4; //Move the array pointer to next empty value
            
        }
        delta = deltaW.Average();//Mean
        deltaM = deltaW.Max(); //Max value

        //Theta waves 6-10 hz (Midline, temporal)
        int[] thetaCH = {1,2,3,4,7,8,11,12,13,14,15,16 };  //Channels where the waves are present
        double[] thetaW = new double[5 * thetaCH.Length]; //Array where the wave values will be stored
        i = 0;
        foreach (int chan in thetaCH)
        {
            //Save the wave values to the array
            thetaW[i] = values[chan - 1][6];
            thetaW[i + 1] = values[chan - 1][7];
            thetaW[i + 2] = values[chan - 1][8];
            thetaW[i + 3] = values[chan - 1][9];
            thetaW[i + 4] = values[chan - 1][10];

            i = i + 5;//Move the array pointer to next empty value

        }
        theta = thetaW.Average(); //Mean
        thetaM = thetaW.Max(); //Max

        //Alpha waves 8-12hz (Back of head, prefrontal for emotion regulations)
        int[] alphaCH = {5, 6, 7, 8, 11, 12}; //Channels in occipital and parietal area
        double[] alphaW = new double[5 * alphaCH.Length];//Array where the wave values will be stored
        i = 0;
        foreach (int chan in alphaCH)
        {
            //Save the wave values to the array
            alphaW[i] = values[chan - 1][8];
            alphaW[i + 1] = values[chan - 1][9];
            alphaW[i + 2] = values[chan - 1][10];
            alphaW[i + 3] = values[chan - 1][11];
            alphaW[i + 4] = values[chan - 1][12];

            i = i + 5;//Move the array pointer to next empty value
        }
        alpha = alphaW.Average();//Mean
        alphaM = alphaW.Max();//Max

        //Mu waves 9-11hz (Motor cortex)
        int[] muCH = { 3, 4, 13, 14 }; //Channels in motor cortex
        double[] muW = new double[3 * muCH.Length];//Array where the wave values will be stored
        i = 0;
        foreach (int chan in muCH)
        {
            //Save the wave values to the array
            muW[i] = values[chan - 1][9];
            muW[i + 1] = values[chan - 1][10];
            muW[i + 2] = values[chan - 1][11];

            i = i + 3;//Move the array pointer to next empty value
        }
        mu = muW.Average();//Mean
        muM = muW.Max();//Max

        //SMR Waves 13-15hz (Occipital)
        int[] smrCH = {5,6,7,8 };//Channels where the waves are present
        int[] smrCHR = { 6, 8 };//Channels in the right side
        int[] smrCHL = { 5, 7 }; // Channels in the left side
        double[] smrW = new double[3 * smrCH.Length];//Array where all the wave values will be stored
        double[] smrWR = new double[3 * smrCHR.Length];//Array where only the right side wave values will be stored
        double[] smrWL = new double[3 * smrCHL.Length];//Array where only the left side wave values will be stored
        i = 0;
        foreach (int chan in smrCH)
        {
            smrW[i] = values[chan - 1][13];
            smrW[i + 1] = values[chan - 1][14];
            smrW[i + 2] = values[chan - 1][15];

            i = i + 3;
        }
        //Calculate the right and left hemisphere serparately
        i = 0;
        foreach (int chan in smrCHR)
        {
            smrWR[i] = values[chan - 1][13];
            smrWR[i + 1] = values[chan - 1][14];
            smrWR[i + 2] = values[chan - 1][15];

            i = i + 3;
        }
        i = 0;
        foreach (int chan in smrCHL)
        {
            smrWL[i] = values[chan - 1][13];
            smrWL[i + 1] = values[chan - 1][14];
            smrWL[i + 2] = values[chan - 1][15];

            i = i + 3;
        }
        smr = smrW.Average(); //Mean
        smrM = smrW.Max(); //Max
        smrR = smrWR.Average();//Mean of right side
        smrL = smrWL.Average(); // Mean of left side

        //Low beta waves (sometimes called High Alpha) 12-16 hz (Frontocentral, posterior)
        int[] betaCH = {1,2,3,4,5,6,7,8,13,14,15,16 };//Channels where the waves are present
        int[] betaCHR = { 2, 4, 6, 8, 14, 16 }; //Channels in right side
        int[] betaCHL = { 1, 3, 5, 7, 13, 15 }; //Channels in left side
        double[] lowbetaW = new double[5 * betaCH.Length];//Array where all the wave values will be stored
        double[] lowbetaWR = new double[5 * betaCHR.Length];//Array where only the right side wave values will be stored
        double[] lowbetaWL = new double[5 * betaCHL.Length];//Array where only the left side wave values will be stored
        i = 0;
        foreach (int chan in betaCH)
        {
            lowbetaW[i] = values[chan - 1][12];
            lowbetaW[i + 1] = values[chan - 1][13];
            lowbetaW[i + 2] = values[chan - 1][14];
            lowbetaW[i + 3] = values[chan - 1][15];
            lowbetaW[i + 4] = values[chan - 1][16];

            i = i + 5;
        }

        //Calculate the right and left hemisphere serparately
        i = 0;
        foreach (int chan in betaCHR)
        {
            lowbetaWR[i] = values[chan - 1][12];
            lowbetaWR[i + 1] = values[chan - 1][13];
            lowbetaWR[i + 2] = values[chan - 1][14];
            lowbetaWR[i + 3] = values[chan - 1][15];
            lowbetaWR[i + 4] = values[chan - 1][16];

            i = i + 5;

        }
        i = 0;
        foreach (int chan in betaCHL)
        {
            lowbetaWL[i] = values[chan - 1][12];
            lowbetaWL[i + 1] = values[chan - 1][13];
            lowbetaWL[i + 2] = values[chan - 1][14];
            lowbetaWL[i + 3] = values[chan - 1][15];
            lowbetaWL[i + 4] = values[chan - 1][16];

            i = i + 5;

        }

        lowbeta = lowbetaW.Average(); //Mean
        lowbetaM = lowbetaW.Max(); //Max
        lowbetaR = lowbetaWR.Average(); //Mean of right side
        lowbetaL = lowbetaWL.Average(); //Mean of left side

        //Beta waves 17-20 hz
        double[] betaW = new double[4 * betaCH.Length];//Array where all the wave values will be stored
        double[] betaWR = new double[4 * betaCHR.Length];//Array where only the right side wave values will be stored
        double[] betaWL = new double[4 * betaCHL.Length];//Array where only the left side wave values will be stored
        i = 0;
        foreach (int chan in betaCH)
        {
            betaW[i] = values[chan - 1][17];
            betaW[i + 1] = values[chan - 1][18];
            betaW[i + 2] = values[chan - 1][19];
            betaW[i + 3] = values[chan - 1][20];

            i = i + 4; 
        }

        //Calculate the right and left hemisphere serparately
        i = 0;
        foreach (int chan in betaCHR)
        {
            betaWR[i] = values[chan - 1][17];
            betaWR[i + 1] = values[chan - 1][18];
            betaWR[i + 2] = values[chan - 1][19];
            betaWR[i + 3] = values[chan - 1][20];

            i = i + 4;
        }
        i = 0;
        foreach (int chan in betaCHL)
        {
            betaWL[i] = values[chan - 1][17];
            betaWL[i + 1] = values[chan - 1][18];
            betaWL[i + 2] = values[chan - 1][19];
            betaWL[i + 3] = values[chan - 1][20];

            i = i + 4;
        }

        beta = betaW.Average(); //Mean
        betaM = betaW.Max(); //Max
        betaR = betaWR.Average(); //Mean of right side
        betaL = betaWL.Average(); // Mean of left side

        //High beta waves 17-20 hz
        double[] highbetaW = new double[9 * betaCH.Length];//Array where all the wave values will be stored
        double[] highbetaWR = new double[9 * betaCHR.Length];//Array where only the right side wave values will be stored
        double[] highbetaWL = new double[9 * betaCHL.Length];//Array where only the left side wave values will be stored
        i = 0;
        foreach (int chan in betaCH)
        {
            highbetaW[i] = values[chan - 1][20];
            highbetaW[i + 1] = values[chan - 1][21];
            highbetaW[i + 2] = values[chan - 1][22];
            highbetaW[i + 3] = values[chan - 1][23];
            highbetaW[i + 4] = values[chan - 1][24];
            highbetaW[i + 5] = values[chan - 1][25];
            highbetaW[i + 6] = values[chan - 1][26];
            highbetaW[i + 7] = values[chan - 1][27];
            highbetaW[i + 8] = values[chan - 1][28];

            i = i + 9;
        }

        //Calculate the right and left hemisphere serparately
        i = 0;
        foreach (int chan in betaCHR)
        {
            highbetaWR[i] = values[chan - 1][20];
            highbetaWR[i + 1] = values[chan - 1][21];
            highbetaWR[i + 2] = values[chan - 1][22];
            highbetaWR[i + 3] = values[chan - 1][23];
            highbetaWR[i + 4] = values[chan - 1][24];
            highbetaWR[i + 5] = values[chan - 1][25];
            highbetaWR[i + 6] = values[chan - 1][26];
            highbetaWR[i + 7] = values[chan - 1][27];
            highbetaWR[i + 8] = values[chan - 1][28];

            i = i + 9;
        }
        i = 0;
        foreach (int chan in betaCHL)
        {
            highbetaWL[i] = values[chan - 1][20];
            highbetaWL[i + 1] = values[chan - 1][21];
            highbetaWL[i + 2] = values[chan - 1][22];
            highbetaWL[i + 3] = values[chan - 1][23];
            highbetaWL[i + 4] = values[chan - 1][24];
            highbetaWL[i + 5] = values[chan - 1][25];
            highbetaWL[i + 6] = values[chan - 1][26];
            highbetaWL[i + 7] = values[chan - 1][27];
            highbetaWL[i + 8] = values[chan - 1][28];

            i = i + 9;
        }
        highbeta = highbetaW.Average(); //Mean
        highbetaM = highbetaW.Max(); //Max
        highbetaR = highbetaWR.Average(); //Mean of right side
        highbetaL = highbetaWL.Average(); //Mean of left side

    }

    //Set waves from the band values send by the OpenBCI GUI
    private void setWaves2()
    {
        //Working with mean of the band values of the channels where the wave is most present

        //Delta waves 1-4 hz (Posterior)
        int[] deltaCH = { 5, 6, 7, 8, 15, 16 }; //Channels where the waves are present
        foreach (int chan in deltaCH)
        {
            delta2 += values2[chan-1][1]; //Add values from relevant channels
        }
        delta2 = delta2 / (double)deltaCH.Length; //Divide it by the number of channels to get the mean


        //Theta waves 6-10 hz (Midline, temporal)
        int[] thetaCH = { 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15, 16 }; //Channels where the waves are present

        foreach (int chan in thetaCH)
        {
            theta2 += values2[chan-1][2];//Add values from relevant channels
        }
        theta2 = theta2 / (double)thetaCH.Length; //Divide it by the number of channels to get the mean


        //Alpha waves 8-12hz (Back of head)
        int[] alphaCH = { 5, 6, 7, 8 }; //Channels in occipital and parietal area
        foreach (int chan in alphaCH)
        {
            alpha2 += values2[chan-1][3];//Add values from relevant channels
        }
        alpha2 = alpha2 / (double)alphaCH.Length; //Divide it by the number of channels to get the mean


        //Beta waves 17-20 hz
        int[] betaCH = { 1, 2, 3, 4, 5, 6, 7, 8, 13, 14, 15 }; //Channels where the waves are present
        foreach (int chan in betaCH)
        {
            beta2 += values2[chan-1][4];//Add values from relevant channels
        }
        beta2 = beta2 / (double)betaCH.Length; //Divide it by the number of channels to get the mean



    }

    //Formatted print of all wave values
    private void printWaves() {

        Debug.Log("Delta: Mean: "+ delta +"\t Max: "+deltaM + "\t\t GUI: "+delta2);
        Debug.Log("Theta: Mean: " + theta + "\t Max: " + thetaM+ "\t\t GUI: " + theta2);
        Debug.Log("Alpha: Mean: " + alpha + "\t Max: " + alphaM + "\t\t GUI: " + alpha2);
        Debug.Log("Mu: Mean: " + mu + "\t Max: " + muM);
        Debug.Log("SMR: Mean: " + smr + "\t Max: " + smrM + "\t Right: "+ smrR + "\t Left: " + smrL);
        Debug.Log("Low beta: Mean: " + lowbeta + "\t Max: " + lowbetaM + "\t Right: " + lowbetaR + "\t Left: " + lowbetaL);
        Debug.Log("Beta: Mean: " + beta + "\t Max: " + betaM+ "\t\t GUI: " + beta2 + "\t Right: " + betaR + "\t Left: " + betaL);
        Debug.Log("High Beta: Mean: " + highbeta + "\t Max: " + highbetaM + "\t Right: " + highbetaR + "\t Left: " + highbetaL);
        Debug.Log("\n");

    }



    
}
