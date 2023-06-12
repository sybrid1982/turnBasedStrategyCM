using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler<OnDeadEventArgs> OnDead;
    public class OnDeadEventArgs : EventArgs
    {
        public Vector3 damageSourcePosition;
        public float damageForce;
    }

    public event EventHandler OnHealthChanged;
    [SerializeField] private int health = 100;
    [SerializeField] private int healthMax = 100;

    public void TakeDamage(int damage, Vector3 damageSourcePosition, float damageForce)
    {
        health -= damage;

        if (health < 0)
        {
            health = 0;
        }

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if(health == 0)
        {
            Die(damageSourcePosition, damageForce);
        }

    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero, 400f);
    }

    private void Die(Vector3 damageSource, float damageForce)
    {
        OnDead?.Invoke(this, new OnDeadEventArgs {
            damageSourcePosition = damageSource,
            damageForce = damageForce
        });
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }
}
