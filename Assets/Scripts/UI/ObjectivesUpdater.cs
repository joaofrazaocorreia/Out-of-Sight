using UnityEngine;

public class ObjectiveUpdater : MonoBehaviour
{
    protected UIManager uiManager;

    protected virtual void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }

    public void ObjectiveEditMain(string text)
    {
        uiManager.EditObjective("main", text);
    }

    public void ObjectiveLeave()
    {
        uiManager.RemoveAllObjectives();
        ObjectiveEditMain("- Leave the location");
    }
}
