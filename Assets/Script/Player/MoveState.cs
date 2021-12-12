using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerCommon 
{ 
    void CharactorMove()
    {
        currentState = PlayerState.move;
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveForward = cameraForward * z + Camera.main.transform.right * x;
        _rb.velocity = moveForward * speed + new Vector3(0, _rb.velocity.y, 0);
        Vector3 diff = transform.position - Player_pos;
        animator.SetBool("Run", true);
        if (Mathf.Abs(diff.x) > 0.001f || Mathf.Abs(diff.z) > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(diff);
            rot = Quaternion.Slerp(_rb.transform.rotation, rot, 0.1f);
            this.transform.rotation = rot;
        }
        Player_pos = transform.position;
    }
}
