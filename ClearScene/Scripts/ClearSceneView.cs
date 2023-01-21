using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerSaveData;

public class ClearSceneView : MonoBehaviour
{
    private QuestSceneManager _questSceneManager;
    [SerializeField] 
    private Button _NextButton;
    public Button NextButton => _NextButton;
    #region :
    // quest name
    [SerializeField] private Text QuestName_text = null;
    // Explored vs Unexplored
    [SerializeField] private Text Explored_text = null;
    [SerializeField] private Text Unexplored_text = null;
    // EXP
    [SerializeField] private Text tx_FirstClearEXP = null;
    [SerializeField] private Text tx_PerfectEXP = null;
    [SerializeField] private Text tx_ShortestClearEXP = null;
    [SerializeField] private Text tx_NoDamegeClearEXP = null;
    // Dalt
    [SerializeField] private Text tx_FirstClearDalt = null;
    [SerializeField] private Text tx_PerfectDalt = null;
    [SerializeField] private Text tx_ShortestClearDalt = null;
    [SerializeField] private Text tx_NoDamegeClearDalt = null;
    #endregion
    //  obj
    [SerializeField] private GameObject[] questIMG;
    [SerializeField] private GameObject[] Clear;
    [SerializeField] private GameObject[] Stars;

    [SerializeField] private int Unexplored;
    [SerializeField] private int Explored;
    [SerializeField] private int exp, dalt;
    [SerializeField] private int MaxQuest = 20; //　例えは10ウエストがあり。
    [SerializeField] private int questPresent = 0;
    [SerializeField] private int HP = 0;
    [SerializeField] private int maxHP;
    [SerializeField] private string QuestName;

    void UpdateInfo()
    {
        // QuestName update
        QuestName_text.text = QuestName;
        // un vs explored test ToString
        Explored_text.text = Explored.ToString();
        Unexplored_text.text = Unexplored.ToString();
    }
    private void Bonus()
    {
        if (questPresent == 0) // for firsttime clear
        {
            exp += 10;
            dalt += 10;
            tx_FirstClearEXP.text = exp.ToString();
            tx_FirstClearDalt.text = dalt.ToString();
        }
        else if (Explored == Unexplored)// perfect
        {
            dalt += 10;
            tx_PerfectEXP.text = exp.ToString();
            tx_PerfectDalt.text = dalt.ToString();
        }
        else if (questPresent == 0) // Shortest
        {
            exp += 10;
            tx_ShortestClearEXP.text = exp.ToString();
            tx_ShortestClearDalt.text = dalt.ToString();
        }
        else if (HP != maxHP) 
        {
            exp += 10;
            dalt += 10;
            tx_NoDamegeClearEXP.text = exp.ToString();
            tx_NoDamegeClearDalt.text = dalt.ToString();
        }
    }
    private void CheckClear()
    {
        if (Explored == Unexplored && HP !=0 ) // perfect
        {
            Clear[0].SetActive(true);
            for (int i = 0; i <= 3; i++)// full 3 stars
            {
                Stars[i].SetActive(true);
            }
        }
        else if(questPresent == MaxQuest && HP != 0 )// clear
        {
            Clear[1].SetActive(true);
            for (int i = 0; i <= 3; i++) // 2 stars
            {
                if (i == 2)
                {
                    Stars[i].SetActive(false);
                }
                else
                {
                    Stars[i].SetActive(true);
                }
            }
        }
        else if (HP != 0 && questPresent < MaxQuest && questPresent <= 10) // try again
        {
            Clear[2].SetActive(true);
            for (int i = 0; i <= 3; i++) // 2 stars
            {
                if (i == 0)
                {
                    Stars[i].SetActive(true);
                }else
                {
                    Stars[i].SetActive(false);
                }
            }
        }
        else if (HP==0) // failed
        { 
            Clear[3].SetActive(true);
            for (int i = 0; i <= 3; i++)// lost 3 stars
            {
                Stars[i].SetActive(false);
            }
        }
    }
    private void CheckClearNum()
    {
        if (Explored==0 || Explored <= Unexplored * 20 / 100)
        {
            Unexplored_text.text = Unexplored.ToString("???");
            
        }
    }
    private void QuestPanelUpdate()
    {
        for ( int i = 1; i <= questPresent; i++)
        {
            if(questPresent == i)
            {
                questIMG[i].SetActive(true);
            }
            else
            {
                questIMG[i].SetActive(false);
            }
        }
        // more
    }
    // --------------------------------
    private void Start()
    {

       HP = SaveDataRepository.Instance.PlayerData.player_status.hp;
       maxHP = SaveDataRepository.Instance.PlayerData.player_status.hp;
       // _questData.map_filename;
       UpdateInfo(); // fist 
        QuestPanelUpdate();
        CheckClear();
        CheckClearNum();
        Bonus();
    }
}

