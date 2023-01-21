using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using PlayerSaveData;

public class BattleEmulatorManager : MonoBehaviour
{
    [SerializeField] private List<string> TekiDataName = new List<string>();
    [SerializeField] private List<string> MikataDataName = new List<string>();
    private List<string> LevelMax = new List<string>() { "10","20","30","40","50"};
    private List<string> Stars = new List<string>() {"1","2","3","4","5" };

    [SerializeField]private int MaxItemOf_File = 10; // Must Set limit Save
    //
    [SerializeField] private Dropdown EnemySelect;
    [SerializeField] private Dropdown MikataSelect;
    //-----------
    // Enemy
    public List<Dropdown> NameSelect; // こちらは敵の名前のdropDownリスト IDジャなく。
    public List<Dropdown> StarsSelect;
    public List<InputField> LevelInput;
    // Mikata
    public List<Dropdown> NameSelect2;
    public List<Dropdown> StarsSelect2;
    public List<InputField> LevelInput2;
    //-----------
    // Mid 
    [SerializeField] private InputField InputNameTekiData;
    [SerializeField] private InputField InputNameMikataData;
    //-----------
    // singleton BattleEmulator
    private static BattleEmulatorManager _instance;
    public static BattleEmulatorManager Instance => _instance;

    [SerializeField] private CardMasterDataModel _cardMasterDataModel;
    public CardMasterDataModel CardMasterDataModel => _cardMasterDataModel;

    [SerializeField] private EnemyGuardianModel _enemyGuardianModel;
    public EnemyGuardianModel EnemyGuardianModel => _enemyGuardianModel;

    [SerializeField] private PlayerSaveModel _playerSaveModel;
    public PlayerSaveModel PlayerSaveModel => _playerSaveModel;
    

    private CardDeck _EmulatorPlayer_hand_list = new CardDeck();
    public CardDeck EmulatorPlayer_hand_list => _EmulatorPlayer_hand_list;

    

    //private string MyPath 
    private const string _saveFileName = "/Emu.data";
    private string path;

    private void Start()
    {
        ClearUItrast();
        
        // Check Exists base File 
        path = Application.persistentDataPath + _saveFileName; 
        if (!File.Exists(path + 0 + "Teki") && !File.Exists(path + 0 + "Mikata"))
        {
            SaveLoadSystem.Save(this, null, 0, "Teki");
            SaveLoadSystem.Save(this, null, 0, "Mikata");
        }
        else
        {
            Debug.Log("Blank Datas already have ! ");
        }
        // Check already have files
        CheckExistData();
        #region Add_DataFuntion

        // Add list Card ( for check ) ----------
        // 各カードタイプの名前のみを取得します。ID と星の検索は、以下に記述された関数によって行われます。
        for (var a = 0; a < 150; a = a + 5)
        {
            MikataDataName.Add(_cardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels[a].crd_name);
        }
        for (var b = 150; b < 250; b = b + 5)
        {
            TekiDataName.Add(_cardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels[b].crd_name);
        }
        //foreach (var ItemOfCard in cardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels)
        //{
        //    foreach (var ItemOf_PlayerHand in playerSaveModel.player_hand_list.deck_list)
        //    {
        //        if (ItemOfCard.crd_id == ItemOf_PlayerHand.crd_id)
        //        {
        //            MikataDataName.Add(ItemOfCard.crd_name);
        //        }
        //    }
        //}

        // Star ----------
        foreach (var item in Stars)
        {
            foreach (var dropdown in StarsSelect)
            {
                dropdown.options.Add(new Dropdown.OptionData() { text = item });
            }
            foreach (var dropdown in StarsSelect2)
            {
                dropdown.options.Add(new Dropdown.OptionData() { text = item });
            }
        }
        // Name ----------
        foreach (var item in TekiDataName)
        {
            foreach (var dropdown in NameSelect)
            {
                dropdown.options.Add(new Dropdown.OptionData() { text = item });
            }
        }
        foreach (var item in MikataDataName)
        {
            foreach (var dropdown in NameSelect2)
            {
                dropdown.options.Add(new Dropdown.OptionData() { text = item });
            }
        }
        #endregion
    }
    // Clean_Lab -----------------
    #region Clean_TrashData
    void ClearUItrast()
    {
        // Enemy 
        foreach (var i in NameSelect)
        {
            i.options.Clear();
            i.options.Add(new Dropdown.OptionData() { text = "" });
        }     
        foreach (var i in StarsSelect)
        {
            i.options.Clear();
            i.options.Add(new Dropdown.OptionData() { text = "" });
        }
        // Mikata
        foreach (var i in NameSelect2)
        {
            i.options.Clear();
            i.options.Add(new Dropdown.OptionData() { text = "" });
        }
        foreach (var i in StarsSelect2)
        {
            i.options.Clear();
            i.options.Add(new Dropdown.OptionData() { text = "" });
        }
        foreach (var i in LevelInput)
        {
            i.text = "";
        }
        foreach (var i in LevelInput2)
        {
            i.text = "";
        }
        // mid Pc_Datalab Add base blank 
        EnemySelect.options.Clear();
        EnemySelect.options.Add(new Dropdown.OptionData() { text = "" });
        MikataSelect.options.Clear();
        MikataSelect.options.Add(new Dropdown.OptionData() { text = "" });
    }
    #endregion
    //------------------------
    #region SaveVsLoad ButtonLab
    public void TekiSaveButton()
    {
        InputNameTekiData.gameObject.SetActive(true);
    }
    public void TekiLoadButton()
    {
        TekiDataUpdate();  
    }
    public void MikaSaveButton()
    {
        InputNameMikataData.gameObject.SetActive(true);
    }
    public void MikaLoadButton()
    {
        MikataDataUpdate();
    }
    #endregion
    //-------------------------
    #region Button delete saved file of ( EnemylistData vs MikataListData )
    public void DeleteOption() // button 1 Teki
    {
        if (EnemySelect.value != 0)
        {
            int nextValue = EnemySelect.value + 1;
            if (File.Exists(path + nextValue + "Teki"))
            {
                DataSave data = SaveLoadSystem.LoadData(nextValue, "Teki"); // load Data + 1 position and save on "data"
                File.Delete(path + nextValue + "Teki");
                //
                File.Delete(path + EnemySelect.value + "Teki"); 
                SaveLoadSystem.Save(null, data, EnemySelect.value, "Teki"); // create new( replay)  data we had deleted base on selecting.value
            }else
            {
                File.Delete(path + EnemySelect.value + "Teki");
            }
            //
            EnemySelect.options.RemoveAt(EnemySelect.value); // remove
            EnemySelect.value = 0; // reset positionSelect         
        }
    }
    public void DeleteOption2() // Button 2 Mikata
    {
        if (MikataSelect.value != 0)
        {
            int nextValue = MikataSelect.value + 1;
            
            //
            if(File.Exists(path + nextValue + "Mikata"))
            {
                DataSave data = SaveLoadSystem.LoadData(nextValue, "Mikata");
                File.Delete(path + nextValue + "Mikata");
                //
                File.Delete(path + MikataSelect.value + "Mikata");
                //
                SaveLoadSystem.Save(null, data, MikataSelect.value, "Mikata");
            }
            else
            {
                File.Delete(path + MikataSelect.value + "Mikata");
            }
            //
            MikataSelect.options.RemoveAt(MikataSelect.value);
            MikataSelect.value = 0;           
        }
    }
    #endregion

