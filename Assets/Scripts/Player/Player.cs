using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Trespassing, Suspicious};
    public enum Disguise {Civillian, Employee, Guard_Tier1, Guard_Tier2}

    public List<Status> status;
    public Disguise disguise;
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();

        disguise = Disguise.Civillian;
        status.Add(Status.Normal);


        //GainStatus(Status.Trespassing);
        //GainStatus(Status.Suspicious);
    }

    private void Update()
    {
        UpdateDisguiseUI();
        UpdateStatusUI();
    }

    public void GainStatus(Status newStatus)
    {
        if(!status.Contains(newStatus))
            status.Add(newStatus);
    }

    public void LoseStatus(Status newStatus)
    {
        while(status.Contains(newStatus))
            status.Remove(newStatus);
    }

    public void GainDisguise(Disguise newDisguise)
    {
        if(disguise != newDisguise)
            disguise = newDisguise;
    }

    private void UpdateStatusUI()
    {
        if(uiManager)
        {
            if(status.Contains(Status.Suspicious))
                uiManager.UpdateStatusText("Suspicious", Color.red);
            
            else if(status.Contains(Status.Trespassing))
                uiManager.UpdateStatusText("Trespassing", Color.yellow);

            else
                uiManager.UpdateStatusText("Concealed", Color.white);
        }
    }

    private void UpdateDisguiseUI()
    {
        if(uiManager)
        {
            string newText;

            switch(disguise)
            {
                case Disguise.Civillian:
                {
                    newText = "Disguised as: Civilian";
                    break;
                }
                
                case Disguise.Guard_Tier1:
                {
                    newText = "Disguised as: Hotel Security";
                    break;
                }
                
                case Disguise.Guard_Tier2:
                {
                    newText = "Disguised as: Elite Security";
                    break;
                }
                
                default:
                {
                    newText = $"Disguised as: {disguise}";
                    break;
                }
            }

            uiManager.UpdateDisguiseText(newText);
        }
    }
}
