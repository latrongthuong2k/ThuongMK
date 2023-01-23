using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeStatus : MonoBehaviour
{
    private static MakeStatus instance;
    public static MakeStatus Instance => instance;
    public int id;
    public GameObject child;
    private void Awake()
    {
        instance = this;
        child.SetActive(false);　// set default hind when init with Card prefab
    }

    //--------------------------------------------------
    public void SetID() // Do if call in other lab
    {
        if (CheckActiveLab(FusionSceneCardMake.Instance.CombineLab) == true && CheckActiveLab(FusionSceneCardMake.Instance.DeckLab) == false)
        {
            id = int.Parse(this.name);
            FindDataExp();
            child.SetActive(true);
        }
    }
    private void FindDataExp()
    {
        foreach (var item in FusionSceneCardMake.Instance.ListHandData.deck_list) // Take from Decklish In FusionSceneCardMake.cs
        {
            if (item.crd_id == id)
            {
                EditTextStatus(item.exp);
            }
        }
    }
    private void EditTextStatus(int SubExp)
    {
        if (child == null)
        {
            Debug.Log("Missing MakeStatus -Child- object!");
            return;
        }
        child.GetComponentInChildren<Text>().text = "Exp: " + SubExp;
    }
    private void MakeNewStatusForMainCard()
    {
        // đầu vào là kết quả tổng exp
        /*
         * tính exp = tổngexp  và nexp 
         * 
         */
    }
    //--------------------------------------------------
    private bool CheckActiveLab(GameObject isActive)
    {
        return isActive.activeSelf;
    }
}
