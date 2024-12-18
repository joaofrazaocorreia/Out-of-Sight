using UnityEngine;

public class PlayerBodyInventory : MonoBehaviour
{
    private GameObject storedBody;
    public bool CarryingBody {get => storedBody != null;}


    public void PickUpBody(GameObject go)
    {
        storedBody = go;
        go.transform.position = new Vector3(0, 100, 0);
        go.SetActive(false);
    }

    public void DropBody()
    {
        if(storedBody != null)
        {
            storedBody.transform.position = transform.GetChild(0).position + transform.GetChild(0).forward * 3;
            storedBody.SetActive(true);

            storedBody = null;
        }
    }
}
