using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerCommon 
{ 
    void CharactorMove()
    {
        currentState = PlayerState.move;
        animator.SetFloat("Move", _rb.velocity.magnitude);
        test(_speed);
        Vector3 diff = transform.position - Player_pos;
        if (Mathf.Abs(diff.x) > 0.001f || Mathf.Abs(diff.z) > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(diff);
            rot = Quaternion.Slerp(_rb.transform.rotation, rot, 0.1f);
            this.transform.rotation = rot;
        }
        Player_pos = transform.position; 
    }

    void test(float speed) 
    {
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveForward = cameraForward * z + Camera.main.transform.right * x;
        _rb.velocity = moveForward * speed + new Vector3(0, _rb.velocity.y, 0);
    }
    void LockOnMove() 
    {
        if (_OnLockOn)
        {
            test(_LSpeed);
            animator.SetFloat("BattleMove_x", x);
            animator.SetFloat("BattleMove_z", z);
        }
        else 
        {
            animator.SetFloat("BattleMove_x", 0f);
            animator.SetFloat("BattleMove_z", 0f);
        }
    }
    void Endmove()
    {
        currentState = PlayerState.idle;
        animator.SetFloat("Move", 0f);
    }
}
