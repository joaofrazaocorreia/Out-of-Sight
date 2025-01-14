using UnityEngine;

public class HotelObjectivesUpdater : ObjectiveUpdater
{
    private static bool hasFoundWorkerHalls;
    private static bool hasFoundKeycard;
    private static bool hasFoundSafeCode;
    private static bool hasFoundLuggage;
    private static bool hasGrabbedLuggage;
    
    protected override void Start()
    {
        base.Start();

        hasFoundWorkerHalls = false;
        hasFoundKeycard = false;
        hasFoundSafeCode = false;
        hasFoundLuggage = false;
        hasGrabbedLuggage = false;
    }

    public void ObjectiveEditCameras(string text)
    {
        uiManager.EditObjective("cameras", text);
    }

    public void ObjectiveEditReception(string text)
    {
        uiManager.EditObjective("reception", text);
    }

    public void ObjectiveCompleteCameras()
    {
        uiManager.EditObjective("cameras", "- Cameras offline (completed)", 0.65f);
    }

    public void ObjectiveCompleteReception()
    {
        uiManager.EditObjective("reception", "- Luggage Clerk distracted", 0.65f);
    }

    public void ObjectiveManagerRoom(string text)
    {
        if(!hasFoundWorkerHalls)
        {
            ObjectiveEditMain(text);
            hasFoundWorkerHalls = true;
        }
    }

    public void ObjectiveEditMain2(string text)
    {
        uiManager.EditObjective("main2", text);
    }

    public void ObjectiveFindSafeCode(string text)
    {
        if(!hasFoundSafeCode)
        {
            ObjectiveEditMain2("- Search the Staff rooms");
            uiManager.RemoveObjective("main");
            ObjectiveEditMain(text);
        }
    }

    public void ObjectiveEnterCamRoom(string text)
    {
        if(!hasFoundKeycard)
        {
            ObjectiveEditCameras(text);
            hasFoundKeycard = true;
        }
    }

    public void ObjectiveOpenSafe(string text)
    {
        hasFoundSafeCode = true;
        uiManager.RemoveObjective("main2");
        ObjectiveEditMain(text);
    }

    public void ObjectiveGetLuggage(string text)
    {
        if(!hasFoundLuggage)
        {
            hasFoundLuggage = true;
            ObjectiveEditReception(text);
            uiManager.EditObjective("reception2", "- Distract the Luggage Clerk");
        }
    }

    public void ObjectiveEnableClerk(string text)
    {
        if(!hasGrabbedLuggage)
        {
            hasGrabbedLuggage = true;
            uiManager.RemoveObjective("reception2");
            ObjectiveEditReception(text);
        }
    }
}
