using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerSaveData;
using System;
using System.DirectoryServices.ActiveDirectory;

public class CloneType : MonoBehaviour
{
    private int id = 1;
    private Button selectCardButton;
    private GameObject handCard;
    private void Start()
    {
        if (this.name != "unCarded" && this.name != "nonSelectObj")
        {
            id = int.Parse(this.name);
        }
        selectCardButton = this.gameObject.GetComponent<Button>();
        selectCardButton.onClick.AddListener(() => { ButtonEvent(); });
    }
    void ButtonEvent()
    {
        ReturnSelectNum();
    }
    void ReturnSelectNum()
    {
        // Button SE
        MMSoundManager.Instance.PlaySe(MMSoundManager.SeCue.mm_se007);
        //=========
        // LabCheck
        LabCaseBehavior();
    }
    //--------------------------------------------------------------------------

    private bool CheckActiveLab(GameObject isActive)
    {
        return isActive.activeSelf;
    }
    //

    //--------------------------------------------------------------------------

    private void LabCaseBehavior()
    {
        Transform MainCardPos = FusionSceneCardMake.Instance.MainCardPos;
        Transform SubCardPos = FusionSceneCardMake.Instance.subCardPosObj;
        Transform MainPredictPos = FusionSceneCardMake.Instance.MainPredictPos;
        if (CheckActiveLab(FusionSceneCardMake.Instance.CombineLab) == true && CheckActiveLab(FusionSceneCardMake.Instance.DeckLab) == false) // CombineLab
        {
            TagetCard();
            if (GetInfoCard.Instance.isMainCard == true) // Main
            {
                if (FusionSceneCardMake.Instance.Arrow.activeSelf == false)
                    FusionSceneCardMake.Instance.Arrow.SetActive(true);
                if (GetInfoCard.Instance.MainCardID == id || id == 1) // id = 1 , it's mean blank card 
                {
                    GetInfoCard.Instance.MainCardIDreturn = id; // add to IDreturn storage
                    HindCard(id, MainCardPos); // relative to the Set Hind to card
                    HindCard(id, MainPredictPos);
                    FusionSceneCardMake.Instance.CombineLab.SetActive(false);
                    FusionSceneCardMake.Instance.DeckLab.SetActive(true);
                }
            }
            else // sub
            {
                GetInfoCard.Instance.CheckExistSubCard(id, SubCardPos); //Update SubcardID, in combineLab
                if (GetInfoCard.Instance.SubCardID == id || id == 1)
                {
                    GetInfoCard.Instance.SubCardIDreturn = id;
                    HindCard(id, SubCardPos);
                    FusionSceneCardMake.Instance.CombineLab.SetActive(false);
                    FusionSceneCardMake.Instance.DeckLab.SetActive(true);
                }
            }
        }
        else // DeckLab
        {
            Transform handCardPos = FusionSceneCardMake.Instance.handPosObj;

            if (GetInfoCard.Instance.isMainCard == true)// main
            {
                GetInfoCard.Instance.OverrideData(id);
                HindCard(id, handCardPos);
                ReturnCardBefore(GetInfoCard.Instance.MainCardIDreturn, handCardPos); // Choose new card and take ID saved in IDreturn storage and unHind it.
                FusionSceneCardMake.Instance.DeckLab.SetActive(false);
                FusionSceneCardMake.Instance.CombineLab.SetActive(true);
                Combine.Instance.UnHindCardOrMakeNewCard(id, MainCardPos); // unHind card in CombineLab or create new one
            }
            else // sub
            {
                GetInfoCard.Instance.OverrideData(id);
                HindCard(id, handCardPos);
                ReturnCardBefore(GetInfoCard.Instance.SubCardIDreturn, handCardPos);
                FusionSceneCardMake.Instance.DeckLab.SetActive(false);
                FusionSceneCardMake.Instance.CombineLab.SetActive(true);
                Combine.Instance.UnHindCardOrMakeNewCard(id, SubCardPos);
            }
        }
    }
    //--------------------------------------------------------------------------

    private void TagetCard() // main to sub or opposite
    {
        if (this.transform.localPosition == Vector3.zero) // it's mean 0,0,0 pos
        {
            GetInfoCard.Instance.isMainCard = true;
        }
        else
        {
            GetInfoCard.Instance.isMainCard = false;
        }
    }
    //--------------------------------------------------------------------------

    private void ReturnCardBefore(int returnID, Transform posParent) // return one 
    {
        if (returnID == 1)
        {
            return;
        }
        Transform Child = posParent.Find(returnID.ToString());
        if (Child.gameObject.activeSelf == true)
        {
            return;
        }
        else
        {
            Child.gameObject.SetActive(true);
        }
    }
    //==========
    private void HindCard(int Id, Transform posParent) // take one card so need to hind
    {
        if (id == 1)
        {
            return;
        }
        if (posParent == FusionSceneCardMake.Instance.subCardPosObj)
        {
            Transform Child = posParent.Find(Id.ToString());
            Child.gameObject.SetActive(false);
            Combine.Instance.CheckExistAndAddOrRemove(posParent, Id, Combine.Orders.Remove);
            return;
        }
        if (posParent == FusionSceneCardMake.Instance.MainCardPos)
        {
            Transform Child = posParent.Find(Id.ToString());
            Child.gameObject.SetActive(false);
            return;
        }
        if (posParent == FusionSceneCardMake.Instance.MainPredictPos)
        {
            Transform Child = posParent.Find(Id.ToString());
            Child.gameObject.SetActive(false);
            return;
        }
        if (posParent == FusionSceneCardMake.Instance.handPosObj)
        {
            Transform Child = posParent.Find(Id.ToString());
            Child.gameObject.SetActive(false);
            return;
        }
    }
    //==========


    //--------------------------------------------------------------------------
}
