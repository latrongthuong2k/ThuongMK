using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using PlayerSaveData;

public class GetInfoCard : MonoBehaviour
{
    #region Static
    private static GetInfoCard _instance;
    public static GetInfoCard Instance => _instance;

    public int MainCardID;
    public int MainCardIDreturn;
    public int SubCardID;
    public int SubCardIDreturn;
    public int NextMainCardID;
    //
    public bool isMainCard;

    public CardCombineDataInfo MainCardData { get; set; } // Take by OverrideData() in CloneType.cs
    public CardCombineDataInfo NextMainCardData; // this too
    public CardCombineDataInfo SubCardData; // this too
    #endregion
    [SerializeField] protected DummyHandModel dummyHandModel = null;
    [SerializeField] protected PlayerSaveModel playerSaveModel = null;
    private void Awake()
    {
        _instance = this;
        SubCardID = 1;
        SubCardIDreturn = 1;
        MainCardIDreturn = 1;
        MainCardID = 1;
        isMainCard = false;
        NextMainCardData = null;
        MainCardData = null;
        SubCardData = null;
    }
    private void FixedUpdate()
    {

        //FindLevel();
    }
    //----------------------
    // Button
    public void CheckExistSubCard(int id, Transform parent)// reset subID to make new card or for setActive it
    {
        if (id == 1)
        {
            return;
        }
        Transform Child = parent.Find(id.ToString());
        if (Child.name == id.ToString())
        {
            SubCardID = id;
        }
    }
    public void SetIdForCheckLevelVsMaxLevel(CardCombineDataInfo Data) // setID for next card if level Below MaxLevel , else it will same MainID
    {
        if (Combine.Instance.masterCharaModelsDict.ContainsKey(Data.crd_id))// check exist ?
        {
            CardCharaDataModel.CardCharaModel masterModel = Combine.Instance.masterCharaModelsDict[Data.crd_id]; // Set Star
            Data.star = masterModel.chr_stars;
            Combine.Instance.SetMaxLeverBaseOnStar(Data, masterModel); // set maxLevel 
            if (Data.level <= Data.levelMax) // below Max
            {
                return;
            }
            else // >= Max
            {
                foreach (var CardMasterID in Combine.Instance.masterCharaModelsDict)
                {
                    if (Data.crd_id == CardMasterID.Key)
                    {
                        Data.crd_id = CardMasterID.Value.chr_next_id;
                        break;
                    }

                }
            }
        }
    }
    public CardCombineDataInfo MakeNewData(int id) // Main vs Sub is using
    {
        var Data = new CardCombineDataInfo();
        if (Combine.Instance.masterCharaModelsDict.ContainsKey(id))// check exist ?
        {
            var masterModel = Combine.Instance.masterCharaModelsDict[id];// **
            foreach (var cardHand in FusionSceneCardMake.Instance.ListHandData.deck_list)
            {
                if (cardHand.crd_id == id)
                {
                    Data.crd_id = id;
                    Data.level = cardHand.level;
                    Data.card_kind = CommonParam.CardType.Chara;
                    Data.star = masterModel.chr_stars; // Set Star
                    Combine.Instance.SetMaxLeverBaseOnStar(Data, masterModel); // add max Level
                    foreach (var DeckExp in Combine.Instance.PlayerDeckLevelCostModel.DeckExpCosts)
                    {
                        if (cardHand.level == DeckExp.level)
                        {
                            Data.nexp = DeckExp.next_exp;
                            Data.needExp = DeckExp.need_exp;
                        }
                    }
                }
            }
        }
        return Data;
    }
    public CardCombineDataInfo MakeNextMainData(CardCombineDataInfo mainData) // make Next Main card
    {
        CardCombineDataInfo Data = new CardCombineDataInfo();
        Data.crd_id = mainData.crd_id;
        Data.level = mainData.level + 1;
        SetIdForCheckLevelVsMaxLevel(Data); // ID vs setMaxLevel , star
        foreach (var DeckExp in Combine.Instance.PlayerDeckLevelCostModel.DeckExpCosts)
        {
            if (Data.level == DeckExp.level)
            {
                Data.needExp = DeckExp.need_exp;
                Data.nexp = DeckExp.next_exp;
            }
        }
        //if (Combine.Instance.masterCharaModelsDict.ContainsKey(Data.crd_id))// check exist ?
        //{
        //    CardCharaDataModel.CardCharaModel masterModel = Combine.Instance.masterCharaModelsDict[Data.crd_id]; // Set Star of nextID this different with SetIdNextBaseOnLevel()
        //    Data.star = masterModel.chr_stars;
        //    Combine.Instance.SetMaxLeverBaseOnStar(Data, masterModel); // set maxLevel do this after Set Star
        //}

        return Data;
    }
    public void OverrideData(int id = 0, CardCombineDataInfo InCaseUpgradeData = null) // Update for GetInfoCard base on ID select
    {
        if (isMainCard == true && InCaseUpgradeData == null) // aslo ID != 0
        {
            CardCombineDataInfo Data = MakeNewData(id);
            MainCardID = id;
            MainCardData = Data; // add everything , Level, star, Need_exp, exp, ID...etc
            CardCombineDataInfo DataNext = MakeNextMainData(Data);
            NextMainCardData = DataNext;
            return;
        }
        if (isMainCard == false && InCaseUpgradeData == null) // aslo ID != 0
        {
            CardCombineDataInfo Data = MakeNewData(id);
            SubCardID = id;
            SubCardData = Data;
            return;
        }
        if (InCaseUpgradeData != null)
        {
            MainCardData.level = InCaseUpgradeData.level;
            MainCardData.crd_id = InCaseUpgradeData.crd_id;
            SetIdForCheckLevelVsMaxLevel(MainCardData); // set iD, set star, Set MaxLevel
            foreach (var DeckExp in Combine.Instance.PlayerDeckLevelCostModel.DeckExpCosts)
            {
                if (MainCardData.level == DeckExp.level)
                {
                    MainCardData.needExp = DeckExp.need_exp;
                    MainCardData.nexp = DeckExp.next_exp;
                }
            }
            //
            CardCombineDataInfo DataNext = MakeNextMainData(InCaseUpgradeData);
            NextMainCardData = DataNext;
            return;
        }

    }
}
