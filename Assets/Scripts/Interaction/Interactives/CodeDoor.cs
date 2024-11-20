using Interaction;

public class CodeDoor : Door, IJammable
{
    private bool jammed;
    
    public bool Jammed => jammed;

    public void ToggleJammed()
    {
        jammed = !jammed;
    }

    public override void Interact()
    {
        if(jammed) return;
        base.Interact();
    }
}
