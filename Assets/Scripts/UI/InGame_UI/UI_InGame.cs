using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_InGame : MonoBehaviour
{
    private UI_CurrentGoal currentGoalUI;
    public UI_OrderList orderListUI { get; private set; }
    public UI_InputHelp inputHelp { get; private set; }
    private CanvasGroup canvas;

    private void Awake()
    {
        currentGoalUI = GetComponentInChildren<UI_CurrentGoal>(true);
        orderListUI = GetComponentInChildren<UI_OrderList>();
        inputHelp = GetComponentInChildren<UI_InputHelp>(true);
        canvas = GetComponent<CanvasGroup>();
    }


    public void UpdateCurrentGoal(string goalText)
    {

        currentGoalUI.UpdateGoal(goalText);
    }
    public void UpdateOrderListUI(List<OrderDataSO> currentOrders) => orderListUI.UpdateOrderList(currentOrders);
    public bool IsActive() => canvas.alpha >= 1f;
}
