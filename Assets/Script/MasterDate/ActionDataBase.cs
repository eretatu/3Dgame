using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create ActionDataBase")]
public class ActionDataBase : ScriptableObject
{
    [SerializeField]
    private List<ActionData> _ActionList = new List<ActionData>();

    public List<ActionData> ActionList 
    {
        get => _ActionList;
    }
}
