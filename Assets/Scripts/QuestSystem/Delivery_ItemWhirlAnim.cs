using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Delivery_ItemWhirlAnim : MonoBehaviour
{
    private MainNPC questMain;

    [Header("Whirl Settings")]
    [SerializeField] private Transform whirlCenter;
    [SerializeField] private Transform whirlTarget;
    private Vector3 whirlDefaultPosition;

    [SerializeField] private float whirlUpDistance = 1f;
    [SerializeField] private float whirlDuration = 1f;
    [Space]
    [SerializeField] private float whirlSpeed = 3.5f;
    [SerializeField] private float whirlRadius = 0.4f;
    private float currentWhirlSpeed => whirlSpeed * 100;

    private bool whirlIsActive = false;
    private Dictionary<Item_Base, Coroutine> activeWhirlCoroutines = new();

    private void Awake()
    {
        questMain = GetComponentInParent<MainNPC>();
        whirlDefaultPosition = whirlCenter.position;
    }

    public void WhirlReset()
    {
        whirlIsActive = false;
        whirlCenter.position = whirlDefaultPosition;
        activeWhirlCoroutines.Clear();
    }

    public void WhirlItems(Item_Base item)
    {
        Coroutine whirlRoutine = StartCoroutine(WhirlSingleItemCo(item));
        activeWhirlCoroutines[item] = whirlRoutine;
    }

    public void WhirlAllItems(List<Item_Base> items, float deliveryDuration)
    {
        Audio.PlaySFX("delivery_door_whirl_start", questMain.soundSource);
        Audio.QueSFX("delivery_door_whirl_end", questMain.soundSource, deliveryDuration - .1f);

        whirlIsActive = true;
        StartCoroutine(MoveLocalPosition(whirlCenter, whirlTarget.localPosition, whirlDuration));

        foreach (var item in items)
        {
            Coroutine whirlRoutine = StartCoroutine(WhirlSingleItemCo(item));
            activeWhirlCoroutines[item] = whirlRoutine;
        }
    }

    private IEnumerator WhirlAllItemsCo(List<Item_Base> allItems)
    {
        foreach (var item in allItems)
        {
            yield return null;
            item.EnableKinematic(true);
            Coroutine whirlRoutine = StartCoroutine(WhirlSingleItemCo(item));
            activeWhirlCoroutines[item] = whirlRoutine;
        }
    }

    private IEnumerator WhirlSingleItemCo(Item_Base item)
    {
        item.EnableCollider(false);
        item.EnableKinematic(true);

        float orbitTimer = 0f;

        // Step 1: Get item’s angle and current radius
        Vector3 initialOffset = item.transform.position - whirlCenter.position;

        float angle = Mathf.Atan2(initialOffset.z, initialOffset.x); // radians
        float currentRadius = new Vector2(initialOffset.x, initialOffset.z).magnitude;
        float targetRadius = whirlRadius;

        // Orbit variety
        float angleOffset = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        float orbitSpeedMult = Random.Range(0.8f, 1.2f);
        float orbitDirection = Random.value < 0.5f ? 1f : -1f;
        float verticalWobble = Random.Range(0.05f, 0.2f);
        float verticalFreq = Random.Range(1f, 3f);

        angle += angleOffset;

        while (whirlIsActive)
        {
            orbitTimer += Time.deltaTime;
            angle += orbitDirection * currentWhirlSpeed * Mathf.Deg2Rad * Time.deltaTime * orbitSpeedMult;

            // Smoothly move from current radius to whirl radius
            currentRadius = Mathf.MoveTowards(currentRadius, targetRadius, Time.deltaTime * 3f);

            float wobbleY = Mathf.Sin(orbitTimer * verticalFreq) * verticalWobble;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                wobbleY,
                Mathf.Sin(angle) * currentRadius
            );

            item.transform.position = whirlCenter.position + offset;
            item.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
            yield return null;

        }

        item.EnableCollider(true);
    }



    public void StopItemsWhirl(Item_Base item)
    {
        if (activeWhirlCoroutines.TryGetValue(item, out var coroutine))
        {
            StopCoroutine(coroutine);
            activeWhirlCoroutines.Remove(item);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(whirlCenter.position, whirlRadius);
    }
}
