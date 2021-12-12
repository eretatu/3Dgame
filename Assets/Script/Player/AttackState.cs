using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public partial class PlayerCommon 
{
    void ComboAttack()
    {
        currentState = PlayerState.attack;
        if (_OnAttack)
        {
            _OnMove = false;
            switch (AttackCount)
            {
                case 0:
                    animator.CrossFadeInFixedTime(script.Attack_1.ActionName, 0);
                    actionData = script.Attack_1;
                    AttackCount++;
                    combo = DOVirtual.DelayedCall(AttackChance, () => { AttackCount = 0; });
                    _OnAttack = false;
                    DOVirtual.DelayedCall(AttackCoolTime, () => { _OnAttack = true; });
                    break;
                case 1:
                    combo.Kill();
                    animator.CrossFadeInFixedTime(script.Attack_2.ActionName, 0);
                    actionData = script.Attack_2;
                    AttackCount++;
                    combo = DOVirtual.DelayedCall(AttackChance, () => { AttackCount = 0; });
                    _OnAttack = false;
                    DOVirtual.DelayedCall(AttackCoolTime, () => { _OnAttack = true; });
                    break;
                case 2:
                    combo.Kill();
                    animator.CrossFadeInFixedTime(script.Attack_3.ActionName, 0);
                    actionData = script.Attack_3;
                    AttackCount = 0;
                    _OnAttack = false;
                    DOVirtual.DelayedCall(AttackCoolTime, () => { _OnAttack = true; });
                    break;
            }

        }
    }

    public ActionData actionData
    {
        get { return action; }
        set { action = value; }
    }
}
