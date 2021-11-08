using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public partial class AttackType 
{

    public enum Type
    {
        none,
        AttackRebellion,
        Attacklaunch,

    }

    private Type _ATtype = Type.none;
    public Type AtType
    {
        get { return _ATtype; }
        set { _ATtype = value; }
    }
}
