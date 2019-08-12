using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NewtonVR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudyLogger : MonoBehaviour
{

    public bool ShouldLog
    {
        get { return shouldLog;}
        set { shouldLog = value; }
    }

    public bool IsPractice { get; set; } = true;

    public List<string> TrialData => trialData;

    private static bool created = false;

    private GameObject barrier;
    private bool shouldLog = false;
    private List<string> trialData;

    private GameObject rightHand;
    private int barrierHitCount = 0;
    private int targetSelected = 0;

    private float targetWidth = 0.0f;
    private float targetDistance = 0.0f;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
            Debug.Log("Awake: " + this.gameObject);
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            trialData = new List<string>();
        }
    }


    private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
    {
        Debug.Log("Scene loaded");
        SetUp();
    }

    void SetUp()
    {
        if (IsPractice)
            return;

        barrierHitCount = 0;
        targetSelected = 0;

        Debug.Log("New Scene    ");

        var targets = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var target in targets)
        {
            target.GetComponent<TargetSelectedAnnoucer>().OnSelectableHit += TargetSelecteted;
        }

        barrier = GameObject.FindGameObjectWithTag("Barrier");
        if (barrier != null)
            barrier.GetComponent<BarrierHitAnnouncer>().OnBarrierHit += OnOnBarrierHit;

        rightHand = GameObject.Find("RightHand");
        

        StartCoroutine(LogData(0.5f));
    }

    private void OnOnBarrierHit()
    {
        
        barrierHitCount++;
        RecordDiscrete();
    }

    private void TargetSelecteted(GameObject selected)
    {
        selected.GetComponent<TargetSelectedAnnoucer>().OnSelectableHit -= TargetSelecteted;
        //Vector3 scale = selected.transform.localScale;
        //targetWidth = scale.x > scale.y ? scale.x : scale.y;
        //targetWidth = targetWidth > scale.z ? targetWidth : scale.z;
        //targetDistance = Mathf.Abs(selected.transform.position.x) * 2 - targetWidth;
        var setter = selected.GetComponent<ScaleDirectionSetter>();
        targetWidth = setter.Width;
        targetDistance = setter.Distance;

        targetSelected++;
        RecordDiscrete();

        if (targetSelected == 2)
        {
            Debug.Log("Trial complete");
            shouldLog = false;
        }
    }

    private void RecordDiscrete()
    {
        var timestamp = DateTime.UtcNow.ToString("hh:mm:ss.fff");
        var handPosition = VectToCSVString(rightHand.transform.position);
        var handRotation = QuatToCVString(rightHand.transform.rotation);
        var entry = String.Format("D,{0},{1},{2},{3},{4},{5},{6}", timestamp,
            targetDistance, targetWidth, barrierHitCount, targetSelected,
            handPosition, handRotation);
        trialData.Add(entry);
    }

    private IEnumerator LogData(float updateInterval)
    {
        shouldLog = true;
        trialData.Add($"New Trial: {SceneManager.GetActiveScene().name}");

        while (shouldLog)
        {
            var timestamp = DateTime.UtcNow.ToString("hh:mm:ss.fff");
            var handPosition = VectToCSVString(rightHand.transform.position);
            var handRotation = QuatToCVString(rightHand.transform.rotation);

            var headPosition = VectToCSVString(Camera.main.transform.position);
            var headRotation = QuatToCVString(Camera.main.transform.rotation);

            var entry = $"C,{timestamp},{handPosition},{handRotation},{headPosition},{headRotation}";

            trialData.Add(entry);

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private string QuatToCVString(Quaternion quat)
    {
        return $"{quat.w},{quat.x},{quat.y},{quat.z}";
    }

    private string VectToCSVString(Vector3 vect)
    {
        return $"{vect.x},{vect.y},{vect.z}";
    }
    private string ValAroToCSVString(float val, float aro)
    {
        return $"{val},{aro}";
    }
    private string WavesToCSVString(Vector3 vect)
    {
        return $"{vect.x},{vect.y},{vect.z}";
    }

}
