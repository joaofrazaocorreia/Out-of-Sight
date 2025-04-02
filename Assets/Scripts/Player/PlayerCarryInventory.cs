using UnityEngine;

public class PlayerCarryInventory : MonoBehaviour
{
    [SerializeField] private Transform bagCarryPosition;
    [SerializeField] private Transform bodyCarryPosition;

    private enum CarriableType {None, Bag, Body};
    private GameObject storedCarriable;
    private CarriableType storedCarriableType;
    private Transform storedCarriableParent;
    public bool CarryingBag {get => storedCarriable != null && storedCarriableType == CarriableType.Bag;}
    public bool CarryingBody {get => storedCarriable != null && storedCarriableType == CarriableType.Body;}
    private Player player;

    private void Start()
    {
        storedCarriable = null;
        storedCarriableType = CarriableType.None;
        storedCarriableParent = null;
        
        player = GetComponent<Player>();
    }

    public void PickUpBag(GameObject go)
    {
        if(!storedCarriable)
        {
            storedCarriable = go;
            storedCarriableType = CarriableType.Bag;
            storedCarriableParent = go.transform.parent;
            player.GainStatus(Player.Status.Doubtful);

            go.transform.parent = bagCarryPosition;
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
        }
    }

    public void PickUpBody(GameObject go)
    {
        if(!storedCarriable)
        {
            storedCarriable = go;
            storedCarriableType = CarriableType.Body;
            storedCarriableParent = go.transform.parent;
            player.GainStatus(Player.Status.Suspicious);

            go.transform.parent = bodyCarryPosition;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    public void DropCarriable()
    {
        if(storedCarriable != null)
        {
            switch(storedCarriableType)
            {
                case CarriableType.Bag:
                    player.LoseStatus(Player.Status.Doubtful);
                    break;

                case CarriableType.Body:
                    player.LoseStatus(Player.Status.Suspicious);
                    break;

                default: break;
            }

            storedCarriable.transform.parent = storedCarriableParent;
            storedCarriable.transform.position = transform.GetChild(0).position + (transform.GetChild(0).forward * 3);
            storedCarriable.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f);

            storedCarriable = null;
            storedCarriableType = CarriableType.None;
            storedCarriableParent = null;
        }
    }
}
