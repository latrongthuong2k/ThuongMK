using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayerSaveData;
public class ThuongSaveModel : ScriptableObject
{
    //static private ThuongSaveModel _instance;
    //static public ThuongSaveModel Iinstance => _instance;

}
[Serializable]
public class BattleEmulation_Model
{
    public string nameCard = "";
    public string stars = "";
    public int crd_id = -1; //  カードID
    public int level = -1; //  レベル
    public int exp = 0;  //  貯まる経験値
    public int use_enable_count = 0;  //  使用可能回数
    public CommonParam.CardType card_kind = CommonParam.CardType.Chara;
    public int card_index = 0;   // 取得カードの場所

    
    public void Initialize()
    {
       
    }
    public BattleEmulation_Model()
    {
        nameCard = "";
        stars = "";
        crd_id = -1; //  カードID
        level = -1; //  レベル
        exp = 0;  //  貯まる経験値
        use_enable_count = 0;  //  使用可能回数
        card_kind = CommonParam.CardType.Chara;
        card_index = 0;
    }
}
[SerializeField]
public class CardCombineDataInfo
{
    public int crd_id = -1;
    public int star = -1;
    public int level = -1;
    public int levelMax = -1;
    public float exp = 0;
    public int needExp = 0;
    public int nexp = 0;
    public CommonParam.CardType card_kind = CommonParam.CardType.Chara;

    public CardCombineDataInfo()
    {
        crd_id = -1;
        star = -1;
        level = -1;
        levelMax = -1;
        exp = 0;
        needExp = 0;
        nexp = 0;
        card_kind = CommonParam.CardType.Chara;
    }
}
[Serializable]
public class ListBattleE_Model
{
    public List<BattleEmulation_Model> ListbattleE_models = new List<BattleEmulation_Model>();

    public ListBattleE_Model()
    {
        foreach (var i in Enumerable.Range(0, 9))
        {
            ListbattleE_models.Add(new BattleEmulation_Model());
        }
    }
}

