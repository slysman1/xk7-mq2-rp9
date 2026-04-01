using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Alexdev.TweenUtils;

public class FurnaceHolder_ProductionResult : ItemHolder
{
    public event Action OnTrayEmptied;
    public MeshRenderer trayMesh ;//{ get; private set; }


    [Header("Tray Setup")]
    [SerializeField] private float amountToMove = 0.325f;
    [SerializeField] private float moveDuration = 0.3f;

    [SerializeField] private Transform smallPlacementPoint;
    [SerializeField] private Transform medPlacementPoint;
    [SerializeField] private Transform largePlacementPoint;
    private Vector3 initialLocalPosition;
    private Coroutine moveRoutine;

    [SerializeField] private ItemDataSO[] goldPlatesData;

    protected override void Awake()
    {
        base.Awake();
        initialLocalPosition = transform.localPosition;
        trayMesh = GetComponent<MeshRenderer>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        Item_CoinTemplate template = item as Item_CoinTemplate;

        Transform slot = GetPlacementPoint();

        template.SetItemHolder(this);
        template.EnableKinematic(true);
        template.transform.parent = transform;
        template.transform.position = slot.position;
        template.transform.rotation = slot.rotation;
        template.EnableHot(true);

        if (goldPlatesData.Contains(template.itemData))
            template.CreateTemperPoints();

        base.OnItemAdded(item);
    }
    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);
        OnTrayEmptied?.Invoke();
    }
    

    public void MoveTray(float duration = -1)
    {
         StartCoroutine(MoveTrayCo(initialLocalPosition + new Vector3(amountToMove, 0f, 0f),duration));
    }
    public void ReturnTray(float duration = -1)
    {
        StartCoroutine(MoveTrayCo(initialLocalPosition, duration,false));
    }
    private IEnumerator MoveTrayCo(Vector3 targetPos,float duration,bool trayEnabled = true)
    {
        trayMesh.enabled = trayEnabled;

        float newDuration = duration > 0 ? duration : moveDuration;
        yield return StartCoroutine(MoveLocalPosition(transform, targetPos, newDuration));
    }


    public override Transform GetPlacementPoint()
    {
        if (currentItems.Count == 0) return base.GetPlacementPoint();

        int value = currentItems[0].itemData.creditValue;

        if (value <= 2)
            return smallPlacementPoint;
        if (value <= 6)
            return medPlacementPoint;

        return largePlacementPoint;
    }
}
