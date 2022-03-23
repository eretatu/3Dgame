using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create EnemyData")]
public class EnemtStats : ScriptableObject
{
    public enum _EnemyType 
    {
        boxing,
        karate,
    }
    [SerializeField]
    private _EnemyType EnemyType;
    [SerializeField]
    private int _EnemyHp;
    public _EnemyType enemyType 
    {
        get => EnemyType;
    }

    public int EnemyHp 
    {
        get => _EnemyHp;
    }
    

}
