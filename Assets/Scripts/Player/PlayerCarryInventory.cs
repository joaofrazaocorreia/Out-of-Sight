using UnityEngine;

public class PlayerCarryInventory : MonoBehaviour
{
    private enum CarriableType {None, Bag, Body};
    private GameObject storedCarriable;
    private CarriableType storedCarriableType;
    public bool CarryingBag {get => storedCarriable != null && storedCarriableType == CarriableType.Bag;}
    public bool CarryingBody {get => storedCarriable != null && storedCarriableType == CarriableType.Body;}
    private Player player;

    private void Start()
    {
        storedCarriable = null;
        storedCarriableType = CarriableType.None;
        
        player = GetComponent<Player>();
    }

    public void PickUpBag(GameObject go)
    {
        if(!storedCarriable)
        {
            storedCarriable = go;
            storedCarriableType = CarriableType.Bag;
            player.status.Add(Player.Status.Doubtful);

            go.transform.position = new Vector3(0, 100, 10);
            go.SetActive(false);
        }
    }

    public void PickUpBody(GameObject go)
    {
        if(!storedCarriable)
        {
            storedCarriable = go;
            storedCarriableType = CarriableType.Body;
            player.status.Add(Player.Status.Suspicious);

            go.transform.position = new Vector3(0, 100, -10);
            go.SetActive(false);
        }
    }

    public void DropCarriable()
    {
        if(storedCarriable != null)
        {
            switch(storedCarriableType)
            {
                case CarriableType.Bag:
                    player.status.Remove(Player.Status.Doubtful);
                    break;

                case CarriableType.Body:
                    player.status.Remove(Player.Status.Suspicious);
                    break;

                default: break;
            }

            storedCarriable.transform.position = transform.GetChild(0).position + (transform.GetChild(0).forward * 3);
            storedCarriable.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f);
            Debug.Log(storedCarriable.transform.GetChild(1).rotation.eulerAngles);
            storedCarriable.SetActive(true);

            storedCarriable = null;
            storedCarriableType = CarriableType.None;
        }
    }
}
