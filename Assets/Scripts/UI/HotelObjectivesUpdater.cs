
public class HotelObjectivesUpdater : ObjectiveUpdater
{
    private static bool hasFoundWorkerHalls;
    private static bool hasFoundSafeCode;
    
    protected override void Start()
    {
        base.Start();

        hasFoundWorkerHalls = false;
        hasFoundSafeCode = false;
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
        uiManager.EditObjective("reception", "- Luggage Clerk distracted (completed)", 0.65f);
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

    public void ObjectiveOpenSafe(string text)
    {
        hasFoundSafeCode = true;
        uiManager.RemoveObjective("main2");
        ObjectiveEditMain(text);
    }
}
