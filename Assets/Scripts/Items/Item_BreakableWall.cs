public class Item_BreakableWall : Item_Base
{
    private Holder_CrackedWall chiselHolder;


    protected override void Awake()
    {
        base.Awake();
        chiselHolder = GetComponentInChildren<Holder_CrackedWall>();
    }
    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowInputUI(enable);
    }

    public override void ShowInputUI(bool enable)
    {
        Item_Base itemInHand = inventory.GetTopItem();

        if (enable)
        {

        if (itemInHand != null)
        {

            if (chiselHolder.AllChiselsIn() && itemInHand.GetComponent<Tool_Hammer>() != null)
                    inputHelp.AddInput(KeyType.LMB, "input_help_hit_the_wall");
            
            if(itemInHand.GetComponent<Tool_Chisel>() != null)
                    inputHelp.AddInput(KeyType.LMB, "input_help_add_chisel");
        }
        }
        else
            inputHelp.RemoveInput();
    }
}
