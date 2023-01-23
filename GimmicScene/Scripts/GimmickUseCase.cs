using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlayerSaveData;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Random = UnityEngine.Random;

public class GimmickUseCase : MonoBehaviour
{
    private       GimmickModel   _gimmickModel = null;
    private const string         commentTag    = "'";
    public        bool           IsKeyWait    { get { return (null != _gimmickModel) ? _gimmickModel.IsKeyWait : false; } }
    public        bool           IsSelectWait { get { return (null != _gimmickModel) ? _gimmickModel.IsSelectWait : false; } }
    private       bool           _isScrollWait    = false;
    private       bool           _isFadeWait      = false;
    private       List<Text>     _selectTextLists = new List<Text>();
    private       List<Button>   _selectButtons   = new List<Button>();
    private       Text           _messageText;
    private       bool           _isMessageWait { get; set; } = false;
    private       string         _displayMessage = "";
    private       MessageManager _messageManager;
    private       int            _messageId = -1;
    private       string         _gimmick   = "";
    //  TODO: シーン遷移のために変数を保存
    private CommonPresenter _commonPresenter;
    private Cards _cards;
    private BattleStatus BattleStatus;
    [SerializeField]
    private Text _chr_nameText;//名前変更用

    [SerializeField]
    private Text _titleText;//タイトル変更用
    
    [SerializeField]
    private GameObject _characterPanel;//会話時の顔パネル変更用

    [SerializeField]
    private GameObject _bg;//背景変更

    [SerializeField] //画面揺らす用
    private Camera _camera;

    [SerializeField]
    private GameObject _flash;//フラッシュ用

    [SerializeField]
    private GameObject _card;//絵と枠を動かすよう

    [SerializeField]
    private GameObject _characterCard;//キャラカード

    private CardMasterDataModel _cardMasterData;//カードマスタリーの取得

    #region カードパラメーター
    private CardCharaDataModel.CardCharaModel _cardCharaModel = null;
    private CardCharaDataModel.CardCharaLevelModel _cardCharaLevelModel = null;
    private CardEquipDataModel.EquipModel _equipModel = null;
    private CardItemDataModel.ItemModel _itemModel = null;
    private CardMagicDataModel.MagicModel _magicModel = null;
    private CardClericDataModel.ClericModel _clericModel = null;
    public CardCharaDataModel.CardCharaModel CardCharaModel { get { return _cardCharaModel; } }
    public CardCharaDataModel.CardCharaLevelModel CardCharaLevelModel { get { return _cardCharaLevelModel; } }
    public CardEquipDataModel.EquipModel EquipModel { get { return _equipModel; } }
    public CardItemDataModel.ItemModel ItemModel { get { return _itemModel; } }
    public CardMagicDataModel.MagicModel MagicModel { get { return _magicModel; } }
    public CardClericDataModel.ClericModel ClericModel { get { return _clericModel; } }
    #endregion

    #region frip用
    //回転スピード
    private float _characterCardFripSpeed = 10f;

    //差し替えるimageを格納する箱
    [SerializeField]
    private List<Sprite> _cardImage = new List<Sprite>();
    #endregion

    #region 仮の立ち絵切り替え用

    [SerializeField]
    private GameObject _characterStandImage;// 立ち絵の位置

    [SerializeField]
    private List<Sprite> _characterStandSprite = new List<Sprite>();//仮の立ち絵切り替え用list
    #endregion

    #region 手札（カード）の表示用
    [SerializeField]
    private GameObject _gimmickMessage;
    [SerializeField]
    private GameObject _gimmickHands;
    #endregion

    [SerializeField]
    private GameObject BlankCard;

    public void Init(string gimmickName, List<Button> selectButtons, List<Text> selectTextLists, Text messageText, CommonPresenter commonPresenter)
    {
        _commonPresenter = commonPresenter;
        _selectButtons = selectButtons;
        _messageManager = commonPresenter.GetComponent<MessageManager>();
        if (null == _gimmickModel)
            _gimmickModel = new GimmickModel();
        _gimmickModel.Init();
        _selectTextLists = selectTextLists;
        _messageText = messageText;
        Load(gimmickName);
        // SaveDataRepository.Instance.PlayerData.save_flag.Init();
    }

    private void Load(string gimmickName)
    {
        //  シナリオとギミックの呼び出し先を変更
        var pathName = (gimmickName.IndexOf("gimmick") >= 0) ? "Gimmick/" : "Scenario/";
        //ResourcesにあるTextAssetを読み込む  Resourcesの(Scenarion内の物を読み込むのものを読み込む)
        var txt = Resources.Load<TextAsset>(pathName + gimmickName);
        //  改行コードを "\n" になるようにする
        var gimmickText = txt.text.Replace("\r\n", "\n").Replace("\r", "\n");
        //  改行コードで区切ってリスト化して、コメントタグ・何もない改行のみの行を無視する
        _gimmickModel.GimmickAllLists = gimmickText.Split('\n')
                                                      .Where(x => x.IndexOf(commentTag, StringComparison.Ordinal) != 0)
                                                      .Where(x => !string.IsNullOrWhiteSpace(x))
                                                      .ToList();
        _gimmickModel.MakeLabel();
        _gimmickModel.SetStartPosition();
    }

    public async void GimmickPlay()
    {
        //  非同期で呼び出し
        await GimmickAction();
        Frip();
        //HaidSelectCard();
    }

