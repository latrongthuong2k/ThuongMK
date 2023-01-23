using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using PlayerSaveData;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FusionSceneCardMake : MonoBehaviour
{
    private static FusionSceneCardMake _instance;
    public static FusionSceneCardMake Instance { get { return _instance; } }

    // ---------****---------- important
    public CardDeck ListHandData = new CardDeck();
    // ---------****----------
    private Cards nonSaveDeck = new Cards();

    [SerializeField] private PlayerSaveModel playerSaveModel = null;
    [SerializeField] private DummyHandModel dummyHandModel = null;
    [SerializeField] private CommonPresenter _commonPresenter;
    [SerializeField] private GameObject cardObj = null;
    //カードの親（置く場所）
    [SerializeField] private Transform EmptyDeckPosObj = null;
    [SerializeField] private Transform EmptyDeckSubPosObj = null;
    public Transform handPosObj = null;
    public Transform deckPosObj = null;
    //=====***======
    public Transform subCardPosObj = null;
    public Transform MainPredictPos = null;
    public Transform MainCardPos = null;
    public GameObject CardPreviewPosParentObj = null;
    public Transform NewPreViewPos = null;
    public Transform NewPreviewPosAnimate = null;
    public Transform OldPreviewPos = null;
    public Transform OldPreviewPosAnimate = null;
    //=====***======
    public GameObject DeckLab;
    public GameObject CombineLab;
    private CardCommonManager _cardCommonManager;
    Vector2 deckPos;
    Vector2 handPos;
    //-------
    [SerializeField] private GameObject ExpStatusPrefab = null;
    public GameObject Arrow = null;
    //-------

    public HorizontalLayoutGroup layoutGroup;
    //-------
    //カード引数
    private int deckNum = 0;
    [SerializeField] private Sprite[] deckNameTex;
    [SerializeField] private Image deckNameImg;

    [SerializeField] private warningWindow _warningWindow = null;

    [SerializeField] private Text pool_Tx;
    [SerializeField] private Text cost_Tx;
    [SerializeField] private Text cardNum_Tx;


    //仮データ
    private static int[] nowdeckNum = { 0, 0, 0, 0, 0 };
    private static int[] nowdeckCost = { 0, 0, 0, 0, 0 };

    private int nowCardNum = 0;
    private int maxCost = 50;
    private int maxCardNum = 15;

    //セーブ用
    string dataPath;
    Cards cards = new Cards();
    Cards hand;


    public Cards getCards() { return cards; }


    private void Awake()
    {
        _instance = this;
        Arrow.SetActive(false);
        CardPreviewPosParentObj.SetActive(false);
        MakeInputListData();
        //_commonPresenter = CommonManager.CommonGameManager.GetComponent<CommonPresenter>();
        MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm08);
        deckPos = deckPosObj.transform.position;
        handPos = deckPosObj.transform.position;
        //デッキの読み込み
        cards = readCards();
        _cardCommonManager = GetComponent<CardCommonManager>();
        _warningWindow = GetComponent<warningWindow>();

        // do not change 
        initLoadCard(); // create all card in inputted list
        DeckLab.SetActive(false);
        initDeckCard(maxCardNum);
        CombineLab.SetActive(true);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="BlankCard"></param>
    void initDeckCard(int BlankCard)
    {
        // in combineLab make 入力する blank Card
        for (int i = 0; i < BlankCard; i++)
        {
            GameObject clone2;
            // subCard blank Card
            clone2 = GameObject.Instantiate(cardObj, subCardPosObj) as GameObject;
            clone2.name = "nonSelectObj";
            clone2.AddComponent<CloneType>();
        }
        //テスト用カードを60枚書き出し
        int MaxDeckNum = 60;
        for (int i = 0; i < MaxDeckNum; i++)
        {
            GameObject clone;
            clone = GameObject.Instantiate(cardObj, EmptyDeckPosObj) as GameObject;
            clone.name = "nonSelectObj";
        }
    }
    //-------------------------------------------------------
    void initLoadCard()
    {
        hand = readCards();
        for (int i = 0; i < hand.cardsID.Count; i++)
        {
            if (hand.cardsID[i] != -1)
            {
                GameObject clone;
                if (hand.cardsID[i] == -2)
                {
                    clone = GameObject.Instantiate(cardObj, handPosObj) as GameObject;
                    clone.AddComponent<HandCard>();//カード内ステイタス用
                }
                else
                {
                    clone = _cardCommonManager.CreateCard(handPosObj,
                                                          hand.cardsID[i],
                                                          hand.cardsLevel[i],
                                                          CommonParam.CardCase.Hand,
                                                          hand.cardsType[i]);
                    clone.AddComponent<CloneType>();
                }
                clone.name = hand.cardsID[i].ToString();
                clone.GetComponent<CardMakeUseCase>().enabled = false;
            }
        }
        //handPosObj.gameObject.SetActive(false);
        EmptyDeckPosObj.gameObject.SetActive(false);
    }
    public void finishButton()
    {
        nonSaveDeck.cardsID.Clear();
        nonSaveDeck.cardsLevel.Clear();
        nonSaveDeck.cardsType.Clear();
        foreach (Transform child in deckPosObj)
        {
            if (child.gameObject.activeSelf)
            {
                nonSaveDeck.cardsID.Add(child.GetComponent<CardSelecter>().cardID);
                nonSaveDeck.cardsLevel.Add(child.GetComponent<CardSelecter>().cardLevel);
                nonSaveDeck.cardsType.Add(child.GetComponent<CardSelecter>().cardType);
            }
            else
            {
                nonSaveDeck.cardsID.Add(-1);
                nonSaveDeck.cardsLevel.Add(-1);
                nonSaveDeck.cardsType.Add(CommonParam.CardType.Chara);
            }
        }
        Debug.Log(nonSaveDeck.cardsID.Count);
        _commonPresenter.ReturnSceneChange();
    }

    //-------------------------------------------------------
    //public void SaveFile(Cards cards, int deckIndex = 5)
    //{
    //    CardDeck cardDeck = null;
    //    if (5 == deckIndex) cardDeck = SaveDataRepository.Instance.PlayerData.player_hand_list;
    //    else cardDeck = SaveDataRepository.Instance.PlayerData.card_deck_list.card_deck_list[deckIndex];
    //    foreach (var deck in cardDeck.deck_list.Select((card, index) => new { card, index }))
    //    {
    //        if (deck.index >= cards.cardsID.Count)
    //            break;
    //        deck.card.crd_id = cards.cardsID[deck.index];
    //        deck.card.level = cards.cardsLevel[deck.index];
    //        deck.card.card_kind = cards.cardsType[deck.index];
    //    }

    //    var path = Application.dataPath + CommonParam.CardDeckListsPath;
    //    CommonParam.SaveJsonFile(path, ref SaveDataRepository.Instance.PlayerData.card_deck_list);
    //    path = Application.dataPath + CommonParam.CardHandDeckPath;
    //    CommonParam.SaveJsonFile(path, ref SaveDataRepository.Instance.PlayerData.player_hand_list);
    //}

    /// <summary>
    ///
    /// </summary>
    private void MakeInputListData()
    {
        // DummyHand Data
        foreach (var deck in ListHandData.deck_list.Select((card, index) => new { card, index }))
        {
            if (deck.index >= cards.cardsID.Count)
                break;
            deck.card.crd_id = dummyHandModel.hand_models[deck.index].crd_id;
            deck.card.level = dummyHandModel.hand_models[deck.index].level;
            deck.card.card_kind = CommonParam.CardType.Chara;
        }
    }
    //-------------------------------------------------------
    public Cards readCards(CardDeck Option = null)
    {
        Cards cardList = new Cards();
        //
        CardDeck cardDeck;
        if (Option != null) // read orther list 
        {
            cardDeck = Option; // for reads subCard;
            foreach (var deck in cardDeck.deck_list.Select((card, index) => new { card, index }))
            {
                if (deck.index >= cards.cardsID.Count)
                    break;
                cardList.cardsID[deck.index] = deck.card.crd_id;
                cardList.cardsLevel[deck.index] = deck.card.level;
                cardList.cardsType[deck.index] = deck.card.card_kind;
            }
        }
        else // read DeckListFix
        {
            cardDeck = ListHandData;
            foreach (var deck in cardDeck.deck_list.Select((card, index) => new { card, index }))
            {
                if (deck.index >= cards.cardsID.Count)
                    break;
                cardList.cardsID[deck.index] = deck.card.crd_id;
                cardList.cardsLevel[deck.index] = deck.card.level;
                cardList.cardsType[deck.index] = deck.card.card_kind;
            }
        }
        return cardList;
    }
    //-------------------------------------------------------

    public void InitCardCombineMainOrNextCard(Transform parentPos)
    {
        if (parentPos == MainCardPos)
        {
            var Data = GetInfoCard.Instance.MainCardData;
            var MainCardData = Data;
            Debug.Log(Data.level);
            GameObject MainCardCombineObj; // Make Main Card
            MainCardCombineObj = _cardCommonManager.CreateCard(MainCardPos,
                                                              MainCardData.crd_id,
                                                              MainCardData.level,
                                                              CommonParam.CardCase.Hand,
                                                              MainCardData.card_kind);
            MainCardCombineObj.name = MainCardData.crd_id.ToString();
            MainCardCombineObj.AddComponent<CloneType>();
            MainCardCombineObj.GetComponent<CardMakeUseCase>().enabled = false;
            MainCardCombineObj.GetComponent<RectTransform>().localScale = new Vector2(1.27f, 1.27f);
            MainCardCombineObj.transform.SetSiblingIndex(0);
            return;
        }
        if (parentPos == MainPredictPos) // make next Card
        {
            var dataMainNext = GetInfoCard.Instance.NextMainCardData;
            Debug.Log(dataMainNext.level);

            GameObject MainCardCombineObj; // predict
            MainCardCombineObj = _cardCommonManager.CreateCard(MainPredictPos,
                                                              dataMainNext.crd_id,
                                                              dataMainNext.level,
                                                              CommonParam.CardCase.Hand,
                                                              dataMainNext.card_kind);
            MainCardCombineObj.name = dataMainNext.crd_id.ToString();
            MainCardCombineObj.AddComponent<CloneType>();
            MainCardCombineObj.GetComponent<CardMakeUseCase>().enabled = false;
            MainCardCombineObj.GetComponent<RectTransform>().localScale = new Vector2(1.27f, 1.27f);
            MainCardCombineObj.transform.SetSiblingIndex(0);
            return;
        }
        if (parentPos == NewPreViewPos) // make next Card
        {
            var MainCardPreview = GetInfoCard.Instance.MainCardData;
            var dataMainNextPreview = GetInfoCard.Instance.NextMainCardData;

            GameObject MainPreviewCardObj; // Preview Next
            MainPreviewCardObj = _cardCommonManager.CreateCard(parentPos,
                                                              dataMainNextPreview.crd_id,
                                                              dataMainNextPreview.level,
                                                              CommonParam.CardCase.Hand,
                                                              dataMainNextPreview.card_kind);
            MainPreviewCardObj.name = dataMainNextPreview.crd_id.ToString();
            MainPreviewCardObj.GetComponent<CardMakeUseCase>().enabled = false;
            // Do some animation in this Father object (pos init)
            MainPreviewCardObj.GetComponent<RectTransform>().localScale = new Vector2(1.27f, 1.27f);
            //---------
            parentPos = OldPreviewPos; //Fix InitPos
            GameObject MainPreviwCardObj2; // Preview Old card
            MainPreviwCardObj2 = _cardCommonManager.CreateCard(parentPos,
                                                              MainCardPreview.crd_id,
                                                              MainCardPreview.level,
                                                              CommonParam.CardCase.Hand,
                                                              MainCardPreview.card_kind);
            MainPreviwCardObj2.name = dataMainNextPreview.crd_id.ToString();
            MainPreviwCardObj2.GetComponent<CardMakeUseCase>().enabled = false;
            // Do some animation in this Father object (pos init)
            MainPreviwCardObj2.GetComponent<RectTransform>().localScale = new Vector2(1.27f, 1.27f);
            return;
        }

    }
    //
    public void InitCardCombineSub()
    {
        var dataMain = GetInfoCard.Instance.SubCardData;

        GameObject SubCardCombineObj;
        SubCardCombineObj = _cardCommonManager.CreateCard(subCardPosObj,
                                                          dataMain.crd_id,
                                                          dataMain.level,
                                                          CommonParam.CardCase.Hand,
                                                          dataMain.card_kind);
        SubCardCombineObj.name = dataMain.crd_id.ToString();
        SubCardCombineObj.AddComponent<CloneType>();
        SubCardCombineObj.GetComponent<CardMakeUseCase>().enabled = false;
        SubCardCombineObj.transform.SetSiblingIndex(0);
        //if (layoutGroup.transform.childCount >= maxCardNum)
        //{
        //    Transform lastChild = layoutGroup.transform.GetChild(layoutGroup.transform.childCount - 1);
        //    Destroy(lastChild.gameObject);
        //}
    }
    //-------------------------------------------------------

}
