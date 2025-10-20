using UnityEngine;

public class TutorialObjectivesUpdater : ObjectiveUpdater
{
    public void ObjectiveStart()
    {
        ObjectiveEdit("main1", "- Wait for instructions", true);
    }

    public void ObjectiveFindLockedDoor()
    {
        ObjectiveEdit("main1", "- Find the locked door");
    }

    public void ObjectiveOpenLockedDoor()
    {
        ObjectiveEdit("main1", "- Unlock the door");
    }

    public void ObjectiveEnterCameraRoom()
    {
        ObjectiveEdit("main1", "- Proceed into the room");
    }

    public void ObjectiveJamCamera()
    {
        ObjectiveEdit("main1", "- Jam the camera on the wall", true);
    }

    public void ObjectiveEnterHall2()
    {
        ObjectiveEdit("main1", "- Proceed to the next room", true);
    }

    public void ObjectiveKnockOutGuard()
    {
        ObjectiveEdit("main1", "- Knock out the Guard", true);
    }

    public void ObjectiveKnockOutEmployee1()
    {
        ObjectiveEdit("main1", "- Knock out the alerted Employee");
    }

    public void ObjectiveHideBodies()
    {
        ObjectiveEdit("main1", "- Hide the bodies in the Storage");
    }

    public void ObjectiveTakeDisguise()
    {
        ObjectiveEdit("main1", "- Take a disguise from a body");
    }

    public void ObjectiveExitBodiesRoom()
    {
        ObjectiveEdit("main1", "- Proceed to the next room", true);
    }

    public void ObjectiveScoutNextRoom()
    {
        ObjectiveEdit("main1", "- Look under the door");
    }

    public void ObjectiveFindDistraction()
    {
        ObjectiveEdit("main1", "- Enter the Phone room");
    }

    public void ObjectiveUseDistraction()
    {
        ObjectiveEdit("main1", "- Lure employee with a distraction");
    }

    public void ObjectiveKnockOutEmployee2()
    {
        ObjectiveEdit("main1", "- Knock out the Employee");
    }

    public void ObjectiveGrabKeycard()
    {
        ObjectiveEdit("main1", "- Grab the dropped keycard");
    }

    public void ObjectiveOpenKeycardDoor()
    {
        ObjectiveEdit("main1", "- Enter the next room");
    }

    public void ObjectiveGrabFiles()
    {
        ObjectiveEdit("main1", "- Grab the Main Objective");
    }
}