    private async UniTask GimmickAction()
    {
        bool isLoop = true;
        _isScrollWait                                             = false;
        SaveDataRepository.Instance.PlayerData.player_status.knockBackType = CommonParam.KnockBackType.None;
        SaveDataRepository.Instance.PlayerData.isResumeStory               = false;
        while (isLoop)
        {
            //  1フレーム待つ
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (IsKeyWait)
            {
                if (!IsKeyWaitPress())
                    continue;
                _gimmickModel.IsKeyWait = false;
            }

            if (_isMessageWait)
            {
                //  メッセージのスキップ
                if (IsKeyWaitPress())
                    _messageManager.SkipMessage(_messageId);
                continue;
            }
            if (IsSelectWait || _isScrollWait || _isFadeWait)
            {
                continue;
            }

            _gimmick = _gimmickModel.GetGimmick();
            var gimmicks = _gimmick.Split(',');
            switch (gimmicks[0])
            {
                case GimmickCommonParam.startCommand:      //  開始（単純にスキップ）
                    break;
                case GimmickCommonParam.labelCommand:      //  ラベル（単純にスキップ）
                    break;
                case GimmickCommonParam.jumpCommand:       //  ジャンプ
                    SetJump(_gimmick);
                    break;
                case GimmickCommonParam.charaCommand:      //  キャラ表示
                    break;
                case GimmickCommonParam.bgmCommand:        //  bgm
                    SetBgm(_gimmick);
                    break;
                case GimmickCommonParam.selCommand:        //  選択処理
                    SetSelectCommand(_gimmick);
                    break;
                case GimmickCommonParam.messageCommand:    //  メッセージ処理
                    SetMessage(_gimmick);
                    break;
                case GimmickCommonParam.endCommand:        //  終了
                    isLoop                                         = false;
                    if(CommonParam.KnockBackType.None == SaveDataRepository.Instance.PlayerData.player_status.knockBackType ||
                       CommonParam.KnockBackType.KnockBackExitDungeon == SaveDataRepository.Instance.PlayerData.player_status.knockBackType ||
                       SaveDataRepository.Instance.PlayerData.isResumeStory)
                        SaveDataRepository.Instance.PlayerData.isClearEventFlag = true;
                    else
                        SaveDataRepository.Instance.PlayerData.isClearEventFlag = false;
                    break;
                case GimmickCommonParam.keyWaitCommand:    //  キー待ち処理
                    _gimmickModel.IsKeyWait = true;
                    break;
                case GimmickCommonParam.chr_nameCommand:     // 名前変更
                    SetName(_gimmick);
                    break;
                case GimmickCommonParam.titleCommand:    // タイトル変更
                    SetTitle(_gimmick);
                    break;
                case GimmickCommonParam.talkOnCommand:   // 顔パネル表示on
                    SetTalkOn(_gimmick);
                    break;
                case GimmickCommonParam.talkOffCommand:  // 顔パネル表示off
                    SetTalkOff(_gimmick);
                    break;
                case GimmickCommonParam.bgCommand:  // 背景変更
                    SetBg(_gimmick);
                    break;
                case GimmickCommonParam.shakeCommand:  // 画面揺らし
                    SetShake(_gimmick);
                    break;
                case GimmickCommonParam.flashCommand:  // フラッシュ
                    SetFlash(_gimmick);
                    break;
                case GimmickCommonParam.fadeCommand:  // フェード
                    SetFade(_gimmick);
                    break;
                case GimmickCommonParam.checkCardCommand:  // 分岐
                    SetCheckCard(_gimmick);
                    break;
                case GimmickCommonParam.checkHandCommand:  // カード選択
                    SetCheckHand(_gimmick);
                    break;
                case GimmickCommonParam.checkLevelCommand:
                    SetCheckLevel(_gimmick);
                    break;
                case GimmickCommonParam.checkHpCommand:
                    SetCheckHp(_gimmick);
                    break;
                case GimmickCommonParam.seCommand:        //  se
                    SetSe(_gimmick);
                    break;
                case GimmickCommonParam.bgmStopCommand:        //  bgmフェードアウト
                    SetBgmStop(_gimmick);
                    break;
                case GimmickCommonParam.cardCommand:        //  cardセット
                    SetCard(_gimmick);
                    break;

                case GimmickCommonParam.openDoorCommand:        //来た方向のドアをオープンする
                    OpenDoor(_gimmick);
                    break;
                case GimmickCommonParam.moveStepCommand:        //数値の方向に２マス移動
                    MoveStep(_gimmick);
                    break;
                case GimmickCommonParam.warpStepCommand:        //ワープする
                    WarpStep(_gimmick);
                    break;

                case GimmickCommonParam.removeCardCommand:
                    RemoveCard(_gimmick);
                    break;
                case GimmickCommonParam.getKeyCommand:
                    SetGetKey(_gimmick);
                    break;
                case GimmickCommonParam.lossKeyCommand:
                    SetLossKey(_gimmick);
                    break;
                case GimmickCommonParam.getMoney:
                    SetGetMoney(_gimmick);
                    break;
                case GimmickCommonParam.getMoneyRand:
                    SetGetMoneyRand(_gimmick);
                    break;
                case GimmickCommonParam.lossMoney:
                    SetLossMoney(_gimmick);
                    break;
                case GimmickCommonParam.checkMoney:
                    SetCheckMoney(_gimmick);
                    break;
                case GimmickCommonParam.flagOnLocal:
                    SetFlagOnLoca(_gimmick);
                    break;
                case GimmickCommonParam.flagOffLocal:
                    SetFlagOffLoca(_gimmick);
                    break;
                case GimmickCommonParam.flagOnGlobal:
                    SetFlagOnGlobal(_gimmick);
                    break;
                case GimmickCommonParam.flagOffGlobal:
                    SetFlagOffGlobal(_gimmick);
                    break;
                case GimmickCommonParam.checkLocalCommand:
                    flagLocalCheck(_gimmick);
                    break;
                case GimmickCommonParam.checkGlobalCommand:
                    flagGlobalCheck(_gimmick);
                    break;
                case GimmickCommonParam.cardTypeCmd:
                    ChooseCardType(_gimmick);
                    break;
                case GimmickCommonParam.deleteCardIDCmd:
                    DeleteCardID(_gimmick);
                    break;
                case GimmickCommonParam.addCardIDCmd:
                    AddCardID(_gimmick);
                    break;
                case GimmickCommonParam.chooseCardHandCmd:
                    ChooseCardHand(_gimmick);
                    break;
                case GimmickCommonParam.chooseCardRevivalCmd:
                    ChooseCardRevival(_gimmick);
                    break;
                case GimmickCommonParam.buffCmd:
                    Buff(_gimmick);
                    break;
                case GimmickCommonParam.backStepCommand:
                    BackStep(_gimmick);
                    isLoop = false;
                    break;
                case GimmickCommonParam.exitQuestCommand:
                    ExitQuest(_gimmick);
                    isLoop = false;
                    break;
                case GimmickCommonParam.moveFloorCommand:
                    MoveFloor(_gimmick);
                    break;
                case GimmickCommonParam.clearQuestCommand:
                    ClearQuest(_gimmick);
                    break;
                case GimmickCommonParam.eventCmd: 
                    EventCheck(_gimmick, ref isLoop);
                    break;
                default:
                    Debug.Log("error command");
                    break;
            }
        }
        //  TODO: K.Goto 終了後にもとの場所に戻る
        _commonPresenter.ReturnSceneChange();
    }

    #region frip処理
    private async UniTaskVoid Frip()
    {
        _characterCard.GetComponent<Image>().sprite = _cardImage[0];
        void Update()
        {
            if (_characterCard.transform.localEulerAngles.y <= 180f)
            {
                //絵を入れ変えるタイミングはカードが９０度になった時
                if (_characterCard.transform.localEulerAngles.y >= 90f && _characterCard.transform.localEulerAngles.y <= 91f)
                {
                    //裏返った時の絵を入れるところ
                    _characterCard.GetComponent<Image>().sprite = _cardImage[1];
                    //Debug.Log ("正常");
                }
                transform.Rotate(new Vector3(0, _characterCardFripSpeed, 0));
            }
        }
    }
    #endregion

