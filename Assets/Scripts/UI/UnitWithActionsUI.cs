using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitWithActionsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI unitCountText;
    void Start()
    {
        UnitActionSystem.Instance.OnActionStarted += UnitActionSystem_OnActionStarted;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        UpdateText();
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            unitCountText.enabled = true;
        }
        else
        {
            unitCountText.enabled = false;
        }
    }

    private void UnitActionSystem_OnActionStarted(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        unitCountText.text = "Units with actions left: " + UnitManager.Instance.GetFriendlyUnitsWithActionPointsCount();
    }
}
