using UnityEngine;

public class Player_Highlighter : MonoBehaviour
{
    private IHighlightable currentHighlight;
    private IHoverable currentHover;
    private Player player;


    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        UpdateHighlight();
        UpdateHover();
    }

   
    public void UpdateHighlight()
    {
        if (player.interaction.holdingRMB && player.inventory.HasItemInHands())
        {
            ClearHighlight();
            return;
        }

        if (player.raycaster.HitHighlight(out RaycastHit hit) == false)
        {
            ClearHighlight();
            return;
        }

        if (ShouldClearHighlightEarly(hit))
        {
            ClearHighlight();
            return;
        }

        ClearHighlight();
        currentHighlight = hit.collider.GetComponentInParent<IHighlightable>();
        currentHighlight?.Highlight(true);
    }

    private void UpdateHover()
    {
        if (player.raycaster.HitHover(out RaycastHit hit))
        {
            IHoverable newHover = hit.collider.GetComponentInParent<IHoverable>();

            if (newHover != currentHover)
            {
                currentHover?.OnHoverExit();
                currentHover = newHover;
                currentHover?.OnHoverEnter();
            }
        }
        else
        {
            if (currentHover != null)
            {
                currentHover.OnHoverExit();
                currentHover = null;
            }
        }
    }

    private bool ShouldClearHighlightEarly(RaycastHit hit)
    {
        Item_Base detectedItem = hit.collider.GetComponent<Item_Base>();
        Item_Base itemInHand = player.inventory.GetTopItem();

        if (detectedItem == null)
            return false;

        if (itemInHand == false)
            return false;

        if (itemInHand is Item_Tool toolInHand)
        {
            if (toolInHand.CanInteractWith(detectedItem.itemData))
                return false;
        }


        //if (detectedItem.HasInteractions())
        //    return false;

        if (detectedItem.CanStackWith(itemInHand, player.inventory.GetCarriedItems().Count))
            return false;

        return true;
    }

    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.Highlight(false);
            currentHighlight = null;
        }

        UI.instance.inputHelp.Refresh();

    }
}