    #region 選択する手札（カード）の表示,非表示
   private void DisplaySelectCard()
    {
        _gimmickMessage.SetActive(false);
        _gimmickHands.SetActive(true);
    }

    private void HaidSelectCard()
    {
        _gimmickMessage.SetActive(true);
        _gimmickHands.SetActive(false);
    }
    #endregion
   
    #region カードマスタリーから情報取得 (card_id,card_level,_cardType)
    private async UniTask SetCardData(int card_id, int card_level, CommonParam.CardType _cardType)
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
        if (CommonParam.CardType.Chara == _cardType)
            await SetCharaCard(card_id, card_level);
    }

    private async UniTask SetCharaCard(int card_id, int card_level)
    {
        var _cardData = _cardMasterData.CardMasterData.CardCharaDataModels.CharaModels.FirstOrDefault(card => card.crd_id == card_id);
        if (null == _cardData) return;
        _cardCharaModel = _cardData;
        var _cardStat = _cardMasterData.CardMasterData
                                      .CardCharaDataModels
                                      .CardCharaLevelModels
                                      .FirstOrDefault(card => card.crd_id == card_id && card.chr_level == card_level);
        if (null == _cardStat) return;
        _cardCharaLevelModel = _cardStat;
        //idとcard_levelが流れたら_cardStatにidとlevelに対応したステータスが配列で追加される
        //_cardStat.chr_hp やcardStat.chr_pyg_atkなどでステータスが拾えます
    }
    #endregion

    #region ジャンプ

    private void SetJump(string jumpCommand)
    {
        var jumpLists = jumpCommand.Split(',');
        _gimmickModel.JumpLabel(jumpLists[1]);
    }

    #endregion

    #region メッセージ表示

    private void SetMessage(string messageCommand)
    {
        var _displayMessage = "";
        var messages = messageCommand.Split(',');
        while (true)
        {
            _displayMessage += messages[1];
            messageCommand = _gimmickModel.GetGimmick();
            messages = messageCommand.Split(',');
            if (!string.IsNullOrWhiteSpace(messages[0]) && !messages[0].Equals(GimmickCommonParam.messageCommand))
                break;
            _displayMessage += "\n";
        }
        //  １行余分に読んでいるので戻す
        _gimmickModel.StepBackPointer();

        //  プレイヤー名の入れ替え
        _displayMessage = _displayMessage.Replace("＠プレイヤー名", SaveDataRepository.Instance.PlayerData.player_status.playerName);

        _messageId = _messageManager.InitMessage(_displayMessage);
        _messageManager.StartDispMessage(_messageId, DispMessage);

        _isMessageWait = true;
    }
    private void DispMessage(string msg, bool isComplete)
    {
        _messageText.text = msg;
        if (isComplete)
            _isMessageWait = false;
    }

    #endregion

    #region BGM処理

    private void SetBgm(string bgmCommand)
    {
        var bgmName = bgmCommand.Split(',');//nameの列を整理するやつ
        switch(bgmName[1])
        {
            case "mm_bgm01":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm01);
                break;
            case "mm_bgm02":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm02);
                break;
            case "mm_bgm03":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm03);
                break;
            case "mm_bgm04":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm04);
                break;
            case "mm_bgm05"://欠番
                //MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm05);
                break;
            case "mm_bgm06":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm06);
                break;
            case "mm_bgm07":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm07);
                break;
            case "mm_bgm08":
                MMSoundManager.Instance.PlayBgm(MMSoundManager.BgmCue.mm_bgm08);
                break;
        }
    }
        #endregion

    #region キー待ち

    private bool IsKeyWaitPress()
    {
        return Input.GetMouseButtonDown(0);
    }

    #endregion

    #region 選択処理

    public void IsSelectWaitPress(int index)
    {
        _selectButtons.ForEach(btn => btn.gameObject.SetActive(false));
        _gimmickModel.SearchLabel(index);
        _gimmickModel.IsSelectWait = false;
    }

    private void SetSelectCommand(string selectCommand)
    {
        var selects = selectCommand.Split(',');
        int index = 0;
        _gimmickModel.SelectLists.Clear();
        while (string.IsNullOrWhiteSpace(selects[0]) || selects[0].Equals(GimmickCommonParam.selCommand))
        {
            if (!string.IsNullOrWhiteSpace(selects[1]))
            {
                _gimmickModel.SelectLists.Add(selects[1]);
                _selectButtons[index].gameObject.SetActive(true);
                _selectTextLists[index++].text = selects[2];
            }

            selectCommand = _gimmickModel.GetGimmick();
            selects = selectCommand.Split(',');
        }

        _gimmickModel.IsSelectWait = true;
    }

    #endregion

    #region 名前表示
    private void SetName(string chr_nameCommand)
    {
        var name = chr_nameCommand.Split(',');//nameの列を整理するやつ
        _chr_nameText.text = name[1];//nameの列の何行目を参照するか
    }

    #endregion

    #region タイトル
    private void SetTitle(string titleCommand)
    {
        var name = titleCommand.Split(',');//nameの列を整理するやつ
        _titleText.text = name[1].ToString();//nameの列の何行目を参照するか
    }
    #endregion

    #region パネル表示on
    private void SetTalkOn(string talkOnCommand)
    {
        var card = talkOnCommand.Split(',');//nameの列を整理するやつ
        //カードの中身を番号によって変更する
        //Debug.Log("通過:"+card[1]);
        _characterPanel.SetActive(true);
        _characterPanel.GetComponent<Image>().sprite = Resources.Load<Sprite>("Gimmick/icon/" + card[1]);
    }
    #endregion

    #region パネル表示off
    private void SetTalkOff(string talkOffCommand)
    {
        //カードの中身を番号によって変更する
        //Debug.Log("通過:"+card[1]);
        _characterPanel.SetActive(false);
    }
    #endregion

    #region 背景表示
    private void SetBg(string bgCommand)
    {
        var bg = bgCommand.Split(',');//nameの列を整理するやつ
        //Debug.Log("通過:"+bg[1]+bg[2]);//imssageだったんで変更 スプライトに統一しても良き？
        _bg.GetComponent<Image>().sprite = Resources.Load<Sprite>("Gimmick/Bg/" + bg[1]);
        //背景を変更する用のもの
    }

    #endregion

    #region 全体を揺らす
    private void SetShake(string shakeCommand)
    {
        var shake = shakeCommand.Split(',');//nameの列を整理するやつ
        this.transform.DOShakePosition(
                                duration: 1f,   // 演出時間
                                strength: float.Parse(shake[1])  // シェイクの強さ
        );
    }
    #endregion

    #region フラッシュ
    private void SetFlash(string flashCommand)
    {
        _flash.SetActive(true);//色を変更してから表示
        var fla = _flash.GetComponent<Image>();
        fla.DOFade(endValue: 0f, duration: 0.5f).OnKill(() =>
        {
            _flash.SetActive(false);
        });//endValue:alpha値の終了値,duration: 何秒間でendValueの値になるか
    }
    #endregion

    #region フェード
    private void SetFade(string fadeCommand)
    {
        _isFadeWait = true;
        var fade = fadeCommand.Split(',');//列を整理するやつ
        _flash.SetActive(true);//色を変更してから表示
        var fla = _flash.GetComponent<Image>();

        switch (fade[1]) 
        {
            case "0"://フェードイン
                _flash.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                _flash.SetActive(true);//色を変更してから表示
                fla.DOFade(endValue: 0f, duration: float.Parse(fade[2])).OnKill(() =>
                {
                    _flash.SetActive(false);
                    _isFadeWait = false;
                });//endValue:alpha値の終了値,duration: 何秒間でendValueの値になるか
                break;
            case "1"://フェードアウト
                _flash.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                _flash.SetActive(true);//色を変更してから表示
                fla.DOFade(endValue: 1f, duration: float.Parse(fade[2])).OnKill(() =>
                {
                    _isFadeWait = false;
                });//endValue:alpha値の終了値,duration: 何秒間でendValueの値になるか
                break;
            default:
                Debug.Log("該当不能:フェード");
                _isFadeWait = false;
                break;
        }
    }
    #endregion

    #region 乱数テーブル(int)
    private int RandTable(int Value)
    {
        int DungeonLevel=0;//要相談
        int _numRand=Random.Range(0, 21);
        switch (Value)
        {
            case 0:
            Value = DungeonLevel * (_numRand)/20;
            break;

            case 1:
            Value = DungeonLevel * (_numRand)/10;
            break;
            
            case 2:
            Value = DungeonLevel * (_numRand)/2;
            break;
            
            case 3:
            Value = DungeonLevel * (_numRand + _numRand)/40;
            break;
            
            case 4:
            Value = DungeonLevel * (_numRand + _numRand)/20;
            break;
            
            case 5:
            Value = DungeonLevel * (_numRand + _numRand)/4;
            break;
            
            case 6:
            Value = DungeonLevel * (_numRand + _numRand)/2;
            break;
            
            case 7:
            Value = DungeonLevel * (_numRand + _numRand);
            break;
        }
        return Value;
    }
    #endregion
    
    #region 手札の中からカードを一枚選択する#50#37
    [SerializeField]
    GimmicHandSearch _gimmicHandSearch;
    private void SetCheckCard(string checkCardCommand)
    {
        //手札を表示
        DisplaySelectCard();
        //手札の中からクリックされたカードを見てStatusを確認する
        //クリック待ち
        if (Input.GetMouseButton(0) && _gimmicHandSearch.GetHandNumber() != -1)
        {
        //クリックされた番号の手札のIDとLevelをマスタリーに流してステータスの所持
        var checkCard = checkCardCommand.Split(',');//列を整理するやつ
        CardDeck player_hand_list = new CardDeck();   
        List<int> id = new List<int>();
        List<int> lv = new List<int>();
        int total = 0;

        #region どのステータスを合計するか
        switch (checkCard[1])
            {
                case "lv":
                    total = player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].level;
                break;

                case "atk_ph":
                    id.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].crd_id);
                    lv.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].level);
                    SetCharaCard(id[_gimmicHandSearch.CheckNumber()],lv[_gimmicHandSearch.CheckNumber()]);
                    total =_cardCharaLevelModel.chr_pyg_atk;
                break;

                case "def_ph":
                    id.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].crd_id);
                    lv.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].level);
                    SetCharaCard(id[_gimmicHandSearch.CheckNumber()],lv[_gimmicHandSearch.CheckNumber()]);
                    SetCharaCard(id[_gimmicHandSearch.CheckNumber()],lv[_gimmicHandSearch.CheckNumber()]);
                    total =_cardCharaLevelModel.chr_pyg_def;
                break;

                case "atk_mg":
                    id.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].crd_id);
                    lv.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].level);
                    SetCharaCard(id[_gimmicHandSearch.CheckNumber()],lv[_gimmicHandSearch.CheckNumber()]);
                    total =_cardCharaLevelModel.chr_mag_atk;
                break;

                case "def_mg":
                    id.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].crd_id);
                    lv.Add(player_hand_list.deck_list[_gimmicHandSearch.CheckNumber()].level);
                    SetCharaCard(id[_gimmicHandSearch.CheckNumber()],lv[_gimmicHandSearch.CheckNumber()]);
                    total =_cardCharaLevelModel.chr_mag_def;
                break;

                default:
                //カードIDと照合してあれば飛ばす
                for(int a=0;a <= player_hand_list.deck_list.Count;a++)
                    {
                        if(int.Parse(player_hand_list.deck_list[a].ToString()) == int.Parse(checkCard[1]))
                        {
                            checkCard = checkCardCommand.Split(',');//列を整理するやつ
                            _gimmickModel.JumpLabel(checkCard[2]);
                            break;
                        }
                        else
                        {
                            Debug.Log("その判断値はありません");
                        }
                    }
                    Debug.Log("その判断値はありません");
                break;
            }
        #endregion
        #region どの比較演算子を使用するか
        switch(checkCard[2])
        {
            case "=":
            if(total == RandTable(int.Parse(checkCard[3])))
            {
                checkCard = checkCardCommand.Split(',');//列を整理するやつ
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
            case "<=":
            if(total <= RandTable(int.Parse(checkCard[3])))
            {
                checkCard = checkCardCommand.Split(',');//列を整理するやつ
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;

            case ">=":
            if(total >= RandTable(int.Parse(checkCard[3])))
            {
                checkCard = checkCardCommand.Split(',');//列を整理するやつ
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
            case "<":
            if(total < RandTable(int.Parse(checkCard[3])))
            {
                checkCard = checkCardCommand.Split(',');//列を整理するやつ
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;

            case ">":
            if(total > RandTable(int.Parse(checkCard[3])))
            {
                checkCard = checkCardCommand.Split(',');//列を整理するやつ
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
        }
        #endregion
            _gimmickModel.IsSelectWait = false;
        }
        else
        {
            _gimmickModel.IsSelectWait = true;
        }
    }
    #endregion

    #region 手札のStatusを加算して比較する#51
    private void SetCheckHand(string checkCardCommand)
    {
        var checkCard = checkCardCommand.Split(',');//列を整理するやつ
        CardDeck player_hand_list = new CardDeck();   
        List<int> id = new List<int>();
        List<int> lv = new List<int>();
        int total = 0;

        #region どのステータスを合計するか
        switch (checkCard[1])
            {
                case "lv":
                    for(int a=0;-1 != player_hand_list.deck_list[a].crd_id;a++)
                    {
                        total = total + player_hand_list.deck_list[a].level;
                    }
                    break;

                case "atk_ph":
                    for(int a=0;-1 != player_hand_list.deck_list[a].crd_id;a++)
                    {
                        id.Add(player_hand_list.deck_list[a].crd_id);
                        lv.Add(player_hand_list.deck_list[a].level);
                        SetCharaCard(id[a],lv[a]);
                        total =total + _cardCharaLevelModel.chr_pyg_atk;
                    }
                    break;

                case "def_ph":
                    for(int a=0;-1 != player_hand_list.deck_list[a].crd_id;a++)
                    {
                        id.Add(player_hand_list.deck_list[a].crd_id);
                        lv.Add(player_hand_list.deck_list[a].level);
                        SetCharaCard(id[a],lv[a]);
                        total =total + _cardCharaLevelModel.chr_pyg_def;
                    }
                    break;

                case "atk_mg":
                    for(int a=0;-1 != player_hand_list.deck_list[a].crd_id;a++)
                    {
                        id.Add(player_hand_list.deck_list[a].crd_id);
                        lv.Add(player_hand_list.deck_list[a].level);
                        SetCharaCard(id[a],lv[a]);
                        total =total + _cardCharaLevelModel.chr_mag_atk;
                    }
                    break;

                case "def_mg":
                    for(int a=0;-1 != player_hand_list.deck_list[a].crd_id;a++)
                    {
                        id.Add(player_hand_list.deck_list[a].crd_id);
                        lv.Add(player_hand_list.deck_list[a].level);
                        SetCharaCard(id[a],lv[a]);
                        total =total + _cardCharaLevelModel.chr_mag_def;
                    }
                    break;

                default:
                Debug.Log("その判断値はありません");
                break;
            }
        #endregion
        #region どの比較演算子を使用するか
        switch(checkCard[2])
        {
            case "=":
            if(total == RandTable(int.Parse(checkCard[3])))
            {
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
            case "<=":
            if(total <= RandTable(int.Parse(checkCard[3])))
            {
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;

            case ">=":
            if(total >= RandTable(int.Parse(checkCard[3])))
            {
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
            case "<":
            if(total < RandTable(int.Parse(checkCard[3])))
            {
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;

            case ">":
            if(total > RandTable(int.Parse(checkCard[3])))
            {
                _gimmickModel.JumpLabel(checkCard[4]);
            }
            break;
        }
        #endregion
    }
    #endregion

    #region プレイヤーのLevelをみてjumpする#28
    private void SetCheckLevel(string checkLevelCommand)
    {
        var checkLevel = checkLevelCommand.Split(',');//nameの列を整理するやつ
        checkLevelCommand = _gimmickModel.GetGimmick();
        
        //Debug.Log("0:"+checkLevel[1]);

        if(RandTable(int.Parse(checkLevel[1])) <= SaveDataRepository.Instance.PlayerData.player_status.level)
        {
            checkLevel = checkLevelCommand.Split(',');//次の列を見たいから
            //Debug.Log("0:"+checkLevel[1]);
            _gimmickModel.JumpLabel(checkLevel[1]);
        }
        else
        {
            Debug.Log("エラー:"+checkLevel[1]);
        }
    }
    #endregion

    #region プレイヤーのHpをみてjumpする#29
    private void SetCheckHp(string checkHpCommand)
    {
        var checkHp = checkHpCommand.Split(',');//nameの列を整理するやつ
        if(RandTable(int.Parse(checkHp[1])) <= SaveDataRepository.Instance.PlayerData.player_status.hp)
        {
            _gimmickModel.JumpLabel(checkHp[2]);
        }
        else
        {
            Debug.Log("エラー:"+checkHp[1]);
        }
    }
    #endregion

    #region BGMStop

    private void SetBgmStop(string bgmStopCommand)
    {
        MMSoundManager.Instance.StopBgm();
    }

    #endregion

    #region SE処理

    private void SetSe(string seCommand)
    {
        var seName = seCommand.Split(',');//nameの列を整理するやつ
        switch (seName[1])
        {
            case "mm_se01":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se001);
                break;
            case "mm_se002":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se002);
                break;
            case "mm_se003":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se003);
                break;
            case "mm_se004":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se004);
                break;
            case "mm_se005"://歯抜け
                //MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se005);
                break;
            case "mm_se006":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se006);
                break;
            case "mm_se007":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se007);
                break;
            case "mm_se008":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se008);
                break;
            case "mm_se009":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se009);
                break;
            case "mm_se010"://歯抜け
                //MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se010);
                break;
            case "mm_se011":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se011);
                break;
            case "mm_se012":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se012);
                break;
            case "mm_se013":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se013);
                break;
            case "mm_se014":
                MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se014);
                break;
        }
    }

    #endregion

    #region カードの動き関連

    private readonly float _cardLeftOutPosition  = -995f;
    private readonly float _cardRightOutPosition = 995f;
    private          bool  _isSlideEnd           = true;
    private void SetCard(string cardCommand)
    {
        var    _cardName         = cardCommand.Split(','); //nameの列を整理するやつ
        string cardFileName      = "";
        string cardMoveDirection = "";

        switch (_cardName[1])
        {
            case "exit":
                _card.transform.DORotate(new Vector3(0,90,0), 0.3f)
                     .OnKill(() =>
                            {
                                _characterCard.GetComponent<Image>().sprite = _cardImage[0];
                                _card.transform.DORotate(new Vector3(0, 180, 0), 0.5f);
                                _characterCard.GetComponent<Image>().DOFade(endValue: 0f, duration: 0.5f).OnKill(() => { });
                            });
                break;

            case "pop":
                _card.transform.DOJump(
                                       _card.transform.position, // 移動終了地点
                                80f,                    // ジャンプの高さ
                                1,                     // ジャンプの総数
                                0.5f                   // 演出時間
                );
                break;

            case "roll":
                _card.transform.DORotate(new Vector3(0,90,0), 0.3f)
                     .OnKill(() =>
                             {
                                Sprite vei = _characterCard.GetComponent<Image>().sprite;
                                _characterCard.GetComponent<Image>().sprite = _cardImage[0];
                                _card.transform.DORotate(new Vector3(0,180,0), 0.3f).OnKill(() =>
                                {
                                    _card.transform.DORotate(new Vector3(0,270,0), 0.3f).OnKill(() =>
                                    {
                                        _characterCard.GetComponent<Image>().sprite = vei;
                                        _card.transform.DORotate(new Vector3(0, 360, 0), 0.3f);
                                    });
                                });
                            });
                break;

            case "shake":
                _isScrollWait = true;
                _card.transform.DOShakePosition(
                                duration: 1f,   // 演出時間
                                strength: float.Parse(_cardName[2])  // シェイクの強さ
                ).OnComplete(() => _isScrollWait = false);
                break;

            case "slide_in":
                _isScrollWait     = true;
                cardFileName      = _cardName[2];
                cardMoveDirection = _cardName[3];
                SlideIn(cardMoveDirection, cardFileName);
                break;

            case "slide_out":
                _isScrollWait     = true;
                cardMoveDirection = _cardName[2];
                //スライドさせて外に出す
                SlideOut(cardMoveDirection, _characterCard);
                break;

            case "chase":
                _isScrollWait     = true;
                cardFileName      = _cardName[2];
                cardMoveDirection = _cardName[3];
                SlideIn(cardMoveDirection, cardFileName);
                SlideOut(cardMoveDirection, _characterCard, true);
                break;

            default:
                _characterCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("Gimmick/Card/" + _cardName[1]);
                break;
        }
    }

    /// <summary>
    /// スライドイン（画面外からスクロールインする）
    /// </summary>
    /// <param name="cardMoveDirection">スクロール方向</param>
    /// <param name="cardFileName">表示するカード名</param>
    private void SlideIn(string cardMoveDirection, string cardFileName)
    {
        //BlankCardPrefabを生成
        GameObject blankCard = Instantiate(BlankCard, _card.transform);
        //生成した物を子オブジェクトにする
        blankCard.transform.parent = _card.transform;
        if(cardMoveDirection.Equals("0")){blankCard.transform.localPosition      = new Vector3(_cardLeftOutPosition,  0, 0);}
        else if(cardMoveDirection.Equals("1")){blankCard.transform.localPosition = new Vector3(_cardRightOutPosition, 0, 0);}
        //ファイル名が入ってくるのでそのカードを生成したものに入れる
        blankCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("Gimmick/Card/" + cardFileName);
        //出現させたものを、中央までスライドさせる
        blankCard.transform.DOLocalMove(new Vector3(0f, 0f, 0f), 2f).OnComplete(() => ChangeCardData(blankCard).Forget());
    }

    /// <summary>
    /// スライドアウトの終了を待機して BlancCard の内容を _characterCard に移して BlankCard を破棄する
    /// </summary>
    /// <param name="blankCard">からのカードに</param>
    private async UniTask ChangeCardData(GameObject blankCard)
    {
        await UniTask.WaitUntil(() => _isSlideEnd);
        //  キャラクターカードに BlankCard で設定した画像をコピー
        _characterCard.GetComponent<Image>().sprite = blankCard.GetComponent<Image>().sprite;
        //  キャラクターカードは画面外かどこかにいるはず、移動が終了していれば blankCard は画面中央にいるので
        //  その位置にキャラクターカードを持ってくる
        _characterCard.transform.localPosition = blankCard.transform.localPosition;
        //  要らなくなったブランクカードを破棄する
        Destroy(blankCard);
        _isScrollWait = false;
    }

    /// <summary>
    /// スライドアウト（画面外にスクロールアウトする）
    /// </summary>
    /// <param name="cardMoveDirection">スクロール方向</param>
    /// <param name="targetCard">スクロール対象カード</param>
    private void SlideOut(string cardMoveDirection, GameObject targetCard, bool isNagative = false)
    {
        _isSlideEnd = false;
        float endXPositon           = (cardMoveDirection.Equals("0")) ? _cardLeftOutPosition : _cardRightOutPosition;
        if (isNagative) endXPositon = -endXPositon;
        targetCard.transform.DOLocalMove(new Vector3(endXPositon, 0f, 0f), 2f).OnComplete(() =>
                                                                                          {
                                                                                              _isSlideEnd = true;
                                                                                              if (false == isNagative) _isScrollWait = false;
                                                                                          });
    }

