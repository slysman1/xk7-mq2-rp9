using System;
using System.Collections;
using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    public static event Action OnReleasedLMB;

    private Player player;
    private Player_Inventory inventory;
    private Player_PreviewHandler previewHandler;
    private Player_Raycaster raycaster;
    private InputSystem_Actions input;

    [Header("Interaction Settings")]
    [Tooltip("How long player needs to hold button to begin time based interaction. This value should be bigger then duration of a CLICK.")]
    [SerializeField] private float holdThreshold = .15f;

    [Header("Input details")]
    private float lmbHoldStartTime;
    private float rmbHoldStartTime;

    public bool holdingRMB { get; private set; }
    public bool holdingLMB { get; private set; }
    private Coroutine lmbInputCo;
    public bool holdingAlterativeInput { get; private set; }
    private Coroutine alternativeInputCo;
    private Coroutine holdRmbCo;


    private void Awake()
    {
        player = GetComponent<Player>();
        raycaster = GetComponent<Player_Raycaster>();
        inventory = GetComponent<Player_Inventory>();
        previewHandler = GetComponent<Player_PreviewHandler>();
    }

    private void Start()
    {
        SetupInputs();
    }

    private void SetupInputs()
    {
        input = player.input;



        input.Player.Interact.performed += ctx =>
        {
            holdingLMB = true;

            if (lmbInputCo != null)
                StopCoroutine(lmbInputCo);

            lmbInputCo = StartCoroutine(LmbInputCo());

        };
        input.Player.Interact.canceled += ctx =>
        {
            holdingLMB = false;
            OnReleasedLMB?.Invoke();
        };

        input.Player.Release.started += ctx =>
        {
            holdingRMB = true;


            if (holdRmbCo != null)
                StopCoroutine(holdRmbCo);

            holdRmbCo = StartCoroutine(RmbInputCo());
        };
        input.Player.Release.canceled += ctx =>
        {
            holdingRMB = false;
        };


        input.Player.AlternativeInput.performed += ctx =>
        {
            holdingAlterativeInput = true;

            if (UI.instance.notification.isShowingNotification)
            {
                UI.instance.HideNotification();
                return;
            }

            if (alternativeInputCo != null)
                StopCoroutine(alternativeInputCo);

            alternativeInputCo = StartCoroutine(AlternativeInteractionCo());
        };

        input.Player.AlternativeInput.canceled += ctx =>
        {
            holdingAlterativeInput = false;

            if (holdingLMB == false)
                UI.instance.pointerUI.CancelFill();
        };


        input.Player.OpenClose.performed += ctx =>
        {
            if (raycaster.HitInteractable(out RaycastHit hit))
            {
                IOpenable openable = hit.collider.GetComponentInParent<IOpenable>();
                openable?.ToggleLid();
            }
        };
    }





    private IEnumerator AlternativeInteractionCo()
    {
        if (raycaster.HitInteractable(out RaycastHit hit) == false)
            yield break;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null )//|| interactable.HasInteractions() == false)
            yield break;

        float duration = interactable.GetInteractionTime();
        bool interactionReady = interactable.AlternativeInteractionReady();

        if (interactionReady == false)
            yield break;

        // 👉 Instant interaction
        if (duration <= 0f)
        {
            interactable.SeconderyInteraction(transform);
            yield break;
        }

        // 👉 Hold interaction
        float timePassed = 0f;

        // Optional UI
        UI.instance.pointerUI.BeginToFeelPointer(duration);

        while (timePassed < duration)
        {
            if (holdingAlterativeInput == false)
                yield break; // released early → cancel

            timePassed += Time.deltaTime;
            yield return null;
        }

        // 👉 Completed hold
        UI.instance.pointerUI.CancelFill();
        interactable.SeconderyInteraction(transform);
    }

    private IEnumerator LmbInputCo()
    {
        lmbHoldStartTime = Time.time; // reset on each new click

        if (holdingRMB && previewHandler.CanPlaceItem())
        {
            previewHandler.FinalizePlacement();
            yield break;
        }

        if (raycaster.HitInteractable(out RaycastHit hit) == false)
            yield break;

        Item_Base item = hit.collider.GetComponentInParent<Item_Base>();
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        Item_Tool tool = inventory.GetToolInHand();

        // Non-item interactable (doors, buttons etc)
        if (item == null && interactable != null)
        {
            interactable.Interact(transform);
            yield break;
        }

        // Tool interaction
        if (tool != null && item != null && tool.CanInteractWith(item.itemData))
        {
            tool.PerformInteraction(item);
            yield break;
        }

        // Item with hold pickup type — wait to distinguish click vs hold
        if (item != null && item.itemData.pickupType == PickupType.Hold)
        {
            while (holdingLMB && GetHoldDurationLMB() < holdThreshold)
                yield return null;
        }

        // Quick click or non-hold item — let item decide
        if (item != null)
            item.Interact(transform);

        lmbInputCo = null;
    }

    private IEnumerator RmbInputCo()
    {
        rmbHoldStartTime = Time.time;


        while (holdingRMB)
        {
            if (HeldEnoughToTryPlacementPreview() && previewHandler.isPlacingItem == false)
            {
                raycaster.ForceUpdate();
                previewHandler.TryPlacementMode();
            }

            yield return null;
        }


        if (previewHandler.isPlacingItem == false)
        {
            if (inventory.DoingAction())
                yield break;

            if (CanAddItemsToHolder(out ItemHolder holder))
            {
                inventory.AddAllItemsDirectlyToHolder(holder);
                yield break;
            }

            Vector3 position = previewHandler.GetQuickDropPosition();
            inventory.InstantReleaseAllItems(position);
        }
        else
            previewHandler.CancelPlacement();
    }

    public bool CanAddItemsToHolder(out ItemHolder holder)
    {
        holder = GetItemHolder();

        if (holder == null)
            return false;

        if (inventory.DoingAction())
            return false;

        if (holder.ItemCanBePlaced(inventory.GetTopItem()) == false)
            return false;

        return true;
    }

    private bool HeldEnoughToTryPlacementPreview() => GetHoldDurationRMB() > holdThreshold;

    public float GetHoldDurationLMB() => Time.time - lmbHoldStartTime;
    public float GetHoldDurationRMB() => Time.time - rmbHoldStartTime;

    public bool QuickPressLMB() => GetHoldDurationLMB() < holdThreshold;

    private ItemHolder GetItemHolder()
    {
        Item_Base itemInHand = inventory.GetTopItem();

        if (itemInHand == null)
            return null;

        if (raycaster.HitInteractable(out RaycastHit rayHit) == false)
            return null;

        ItemHolder[] allHolders = rayHit.collider.transform.root.GetComponentsInChildren<ItemHolder>();
        foreach (var holder in allHolders)
        {
            if (holder.ItemCanBePlaced(itemInHand))
                return holder;
        }

        return null;
    }

}
