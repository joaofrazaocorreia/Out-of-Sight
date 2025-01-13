using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Doubtful, Trespassing, Suspicious};
    public enum Disguise {Civillian, Employee, Guard_Tier1, Guard_Tier2};

    [SerializeField] private Sprite civillianDisguiseSprite;
    [SerializeField] private Sprite workerDisguiseSprite;
    [SerializeField] private Sprite guardTier1DisguiseSprite;
    [SerializeField] private Sprite guardTier2DisguiseSprite;

    public List<Status> status;
    public Disguise disguise;
    private UIManager uiManager;
    public bool detectable;

    private void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        detectable = true;

        disguise = Disguise.Civillian;
        status.Add(Status.Normal);
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
            
            else if(status.Contains(Status.Doubtful))
                uiManager.UpdateStatusText("Doubtful", new Color(0.75f, 0.75f, 0.3f));

            else
                uiManager.UpdateStatusText("Concealed", Color.white);
        }
    }

    private void UpdateDisguiseUI()
    {
        if(uiManager)
        {
            string newText;
            Sprite newImage;

            switch(disguise)
            {
                case Disguise.Civillian:
                {
                    newText = "Disguise: Civilian";
                    newImage = civillianDisguiseSprite;
                    break;
                }
                case Disguise.Employee:
                {
                    newText = "Disguise: Hotel Employee";
                    newImage = workerDisguiseSprite;
                    break;
                }
                
                case Disguise.Guard_Tier1:
                {
                    newText = "Disguise: Hotel Security";
                    newImage = guardTier1DisguiseSprite;
                    break;
                }
                
                case Disguise.Guard_Tier2:
                {
                    newText = "Disguise: Elite Security";
                    newImage = guardTier2DisguiseSprite;
                    break;
                }
                
                default:
                {
                    newText = $"Disguise: {disguise}";
                    newImage = civillianDisguiseSprite;
                    break;
                }
            }

            uiManager.UpdateDisguiseText(newText);
            uiManager.UpdateDisguiseImage(newImage);
        }
    }
}