#endregion

    #region グレイブヤードに送る処理
    private void RemoveCard (string removeCardCommand)
    {
        var _removeCard = removeCardCommand.Split(',');//nameの列を整理するやつ
        //_removeCard[1]<-このIDのカードをグレイブヤードに送る

    }
    #endregion

    #region サポートカードを入れる処理
    private void SupportCardCommand (string supportCardCommand)
    {
        var _supportCard = supportCardCommand.Split(',');//nameの列を整理するやつ
        // _supportCard[1]<-このIDのカードを手札に加える
        //とりあえず今のところは無いのでスルー
    }
    #endregion
    
    #region BackStep

    private void BackStep(string backStepCommand)
    {
        var backStep = backStepCommand.Split(',');//nameの列を整理するやつ
        switch(backStep[1])
        {
            case "0":
                SaveDataRepository.Instance.PlayerData.player_status.knockBackType = CommonParam.KnockBackType.KnockBackCloseClearEvent;
                break;
            case "1":
                SaveDataRepository.Instance.PlayerData.player_status.knockBackType = CommonParam.KnockBackType.KnockBackNormalClearEvent;

                break;
            default:
                Debug.Log("例外：BackStep");
                break;
        }
    }
    #endregion

    #region Questエスケープ類
    private void ExitQuest(string exitQuestCommand)
    {
        var exitQuest = exitQuestCommand.Split(',');//nameの列を整理するやつ

        SaveDataRepository.Instance.PlayerData.player_status.knockBackType = CommonParam.KnockBackType.KnockBackClose;
        SaveDataRepository.Instance.PlayerData.isStoryExit                 = true;
    }

    /// <summary>
    /// ダンジョン脱出
    /// </summary>
    /// <param name="clearQuestCommand"></param>
    private void ClearQuest(string clearQuestCommand)
    {
        var clearQuest = clearQuestCommand.Split(',');//nameの列を整理するやつ

        SaveDataRepository.Instance.PlayerData.player_status.knockBackType = CommonParam.KnockBackType.KnockBackExitDungeon;
        SaveDataRepository.Instance.PlayerData.isStoryExit                 = true;
    }
    #endregion
    //67#move_step 数値方向に2マス移動させる
    //数値１＝0：北、1：東、２：南、３：西　　移動経路のカードはすべてオープンされる

    private void OpenDoor(string openDoorCommand)
    {
    }
    private void MoveStep(string moveStepCommand)
    {
        var moveStep = moveStepCommand.Split(',');
        switch(moveStep[1])
        {
            //今の位置からどの方向に進んでほしいかを送る
            case "0":
            SaveDataRepository.Instance.PlayerData.gimmick_data.gimmickMapPoint = new CommonParam.Point(1, 0);
            break;
            case "1":
            SaveDataRepository.Instance.PlayerData.gimmick_data.gimmickMapPoint = new CommonParam.Point(0, 1); 
            break;
            case "2":
            SaveDataRepository.Instance.PlayerData.gimmick_data.gimmickMapPoint = new CommonParam.Point(-1, 0); 
            break;
            case "3":
            SaveDataRepository.Instance.PlayerData.gimmick_data.gimmickMapPoint = new CommonParam.Point(0, -1); 
            break;
        }
    }
    private void WarpStep(string warpStepCommand)
    {
        var warpStep = warpStepCommand.Split(',');
        //73#warpFloorStep 瞬間移動（ワープ）と同じだが、エフェクトがなく移動だけする。
        //瞬間移動（ワープ）をする。数値１＝フロア番号、数値２＝X位置、数値３＝Y位置　へ移動する。移動先のカードはオープン。
        SaveDataRepository.Instance.PlayerData.gimmick_data.gimmickMapPoint = new CommonParam.Point(int.Parse(warpStep[2]),int.Parse(warpStep[3]));
        SaveDataRepository.Instance.PlayerData.player_status.floorNum = int.Parse(warpStep[1]);
    }
    private void MoveFloor(string moveFloorCommand)
    {
        var moveFloor = moveFloorCommand.Split(',');
        //70#move_floor フロアを上下に移動する。
        SaveDataRepository.Instance.PlayerData.player_status.floorNum += int.Parse(moveFloor[0]);
    }

