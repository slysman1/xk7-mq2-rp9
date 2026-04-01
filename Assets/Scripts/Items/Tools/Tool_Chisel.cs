using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_Chisel : Item_Tool
{
    public Holder_CrackedWall wall { get; private set; }
    private FocusArea focusPoint;

    public bool wasHit { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        focusPoint = GetComponentInChildren<FocusArea>(true);
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowInputUI(enable);

        if(wall != null && focusPoint != null)
            focusPoint.gameObject.SetActive(enable);
    }

    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        if (enable)
        {
            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                {
                    if (wall != null)
                    {
                        inputHelp.AddInput(KeyType.LMB, "input_help_pull_out_chisel");
                    }
                    else
                        inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
                }
            }

            if (wall != null)
            {
                Item_Tool toolInHands = inventory.GetToolInHand();

                if(toolInHands != null && toolInHands.GetComponent<Tool_Hammer>() != null)
                        inputHelp.AddInput(KeyType.LMB, "input_help_hit_chisel");
                else
                        inputHelp.AddInput(KeyType.LMB, "input_help_cannot_hit_chisel_need_hammer");
            }


        }
        else
            inputHelp.RemoveInput();
    }

    public override void Interact(Transform carryPoint)
    {
        base.Interact(carryPoint);
        StartCoroutine(MovePosition(transform, transform.position + (-transform.up * .1f), .15f));
        //EnableInteraction(false);
        focusPoint.DisableCollider();
        wasHit = true;
        wall.WeakenTheWall();
    }

    public void SetupChisel(Holder_CrackedWall wall)
    {
        this.wall = wall;
        SetCanPickUpTo(false);
    }

    public Transform GetFocusPointTransform() => focusPoint.transform;
}
