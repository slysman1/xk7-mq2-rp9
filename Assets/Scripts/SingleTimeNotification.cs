using UnityEngine;

public class SingleTimeNotification : MonoBehaviour
{
    private Item_Base item;
    private Workstation workstation;
    [SerializeField] private string notificationKey;

    protected virtual void Awake()
    {
        workstation = GetComponent<Workstation>();
        item = GetComponent<Item_Base>();
        item.OnItemHighlighted += ShowNotification;

        //if (workstation != null)
        //    workstation.OnHighlitgh += ShowNotification;
    }

    protected virtual void ShowNotification()
    {
        UI.instance.NotifyPlayer(notificationKey);
    }

    protected virtual void OnDestroy()
    {
        if (item != null)
            item.OnItemHighlighted -= ShowNotification;

        //if(workstation != null)
        //    workstation.OnHighlitgh -= ShowNotification;
    }

}
