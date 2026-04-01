using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Alexdev.TweenUtils;

public class FurnaceHolder_ProductionResult : ItemHolder
{
    public event Action OnTrayEmptied;
    private MeshRenderer trayMesh;
    private List<FurnaceHolder_ProductionResultSlot> slots;


    [Header("Tray Setup")]
    [SerializeField] private float amountToMove = 0.325f;
    [SerializeField] private float moveDuration = 0.3f;

    [SerializeField] private Transform smallPlacementPoint;
    [SerializeField] private Transform medPlacementPoint;
    [SerializeField] private Transform largePlacementPoint;

    [Space]
    [SerializeField] private ItemDataSO[] goldPlatesData;
    private Vector3 defaultOriginalPosition;

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<FurnaceHolder_ProductionResultSlot>();
        defaultOriginalPosition = transform.localPosition;
        trayMesh = GetComponent<MeshRenderer>();
        trayMesh.enabled = false;   
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item); // lands on slot

        Item_CoinTemplate template = item as Item_CoinTemplate;

        // reposition to correct sized point
        int value = template.itemData.creditValue;
        Transform point = value <= 2 ? smallPlacementPoint : value <= 6 ? medPlacementPoint : largePlacementPoint;
        template.transform.position = point.position;
        template.transform.rotation = point.rotation;

        template.EnableHot(true);
        if (goldPlatesData.Contains(template.itemData))
            template.InitializeTemperPoints();
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);
        OnTrayEmptied?.Invoke();
    }
    

    public void MoveTray(float duration = -1)
    {
         StartCoroutine(MoveTrayCo(defaultOriginalPosition + new Vector3(amountToMove, 0f, 0f),duration));
         currentItems[0].EnableCollider(true);
    }
    public void ReturnTray(float duration = -1)
    {
        StartCoroutine(MoveTrayCo(defaultOriginalPosition, duration,false));
    }
    private IEnumerator MoveTrayCo(Vector3 targetPos,float duration,bool trayEnabled = true)
    {
        trayMesh.enabled = trayEnabled;

        float newDuration = duration > 0 ? duration : moveDuration;
        yield return StartCoroutine(MoveLocalPosition(transform, targetPos, newDuration));
    }

}
