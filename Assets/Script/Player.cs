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
    private float x;
    private float z;
    private Rigidbody _rb;
    private bool _isGrounded = false;
    private bool _OnAttack = true;
    private Ray _ray;
    private Vector3 Player_pos;
    private AnimationSecect script;
    [SerializeField]
    GameObject AnimSerect;
    private Animator animator;

    enum PlayerState 
    {
        idle,
        move,
        attack
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
        Player_pos = GetComponent<Transform>().position;
    }

    void Update()
    {
        _ray = new Ray(gameObject.transform.position + 0.18f * gameObject.transform.up, -gameObject.transform.up);

        _isGrounded = Physics.SphereCast(_ray, 0.13f, 0.08f);

        Debug.DrawRay(gameObject.transform.position + 0.2f * gameObject.transform.up, -0.22f * gameObject.transform.up);



        if (_isGrounded)
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            ComboAttack();
        }

    }

    void FixedUpdate()
    {
        CharactorMove();
    }




    void CharactorMove() 
    {
        

        if((x != 0  || z !=0) && !animator.GetCurrentAnimatorStateInfo(0).IsTag("launch") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Rebellion")) 
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
        }
        else 
        {
            currentState = PlayerState.idle;
            animator.SetBool("Run", false);
        }
       
        
        Player_pos = transform.position;


    }




    void ComboAttack()
    {
        if(Input.GetButtonDown("Jump") && _OnAttack) { 
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


