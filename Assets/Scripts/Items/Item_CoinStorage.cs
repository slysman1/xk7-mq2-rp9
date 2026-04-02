using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Item_CoinStorage : Item_Base, IOpenable
{
    public StorageHolder_Coin coinHolder { get; private set; }

    [Header("Chest Lid Settings")]
    [SerializeField] protected Transform lidTransform;
    [SerializeField] private float openLidDur = .3f;
    [SerializeField] private float closeLidDur = .3f;
    [SerializeField] private Vector3 openLidRot;
    [SerializeField] protected bool lidIsClosed;

    private Coroutine straightUpCo;


    protected override void Awake()
    {
        base.Awake();
        coinHolder = GetComponentInChildren<StorageHolder_Coin>();

        if (lidIsClosed)
            CloseLid();
    }


    public override void Interact(Transform caller)
    {

        if (IsStandingStraight() == false || straightUpCo != null)
        {
            straightUpCo = StartCoroutine(StraightUpStorageCo());
            return;
        }


        if (player.interaction.QuickPressLMB())
        {
            if (lidTransform != null && lidIsClosed)
            {
                OpenLid();
                return;
            }
            else
                coinHolder.TakeItems(1);
        }
        else
            base.Interact(caller);

    }

    public override void SeconderyInteraction(Transform caller = null)
    {
        if (lidIsClosed)
            return;

        if (inventory.DoingAction())
            return;

        coinHolder.TakeItems();
    }

    public override void ShowInputUI(bool enable)
    {
        UI_InputHelp inputHelp = UI.instance.inGameUI.inputHelp;

        if (enable)
        {
            string openClose = lidIsClosed ? "input_help_coin_storage_open" : "input_help_coin_storage_close";
            inputHelp.AddInput(KeyType.E, openClose);

            if (coinHolder.currentItems.Count > 0)
            {
                inputHelp.AddInput(KeyType.LMB, "input_help_coin_storage_take_coin");

                if (coinHolder.currentItems.Count > 1)
                    inputHelp.AddInput(KeyType.F, "input_help_coin_storage_take_max_coins");
            }



            if (player.inventory.CanPickup(this))
                inputHelp.AddInput(KeyType.LMB_Hold, "input_help_coin_storage_pick_up_chest");

            if (player.inventory.TryGetCoinsInHands(out int stampedCoins, out int unstampedCoins))
            {
                if (unstampedCoins == 0)
                    inputHelp.AddInput(KeyType.RMB, "input_help_coin_storage_add_coins", true);
                else
                    inputHelp.AddInput(KeyType.RMB, "input_help_coin_storage_cannot_add_coins_with_no_stamp", true);


            }
        }
        else
            inputHelp.RemoveInput();
    }

    public void ToggleLid()
    {
        if (lidTransform == null)
            return;

        if (lidIsClosed)
        {
            OpenLid();
            return;
        }

        if (lidIsClosed == false)
        {
            CloseLid();
            return;
        }
    }

    protected void OpenLid()
    {
        if (lidTransform == null)
            return;

        StartCoroutine(OpenLidCo());
    }
    protected void CloseLid()
    {
        if (lidTransform == null)
            return;

        lidIsClosed = true;
        Audio.PlaySFX("chest_close", transform);
        StartCoroutine(SetLocalRotationAs(lidTransform, Vector3.zero, closeLidDur));
    }

    private IEnumerator OpenLidCo()
    {
        if (IsStandingStraight() == false)
        {
            EnableKinematic(false);
            SetVelocity(new Vector3(0, 3.5f, 0));
            yield return StartCoroutine(SetRotationAs(transform, new Vector3(0, transform.rotation.y, 0), .8f));
        }
        else
            yield return null;

        lidIsClosed = false;
        Audio.PlaySFX("chest_open", transform);
        StartCoroutine(SetLocalRotationAs(lidTransform, openLidRot, openLidDur));
    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();
        CloseLid();


        foreach (var item in coinHolder.currentItems)
            item.EnableCamPriority(true);
    }

    private IEnumerator StraightUpStorageCo()
    {
        lidIsClosed = true;
        Audio.PlaySFX("chest_close", transform);
        yield return StartCoroutine(SetLocalRotationAs(lidTransform, Vector3.zero, closeLidDur));

        EnableKinematic(false);
        SetVelocity(new Vector3(0, 3.5f, 0));
        yield return StartCoroutine(SetRotationAs(transform, new Vector3(0, transform.rotation.y, 0), .4f));

        straightUpCo = null;
    }

    public bool IsLidOpen() => lidIsClosed == false;
}