#region イベント関連

    private void EventCheck(string eventCommand, ref bool isLoop)
    {
        var events = eventCommand.Split(','); //nameの列を整理するやつ
        switch(events[1])
        {
            case "over":
                //  TODO: ここに処理を追加
                break;
            case "resume":
                isLoop                                      = false;
                SaveDataRepository.Instance.PlayerData.isResumeStory = true;
                break;
            default:
                Debug.Log("例外：event");
                break;
        }
    }

#endregion

    //--------- トゥオン:　チェック中
    #region key
    private void check_Key(string checkKeyCmd)
    {
        var checkCmd = checkKeyCmd.Split(',');
        int checkKeyNum = int.Parse(checkCmd[0]);
          if(SaveDataRepository.Instance.PlayerData.player_status.keyCount.Contains(checkKeyNum))
                {
                    _gimmickModel.JumpLabel(checkCmd[0]);
                }

    }
    private void SetGetKey(string getKeyCmd)
    {
        foreach (var i in Enumerable.Range(0, 5))
        {
            SaveDataRepository.Instance.PlayerData.player_status.keyCount.Add(1);
        }
    }
    private void SetLossKey(string lossKeyCmd)
    {
        foreach (var i in Enumerable.Range(0, 5))
        {
            SaveDataRepository.Instance.PlayerData.player_status.keyCount.RemoveAt(1);
        }
    }

    #endregion
    //-----------
    #region GetMoney
    //
    private int gold = 0;
    private int dungeonLevel;//現在調整中のため不明
    private void SetGetMoney(string getMoneyCommand)
    {
        int get = 0;// 減算用
        var getMoney = getMoneyCommand.Split(',');
        gold = SaveDataRepository.Instance.PlayerData.player_status.dult;
        get = int.Parse(getMoney[1]);//stringからintに変更
        SaveDataRepository.Instance.PlayerData.player_status.dult = gold + get;
    }
    private void SetGetMoneyRand(string getMoneyCommand)
    {
        int get = 0;// 減算用
        var getMoney = getMoneyCommand.Split(',');
        int _numRand;
        gold = SaveDataRepository.Instance.PlayerData.player_status.dult;
        _numRand = Random.Range(0, 21);
        
        switch (getMoney[0])
        {
            case "1":
                get = dungeonLevel * _numRand / 20;
                break;
            case "2":
                get = dungeonLevel * _numRand / 10;
                break;
            case "3":
                get = dungeonLevel * _numRand / 2;
                break;
            case "4":
                get = dungeonLevel * (_numRand + _numRand) / 40;
                break;
            case "5":
                get = dungeonLevel * (_numRand + _numRand) / 20;
                break;
            case "6":
                get = dungeonLevel * (_numRand + _numRand) / 4;
                break;
            case "7":
                get = dungeonLevel * (_numRand + _numRand) / 2;
                break;
            case "8":
                get = dungeonLevel * (_numRand + _numRand);
                break;
            default:
                Debug.Log("エラー：そのランダムテーブルは無い/get_money_rnd");
                break;
        }
        SaveDataRepository.Instance.PlayerData.player_status.dult = gold + get;
    }
    private void SetLossMoney(string lossMoneyCommand)
    {
        int loss = 0;// 減算用
        var lossMoney = lossMoneyCommand.Split(',');
        gold = SaveDataRepository.Instance.PlayerData.player_status.dult;
        loss = int.Parse(lossMoney[0]);//stringからintに変更
        SaveDataRepository.Instance.PlayerData.player_status.dult = gold - loss;
    }
    private void SetCheckMoney(string checkmoneyComand)
    {
        int Check = 0;
        var checkMoney = checkmoneyComand.Split(',');
        Check = int.Parse(checkMoney[0]);//stringからintに変更
        if (Check == SaveDataRepository.Instance.PlayerData.player_status.dult)
        {
            //checkMoney[2]のラベルにジャンプする
            _gimmickModel.JumpLabel(checkMoney[0]);
        }
        else
        {
            Debug.Log("エラー：そのlabelは無いよ" + checkMoney[0]);
        }
    }
    #endregion
    //-----------
    #region RandomJump
    private void RandomJump(string JumbCommand)
    {
        var jumpCmd = JumbCommand.Split(',');
        int _num;
        _num = int.Parse(jumpCmd[0]);
        if (_num < 40)
        {
            _gimmickModel.JumpLabel(jumpCmd[1]);
        }
        else if(_num > 41 && _num < 70)
        {
            _gimmickModel.JumpLabel(jumpCmd[2]);
        }
        else if(_num > 71 && _num < 90)
        {
            _gimmickModel.JumpLabel(jumpCmd[3]);
        }
    }
    #endregion
    //-----------
    #region flag
    //public bool flagLocal = false;
    //public bool flagGlobal = false;

    private const int _flagOn  = 1;
    private const int _flagOff = 0;


    private void SetFlag(string[] flagVal, int flagValue, bool isLocal)
    {
        var saveFlags = isLocal ? SaveDataRepository.Instance.PlayerData.save_flag.LocalFlag : SaveDataRepository.Instance.PlayerData.save_flag.GlobalFlag; 
        //  数値が入っているときのみ対応する
        if (int.TryParse(flagVal[1], out int flagIndex))
        {
            if (0 <= flagIndex && flagIndex < SaveFlag.SaveFlagMax)
                saveFlags[flagIndex].flagNum = flagValue;
            else
                Debug.LogError("flag管理は0から200の間でお願いします");   
        }
        else
        {
            Debug.LogError("flagは数値を指定してください");   
        }
    }
    private void SetFlagOnLoca(string flagOnLocaCommand)
    {
        var flagVal = flagOnLocaCommand.Split(',');
        SetFlag(flagVal, _flagOn, true);
    }
    private void SetFlagOffLoca(string flagOffLocaCommand)
    {
        var flagVal = flagOffLocaCommand.Split(',');
        SetFlag(flagVal, _flagOff, true);
    }
    private void SetFlagOnGlobal(string flagOnGlobalCommand)
    {
        var flagVal = flagOnGlobalCommand.Split(',');
        SetFlag(flagVal, _flagOn, false);
    }
    private void SetFlagOffGlobal(string flagOffGlobalCommand)
    {
        var flagVal = flagOffGlobalCommand.Split(',');
        SetFlag(flagVal, _flagOff, false);
    }


    private void FlagJumpCheck(string[] flagVals, bool isLocal)
    {
        var saveFlags     = isLocal ? SaveDataRepository.Instance.PlayerData.save_flag.LocalFlag : SaveDataRepository.Instance.PlayerData.save_flag.GlobalFlag; 
        var nextJumpLabel = _gimmickModel.GetGimmick().Split(',')[1];
        if (int.TryParse(flagVals[1], out int firstFlagIndex))
        {
            if (flagVals.Length > 3)
            {
                if (int.TryParse(flagVals[3], out int seconfFlagIndex))
                {
                    if (flagVals[2].Contains("and"))
                    {
                        if (_flagOn == saveFlags[firstFlagIndex].flagNum && _flagOn == saveFlags[seconfFlagIndex].flagNum)
                            _gimmickModel.JumpLabel(nextJumpLabel);
                    }
                    else if (flagVals[2].Contains("or"))
                    {
                        if (_flagOn == saveFlags[firstFlagIndex].flagNum || _flagOn == saveFlags[seconfFlagIndex].flagNum)
                            _gimmickModel.JumpLabel(nextJumpLabel);
                    }
                    else
                    {
                        Debug.LogError("and または or でフラグをチェックしてください");
                    }
                }
                else
                {
                    if (_flagOn == saveFlags[firstFlagIndex].flagNum)
                    {
                        _gimmickModel.JumpLabel(nextJumpLabel);
                    }
                }
            }
            else
            {
                if (_flagOn == saveFlags[firstFlagIndex].flagNum)
                {
                    _gimmickModel.JumpLabel(nextJumpLabel);
                }
            }
        }
    }
    private void flagLocalCheck(string localCheckCommand)
    {
        var localCheck    = localCheckCommand.Split(',');
        FlagJumpCheck(localCheck, true);
    }
    private void flagGlobalCheck(string globalCheckCommand )
    {
        var localCheck = globalCheckCommand.Split(',');
        FlagJumpCheck(localCheck, false);
    }
    #endregion
    //-----------
    #region Chose_card_type
    private void ChooseCardType(string cardTypeCmd)
    {
        var choose = cardTypeCmd.Split(',');
        int cardID = int.Parse(choose[0]);
        if (_cards.cardsID.Contains(cardID))
        {
            _cards.cardsID.IndexOf(cardID); // missing
        }
        else
        {
            _gimmickModel.JumpLabel(choose[0]);
        }
    }
    #endregion
    //-----------
    #region DeckCard / choose_Hand / revival_card
    private List<int> cardInHand = new List<int>(); // ?? _playerSaveModel.Deck_listはclassなので、IDを追加する方わかりません。
    private List<int> revivalCard = new List<int>();

    //-----------
    private void DeleteCardID(string deleteCardIDCmd)
    {
        var cardClearID = deleteCardIDCmd.Split(',');
        int cardID = int.Parse(cardClearID[0]);
        if (_cards.cardsID.Contains(cardID))
        {
            _cards.cardsID.RemoveAt(cardID);
        }
    }
    private void AddCardID(string addCardIDCmd)
    {
        var addID = addCardIDCmd.Split(',');
        int cardID = int.Parse(addID[0]);
        if (_cards.cardsID.Contains(cardID))
        {
            Debug.Log("このカードIDが存在しています、他のカードIDを入力してきださい");
        }else
        {
            _cards.cardsID.Add(cardID);
        }
    }
    //----------- choose_Hand
    private void ChooseCardHand(string chooseCardHandCmd)
    {
        var checkHand = chooseCardHandCmd.Split(',');
        int cardID = int.Parse(checkHand[0]);
        if (_cards.cardsID.Contains(cardID))
        {
            cardInHand.Add(_cards.cardsID.IndexOf(cardID));//戦闘用手札から１枚のカードを選択させ
        }
        else
        {
            _gimmickModel.JumpLabel(checkHand[0]);
        }     
        _cards.cardsID.Add(cardID);
    }
    //----------- revival_card
    private void ChooseCardRevival(string chooseCardRevivalCmd)
    {
        int cardID = int.Parse(chooseCardRevivalCmd);
        if (revivalCard.Contains(cardID))
        {
            _cards.cardsID.Add(revivalCard.IndexOf(cardID)); // revivalからカードを追加。
        }
    }
    #endregion
    //-----------
    #region Buff / Debuff 
    private void Buff(string buffCmd)
    {
        var checkBuffDebuff = buffCmd.Split(',');
        int buff = 120 / 100; // 20% 
        switch(checkBuffDebuff[0])
        { 
            case "HP":
                BattleStatus.chr_hp *= buff;
                break;
            case "atk_ph":
                BattleStatus.chr_pyg_atk *= buff;
                break;
            case "def_ph":
                BattleStatus.chr_pyg_def *= buff;
                break;
            case "atk_mg":
                BattleStatus.chr_mag_atk *= buff;
                break;
            case "def_mg":
                BattleStatus.chr_mag_def *= buff; 
                break;
        }
    }
    #endregion

    #region Get/Lost exp/hp ( card vs pc )
    // Card
    private void GetExpCard(string getExpCardCmd)
    {
        int exp = int.Parse(getExpCardCmd);
        if (BattleStatus.chr_getexp > 0)
        {
            BattleStatus.chr_getexp += exp;
        }
    }
    private void GetHpCard(string getHpCardCmd)
    {
        int hp = int.Parse(getHpCardCmd);
        if (BattleStatus.chr_hp > 0)
        {
            BattleStatus.chr_hp += hp;
        }
        else // if card hp <=  0 グレイブヤードへ行く
        {

        }
    }
    // Pc
    private void GetExpPC(string getExpPcCmd)
    {
        int exp = int.Parse(getExpPcCmd);
        if (SaveDataRepository.Instance.PlayerData.player_status.exp > 0)
        {
            SaveDataRepository.Instance.PlayerData.player_status.exp += exp;
        }
    }
    private void GetHpPC(string getHpPcCmd)
    {
        int hp = int.Parse(getHpPcCmd);
        if (SaveDataRepository.Instance.PlayerData.player_status.hp > 0)
        {
            SaveDataRepository.Instance.PlayerData.player_status.hp += hp;
        }
        else // if pc hp <=  0 game over
        {

        }
    }
    #endregion
}
