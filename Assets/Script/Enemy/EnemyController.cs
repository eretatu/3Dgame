
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyController : MonoBehaviour,IDamagable
{
    
    [SerializeField]
    EnemtStats enemtStats;
    [SerializeField]
    GameObject Target;
    [SerializeField]
    float _MoveDistance;
    [SerializeField]
    float _AttackDistance;
    [SerializeField]
    float _MoveSpeed;
    private Rigidbody _rb;
    float MaxHp;
    Animator E_animator;

    enum EnemyState
    {
        Move,
        Attack,
        Damage,
        Stay
    }

    private EnemyState _currentState = EnemyState.Stay;
    private EnemyState currentState
    {
        get => _currentState;
        set { _currentState = value; }
    }
    void Start()
    {
        MaxHp = enemtStats.EnemyHp;
        E_animator = this.gameObject.GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Stay:
                break;
            case EnemyState.Move:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Damage:
                break;

        }

        if(Target != null) 
        {
            Vector3 TargetPos = Target.transform.position;

            TargetPos.y = transform.position.y;
            

            var distance = Vector3.Distance(transform.position, Target.transform.position);

            if(distance < _MoveDistance && distance > _AttackDistance) 
            {
                E_animator.SetBool("Attack", false);
                transform.LookAt(TargetPos);
                var direction = (Target.transform.position - transform.position).normalized;
                _rb.velocity = direction * _MoveSpeed;
            }
            if(distance < _AttackDistance) 
            {
                E_animator.SetBool("Attack",true);
            }
        }
    }

    public void AddDamage(float damage, ActionData.ActionType actionData) 
    {
        MaxHp -= damage;
        switch (actionData)
        {
            case ActionData.ActionType.launch:
                Debug.Log(actionData);
                E_animator.CrossFadeInFixedTime("damege", 0);
                break;
            case ActionData.ActionType.Rebellion:
                Debug.Log(actionData);
                break;
        }
        Debug.Log(MaxHp);

    }
}
