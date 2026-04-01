using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Woodstation_Button : Interaction_Button
{
    private Workstation_WoodStation woodstation;
    private Item_Base woodStationBase;
    private Player player;
    

    [Header("Levelr")]
    [SerializeField] private Transform lever;
    [SerializeField] private Vector3 leverIdleRot;
    [SerializeField] private Vector3 leverActivatedRot;
    [SerializeField] private float leverActiveDur;
    [SerializeField] private float leverResetDur;

    [Header("Lid ")]
    [SerializeField] private Transform lid;
    [SerializeField] private Vector3 lidIdleRot;
    [SerializeField] private Vector3 lidActivatedRot;
    [SerializeField] private float lidActiveDur;
    [SerializeField] private float lidResetDur;



    protected override void Awake()
    {
        base.Awake();
        woodstation = GetComponentInParent<Workstation_WoodStation>();
        woodStationBase = GetComponentInParent<Item_Base>();
        
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);


        if (player == null)
            player = Player.instance;

        ItemDataSO itemData = woodStationBase.itemData;

        if (enable)
        {
            if (woodstation.currentLogSet == null)
                UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB, "input_help_wood_station_can_request_log");
            else
                UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB, "input_help_wood_station_cannot_request_log");
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }

    public override void Interact(Transform caller = null)
    {
        if (woodstation.currentLogSet != null)
            return;

        base.Interact(caller);

        if (interactionCo == null)
            interactionCo = StartCoroutine(InteractionCo());
    }

    private IEnumerator InteractionCo()
    {
        float stationCooldown = woodstation.GetStationCooldown();

        Audio.PlaySFX("woodcut_lever", transform, leverResetDur);
        yield return StartCoroutine(SetLocalRotationAs(lever,leverActivatedRot,leverActiveDur)); 
        Audio.PlaySFX("woodcut_lever_return", transform, leverResetDur);
        StartCoroutine(SetLocalRotationAs(lever, leverIdleRot,leverResetDur));

        Audio.PlaySFX("woodcut_lever_return", transform, leverResetDur);
        Audio.QueSFX("woodcut_lever_return_finish", transform, leverResetDur - .1f);
        Audio.PlaySFX("woodcut_lid_open", transform);
        
        /*yield return*/ StartCoroutine(SetLocalRotationAs(lid,lidActivatedRot,lidActiveDur));


        //Audio.PlaySFX("woodcut_log_move_in", transform.position);
        woodstation.CreateLog();
        yield return new WaitForSeconds(stationCooldown);

        StartCoroutine(SetLocalRotationAs(lid, lidIdleRot, lidResetDur));
        interactionCo = null;
    }
}
