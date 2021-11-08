using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public  class AttackType 
{

    public enum Type
    {
        none,
        AttackRebellion,
        Attacklaunch,

    }

    public Type _ATtype;
    public Type AtType
    {
        get => _ATtype;
        set 
        { _ATtype = value; }
    }
}
