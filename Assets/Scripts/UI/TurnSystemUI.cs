using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnSystemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject enemyTurnVisual;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChange;
        endTurnButton.onClick.AddListener(() => {
            OnTurnButtonClick();
        });

        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndButtonTurnVisibility();
    }

    private void OnTurnButtonClick()
    {
        TurnSystem.Instance.NextTurn();
    }

    private void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndButtonTurnVisibility();
    }

    private void UpdateTurnText()
    {
        int turnNumber = TurnSystem.Instance.GetTurnNumber();
        turnNumberText.text = "TURN: " + turnNumber;
    }

    private void UpdateEnemyTurnVisual()
    {
        enemyTurnVisual.SetActive(!TurnSystem.Instance.IsPlayerTurn());
    }

    private void UpdateEndButtonTurnVisibility()
    {
        endTurnButton.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }
}
