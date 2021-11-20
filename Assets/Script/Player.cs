using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public partial class Player : MonoBehaviour 
{ 
    //移動速度
    public float speed;
    //入力受付終了
    private Tween combo;
    //入力受付時間
    private float AttackChance = 0.7f;
    //コンボの上限
    private int AttackCount = 0;
    //プレイヤーのポジション
    private Vector3 _Player_pos;
    private Rigidbody _rb;
    private bool _isGrounded = false;
    private bool _OnAttack = true;
    private Ray _ray;
    public Vector3 moving, latestPos;
    private AnimationSecect script;
    [SerializeField]
    GameObject AnimSerect;
    private Animator animator;

    


    void Start()
    {

        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        script = AnimSerect.GetComponent<AnimationSecect>();
    }

    void Update()
    {
        _ray = new Ray(gameObject.transform.position + 0.18f * gameObject.transform.up, -gameObject.transform.up);

        _isGrounded = Physics.SphereCast(_ray, 0.13f, 0.08f);

        Debug.DrawRay(gameObject.transform.position + 0.2f * gameObject.transform.up, -0.22f * gameObject.transform.up);



        if (_isGrounded)
        {
            MovementControll();
            Movement();
            ComboAttack();

        }
      
    }

    void FixedUpdate()
    {
        RotateToMovingDirection();
    }

    public void RotateToMovingDirection()
    {
        Vector3 differenceDis = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(latestPos.x, 0, latestPos.z);
        latestPos = transform.position;
        //移動してなくても回転してしまうので、一定の距離以上移動したら回転させる
        if (Mathf.Abs(differenceDis.x) > 0.001f || Mathf.Abs(differenceDis.z) > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(differenceDis);
            rot = Quaternion.Slerp(_rb.transform.rotation, rot, 0.1f);
            this.transform.rotation = rot;
            //アニメーションを追加する場合
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }
    }
    void MovementControll()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var virtical = Input.GetAxisRaw("Vertical");
        //斜め移動と縦横の移動を同じ速度にするためにVector3をNormalize()する。
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveForward = cameraForward * virtical + Camera.main.transform.right * horizontal;
        moving = new Vector3(horizontal, 0, virtical);
        moving.Normalize();         
        moving = moveForward * speed + new Vector3(0, _rb.velocity.y, 0);
        _rb.velocity = moving;
    }

    void Movement()
    {

    }
    
    

    void ComboAttack()
    {

        if (Input.GetButtonDown("Jump") && _OnAttack)
        {
            switch (AttackCount)
            {
                case 0:
                    animator.CrossFadeInFixedTime(script.Attack_1, 0);
                    AttackCount++;
                    Debug.Log("Attack1");
                    combo = DOVirtual.DelayedCall(AttackChance, () => { AttackCount = 0; Debug.Log("Attack1を終了"); });
                    _OnAttack = false;
                    DOVirtual.DelayedCall(0.3f, () => { _OnAttack = true; Debug.Log("クールタイム終了"); });
                    break;
                case 1:
                    combo.Kill();
                    animator.CrossFadeInFixedTime(script.Attack_2, 0);
                    AttackCount++;
                    Debug.Log("Attack2");
                    combo = DOVirtual.DelayedCall(AttackChance, () => { AttackCount = 0; Debug.Log("Attack2を終了"); });
                    _OnAttack = false;
                    DOVirtual.DelayedCall(0.3f, () => { _OnAttack = true; Debug.Log("クールタイム終了"); });
                    break;
                case 2:
                    combo.Kill();
                    animator.CrossFadeInFixedTime(script.Attack_3, 0);
                    Debug.Log("Attack3");
                    AttackCount = 0;
                    _OnAttack = false;
                    DOVirtual.DelayedCall(0.3f, () => { _OnAttack = true; Debug.Log("クールタイム終了"); });
                    break;
            }
        }

    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Enemy"))
        {
            if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("launch"))
            {
                this.gameObject.transform.position += new Vector3(0, 1, 0);
                animator.CrossFadeInFixedTime("damage", 0);
                Debug.Log("吹っ飛び");
            }
            else if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Rebellion"))
            {
                Debug.Log("ノックバック");
            }

        }

    }
}


