using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Ludiq;

public class Combine : MonoBehaviour
{
    private static Combine _instance;
    public static Combine Instance => _instance;

    public NexpControl ExpControlBorder;
    [SerializeField] private ParticleSystem[] particleEffectFireFlower;
    [SerializeField] private ParticleSystem[] particleEffectStar;
    [SerializeField] private CardMasterDataModel CardMasterDataModel;
    [SerializeField] public PlayerDeckLevelCostModel PlayerDeckLevelCostModel;
    //
    [SerializeField] public Dictionary<int, CardCharaDataModel.CardCharaModel> masterCharaModelsDict = new Dictionary<int, CardCharaDataModel.CardCharaModel>();
    [SerializeField] private List<CardCombineDataInfo> CardsCharaInUse;
    //
    [SerializeField] private Text[] SumAllExpText;
    [SerializeField] private Text[] MainInformationTextManager;
    [SerializeField] private Text[] ForecastInformationTextManager;
    [SerializeField] private Transform PosDeck;
    //
    [SerializeField] private List<int> CreatedCardSub_IDs = new List<int>();
    [SerializeField] private List<CardCombineDataInfo> _cardSubCombineInfor = new List<CardCombineDataInfo>();
    private Button CombineButton;

    public enum Orders
    {
        Remove, AddAndSetInfo, Nothing
    }

    void Start()
    {
        _instance = this;
        // Make Dictionary CardMasterDataModel
        foreach (var MasterModel in CardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels)
        {
            masterCharaModelsDict[MasterModel.crd_id] = MasterModel;
        }
    }
    public void CheckExistAndAddOrRemove(Transform posDeck, int id, Orders orders)
    {
        Transform Card = posDeck.Find(id.ToString());
        if (Card == null)
        {
            Debug.LogWarning(Card.name
                             + " Object isn't exist in "
                             + posDeck.name + "(Object TransformPos) "
                             + "Pls Check all this ( In CloneType.cs : LabCaseBehavior() -> OverrideData() -> UnHindCardOrMakeNewCard() -> ( InitCardCombineSub() or InitCardCombineMain() ) and make sure data type putin is find");
        }
        switch (orders)
        {
            case Orders.AddAndSetInfo:
                if (Card != null && Card.gameObject.activeSelf == true)
                {
                    // find all object
                    CreatedCardSub_IDs.Add(id);
                    ManagerOfCardInUseList(id, Orders.AddAndSetInfo);
                }
                break;
            case Orders.Remove:
                if (Card != null && Card.gameObject.activeSelf == false)
                {
                    CreatedCardSub_IDs.Remove(id);
                    ManagerOfCardInUseList(id, Orders.Remove);
                }
                break;
        }
    }
    //--------------------------------------------------
    private void ManagerOfCardInUseList(int id = 0, Orders orders = Orders.Nothing)// with ID targer
    {
        switch (orders) // chỗ này là lệnh chỉnh sửa List quản lý _cardSubCombineInfor
                        // mặc dù Setinfo lặp lại giống bên MakeData ( cho sub vs main) bên combine nhưng
                        // việc này là cần thiết để quản lý một list các cardSub được tạo ra. 
        {
            case Orders.Remove:
                ManagerRemove(id);
                break;
            case Orders.AddAndSetInfo:
                // Load all -ID- in CreatedCardObjects and add Type -CardCombineInfo()- then Edit : (ID) and (SetInfo) each of them 
                _cardSubCombineInfor.Add(new CardCombineDataInfo());// add type
                foreach (var item in _cardSubCombineInfor)
                {
                    if (item.crd_id == -1)
                    {
                        item.crd_id = id; // set ID for each item
                        SetInfo(item); // set info for each
                    }
                }
                break;
            default:
                Debug.LogWarning(id + "Still haven't selected the order");
                break;
        }
    }
    private void ManagerRemove(int id)
    {
        for (int i = 0; i < _cardSubCombineInfor.Count; i++)
        {
            if (id == _cardSubCombineInfor[i].crd_id)
            {
                _cardSubCombineInfor.Remove(_cardSubCombineInfor[i]);
            }
        }
    }

    public void SetMaxLeverBaseOnStar(CardCombineDataInfo Card, CardCharaDataModel.CardCharaModel MasterModel)
    {
        Card.star = MasterModel.chr_stars;
        if (Card.star == 1)
            Card.levelMax = 10;
        else if (Card.star == 2)
            Card.levelMax = 20;
        else if (Card.star == 3)
            Card.levelMax = 30;
        else if (Card.star == 4)
            Card.levelMax = 40;
        else if (Card.star == 5)
            Card.levelMax = 50;
    }
    // new Way
    private void SetInfo(CardCombineDataInfo Card) // work in _cardSubCombineInfor list
    {
        if (masterCharaModelsDict.ContainsKey(Card.crd_id))// check exist ?
        {
            CardCharaDataModel.CardCharaModel masterModel = masterCharaModelsDict[Card.crd_id];
            foreach (var cardHand in FusionSceneCardMake.Instance.ListHandData.deck_list) //  cardHand data 　
            {
                if (cardHand.crd_id == Card.crd_id)
                {
                    Card.level = cardHand.level;
                    Card.star = masterModel.chr_stars;
                    SetMaxLeverBaseOnStar(Card, masterModel);
                    foreach (var DeckExpData in PlayerDeckLevelCostModel.DeckExpCosts)
                    {
                        if (Card.level == DeckExpData.level)
                        {
                            Card.needExp = DeckExpData.need_exp;
                            Card.nexp = DeckExpData.next_exp;
                        }
                    }
                }
            }
        }
    }

