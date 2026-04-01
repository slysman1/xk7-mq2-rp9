using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hammer_ItemCombiner : MonoBehaviour
{
    [Header("Convert Metal")]
    [SerializeField] private float onConverVelocityY = 1.5f;
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private float detectionRadius = 1f;
    [Range(1, 10)]
    [SerializeField]
    private int platesNeededToRefine = 1;

    [Header("Templates Details")]
    [SerializeField] private Item_CoinTemplate[] copperTemplates;
    [SerializeField] private Item_CoinTemplate[] silverTemplates;
    [SerializeField] private Item_CoinTemplate[] goldTemplates;



    [Header("Metal Bar Details")]
    [SerializeField] private Item_MetalBar[] copperBarPrefabs;
    [SerializeField] private Item_MetalBar[] silverBarPrefabs;
    [SerializeField] private Item_MetalBar[] goldBarPrefabs;

    #region Metal Bar Region
    public void TryCombineBars()
    {
        Collider[] colliders = Physics.OverlapSphere(detectionPoint.position, detectionRadius);

        List<Item_MetalBar> copper = new();
        List<Item_MetalBar> silver = new();
        List<Item_MetalBar> gold = new();

        foreach (var col in colliders)
        {
            var bar = col.GetComponent<Item_MetalBar>();
            if (bar == null) continue;

            if (MetalConfig.IsCopper(bar))
                copper.Add(bar);

            if (MetalConfig.IsSilver(bar))
                silver.Add(bar);

            if (MetalConfig.IsGold(bar))
                gold.Add(bar);
        }

        CombineMetalBar(copper.ToArray(), copperBarPrefabs);
        CombineMetalBar(silver.ToArray(), silverBarPrefabs);
        CombineMetalBar(gold.ToArray(), goldBarPrefabs);
    }

    public void CombineMetalBar(Item_MetalBar[] bars, Item_MetalBar[] prefabs)
    {
        List<Item_MetalBar> twos = new();
        List<Item_MetalBar> sixes = new();
        List<Item_MetalBar> created = new();

        foreach (var bar in bars)
        {
            if (bar == null) continue;

            int val = bar.itemData.creditValue;

            if (val == 2) twos.Add(bar);
            else if (val == 6) sixes.Add(bar);
        }

        int simTwos = twos.Count;
        int simSixes = sixes.Count;
        int tens = 0;

        // 🧠 SIMULATION (IDENTICAL)
        while (true)
        {
            if (simSixes > 0 && simTwos >= 2)
            {
                simSixes--;
                simTwos -= 2;
                tens++;
            }
            else if (simSixes >= 2)
            {
                simSixes -= 2;
                tens++;
                simTwos += 1;
            }
            else if (simTwos >= 5)
            {
                simTwos -= 5;
                tens++;
            }
            else break;
        }

        // 🔥 EXECUTION
        if (tens > 0)
        {
            for (int i = 0; i < tens; i++)
            {
                if (sixes.Count > 0 && twos.Count >= 2)
                {
                    RemoveAndDestroyBars(sixes, 1);
                    RemoveAndDestroyBars(twos, 2);
                }
                else if (sixes.Count >= 2)
                {
                    RemoveAndDestroyBars(sixes, 2);

                    ItemDataSO smallData = GetmetalBarByValue(prefabs, 2);
                    var newBar = ItemManager.instance.CreateItem(smallData).GetComponent<Item_MetalBar>();
                    created.Add(newBar);
                }
                else
                {
                    RemoveAndDestroyBars(twos, 5);
                }
            }

            ItemDataSO bigData = GetmetalBarByValue(prefabs, 10);

            for (int i = 0; i < tens; i++)
            {
                var newBar = ItemManager.instance.CreateItem(bigData).GetComponent<Item_MetalBar>();
                created.Add(newBar);
            }
        }

        // 🔄 leftover 2 → 6
        int extraSixes = twos.Count / 3;

        for (int i = 0; i < extraSixes; i++)
        {
            RemoveAndDestroyBars(twos, 3);

            ItemDataSO midData = GetmetalBarByValue(prefabs, 6);
            var newBar = ItemManager.instance.CreateItem(midData).GetComponent<Item_MetalBar>();
            created.Add(newBar);
        }

        // 📍 spawn stack
        float yOffset = 0f;

        foreach (var bar in created.OrderByDescending(b => b.itemData.creditValue))
        {
            bar.transform.position = detectionPoint.position + Vector3.up * yOffset;
            bar.EnableKinematic(false);
            bar.SetVelocity(Vector3.up * onConverVelocityY);
            bar.SetTorque(new Vector3(0, 0, Random.Range(5, 15)));

            yOffset += 0.1f;
        }
    }

    private void RemoveAndDestroyBars(List<Item_MetalBar> list, int count)
    {
        int removeCount = Mathf.Min(count, list.Count);

        for (int i = 0; i < removeCount; i++)
        {
            var item = list[0];
            list.RemoveAt(0);

            if (item != null)
                Destroy(item.gameObject);
        }
    }
    #endregion



    public void CombineTemplate(Item_CoinTemplate[] templates, Item_CoinTemplate[] prefabs,bool shouldBeEmpty)
    {
        List<Item_CoinTemplate> twos = new();
        List<Item_CoinTemplate> sixes = new();

        // 🔥 collect ALL newly created plates
        List<Item_CoinTemplate> createdPlates = new();

        foreach (var plate in templates)
        {
            if (plate == null)
                continue;

            if (plate.EmptyPlate() != shouldBeEmpty)
                continue;

            int val = plate.itemData.creditValue;

            if (val == 2) twos.Add(plate);
            else if (val == 6) sixes.Add(plate);
        }

        int simTwos = twos.Count;
        int simSixes = sixes.Count;
        int tens = 0;

        // 🧠 SIMULATION
        while (true)
        {
            if (simSixes > 0 && simTwos >= 2)
            {
                simSixes--;
                simTwos -= 2;
                tens++;
            }
            else if (simSixes >= 2)
            {
                simSixes -= 2;
                tens++;
                simTwos += 1;
            }
            else if (simTwos >= 5)
            {
                simTwos -= 5;
                tens++;
            }
            else break;
        }

        // 🔥 EXECUTION
        if (tens > 0)
        {
            for (int i = 0; i < tens; i++)
            {
                if (sixes.Count > 0 && twos.Count >= 2)
                {
                    RemoveAndDestroy(sixes, 1);
                    RemoveAndDestroy(twos, 2);
                }
                else if (sixes.Count >= 2)
                {
                    RemoveAndDestroy(sixes, 2);

                    ItemDataSO smallPlateData = GetTemplateByValue(prefabs, 2);
                    Item_CoinTemplate newPlate = ItemManager.instance.CreateItem(smallPlateData).GetComponent<Item_CoinTemplate>();
                    createdPlates.Add(newPlate);
                }
                else
                {
                    RemoveAndDestroy(twos, 5);
                }
            }

            ItemDataSO bigPlateData = GetTemplateByValue(prefabs, 10);

            for (int i = 0; i < tens; i++)
            {
                Item_CoinTemplate newPlate = ItemManager.instance.CreateItem(bigPlateData).GetComponent<Item_CoinTemplate>();
                createdPlates.Add(newPlate);
            }
        }

        // 🔄 leftover 2 → 6
        int extraSixes = twos.Count / 3;

        for (int i = 0; i < extraSixes; i++)
        {
            RemoveAndDestroy(twos, 3);

            ItemDataSO midPlateData = GetTemplateByValue(prefabs, 6);
            Item_CoinTemplate newPlate = ItemManager.instance.CreateItem(midPlateData).GetComponent<Item_CoinTemplate>();
            createdPlates.Add(newPlate);
        }

        // 📊 SORT (big → small)
        createdPlates.Sort((a, b) => b.itemData.creditValue.CompareTo(a.itemData.creditValue));

        // 📍 POSITION STACK (big bottom)
        float yOffset = 0f;
        float step = .1f; // tweak

        for (int i = 0; i < createdPlates.Count; i++)
        {
            Vector3 position = detectionPoint.position + Vector3.up * yOffset;
            SetupCreatedPlate(createdPlates[i], position,shouldBeEmpty);

            yOffset = yOffset + step;
        }
    }

    private void SetupCreatedPlate(Item_CoinTemplate newTemplate, Vector3 position,bool shouldBeEmpty)
    {
        newTemplate.transform.position = position;
        newTemplate.EnableHot(false);
        newTemplate.MakePlateEmpty(shouldBeEmpty);
        newTemplate.EnableKinematic(false);
        newTemplate.SetVelocity(Vector3.up * onConverVelocityY);
        newTemplate.SetTorque(new Vector3(Random.Range(5, 15), 0, 0));

    }


    private void RemoveAndDestroy(List<Item_CoinTemplate> list, int count)
    {
        int removeCount = Mathf.Min(count, list.Count);

        for (int i = 0; i < removeCount; i++)
        {
            var item = list[0];
            list.RemoveAt(0);

            if (item != null)
                Destroy(item.gameObject);
        }
    }

    public void TryCombinePlates(bool useEmptyPlates)
    {
        Collider[] colliders = Physics.OverlapSphere(detectionPoint.position, detectionRadius);

        
        List<Item_CoinTemplate> copper = new();
        List<Item_CoinTemplate> silver = new();
        List<Item_CoinTemplate> gold = new();

        foreach (var col in colliders)
        {
            Item_CoinTemplate plate = col.GetComponent<Item_CoinTemplate>();

            if (plate == null)
                continue;

            if (plate.EmptyPlate() && useEmptyPlates == false)
                continue;

            if (plate.EmptyPlate() == false && useEmptyPlates)
                continue;

            if (MetalConfig.IsCopper(plate))
                copper.Add(plate);

            if (MetalConfig.IsSilver(plate))
                silver.Add(plate);

            if (MetalConfig.IsGold(plate))
                gold.Add(plate);
        }


        CombineTemplate(copper.ToArray(), copperTemplates,useEmptyPlates);
        CombineTemplate(silver.ToArray(), silverTemplates, useEmptyPlates);
        CombineTemplate(gold.ToArray(), goldTemplates, useEmptyPlates);
    }

    public void TryConvertToBars(Item_CoinTemplate templateToConvert)
    {
        Collider[] colliders = Physics.OverlapSphere(detectionPoint.position, detectionRadius);

        List<Item_CoinTemplate> tens = new();

        foreach (var col in colliders)
        {
            var plate = col.GetComponent<Item_CoinTemplate>();

            if (plate == null || !plate.EmptyPlate())
                continue;

            if (plate.itemData.creditValue == 10)
                tens.Add(plate);
        }

        if (tens.Count == 0)
            return;

        int platesPerBar = Mathf.Max(1, platesNeededToRefine);
        int totalBars = tens.Count / platesPerBar;

        if (totalBars <= 0) return;

        // 🔥 get correct bar prefab set
        Item_MetalBar[] barPrefabs = GetBarPrefabsByTemplate(templateToConvert);
        if (barPrefabs == null) return;

        ItemDataSO barData = GetmetalBarByValue(barPrefabs, 10); // or whatever value system you use
        if (barData == null) return;

        // ❌ remove plates
        int toRemove = totalBars * platesPerBar;
        for (int i = 0; i < toRemove; i++)
        {
            if (tens[i] != null)
                Destroy(tens[i].gameObject);
        }


        for (int i = 0; i < totalBars; i++)
        {
            var bar = ItemManager.instance.CreateItem(barData).GetComponent<Item_MetalBar>();

            bar.transform.position = detectionPoint.position;
            bar.EnableKinematic(false);
            bar.SetVelocity(Vector3.up * onConverVelocityY);
            bar.SetTorque(new Vector3(0, 0, Random.Range(5, 15)));
        }
    }


    private ItemDataSO GetTemplateByValue(Item_CoinTemplate[] platePrefabs, int value)
    {
        foreach (var prefab in platePrefabs)
            if (prefab.itemData.creditValue == value)
                return prefab.itemData;

        return null;
    }


    private ItemDataSO GetmetalBarByValue(Item_MetalBar[] barPrefabs, int value)
    {
        foreach (var prefab in barPrefabs)
            if (prefab.itemData.creditValue == value)
                return prefab.itemData;

        return null;
    }

    private Item_MetalBar[] GetBarPrefabsByTemplate(Item_CoinTemplate template)
    {
        if (MetalConfig.IsCopper(template)) 
            return copperBarPrefabs;

        if (MetalConfig.IsSilver(template))
            return silverBarPrefabs;

        if (MetalConfig.IsGold(template))
            return goldBarPrefabs;

        return null;
    }

    public bool CanCombineBars(Transform target)
    {
        var bars = GetNearby<Item_MetalBar>(target);

        int twos = 0, sixes = 0;

        foreach (var b in bars)
        {
            int v = b.itemData.creditValue;
            if (v == 2) twos++;
            else if (v == 6) sixes++;
        }

        return CanMakeTen(twos, sixes) || (twos >= 3); // 2→6 also counts
    }

    public bool CanCombineTemplates(Transform target)
    {
        var plates = GetNearby<Item_CoinTemplate>(target)
            .Where(p => p.EmptyPlate());

        int twos = 0, sixes = 0;

        foreach (var p in plates)
        {
            int v = p.itemData.creditValue;
            if (v == 2) twos++;
            else if (v == 6) sixes++;
        }

        return CanMakeTen(twos, sixes) || (twos >= 3);
    }

    public bool CanRefineTemplates(Transform target)
    {
        int tens = GetNearby<Item_CoinTemplate>(target)
            .Count(p => p.EmptyPlate() && p.itemData.creditValue == 10);

        return tens >= platesNeededToRefine;
    }

    private List<T> GetNearby<T>(Transform target) where T : Component
    {
        return Physics.OverlapSphere(target.position, detectionRadius)
            .Select(c => c.GetComponent<T>())
            .Where(x => x != null)
            .ToList();
    }

    private bool CanMakeTen(int twos, int sixes)
    {
        while (true)
        {
            if (sixes > 0 && twos >= 2)
            {
                sixes--; twos -= 2;
                return true;
            }
            else if (sixes >= 2)
            {
                sixes -= 2;
                twos += 1;
                return true;
            }
            else if (twos >= 5)
            {
                return true;
            }
            else break;
        }

        return false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
    }
}
