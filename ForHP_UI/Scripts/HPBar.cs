using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerSaveData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public SecondHPbar secondHPbar;
    public NexpControl nexpControl;

    private int HP;

    private int MaxHP = 100;

    [SerializeField] private Text HP_Text = null;

    [SerializeField] private Text MaxHP_Text = null;
    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private int Nexp;

    [SerializeField] private Text Nexp_Text = null;

    [SerializeField] private PlayerDeckLevelCostModel deckExpCostMasterData = null;

    private void Awake()
    {
        var table = deckExpCostMasterData.DeckExpCosts.FirstOrDefault(x => x.level == SaveDataRepository.Instance.PlayerData.player_status.level);
        if(null != table)
            MaxHP = table.hp_max;
    }

    private void Start()
    {
        // HP = Mathf.Clamp(0, MaxHP, SaveDataRepository.Instance.PlayerData.player_status.hp);
        // secondHPbar.SetMaxHealth(MaxHP);
        // SetPlayerHP(HP);
        // if (null != MaxHP_Text)
        //     MaxHP_Text.text = MaxHP.ToString();
        // HP_Text.text = HP.ToString();
        //
        // if (null == nexpControl) return;
        // ///
        // Nexp = SaveDataRepository.Instance.PlayerData.player_status.exp;
        // Nexp_Text.text = Nexp.ToString("D3");
        // nexpControl.SetExp(Nexp);
    }

    private void Update()
    {

        UpdateHP_Text();

        UpdateNexp_Text();

        // if (Input.GetKeyDown(KeyCode.Space) && HP > 0 && Nexp < 1000 )
        // {
        //     TakeDamege(50);
        //
        //    
        //
        // }
    }
    //　HP UI 表示updateFrame
    void UpdateHP_Text()
    {
        if (HP > 0)
        {
            HP_Text.text = HP.ToString("D3");
        }
        if (HP < 0)
        {
            HP_Text.text = ("0");
        }
    }

    void UpdateNexp_Text()
    {
        if (Nexp > 0)
        {
            Nexp_Text.text = Nexp.ToString("D3");
        }

    }

    // Funtion DMG , Heal
    void TakeDamege(int damage)
    {
        HP -= damage;
        secondHPbar.SetHealth(HP);
        Nexp += damage;
        nexpControl.SetExp(Nexp);
    }
    void Healing(int hpHeal)
    {
        HP += hpHeal;
        secondHPbar.SetHealth(HP);
    }


    //210901 LEEGEONHWI
    public void SetPlayerHP(int _hp)
    {
        var table = deckExpCostMasterData.DeckExpCosts.FirstOrDefault(x => x.level == SaveDataRepository.Instance.PlayerData.player_status.level);
        MaxHP = table.hp_max;

        HP = _hp;
        HP_Text.text = HP.ToString("D3");
        secondHPbar.SetHealth(_hp);
    }
}
