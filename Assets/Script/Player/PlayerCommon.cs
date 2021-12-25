using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public partial class PlayerCommon : MonoBehaviour
{
    //移動速度
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _LSpeed;
    //入力受付終了
    private Tween combo;
    //入力受付時間
    private float AttackChance = 0.7f;
    //コンボの上限
    private int AttackCount = 0;
    //攻撃クールタイム
    private float AttackCoolTime = 0.4f;
    private float x;
    private float z;
    private Rigidbody _rb;
    private bool _isGrounded = false;
    private bool _OnAttack = true;
    private bool _OnMove = false;
    private bool _OnLockOn = false;
    private Ray _ray;
    private Vector3 Player_pos;
    private AnimationSecect script;
    private LockOn LockTarget;
    [SerializeField]
    GameObject AnimSerect;
    [SerializeField]
    Collider collider;
    [SerializeField]
    GameObject LockOn;
    private Animator animator;
    private ActionData action;

    enum PlayerState
    {
        idle,
        move,
        attack,
        lockOn
    }
    private PlayerState _currentState = PlayerState.idle;
    private PlayerState currentState
    {
        get => _currentState;
        set { _currentState = value; }
    }



    void Start()
    {
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        script = AnimSerect.GetComponent<AnimationSecect>();
        LockTarget = LockOn.GetComponent<LockOn>();
        Player_pos = GetComponent<Transform>().position;
    }

    void Update()
    {
        _ray = new Ray(gameObject.transform.position + 0.18f * gameObject.transform.up, -gameObject.transform.up);

        _isGrounded = Physics.SphereCast(_ray, 0.13f, 0.08f);

        Debug.DrawRay(gameObject.transform.position + 0.2f * gameObject.transform.up, -0.22f * gameObject.transform.up);



        if (_isGrounded)
        {
            LockOnTarget();
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            switch (currentState)
            {
                case PlayerState.idle:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ComboAttack();
                    }
                    else if ((x != 0 || z != 0) && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    {
                        _OnMove = true;
                    }
                    break;
                case PlayerState.move:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ComboAttack();
                    }
                    else if (x == 0 && z == 0)
                    {
                        _OnMove = false;
                    }
                    break;
                case PlayerState.attack:
                    if (!Input.GetMouseButtonDown(0))
                    {
                        EndAttack();
                    }
                    break;
            }
            if (_OnLockOn) 
            {
                transform.LookAt(LockTarget.target.transform);
            }


        }

    }
    void FixedUpdate()
    {
        if (_OnMove && !_OnLockOn)
        {
            CharactorMove();
        }
        else if (_OnLockOn) 
        {
            LockOnMove();
        }
        else if (!_OnMove)
        {
            Endmove();
        }
    }

    void InvalidCollider()
    {
        collider.enabled = false;
    }

    void ValidityCollider()
    {
        collider.enabled = true;
    }
    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Enemy"))
        {
            if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("launch"))
            {
                this.gameObject.transform.position += new Vector3(0, 1, 0);
                //animator.CrossFadeInFixedTime("damage", 0);
                Debug.Log("吹っ飛び");
            }
            else if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Rebellion"))
            {
                Debug.Log("ノックバック");
            }

        }

    }

    private void LockOnTarget()
    {
        if (LockTarget.target != null)
        {
            //ロックオン開始
            if (Input.GetKeyDown(KeyCode.R) && !_OnLockOn)
            {
                _OnLockOn = true;
                animator.SetBool("LockOn", true);
                animator.SetFloat("Move", 0f);

            }
            //ロックオン解除
            else if (Input.GetKeyDown(KeyCode.R) && _OnLockOn)
            {
                _OnLockOn = false;
                animator.SetBool("LockOn", false);
                return;
            }
        }
        //ロックオン中にtargetがいない場合
        if (LockTarget.target == null)
        {
            _OnLockOn = false;
            animator.SetBool("LockOn", false);
            return;
        }



    }


}
