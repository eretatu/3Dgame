using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{

    public float speed;
    public float jumpspeed;


    private Rigidbody _rb;
    private bool _isGrounded = false;
    private Ray _ray;
    private AnimationSecect script;
    [SerializeField]
    GameObject AnimSerect;
    [SerializeField]
    AnimationClip[] newClip;
    private AnimatorOverrideController newAnime;
    private Animator animator;
    public Vector3 moving, latestPos;



    void Start()
    {
        newAnime = new AnimatorOverrideController();
        newAnime.runtimeAnimatorController = GetComponent<Animator>().runtimeAnimatorController;
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        script = AnimSerect.GetComponent<AnimationSecect>();

    }

    void Update()
    {
        test();
        _ray = new Ray(gameObject.transform.position + 0.18f * gameObject.transform.up, -gameObject.transform.up);

        _isGrounded = Physics.SphereCast(_ray, 0.13f, 0.08f);

        Debug.DrawRay(gameObject.transform.position + 0.2f * gameObject.transform.up, -0.22f * gameObject.transform.up);



        if (_isGrounded)
        {

            MovementControll();
            Movement();
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    private void FixedUpdate()
    {
        RotateToMovingDirection();
    }

    void MovementControll()
    {
        //�΂߈ړ��Əc���̈ړ��𓯂����x�ɂ��邽�߂�Vector3��Normalize()����B
        moving = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moving.Normalize();
        moving = moving * speed;
    }

    public void RotateToMovingDirection()
    {
        Vector3 differenceDis = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(latestPos.x, 0, latestPos.z);
        latestPos = transform.position;
        //�ړ����ĂȂ��Ă���]���Ă��܂��̂ŁA���̋����ȏ�ړ��������]������
        if (Mathf.Abs(differenceDis.x) > 0.001f || Mathf.Abs(differenceDis.z) > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(differenceDis);
            rot = Quaternion.Slerp(_rb.transform.rotation, rot, 0.1f);
            this.transform.rotation = rot;
            animator.SetBool("Run",true);
            
        }
        else
        {
            animator.SetBool("Run", false);
        }
    }

    void Movement()
    {
        //�U�����̈ړ�����
        if(!animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
        {
            _rb.velocity = moving;
        }
        
        /*if (Input.GetButtonDown("Jump"))
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 5, _rb.velocity.z);
        }*/
    }

    public void test() 
    {

        //newClip�ɂ̓C���X�y�N�^�[�ŃA�j���[�V����Clip��o�^
        foreach(var addclip in newClip) 
        {
            //Attack_1�ɂ̓{�^������擾����Clip���������Ă���
            if(addclip.name == script.Attack_1) 
            {
                newAnime["MC2_SAMK"] = addclip; 
            }
            if(addclip.name == script.Attack_2) 
            {
                newAnime["ScrewK01_zero"] = addclip;
            }
        }
            
        GetComponent<Animator>().runtimeAnimatorController = newAnime;
    }

 
 
}
