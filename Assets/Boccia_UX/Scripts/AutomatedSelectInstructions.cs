using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ElevationAdjuster),typeof(RampRotation))]
public class AutomatedSelectInstructions : MonoBehaviour
{
    [SerializeField]
    private Color onTargetColor;
    [SerializeField]
    private Color offTargetColor;

    [SerializeField]
    private Controller bciController;

    public string nextTarget;
    public string sceneTarget = "Back";
    public string defaultTarget;

    [SerializeField]
    public GameObject rampShaft;
    public GameObject rampPlate;

    //Reseting Position Info
    [SerializeField]
    private ElevationAdjuster elevationScript;
    [SerializeField]
    private RampRotation rotationScript;

    //EKL Edits
    public GameObject currentTargetGO;
    public GameObject nextTargetGO;
    public GameObject prevTargetGO;
    public bool needToCleanList = false;
    public bool inMainDisplay = true;
    [SerializeField]
    private float nextSelectTargetOnTime = 2f;

    public int pathToFollow;
    private Dictionary<int, Action> pathDict = new Dictionary<int, Action>();

    [SerializeField]
    private List<string> selectionTargets = new List<string>();
    private List<string> startingList;

    // Start is called before the first frame update
    void Start()
    {
        // Create a dictionary with int keys and Action values
        pathDict.Add(1, SetTargetPath1);
        pathDict.Add(2, SetTargetPath2);
        pathDict.Add(3, SetTargetPath3);
        pathDict.Add(4, SetTargetPath4);
        pathDict.Add(5, SetTargetPath5);
        pathDict.Add(6, SetTargetPath6);

        pathDict[pathToFollow]();

        // Select path for experiment run
        //pathDict[pathToFollow]();

        //SetTargetPath1();
        //Save the starting order above as our starting list
        startingList = selectionTargets;

        //Set-up the Instruction Target at the start
        SetInstructionTarget();

        //put the previous target as the first on in the list
        prevTargetGO = FindGOWithName(selectionTargets.First());

        //Set the objects
        elevationScript = this.GetComponent<ElevationAdjuster>();
        rotationScript = this.GetComponent<RampRotation>();
    }

    public GameObject FindGOWithName(string targetName)
    {
        GameObject tempGO = GameObject.Find(targetName);

        if(tempGO !=null)
        {
            UnityEngine.Debug.Log("Game object target found");
            return tempGO;
        }
        else
        {
            //UnityEngine.Debug.Log("Could not find game object with target name, returning null game object");
            return null;
        }
    }

    public void SetInstructionTarget()
    {

        //Handle if there are any items in the temp list.

        //Handle case when there are no more selection targets
        if (selectionTargets.Count == 0)
        {
            print("You have cleared the selection target list! To reset press 'R' ".Color("orange"));
            print("Setting default target to Drop".Color("orange"));
            nextTarget = "DropButton";
        }
        else
        {
            nextTarget = selectionTargets.First();
        }
        UnityEngine.Debug.Log("Next target is: " + nextTarget);
        nextTargetGO = FindGOWithName(nextTarget);
        

        //Handle storing logic if the next action is a scene swap action. This is hardcoded for now
        if(nextTarget == "ElevationButton" || nextTarget == "RotationButton")
        {
            sceneTarget = nextTarget;
            print("Target scene is: " + sceneTarget);
        }
        //Handle null case
        if (nextTargetGO == null)
        {
            UnityEngine.Debug.Log("Couldn't find that object - setting it to a default");
            

            if(inMainDisplay)
            {
                //Set the previous scene target as the new target in temp list
                selectionTargets.Insert(0, sceneTarget);
                UnityEngine.Debug.Log("Oops - looks like you're in the main display when you shouldn't be. Let's move you back to the last target scene.");
                UnityEngine.Debug.Log("The new target will be: " + sceneTarget);
                nextTarget = selectionTargets.First();
                UnityEngine.Debug.Log("Next target is: " + nextTarget);
                nextTargetGO = FindGOWithName(nextTarget);
            }
            else
            {
                selectionTargets.Insert(0, defaultTarget);
                UnityEngine.Debug.Log("The new target will be: " + defaultTarget);
                nextTarget = selectionTargets.First();
                UnityEngine.Debug.Log("Next target is: " + nextTarget);
                nextTargetGO = FindGOWithName(nextTarget);
            }

        }


        prevTargetGO = currentTargetGO;
        currentTargetGO = FindGOWithName(nextTarget);
        //Now apply changes to that target
        StartCoroutine("FlashSequenceTarget");
    }

