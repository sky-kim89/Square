using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eFindUnitType
{
    Range //�Ÿ���
    //Hp ����
    //����� �켱
    //���� �켱
}
public class UnitManager : MonoBehaviour
{
    private List<HeroUnit> MyHeroUniy = new List<HeroUnit>();
    private List<HeroUnit> EnemyHeroUniy = new List<HeroUnit>();

    private GameObject HeroPrefab = null;
    private GameObject UnitPrefab = null;

    public Unit FindUnit(bool isEnemy, eFindUnitType findType = eFindUnitType.Range)
    {
        Unit target = null;
        return target;
    }

    public void StartStage(int stage)
    {

    }

    public void CreateUnit()
    {

    }
}
