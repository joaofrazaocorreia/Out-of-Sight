using System.Collections.Generic;
using UnityEngine;

public class ObjectiveUpdater : MonoBehaviour
{
    [SerializeField] protected bool setsUpVariables = false;
    protected static UIManager uiManager;
    protected static List<string> seenObjectives;

    protected virtual void Start()
    {
        if(setsUpVariables)
        {
            uiManager = FindAnyObjectByType<UIManager>();
            seenObjectives = new List<string>();
        }
    }
    
    public virtual void ObjectiveEdit(string objective, string text, bool repeatable = false)
    {
        if(!seenObjectives.Contains(text) || repeatable)
        {
            uiManager.EditObjective(objective, text);
            seenObjectives.Add(text);
        }
    }

    public virtual void ObjectiveRemove(string objective)
    {
        uiManager.RemoveObjective(objective);
    }

    public virtual void ObjectiveLeave()
    {
        uiManager.RemoveAllObjectives();
        ObjectiveEditMain("- Leave the location");
    }

    public virtual void ObjectiveEditMain(string text)
    {
        ObjectiveEdit("main1", text);
    }
}
