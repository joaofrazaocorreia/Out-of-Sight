

public class HotelObjectivesUpdater : ObjectiveUpdater
{
    private static bool hasFoundWorkerHalls;
    private static bool hasFoundKeycard;
    private static bool hasFoundSafe;
    private static bool hasFoundSafeCode;
    private static bool hasFoundLuggage;
    private static bool hasGrabbedLuggage;
    
    protected override void Start()
    {
        base.Start();

        hasFoundWorkerHalls = false;
        hasFoundKeycard = false;
        hasFoundSafe = false;
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
        uiManager.RemoveObjective("cameras");
    }

    public void ObjectiveCompleteReception()
    {
        uiManager.RemoveObjective("reception");
    }

    public void ObjectiveManagerRoom(string text)
    {
        if(!hasFoundWorkerHalls && !hasFoundSafeCode)
        {
            hasFoundWorkerHalls = true;
            ObjectiveEditMain(text);
        }
    }

    public void ObjectiveEditMain2(string text)
    {
        uiManager.EditObjective("main2", text);
    }

    public void ObjectiveFindSafeCode()
    {
        if(!hasFoundSafe && !hasFoundSafeCode)
        {
            hasFoundSafe = true;
            uiManager.RemoveObjective("main");
            ObjectiveEditMain2("- Search the Staff rooms");
            ObjectiveEditMain("- Obtain the safe's combination");
        }
    }

    public void ObjectiveEnterCamRoom(string text)
    {
        if(!hasFoundKeycard)
        {
            hasFoundKeycard = true;
            ObjectiveEditCameras(text);
        }
    }

    public void ObjectiveOpenSafe(string text)
    {
        if(!hasFoundSafeCode)
        {
            hasFoundSafeCode = true;
            uiManager.RemoveObjective("main2");
            ObjectiveEditMain(text);
        }
    }

    public void ObjectiveGetLuggage()
    {
        if(!hasFoundLuggage)
        {
            hasFoundLuggage = true;
            ObjectiveEditReception("- Obtain some luggage");
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
