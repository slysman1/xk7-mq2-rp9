using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Alexdev.TweenUtils;

public class Holder_CrackedWall : ItemHolder
{
    [SerializeField] private CrackedWall_Brick[] interactiveBricks;
    private CrackedWall_Brick[] allBricks;
    private List<CrackedWall_ChiselSlot> mainChiselSetSlots = new List<CrackedWall_ChiselSlot> ();
    private Dictionary<CrackedWall_ChiselSlot, bool> slotUsed = new();
    private CrackedWall_ChiselSlot[] allChiselSlots;


    [SerializeField] private ItemDataSO interactionTool;
    [SerializeField] private int neededChiselsToCrack = 4;


    private int currentIndex = 0;
    private int pushBrickIndex = 0;
    private bool isColapsed;

    [Header("Push bricks")]
    [SerializeField] private float pushForce = -8.5f;
    [SerializeField] private float pushMultiplier = 1.6f;

    [Header("Starting State")]
    [SerializeField] private int chiselsAtStart;
    [SerializeField] private ItemDataSO chiselData;

    protected override void Awake()
    {
        base.Awake();

        allBricks = GetComponentsInChildren<CrackedWall_Brick>(true);
        allChiselSlots = GetComponentsInChildren<CrackedWall_ChiselSlot>(true);

        maxCapacity = neededChiselsToCrack;

        SetupNextChiselSet();
        //Collapse();

    }

    private void Start()
    {
        InitializeChisels();
    }

    private void InitializeChisels()
    {
        for (int i = 0; i < chiselsAtStart; i++)
        {
            Tool_Chisel chisel = ItemManager.instance.CreateItem(chiselData).GetComponent<Tool_Chisel>();
            AddItem(chisel);
        }
    }

    private void SetupNextChiselSet()
    {
        mainChiselSetSlots.Clear();
        slotUsed.Clear();

        // Start offset is based on which stage (pushBrickIndex)
        int startIndex = pushBrickIndex * neededChiselsToCrack;

        for (int i = startIndex; i < startIndex + neededChiselsToCrack && i < allChiselSlots.Length; i++)
        {
            var slot = allChiselSlots[i];
            mainChiselSetSlots.Add(slot);
            slotUsed[slot] = false;
            slot.gameObject.SetActive(false);
        }

        currentIndex = 0;
    }


    public void WeakenTheWall()
    {
        if (AllChiselsIn())
        {
            interactiveBricks[0].EnableBrick(true);
        }
    }


    public bool AllChiselsIn()
    {
        if (currentItems.Count < neededChiselsToCrack)
            return false;

        foreach (var item in currentItems)
        {
            Tool_Chisel chisel = item as Tool_Chisel;
            if (chisel == null || chisel.wasHit == false)
                return false;
        }

        return true;
    }

    public void PushBricks(Transform hitBrick)
    {
        if (isColapsed)
        {
            hitBrick.GetComponent<CrackedWall_Brick>().TryToPush(pushForce * pushMultiplier);
            return;
        }

        if (!AllChiselsIn())
            return;

        if (pushBrickIndex == 0)
        {
            if (!interactiveBricks[0].focusArea.hovered)
                return;

            Transform brickToMove = interactiveBricks[0].transform;
            StartCoroutine(MovePosition(brickToMove, brickToMove.position + (-brickToMove.forward * .2f), .2f));
            pushBrickIndex++;
            interactiveBricks[0].EnableBrick(false);

            RemoveChiselsAndSlots();
            return;
        }

        if (pushBrickIndex == 1)
        {
            if (!interactiveBricks[0].focusArea.hovered)
                return;

            interactiveBricks[0].TryToPush(pushForce);
            pushBrickIndex += 10;

            Collapse();
            RemoveChiselsAndSlots();
            return;
        }

        if (pushBrickIndex == 2)
        {
            if (!interactiveBricks[1].focusArea.hovered)
                return;

            interactiveBricks[1].TryToPush(pushForce);
            pushBrickIndex++;

            RemoveChiselsAndSlots();
        }
    }


    private void RemoveChiselsAndSlots()
    {
        RemoveAmount(currentItems.Count, true);

        // Remove used slots
        for (int i = mainChiselSetSlots.Count - 1; i >= 0; i--)
        {
            var slot = mainChiselSetSlots[i];
            if (slotUsed.ContainsKey(slot) && slotUsed[slot])
            {
                slotUsed.Remove(slot);
                mainChiselSetSlots.RemoveAt(i);
                slot.gameObject.SetActive(false);
            }
        }

        SetupNextChiselSet();

    }


    [ContextMenu("Collapse Wall")]
    public void Collapse()
    {
        foreach (var brick in allBricks)
            brick.Collapse();

        isColapsed = true;
    }

    public override void Highlight(bool enable)
    {
        // Only show the next available slot when highlighting
        EnablePlaceholder(enable);

        //foreach (var brick in allBricks)
        //{
        //    if (brick.canBePushed)
        //        brick.Highlight(enable);
        //}
    }



    private void EnablePlaceholder(bool enable)
    {
        // Hide all slots first
        foreach (var slot in mainChiselSetSlots)
            slot.gameObject.SetActive(false);

        if (!enable)
            return;

        // Find the next available slot and show only it
        CrackedWall_ChiselSlot nextSlot = GetNextSlot();
        if (nextSlot != null)
            nextSlot.gameObject.SetActive(true);
    }

    public void OnChiselUsed(CrackedWall_ChiselSlot slot)
    {
        if (slotUsed.ContainsKey(slot))
            slotUsed[slot] = true;

        currentIndex = Mathf.Min(currentIndex + 1, mainChiselSetSlots.Count - 1);

        // Refresh next slot visibility
        EnablePlaceholder(true);
    }

    protected override void OnItemAdded(Item_Base item)
    {
        CrackedWall_ChiselSlot slot = GetNextSlot();
        if (slot == null)
            return;

        item.EnableKinematic(true);
        item.SetItemHolder(this);
        item.transform.position = slot.transform.position;
        item.transform.rotation = slot.transform.rotation;
        item.transform.parent = transform;

        slotUsed[slot] = true;
        currentIndex = Mathf.Min(currentIndex + 1, mainChiselSetSlots.Count - 1);

        EnablePlaceholder(false);

        Tool_Chisel chisel = item as Tool_Chisel;
        chisel.SetupChisel(this);
    }


    private CrackedWall_ChiselSlot GetNextSlot()
    {
        foreach (var slot in mainChiselSetSlots)
        {
            if (!slotUsed[slot])
                return slot;

        }

        return null;
    }

}
