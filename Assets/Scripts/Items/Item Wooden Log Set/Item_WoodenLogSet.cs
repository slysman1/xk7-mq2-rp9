
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item_WoodenLogSet : Item_Base
{
    public FocusPoint currentFocus { get; private set; }
    //private Object_Outline[] outlines;
    private Workstation_WoodStation woodStation;


    [SerializeField] private GameObject[] longLogs;
    [SerializeField] private List<Item_Base> shortLogs = new List<Item_Base>();
    [SerializeField] private List<FocusPoint> focusPoints;
    private int chopCount;
    private int maxChopCount = 3;


    [Header("On Chop Velocity")]
    [SerializeField] private float upVelocity;
    [SerializeField] private float forwardVelocity;
    [SerializeField] private Vector3 torqFlip;

    private Coroutine interactionCo;
    public bool canBeChopped { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        shortLogs.Clear();
        Item_WoodenLog[] foundWoodenLogs = GetComponentsInChildren<Item_WoodenLog>(true);

        foreach (var log in foundWoodenLogs)
            this.shortLogs.Add(log);

        //for (int i = 1; i < longLogs.Length; i++)
        //    longLogs[i].gameObject.SetActive(false);

        currentFocus = focusPoints[0];
        woodStation = GetComponentInParent<Workstation_WoodStation>();
        //outlines = GetComponentsInChildren<Object_Outline>(true);
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);

        if(currentFocus != null)
            currentFocus.gameObject.SetActive(enable);

        
        UI_InputHelp inputHelp = UI.instance.inGameUI.inputHelp;
        Item_Tool toolInHand = player.inventory.GetToolInHand();
        bool playerHasNeededTool = toolInHand == null? false : toolInHand.CanInteractWith(itemData);



        if (enable)
        {
            if (playerHasNeededTool)
                inputHelp.AddInput(KeyType.LMB, "input_logset_can_use");
            else
                inputHelp.AddInput(KeyType.LMB, "input_logset_cannot_use");
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }


    public override void Interact(Transform carryPoint)
    {
        base.Interact(carryPoint);

        if(woodStation == null)
            woodStation = GetComponentInParent<Workstation_WoodStation>(true);

        if(interactionCo == null)
            interactionCo = StartCoroutine(InteractionCo());

    }

    public bool CanBeChopped() => currentFocus.hovered && interactionCo == null;

    private IEnumerator InteractionCo()
    {
        currentFocus.gameObject.SetActive(false);
        Item_Base logToChop = shortLogs.FirstOrDefault();
        ChopLog(logToChop);

        // This will move log set on woodstation if we still have logs to chop
        if (shortLogs.Count > 0)
            woodStation.MoveLogSet();

        yield return new WaitForSeconds(woodStation.GetStationCooldown());
        interactionCo = null;
    }

    private void ChopLog(Item_Base logToChop, int direction = 1 )
    {
        Vector3 forward = (logToChop.transform.right * forwardVelocity) * direction;
        Vector3 up = logToChop.transform.up * upVelocity;

        //logToChop.outline.EnableOutline(OutlineType.None);
        logToChop.gameObject.SetActive(true);
        logToChop.transform.parent = null;
        logToChop.EnableKinematic(false);
       //logToChop.EnableInteraction(true);
        logToChop.SetVelocity(forward + up);
        logToChop.SetTorque(torqFlip);

        shortLogs.Remove(logToChop);

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        
        chopCount++;

        if (chopCount < focusPoints.Count)
            currentFocus = focusPoints[chopCount];

        if (chopCount == 1)
        {
            longLogs[0].SetActive(false);
            longLogs[1].transform.localScale = Vector3.one;
        }

        if (chopCount == 2)
        {
            longLogs[1].SetActive(false);
            longLogs[2].transform.localScale = Vector3.one;
        }

        if (chopCount == 3)
        {

            ChopLog(shortLogs[0], -1);

            Destroy(gameObject);
        }

        //outlines = GetComponentsInChildren<Object_Outline>(true);
    }

    //public override void EnableInteraction(bool enable,float delay = 0)
    //{
    //    base.EnableInteraction(enable);
    //    canBeChopped = enable;
    //}

    public int GetChopCount() => chopCount;

    public override bool CanBePickedUp() => false;
}