    //--------------------------
    #region Input_NameFuntion Event
    // Input Name of list for save data
    public string TekiDataSavedName;
    public string MikataDataSavedName;

    public void InputNameEnd(string name)
    {
        if (name != null)
        {
            int Numstt = 0;
            for (int i = 0; i < MaxItemOf_File; i++)
            {
                if (File.Exists(path + i + "Teki"))
                { Numstt = i;}
                else
                    break;
            }
            Numstt++;
            TekiDataSavedName = name;
            EnemySelect.value = 0;
            SaveLoadSystem.Save(this, null, Numstt , "Teki");
            EnemySelect.options.Add(new Dropdown.OptionData() { text = name });
            InputNameTekiData.gameObject.SetActive(false);
            InputNameTekiData.text = "";
        }
        else
            return;
    }
    public void InputNameEnd2(string name)
    {
        if (name != null)
        {
            int Numstt = 0; // 最初は空optionのでそれ（０）を意外１からAddデータする！
            for (int i = 0; i < MaxItemOf_File; i++)
            {
                if (File.Exists(path + i + "Mikata"))
                {
                    Numstt = i; //ファイルを数える
                }
                else
                    break;
            }
            Numstt++;
            MikataDataSavedName = name;
            MikataSelect.value = 0;
            SaveLoadSystem.Save(this, null, Numstt , "Mikata");
            MikataSelect.options.Add(new Dropdown.OptionData() { text = name });
            InputNameMikataData.gameObject.SetActive(false);
            // ファイルの名前を設定する
            InputNameMikataData.text = "";
        }
        else
            return;
    }
    #endregion

