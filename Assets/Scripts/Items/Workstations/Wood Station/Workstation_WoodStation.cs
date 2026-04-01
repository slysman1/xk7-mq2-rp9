using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Workstation_WoodStation : Workstation
{
    //private Object_Outline[] outlines;

    [Header("New Log Creation")]
    [SerializeField] private ItemDataSO logSetPrefab;
    [SerializeField] private Transform logSetCreationPoint;
    [SerializeField] private Vector3[] logReadyPosition;
    [SerializeField] private float logMoveDelay = .3f;
    [SerializeField] private float logReadyMoveDur;
    public Item_WoodenLogSet currentLogSet { get; private set; }

    [Header("Interaction setup")]
    [SerializeField] private float timeNeededToInteract = .75f;

    private Coroutine interactionCo;
    private Coroutine moveLogCo;


    public void CreateLog()
    {
        if (interactionCo != null)
            return;

        interactionCo = StartCoroutine(CreateLogCo());
    }

    private IEnumerator CreateLogCo()
    {
        if (currentLogSet == null)
        {

            currentLogSet = ItemManager.instance.CreateItem(logSetPrefab).GetComponent<Item_WoodenLogSet>();

            currentLogSet.EnableKinematic(true);
            currentLogSet.transform.parent = transform;
            currentLogSet.transform.localPosition = logSetCreationPoint.localPosition;
            currentLogSet.transform.localRotation = logSetCreationPoint.localRotation;

            yield return StartCoroutine(MoveLocalPosition(currentLogSet.transform, logReadyPosition[0], logReadyMoveDur));

        }
        //else

        yield return null;
        interactionCo = null;
    }

    public void MoveLogSet()
    {
        if (moveLogCo != null)
            return;

        moveLogCo = StartCoroutine(MoveLogSetCo());
    }

    private IEnumerator MoveLogSetCo()
    {
        int index = currentLogSet.GetChopCount();
        yield return new WaitForSeconds(logMoveDelay);
        yield return StartCoroutine(MoveLocalPosition(currentLogSet.transform, logReadyPosition[index], logReadyMoveDur));

        moveLogCo = null;
    }

    public float GetInteractionTimeNeeded() => timeNeededToInteract;

    public float GetStationCooldown() => logMoveDelay + logReadyMoveDur;
}
