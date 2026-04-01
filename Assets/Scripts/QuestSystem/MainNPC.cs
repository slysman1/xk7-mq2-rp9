using System;
using System.Collections;
using UnityEngine;

public class MainNPC : Interaction_Button
{
    
    public static event Action OnDoorKnocked;
    public static event Action OnQuickDeliveryReport;
    private Player player;

    private Quest_WindowAnimation windowAnimation;
    private Order_DeliveryManager questDelivery;

    public DialogueNodeSO dialogueStart;
    public Transform soundSource;


    [SerializeField] private Object_Outline[] outlineSet;
    [SerializeField] private float openUIDelay = .25f;
    [Header("Knocks VFX")]
    [SerializeField] private ParticleSystem[] knockFxGeneral;
    [SerializeField] private ParticleSystem[] knockFxDelivery;

    private Coroutine secondInteractionCo;

    [Header("Tutorial Utils")]
    [SerializeField] private TutorialStep quickDeliveryTutorialCondition;

    protected override void Awake()
    {
        base.Awake();

        windowAnimation = GetComponentInChildren<Quest_WindowAnimation>();
        questDelivery = GetComponent<Order_DeliveryManager>();
        UI.OnEnableInGameUI += CloseWindow;
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            interactionCo = null;
            secondInteractionCo = null;
        }
    }

    public override void Interact(Transform caller = null)
    {
        if (interactionCo != null || secondInteractionCo != null)
            return;

        if (questDelivery.deliveryCo != null)
            return;


        base.Interact(caller);


        if (LookingAtTheDoor() == false)
            return;

        Player.instance.input.Player.Disable();
        interactionCo = StartCoroutine(InteractionCo());
        Highlight(false);
    }

    private bool LookingAtTheDoor()
    {
        foreach (var outline in outlineSet)
            if (outline.currentOutlineType != OutlineType.Highlight)
                return false;

        return true;
    }

    public override void SeconderyInteraction(Transform caller = null)
    {
        if (questDelivery.deliveryCo != null)
            return;


        if (TutorialManager.instance.CompletedStepNeededToTakeOrders() == false)
            return;

        if (LookingAtTheDoor() == false)
            return;

        if (secondInteractionCo != null || interactionCo != null)
            return;


        base.SeconderyInteraction(caller);


        secondInteractionCo = StartCoroutine(SecondInteractionCo());
        Highlight(false);
    }

    public void CloseWindow()
    {
        interactionCo = null;
        StartCoroutine(windowAnimation.CloseWindowCo(openUIDelay));
    }

    private IEnumerator InteractionCo()
    {
        KnockFX(knockFxGeneral, 0);

        yield return new WaitForSeconds(.2f);
        KnockFX(knockFxGeneral, 1);

        yield return windowAnimation.OpenWindowCo();
        yield return new WaitForSeconds(openUIDelay);

        OnDoorKnocked?.Invoke();

        interactionCo = null;
        secondInteractionCo = null;
    }

    private void KnockFX(ParticleSystem[] knockSet, int knockIndex)
    {
        Audio.PlaySFX("delivery_door_knock", transform);
        knockSet[knockIndex].Play();
    }

    private IEnumerator SecondInteractionCo()
    {
        KnockFX(knockFxDelivery, 0);
        yield return new WaitForSeconds(.2f);

        KnockFX(knockFxDelivery, 1);
        yield return new WaitForSeconds(.4f);

        KnockFX(knockFxDelivery, 2);
        yield return new WaitForSeconds(1f);
        questDelivery.TryToDeliver();

        //yield return new WaitForSeconds(3.5f);
        OnQuickDeliveryReport?.Invoke();


        secondInteractionCo = null;
        interactionCo = null;
    }


    public override void Highlight(bool enable)
    {
        foreach (var item in outlineSet)
            item.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);

        ShowInputUI(enable);
    }

    protected override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);
        UI_InputHelp inputHelp = UI.instance.inGameUI.inputHelp;


        if (enable)
        {
            inputHelp.AddInput(KeyType.LMB, "input_help_knock_on_door");

            if (TutorialManager.instance.CompletedStepNeededToTakeOrders())
                inputHelp.AddInput(KeyType.F, "input_help_knock_report_delivery");
        }
        else
            inputHelp.RemoveInput();
    }




}
