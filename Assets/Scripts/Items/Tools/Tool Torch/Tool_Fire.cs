using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_Fire : Item_Tool
{
    [Header("Light source")]
    [SerializeField] private GameObject[] lightSource;
    [SerializeField] private LayerMask whatIsGround;


    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        yield return null;
        Item_DirtWeb dirt = item as Item_DirtWeb;

        if (dirt == null)
            yield break;    

        Vector3 startPos = transform.localPosition;
        Vector3 pokeOffset = Vector3.forward * 0.15f; // distance of poke
        float duration = 0.15f;

        // Move forward
        Audio.PlaySFX("torch_move", transform);
        yield return StartCoroutine(MoveLocalPosition(transform, startPos + pokeOffset, duration));
        Audio.PlaySFX("torch_burn_web", dirt.transform ,3f);
        dirt.BurnWeb(interactionDur);

        StartCoroutine(MoveLocalPosition(transform, GetInHandPosition(), .2f));

        interactionCo = null;
    }

    public override void OnItemUnpack()
    {
        base.OnItemUnpack();
        EnableLightSource(false);
    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();
        EnableLightSource(true);
    }

    public override void OnItemDrop()
    {
        base.OnItemDrop();
        EnableLightSource(true);
    }

    public override void OnItemBeingPlaced(Vector3 placementPosition)
    {
        base.OnItemBeingPlaced(placementPosition);

        if (itemData.placementType == PlacementType.WallOnly)
            Audio.PlaySFX("torch_attached_to_wall", placementPosition);
    }

    private void EnableLightSource(bool enable)
    {
        foreach (var item in lightSource)
        {
            if (item != null)
                item.gameObject.SetActive(enable);
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (((1 << collision.gameObject.layer) & whatIsGround) != 0)
        {
            EnableLightSource(false);
        }
    }
}
