using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create ActionData")]
public class ActionData : ScriptableObject
{
    public enum ActionType
    {
        launch,
        Rebellion
    }
    [SerializeField]
    private ActionType _ActionType;
    [SerializeField]
    private string _IndicatesName;
    [SerializeField]
    private string _ActionName;
    [SerializeField]
    private int _Damage;

    public ActionType actionType 
    {
        get => _ActionType;
    }

    public string IndicatesName
    {
        get => _IndicatesName;
    }

    public string ActionName 
    {
        get => _ActionName;
    }

    public int Damege 
    {
        get => _Damage;
    }
}
