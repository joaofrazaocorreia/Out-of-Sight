

public class HotelObjectivesUpdater : ObjectiveUpdater
{
    public static bool camerasComplete;
    public static bool conciergeComplete;
    public static bool gotManagerKeycard;

    protected override void Start()
    {
        if(setsUpVariables)
        {
            base.Start();

            camerasComplete = false;
            conciergeComplete = false;
            gotManagerKeycard = false;
        }
    }

    public void ObjectiveFoundManagerOffice()
    {
        if(!gotManagerKeycard)
            ObjectiveEdit("main1", "- Find a spare Manager Keycard");
    }

    public void ObjectiveGotManagerKeycard()
    {
        gotManagerKeycard = true;
        ObjectiveEdit("main1", "- Enter the Manager's office");
    }

    public void ObjectiveEnterGMOffice()
    {
        ObjectiveEdit("main1", "- Search the Manager's PC");
    }

    public void ObjectiveFoundPC()
    {
        ObjectiveEdit("main2", "- Search the office for the password");
    }

    public void ObjectiveFoundPassword()
    {
        ObjectiveEdit("main1", "- Download a copy of the PC's files");
        ObjectiveRemove("main2");
    }

    public void ObjectiveFoundLuggage()
    {
        if(!conciergeComplete)
            ObjectiveEdit("luggage", "- Distract the concierge with luggage");
    }

    public void ObjectiveKnockedConcierge()
    {
        conciergeComplete = true;
        ObjectiveRemove("luggage");
    }

    public void ObjectiveFoundSecurity()
    {
        if(!camerasComplete)
            ObjectiveEdit("cameras1", "- Cameras: Obtain a security keycard ");
    }

    public void ObjectiveGotSecurityKeycard()
    {
        if(!camerasComplete)
            ObjectiveEdit("cameras1", "- Cameras: Enter the security room");
    }

    public void ObjectiveEnteredSecurity()
    {
        if(!camerasComplete)
            ObjectiveEdit("cameras1", "- Cameras: Knock out the camera operator");
    }

    public void ObjectiveFoundFuseBox()
    {
        if(!camerasComplete)
            ObjectiveEdit("distraction1", "- Distraction: Jam the fuse box");
    }

    public void ObjectiveKnockedCamOp()
    {
        camerasComplete = true;
        ObjectiveRemove("cameras1");
    }
}
