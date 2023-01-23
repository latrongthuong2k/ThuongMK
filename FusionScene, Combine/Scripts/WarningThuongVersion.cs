using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningThuongVersion : MonoBehaviour
{
    [SerializeField] private GameObject warningObj = null;
    [SerializeField] private Text warningText = null;
    [SerializeField] private GameObject ButtonLab1 = null;
    [SerializeField] private GameObject ButtonLab2 = null;
    [SerializeField] private Button yesButton = null;
    [SerializeField] private Button noButton = null;
    [SerializeField] private Button SceneButton = null;
    private void Start()
    {
        warningObj.SetActive(false);
    }
    public void ShowWarning(string w_text)
    {
        warningObj.SetActive(true);
        ButtonLab1.SetActive(true);
        ButtonLab2.SetActive(false);
    }
    public void ShowNote(string w_text)
    {
        warningObj.SetActive(true);
        ButtonLab1.SetActive(false);
        ButtonLab2.SetActive(true);
    }

    public void YesPass()
    {
        warningObj.SetActive(false);
    }
}