    /// <summary>
    /// Flash the target button in the sequence. Waits for time set in nextSelectTargetOnTime.
    /// </summary>
    /// <returns></returns>
    public IEnumerator FlashSequenceTarget()
    {
        //Putting in a half-second buffer from starting the IEnumerator to hopefully fix a display bug. 
        yield return new WaitForSeconds(0.5f);
        OnTargetButton(currentTargetGO);
        yield return new WaitForSeconds(nextSelectTargetOnTime);
        //change color back to off color state
        OffTargetButton(currentTargetGO);
        yield return null;
    }

    public void CleanUpInstructionTargets()
    {
        
    
        if (needToCleanList)
        {
            print("Cleaning our list up....");
            //change color back to off color state in case it is still on for any reason.
            OffTargetButton(prevTargetGO);
            OffTargetButton(currentTargetGO);
            //Remove the first item from the list
            selectionTargets.RemoveAt(0);
            needToCleanList = false;
        }
        
    }

    public void ToggleMainDisplay(bool toggle)
    {
        inMainDisplay = toggle;
    }

    /// <summary>
    /// Resets the starting target list to the hardcoded defaults
    /// </summary>
    public void ResetTargetList()
    {
        //Clear selection targets
        selectionTargets.Clear();
        //Stop all coroutines
        StopAllCoroutines();

        //Reset positions
        elevationScript.ResetHeight();
        rotationScript.ResetAngle();
        pathDict[pathToFollow]();
        selectionTargets = startingList;
        SetInstructionTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            SetInstructionTarget();
        }

        if(Input.GetKeyDown(KeyCode.N))
        {
            needToCleanList = true;
            CleanUpInstructionTargets();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTargetList();
        }

    }

    //Highlighting the Button to Select
    public virtual void OnTargetButton(GameObject myTarget) //pass in the object to highlight
    {
        if(myTarget.GetComponent<Image>() != null)
        {
            myTarget.GetComponent<Image>().color = onTargetColor;
        }
    }

    public virtual void OffTargetButton(GameObject myTarget) //pass in the object to highlight
    {
        if (myTarget.GetComponent<Image>() != null)
        {
            myTarget.GetComponent<Image>().color = offTargetColor;
        }
    }

    /// <summary>
    /// Puts together a pre-definied list of targets together to go through. This is hardcoded
    /// </summary>
    public virtual void SetTargetPath1()
    {
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }

    public virtual void SetTargetPath2()
    {
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }

    public virtual void SetTargetPath3()
    {
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("Back");
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }

    public virtual void SetTargetPath4()
    {
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("Back");
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }

    public virtual void SetTargetPath5()
    {
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("UpButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }

    public virtual void SetTargetPath6()
    {
        selectionTargets.Add("RotationButton");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SRight");
        selectionTargets.Add("SLeft");
        selectionTargets.Add("Back");
        selectionTargets.Add("ElevationButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("DownButton");
        selectionTargets.Add("Back");
        selectionTargets.Add("DropButton");
    }


    public int GetNumSelecitonTargetsLeft()
    {
        print("Asked for number of target selections left: " + selectionTargets.Count);

        return selectionTargets.Count;
    }

    // public IEnumerator WaitToStartStim()
    // {
    //     yield return new WaitForSecondsRealtime(3);
        
    //     bciController.StartStopStimulus();
    // }

}