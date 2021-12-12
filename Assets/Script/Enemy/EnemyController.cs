
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyController : MonoBehaviour
{
    Animator animator;

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
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Stay:
                TargetSearch();
                break;
            case EnemyState.Move:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Damage:
                break;

        }
    }

    private void TargetSearch()
    {

    }

}
