using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform swordTransform;

    private void Awake() {
        if(TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
            moveAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorStarted;
        }
        if(TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
        }
        if(TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted;
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;
        }
    }

    private void Start() {
        EquipRifle();
    }

    private void SwordAction_OnSwordActionCompleted(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        animator.SetTrigger("SwordSlash");
        EquipSword();
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsMoving", true);
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsMoving", false);
    }

    private void MoveAction_OnChangedFloorStarted(object sender, MoveAction.OnChangeFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor)
        {
            animator.SetTrigger("JumpUp");
        }
        else
        {
            animator.SetTrigger("JumpDown");
        }
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        EquipRifle();
        animator.SetTrigger("Shoot");

        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();

        float unitShoulderHeight = 1.7f;
        targetUnitShootAtPosition.y += unitShoulderHeight;

        bulletProjectile.SetUp(targetUnitShootAtPosition);
    }

    private void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);
    }
}