    //---------------------------
    #region Update_Data To DropDownSelect ( Name / Stars )
    private void TekiDataUpdate()
    {
        DataSave data = SaveLoadSystem.LoadData(EnemySelect.value,"Teki" );

        for (int i = 0; i < 9; i++)
        {
            // names
            NameSelect[i].value = data.dropdownNumberValueTeki[i];
            // stars
            StarsSelect[i].value = data.StarsTeki[i];
            //
            LevelInput[i].text = data.TekiLevel[i];
        }
    }
    private void MikataDataUpdate()
    {
        DataSave data = SaveLoadSystem.LoadData(MikataSelect.value,"Mikata");
        for (int i = 0; i < 12; i++)
        {
            // names
            NameSelect2[i].value = data.dropdownNumberValueMikata[i];
            // stars
            StarsSelect2[i].value = data.StarsMikata[i];
            //
            LevelInput2[i].text = data.MikataLevel[i];
        }
    }
    //-----------------------------
    private void CheckExistData()
    {
       
        for (int i = 1; i < MaxItemOf_File; i++)
        {
            // Teki
            if (File.Exists(path + i + "Teki"))
            {
                DataSave data = SaveLoadSystem.LoadData(i, "Teki");
                EnemySelect.options.Add(new Dropdown.OptionData() { text = data._TekiDataSavedName });
            }
        }
        for (int i = 1; i < MaxItemOf_File; i++)
        {
            if (File.Exists(path + i + "Mikata"))
            {
                DataSave data = SaveLoadSystem.LoadData(i, "Mikata");
                MikataSelect.options.Add(new Dropdown.OptionData() { text = data._MikataDataSavedName });
            }
        }
    }
    #endregion
    //----------------------------
    [SerializeField] private  ListBattleE_Model _BattleE_Model_Kind = new ListBattleE_Model();
    public  ListBattleE_Model battleE_Model_Kind => _BattleE_Model_Kind;

    [SerializeField] private  ListBattleE_Model _BattleE_Model_Bad = new ListBattleE_Model();
    public  ListBattleE_Model battleE_Model_Bad => _BattleE_Model_Bad;

    private List<string> NameB = new List<string>();
    private List<string> StarsB = new List<string>();
    private List<string> LevelB = new List<string>();
    private List<string> NameK = new List<string>();
    private List<string> StarsK = new List<string>();
    private List<string> LevelK = new List<string>();

    public void Get_Name_Stars_Level()
    {
        NameK.Clear();
        StarsK.Clear();
        LevelK.Clear();
        NameB.Clear();
        StarsB.Clear();
        LevelB.Clear();

        for (int i = 0; i < 9; i++)
        {
            if (NameSelect[i].value == 0 || StarsSelect[i].value == 0 || LevelInput[i].text == "" && LevelInput[i].text != "0")
            {
                continue;
            }else if (NameSelect[i].value > 0 && StarsSelect[i].value > 0 && LevelInput[i].text != "" && LevelInput[i].text != "0")
            {
                NameK.Add(NameSelect[i].options[NameSelect[i].value].text);
                StarsK.Add(StarsSelect[i].options[StarsSelect[i].value].text);
                LevelK.Add(LevelInput[i].text);
            }
        }
        for (int i = 0; i < NameK.Count && NameK != null && StarsK != null && LevelK != null; i++)
        {
            _BattleE_Model_Kind.ListbattleE_models[i].nameCard = NameK[i];
            _BattleE_Model_Kind.ListbattleE_models[i].stars = StarsK[i];
            _BattleE_Model_Kind.ListbattleE_models[i].level = int.Parse(LevelK[i]);
        }
        //==========
        for (int i = 0; i < 9; i++)
        {
            if (NameSelect2[i].value == 0 || StarsSelect2[i].value == 0 || LevelInput2[i].text == "" && LevelInput2[i].text != "0")
            {
                continue;
            }else if (NameSelect2[i].value > 0 && StarsSelect2[i].value > 0 && LevelInput2[i].text != "" && LevelInput2[i].text != "0")
            {
                NameB.Add(NameSelect2[i].options[NameSelect2[i].value].text);
                StarsB.Add(StarsSelect2[i].options[StarsSelect2[i].value].text);
                LevelK.Add(LevelInput2[i].text);
            }
        }

        for (int i = 0; i < NameB.Count && NameB != null && StarsB != null && LevelB != null; i++)
        {
            _BattleE_Model_Bad.ListbattleE_models[i].nameCard = NameB[i];
            _BattleE_Model_Bad.ListbattleE_models[i].stars = StarsB[i];
            _BattleE_Model_Bad.ListbattleE_models[i].level = int.Parse(LevelB[i]);
        }

        FindID();
    }
    void FindID()
    {
        var masterChara = CardMasterDataModel.CardMasterData.CardCharaDataModels.CharaModels;
        //var MasterLevel = cardMasterDataModel.CardMasterData.CardCharaDataModels.CardCharaLevelModels;
        if(_BattleE_Model_Kind.ListbattleE_models != null)
        {
            foreach (var item in _BattleE_Model_Kind.ListbattleE_models)
            {
                foreach (var model in masterChara)
                {
                    if (item.nameCard == model.crd_name && item.stars == model.chr_stars.ToString())
                    {
                        item.crd_id = model.crd_id;
                    }
                }
            }
        }
        if (_BattleE_Model_Bad.ListbattleE_models != null)
        {
            foreach (var item in _BattleE_Model_Bad.ListbattleE_models)
            {
                foreach (var model in masterChara)
                {
                    if (item.nameCard == model.crd_name && item.stars == model.chr_stars.ToString())
                    {
                        item.crd_id = model.crd_id;
                    }
                }
            }
        }
    }
}
















