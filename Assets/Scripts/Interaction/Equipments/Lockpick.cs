public class Lockpick : EquipmentObject
{
    private void Start()
    {
        CanBeUsed = true;
    }

    public override void Used(InteractiveObject activeInteractiveObject)
    {
        return;
    }
}
