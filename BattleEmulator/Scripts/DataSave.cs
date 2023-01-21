using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataSave
{
    public string _TekiDataSavedName;
    public string _MikataDataSavedName;

    public int[] dropdownNumberValueTeki;
    public int[] StarsTeki;
    public string[] TekiLevel;

    public int[] dropdownNumberValueMikata;
    public int[] StarsMikata;
    public string[] MikataLevel;

    public DataSave(BattleEmulatorManager battleEmulator, DataSave _dataSave2, string Target)
    {
        switch (Target)
        {
            case"Teki":
                 // Save name for each data 
                dropdownNumberValueTeki = new int[9];
                StarsTeki = new int[9];
                TekiLevel = new string[9];

                if(battleEmulator != null)
                {
                    _TekiDataSavedName = battleEmulator.TekiDataSavedName;
                    for (int i = 0; i < 9; i++)
                    {
                        dropdownNumberValueTeki[i] = battleEmulator.NameSelect[i].value;
                        //
                        StarsTeki[i] = battleEmulator.StarsSelect[i].value;
                        //
                        TekiLevel[i] = battleEmulator.LevelInput[i].text;
                    }
                }
                else // in this case we get the new Data from dataSave2, ( /BattleEmulatorManager.cs/ DeleteOption())
                {
                    _TekiDataSavedName = _dataSave2._TekiDataSavedName;
                    for (int i = 0; i < 9; i++) 
                    {
                        dropdownNumberValueTeki[i] = _dataSave2.dropdownNumberValueTeki[i];
                        //
                        StarsTeki[i] = _dataSave2.StarsTeki[i];
                        //
                        TekiLevel[i] = _dataSave2.TekiLevel[i];
                    }
                }
                break;
            case"Mikata":
               
                dropdownNumberValueMikata = new int[12];
                StarsMikata = new int[12];
                MikataLevel = new string[12];

                if(battleEmulator != null)
                {
                    _MikataDataSavedName = battleEmulator.MikataDataSavedName;
                    for (int i = 0; i < 12; i++)
                    {
                        dropdownNumberValueMikata[i] = battleEmulator.NameSelect2[i].value;
                        //
                        StarsMikata[i] = battleEmulator.StarsSelect2[i].value;
                        //
                        MikataLevel[i] = battleEmulator.LevelInput2[i].text;
                    }
                }
                else 
                {
                    _MikataDataSavedName = _dataSave2._MikataDataSavedName;
                    for (int i = 0; i < 9; i++)
                    {
                        dropdownNumberValueMikata[i] = _dataSave2.dropdownNumberValueMikata[i];
                        //
                        StarsMikata[i] = _dataSave2.StarsMikata[i];
                        //
                        MikataLevel[i] = _dataSave2.MikataLevel[i];
                    }
                }
                break;
        }
    }
}
