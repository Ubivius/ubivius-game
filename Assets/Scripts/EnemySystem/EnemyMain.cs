using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubv.server.logic.ai;
using ubv.common.gameplay.shooting;

public class EnemyMain : MonoBehaviour
{
    [SerializeField] public int HealthPoint;
    //public event EventHandler OnDestroySelf;
    //public event EventHandler<OnDamagedEventArgs> OnDamaged;
    /*public class OnDamagedEventArgs
    {
        public Player attacker;
        public float damageMultiplier;
    }*/

    //public Enemy Enemy { get; private set; }
    public EnemyMovementUpdater EnemyPathFindingMovement { get; private set; }
    //public EnemyTargeting EnemyTargeting { get; private set; }
    //public EnemyStats EnemyStats { get; private set; }
    public Rigidbody2D EnemyRigidbody2D { get; private set; }
    //public ICharacterAnims CharacterAnims { get; private set; }
    //public IAimShootAnims AimShootAnims { get; private set; }

    public HealthSystem HealthSystem { get; private set; }

    private void Awake()
    {
        //Enemy = GetComponent<Enemy>();

        EnemyPathFindingMovement = GetComponent<EnemyMovementUpdater>();
        //EnemyTargeting = GetComponent<EnemyTargeting>();
        //EnemyStats = GetComponent<EnemyStats>();
        EnemyRigidbody2D = GetComponent<Rigidbody2D>();
        //CharacterAnims = GetComponent<ICharacterAnims>();
        //AimShootAnims = GetComponent<IAimShootAnims>();

        HealthSystem = new HealthSystem(HealthPoint);
        Hittable hit = GetComponent<Hittable>();
        hit.OnHit += Hit;
    }

    public Vector2 GetPosition()
    {
        return (Vector2) transform.position;
    }

    private void Hit()
    {
        Debug.Log("OUCH CRISS!");
        HealthSystem.Damage(25);
    }

    /*public void DestroySelf()
    {
        OnDestroySelf?.Invoke(this, EventArgs.Empty);
        GetComponent<CharacterAim_Base>()?.DestroySelf();
    }

    public void Damage(Player attacker, float damageMultiplier)
    {
        OnDamaged?.Invoke(this, new OnDamagedEventArgs
        {
            attacker = attacker,
            damageMultiplier = damageMultiplier,
        });
    }*/
}
