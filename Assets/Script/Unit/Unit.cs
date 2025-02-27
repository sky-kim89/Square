using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;

public enum eUnitStateType
{
    None = -1,
    Attack = 0,
    Hit = 1,
    Hiting = 2,
    Move = 3,
    Die = 4,
    Kill = 5,
    Start = 6,
    Skilling = 7,
    Attacking = 8,
    Dieing = 9
    //���� �� ���� �̻� �߰� �ʿ�
}
public enum eGradeType
{
    Common = 1,
    UnCommon = 2,
    Rare = 3,
    Unique = 4,
    Epic = 5,

    List = 6
}

//������ ��� ���� ��ų�� ���� ����.
public enum eJobType
{
    Warrior,
    Tanker,
    Assassin,
    Archer,
    Magician,
    Healer,
}


public class HeroSaveData
{
    public string Name = string.Empty;
    public int AddLevel = 1; //��ȭ ����
    public int UpGrade = 0;  //��ȭ ���
    public int AddUnitCount = 0; //�߰� ���� ��

    //���� ����
    //���� ������
    //���� ��ġ ����
}

[System.Serializable]
public class UnitData
{
    //public eAttackType AttackType = eAttackType.Normal;
    //public eJobType JobType = eJobType.Warrior;
    //public eSizeType Size = eSizeType.Middle;
    public eGradeType Grade = eGradeType.Common;
    public eJobType Job = eJobType.Warrior;

    public string Name = string.Empty;

    public float AP = 1; //�Ϲ� ����
    public float HP = 1; //ü��
    public float SP = 1; //��ų ����
    public float LP = 1; //������ (���� ����)

    public int Level = 1; //����

    public int AddLevel = 0;
    public int UpGrade = 0;
    public int AddUnitCount = 0;

    public float DamageRate = 1.0f;

    public int Exp = 0;

    public float AttackRange = 1;
    public float AttackSpeed = 1.0f;
    public float MoveSpeed = 1;

    //public ISkill[] Skills = new ISkill[2];
    public Color[] UnitColors = new Color[5];

    public UnitData HalfData()
    {
        UnitData data = new UnitData();

        float def = GameManager.Instance.LP_def;
        float addLp = GameManager.Instance.Lp;

        data.AP = AP * (def + LP * addLp);
        data.SP = SP * (def + LP * addLp);
        data.HP = HP * (def + LP * addLp);
        data.LP = LP * (def + LP * addLp);

        data.DamageRate = DamageRate * (def + LP * addLp);

        data.AttackRange = AttackRange * 0.9f;
        return data;
    }


    public HeroSaveData GetSaveData()
    {
        HeroSaveData data = new HeroSaveData();
        data.Name = Name;
        data.AddLevel = AddLevel;
        data.UpGrade = UpGrade;
        data.AddUnitCount = AddUnitCount;

        return data;
    }
}
//��ų�� ���� �⺻ ���� = ����
public class Unit : MonoBehaviour
{
    [SerializeField]
    protected UnitStateUI m_StateUI = null;
    [SerializeField]
    protected Animator m_Animator = null;
    [SerializeField]
    protected GameObject m_RootObj = null;


    protected UnitData UnitData = null;
    protected UnitData BuffUnitData = null;

    protected eUnitStateType m_UnitState = eUnitStateType.None;

    protected List<Buff> m_Buffs = new List<Buff>();

    protected Unit Target = null;

    public float AP = 0;
    public float HP = 0;
    public float MaxHP = 0;

    public bool isEnemy = false;

    // Update is called once per frame
    void Update()
    {

        Target = UnitManager.Instance.FindUnit(this);

        if (InByUnitToAttackRange())
        {
            Attack();
        }
        else
        {
            Move(BuffUnitData.MoveSpeed);
        }
    }
    //�̵�
    public virtual void Move(float speed)
    {
        if (m_UnitState != eUnitStateType.Hiting && m_UnitState != eUnitStateType.Attacking && m_UnitState != eUnitStateType.Skilling)
        {
            Quaternion temp = Quaternion.LookRotation(transform.position - Target.transform.position);
            transform.rotation = Quaternion.Euler(0, temp.eulerAngles.y, 0);

            transform.Translate(Vector3.back * Time.deltaTime * speed); //����
        }
    }

    protected virtual bool InByUnitToAttackRange()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, -transform.forward, BuffUnitData.AttackRange);
        //if (Physics.Raycast(transform.position, -transform.forward, out hit, Data.AttackRange))
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Unit unit = hits[i].transform.GetComponent<Unit>();
                if (unit != null && isEnemy != unit.isEnemy)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public virtual void Init(UnitData data)
    {
        UnitData = data;
        AP = data.AP * GameManager.Instance.Ap * (1f + data.Level * GameManager.Instance.Level);
        HP = data.HP * GameManager.Instance.Hp * (1f + data.Level * GameManager.Instance.Level);
        MaxHP = HP; 
    }

    //����
    public virtual void Attack()
    {
        if(m_UnitState == eUnitStateType.Move)
        {
            //�̻��� ������ ��������.
            //Ȱ, ��, â?, ��Ŀ, ��, ����
        }
    }

    //�ǰ�
    public virtual void Hit(Damage damage)
    {
        if (m_UnitState != eUnitStateType.Dieing)
        {
            m_UnitState = eUnitStateType.Hit;
            //ActiveSkillOperate(damage.Unit);

            //Job.Hit(damage);
            float hp = (float)HP / (float)MaxHP;

            m_StateUI.SetHP(hp);
            //m_StateUI.Hit(damage.Damage);
            if (HP <= 0)
            {
                Die(damage.Unit);
            }
            else
            {
                m_Animator.Play("Hit");
            }
        }
    }

    //����
    public virtual void Die(Unit unit)
    {
        if (unit != null)
        {
            unit.Kill(this);
        }

        m_Animator.Play("Die");
        gameObject.DisableCollider();

        m_UnitState = eUnitStateType.Dieing;

        DelayAction(0.5f, () =>
        {
            m_UnitState = eUnitStateType.Die;
            m_RootObj.SetActive(false);
        });
        //ActiveSkillOperate(unit);
    }

    //����
    public virtual void Kill(Unit unit)
    {
        //���� ���� ��� �ߵ��ϴ� ��ų �ߵ���
    }
    protected virtual void DelayAction(float delay, Action action)
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(DelayActionHandler(delay, action));
        }
    }

    protected IEnumerator DelayActionHandler(float delay, Action action)
    {
        if (delay != 0)
            yield return new WaitForSeconds(delay);

        if (action != null)
        {
            action();
        }
    }
}