    // 古い書き方
    //private void SetInfo() 
    //{
    //    foreach (var Card in _cardCombineInfor)
    //    {
    //        foreach (var MasterModel in CardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels)
    //        {
    //            if (Card.crd_id == MasterModel.crd_id)
    //            { 
    //                SetMaxLevelForEachCardBaseOnStar(Card, MasterModel);
    //                foreach (var cardHand in FusionSceneCardMake.Instance.ListHandData.deck_list) //  cardHand data 　
    //                {
    //                    if(cardHand.crd_id == Card.crd_id)
    //                    {
    //                        Card.level = cardHand.level;
    //                        foreach (var DeckExpData in PlayerDeckLevelCostModel.DeckExpCosts)
    //                        {
    //                            if (Card.level == DeckExpData.level)
    //                            {
    //                                Card.needExp = DeckExpData.need_exp;
    //                                Card.nexp = DeckExpData.next_exp;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //}
    //--------------------------------------------------
    #region 経験値の付与
    // All This funcion will do 決まった順番 in CloneType.cs

    public void UpdateAllTextStatus()
    {
        float Total = UpdateTotalExp(); // include bonus
        int Bonus = DoSumBonusExp();
        // Sum Exp
        SumAllExpText[0].text = "Total +Exp : " + Total;
        // Sum BonusExp
        SumAllExpText[1].text = "(+Bonus : " + +Bonus + ")";
        UpdateMainStatusText(GetInfoCard.Instance.MainCardData); ;
        SetExpBorderForMainCard(GetInfoCard.Instance.MainCardData);
        MakePredictCardStatus(Total, Bonus, GetInfoCard.Instance.NextMainCardData);

    }
    private float UpdateTotalExp()
    {
        float SumExp = 0;
        float roundedNumber = 0;
        foreach (var item in _cardSubCombineInfor)
        {
            SumExp = SumExp + ((item.exp + DoSumBonusExp() + 1) * 8 / 10 * item.star * 10);
            roundedNumber = (float)Math.Round(SumExp, 1);
        }
        return roundedNumber;
    }
    private void SetExpBorderForMainCard(CardCombineDataInfo predictData)
    {
        ExpControlBorder.SetExp(predictData.exp);
        ExpControlBorder.SetMaxExp(predictData.needExp);
    }
    private int DoSumBonusExp()
    {
        int bonusExp = 0;
        foreach (var item in _cardSubCombineInfor)
        {
            if (item.level == 9)
            {
                bonusExp++; // plus 1 exp bonus;
            }
        }
        return bonusExp;
    }
    //--------------------------------------------------
    public void UpdateMainStatusText(CardCombineDataInfo MainData) // every Card change
    {
        // created Main Data need take from GetInfor.cs
        MainInformationTextManager[0].text = "Level : " + MainData.level;
        MainInformationTextManager[1].text = "Exp : " + MainData.exp;
        MainInformationTextManager[2].text = "Nexp : " + MainData.needExp;
        MainInformationTextManager[3].text = "Max : " + MainData.levelMax;
    }
    private void MakePredictCardStatus(float Total, int Bonus, CardCombineDataInfo predictData)
    {
        float ForecastExpSum = GetInfoCard.Instance.MainCardData.exp + Total;

        //
        ForecastInformationTextManager[0].text = "Level : " + predictData.level;
        ForecastInformationTextManager[1].text = "Exp : " + ForecastExpSum + "( " + "+" + Total + ")" + "(" + "+" + Bonus + ")";
        ForecastInformationTextManager[2].text = "Nexp : " + predictData.needExp;
        ForecastInformationTextManager[3].text = "Max : " + predictData.levelMax;

    }
    #endregion
    //=========**********=========
    public async void DoComBineCardSubToMain()
    {
        var MainData = GetInfoCard.Instance.MainCardData;
        var NextMainData = GetInfoCard.Instance.NextMainCardData;
        var Info = GetInfoCard.Instance;
        var Fusion = FusionSceneCardMake.Instance;
        float Total = UpdateTotalExp();
        //
        if (Total >= NextMainData.needExp || MainData.exp >= NextMainData.needExp)
        {
            // Cacutale
            MainData.exp = Total - NextMainData.needExp;
            MainData.level++;
            //    
            Fusion.CardPreviewPosParentObj.SetActive(true);
            //Make new card to Preview 
            Fusion.InitCardCombineMainOrNextCard(Fusion.NewPreViewPos);
            //
            DoReplaceTheOldCard(MainData);
            //=== Clean Subs
            CleanSubsCardIfComBineDone();
            //===
            UpdateAllTextStatus();
            await DoWorkOder();
            await SmallWork();
            await SmallWork2();
        }
        else if (Total < NextMainData.needExp || MainData.exp < NextMainData.needExp)
        {
            MainData.exp += Total;
            CleanSubsCardIfComBineDone(); // this will check _cardSubCombineInfor and update total Too
            UpdateAllTextStatus();
        }

        //so affter all animation is done then when click in screen Preview lab will off and make new card, then delete subs 
    }
    private void CleanSubsCardIfComBineDone()
    {
        var Fusion = FusionSceneCardMake.Instance;
        _cardSubCombineInfor.Clear();
        // set Hide all
        for (int i = 0; i < Fusion.subCardPosObj.childCount; i++)
        {
            if (Fusion.subCardPosObj.GetChild(i).name != "nonSelectObj")
            {
                Fusion.subCardPosObj.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
    private async Task DoWorkOder()
    {
        var Fusion = FusionSceneCardMake.Instance;
        Fusion.NewPreviewPosAnimate.gameObject.SetActive(false);
        Fusion.OldPreviewPosAnimate.gameObject.SetActive(true);
        Fusion.OldPreviewPosAnimate.GetComponent<Animator>().Play("CombinePreviewOldCard");
        await Task.Delay(1300);
        Fusion.OldPreviewPosAnimate.gameObject.SetActive(false); // old
        Fusion.NewPreviewPosAnimate.gameObject.SetActive(true);// new
        Fusion.NewPreviewPosAnimate.GetComponent<Animator>().Play("CombinePrevewCardAnimate");

    }
    private async Task SmallWork()
    {
        await Task.Delay(2000);
        foreach (var item in particleEffectFireFlower)
        {
            item.Play();
        }
    }
    private async Task SmallWork2()
    {
        await Task.Delay(2000);
        foreach (var item in particleEffectStar)
        {
            item.Play();
        }
    }
    private void DoReplaceTheOldCard(CardCombineDataInfo MainData)
    {
        Transform MainCardPos = FusionSceneCardMake.Instance.MainCardPos;
        GetInfoCard Info = GetInfoCard.Instance;
        FusionSceneCardMake Fusion = FusionSceneCardMake.Instance;
        Info.OverrideData(0, MainData);
        UnHindCardOrMakeNewCard(Info.MainCardData.crd_id, MainCardPos);
    }
    private void cleanPreview()
    {
        FusionSceneCardMake Fusion = FusionSceneCardMake.Instance;
        Destroy(Fusion.NewPreViewPos.Find(GetInfoCard.Instance.MainCardData.crd_id.ToString()).gameObject);
        Destroy(Fusion.OldPreviewPos.Find(GetInfoCard.Instance.NextMainCardData.crd_id.ToString()).gameObject);
    }

    //-------------------------------------
    public void UnHindCardOrMakeNewCard(int Id, Transform posParent) // just unHind for Combine lab
    {
        if (posParent == FusionSceneCardMake.Instance.MainCardPos)
        {
            Transform Child = posParent.Find(Id.ToString());
            Transform PredictFatherObj = FusionSceneCardMake.Instance.MainPredictPos; // setup for predict
            if (Child == null) // make Card
            {
                FusionSceneCardMake.Instance.InitCardCombineMainOrNextCard(posParent);
                // copy
                FusionSceneCardMake.Instance.InitCardCombineMainOrNextCard(PredictFatherObj);
                // Text
                UpdateAllTextStatus();// MainStatus, NextStatus, SumTotal, SumBonus
            }
            else if (Child.name == Id.ToString()) // unhide
            {
                // Main
                Child.gameObject.SetActive(true);
                //Find NextCardMain
                Transform ChildPredict = PredictFatherObj.Find(Id.ToString());
                ChildPredict.gameObject.SetActive(true);
                UpdateAllTextStatus(); // MainStatus, NextStatus, SumTotal, SumBonus
            }
        }
        else if (posParent == FusionSceneCardMake.Instance.subCardPosObj)
        {
            Transform Child = posParent.Find(Id.ToString());
            if (Child == null)
            {
                FusionSceneCardMake.Instance.InitCardCombineSub(); // sub
                MakeStatus.Instance.SetID();
                CheckExistAndAddOrRemove(FusionSceneCardMake.Instance.subCardPosObj, Id, Combine.Orders.AddAndSetInfo);
                UpdateAllTextStatus();
            }
            else if (Child.name == Id.ToString())
            {
                Child.gameObject.SetActive(true);
            }
        }
    }
    public void OutOfPreView()
    {
        var Fusion = FusionSceneCardMake.Instance;
        Fusion.CardPreviewPosParentObj.SetActive(false);
        cleanPreview();
    }
}
