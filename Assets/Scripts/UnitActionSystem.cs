using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    public event EventHandler<Unit> OnSelectedUnitChange;
    public event EventHandler OnSelectedActionChange;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;


    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private Unit selectedUnit;

    private BaseAction selectedAction;

    private bool isBusy;

    private void Awake() 
    {
        if (Instance != null) {
            Debug.LogError("There's more than one UnitAction System! " + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetSelectedUnit(selectedUnit);
    }

    private void Update() {
        if (isBusy) 
        {
            return;
        }
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if(selectedUnit == null)
        {
            if (UnitManager.Instance.GetFriendlyUnitList().Count > 0) {
                Unit nextPlayerUnit = UnitManager.Instance.GetFriendlyUnitList()[0];
                SetSelectedUnit(nextPlayerUnit);
            } else
            {
                // Game is over
                // for now, return
                return;
            }
        }
        if (InputManager.Instance.GetJumpToNextUnit()) {
            List<Unit> playerUnitList = UnitManager.Instance.GetFriendlyUnitList();
            if (playerUnitList.Count == 0)
            {
                return;
            }
            int currentUnitIndex = playerUnitList.FindIndex((unit) => unit.GetWorldPosition() == selectedUnit.GetWorldPosition());
            if (currentUnitIndex < playerUnitList.Count - 1) {
                SetSelectedUnit(playerUnitList[currentUnitIndex + 1]);
            } else {
                SetSelectedUnit(playerUnitList[0]);
            }
        }

        if(TryHandleUnitSelection())
        {
            return;
        }
        HandleSelectedAction();
    }

    private void HandleSelectedAction()
    {
        if(InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPositionOnlyHitVisible());

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                return;
            }
            if(!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
                return;
            }

            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);

            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetBusy()
    {
        isBusy = true;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame()) {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask)) {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit)) {
                    if (unit == selectedUnit)
                    {
                        return false;
                    }
                    if (unit.IsEnemy())
                    {
                        return false;
                    }

                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }
        return false;
    }

    private void SetSelectedUnit(Unit unit) {
        selectedUnit = unit;
        SetSelectedAction(unit.GetAction<MoveAction>());
        OnSelectedUnitChange?.Invoke(this, selectedUnit);    // Instead send the selected unit so the camera can adjust its height
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;
        OnSelectedActionChange?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }
}
