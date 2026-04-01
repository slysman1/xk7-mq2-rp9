using UnityEngine;

public class SingleTimeNotification_Template : SingleTimeNotification
{
    private Item_CoinTemplate template;

    [Header("Notification keys")]
    [SerializeField] private string emptyTemplateKey;
    [SerializeField] private string templateKey;
    [SerializeField] private string templateHotKey;
    [SerializeField] private string goldenTemplateKey;

    [Header("Item data")]
    [SerializeField] private ItemDataSO[] silverTemplateData;

    private bool isSilverTemplate;

    protected override void Awake()
    {
        base.Awake();
        template = GetComponent<Item_CoinTemplate>();
        isSilverTemplate = MetalConfig.IsSilver(template);
    }

}
