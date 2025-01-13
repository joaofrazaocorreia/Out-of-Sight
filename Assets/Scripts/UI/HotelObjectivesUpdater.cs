
public class HotelObjectivesUpdater : ObjectiveUpdater
{
    private bool hasFoundWorkerHalls;
    
    protected override void Start()
    {
        base.Start();

        hasFoundWorkerHalls = false;
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
}
