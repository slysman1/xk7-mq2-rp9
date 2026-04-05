using System;
using System.Collections;
using UnityEngine;

using static Alexdev.TweenUtils;

public class Order_RequestButton : Interaction_Button
{
    public static event Action OnOrderRequested;

    private DirtManager dirtManager => DirtManager.instance;
    protected UI_OnObjectIndicator objectIndicator;


    [Header("References")]
    [SerializeField] private Transform bell;
    [SerializeField] private Transform rope;

    [Header("Tuning")]
    [SerializeField] private int amountOfSwings;
    [SerializeField] private Vector3 ropeSwingAngle = new Vector3(0f, 0f, -11.73f);
    [SerializeField] private Vector3 bellSwingAngle = new Vector3(0, 0, -7f);
    [SerializeField] private float ropeSwingDuration = 0.15f;
    [SerializeField] private float bellSwingDuration = 0.15f;
    private Coroutine bellCo;



    public override void Highlight(bool enable)
    {
        outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
    }

    public override void Interact(Transform caller = null)
    {
        if (bellCo != null)
            return;

        bellCo = StartCoroutine(RingBellCo());
        OnOrderRequested?.Invoke();


      
        if (dirtManager.CellIsClean() == false)
        {
            Debug.Log("Need to clean cell");
            return;
        }


        OrderManager.instance.RequestNextOrder();
        OnOrderRequested?.Invoke();
        
    }

    private IEnumerator RingBellCo()
    {
        int direction = -1;
        int swings = amountOfSwings;

        yield return RotateLocal(rope, ropeSwingAngle, ropeSwingDuration);
        StartCoroutine(RotateLocal(bell, bellSwingAngle, bellSwingDuration));
        Audio.PlaySFX("quest_bell_ring", transform);

        while (swings > 0)
        {
            yield return RotateLocal(rope, (ropeSwingAngle * direction) * 2, ropeSwingDuration);
            StartCoroutine(RotateLocal(bell, (bellSwingAngle * direction) * 2, bellSwingDuration));

            swings--;
            direction = direction * -1;
            Audio.PlaySFX("quest_bell_ring", transform);
        }


        yield return RotateLocal(rope, GetResetAngle(rope), ropeSwingDuration);
        StartCoroutine(RotateLocal(bell, GetResetAngle(bell), bellSwingDuration));

        bellCo = null;
    }

    private Vector3 GetResetAngle(Transform transform)
    {
        return new Vector3(
            Mathf.DeltaAngle(transform.localEulerAngles.x, 0f),
            Mathf.DeltaAngle(transform.localEulerAngles.y, 0f),
            Mathf.DeltaAngle(transform.localEulerAngles.z, 0f)
        );
    }
}
