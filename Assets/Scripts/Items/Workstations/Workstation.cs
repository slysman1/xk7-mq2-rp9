using System;
using UnityEngine;

public class Workstation : MonoBehaviour
{
    protected Player player
    {
        get
        {
            if (_player == null)
                _player = FindFirstObjectByType<Player>();

            return _player;
        }
    }
    private Player _player;
    protected Player_Inventory inventory => player.inventory;


    protected UI_InputHelp inputHelp;
    protected Item_Base itemBase;
    protected Item_Tool toolInHand;
    protected Item_Base itemInHand;


    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        inputHelp = UI.instance.inputHelp;
        itemBase = GetComponent<Item_Base>();
    }



    public virtual void ExecuteInteraction(Transform caller = null)
    {
        
    }

    public virtual void ExecuteSecondInteraction(Transform caller = null)
    {
        
    }

    


    protected virtual bool CanBeExecuted()
    {
        return true;
    }

    public virtual float GetInteractionTime()
    {
        return 0;
    }

    public virtual bool IsBusy() => true;

    public virtual bool CanExecuteSecondInteraction() => true;

    protected virtual bool IsStandingStraight()
    {
        return Vector3.Angle(transform.up, Vector3.up) < 5f; // tolerance in degrees
    }

}
