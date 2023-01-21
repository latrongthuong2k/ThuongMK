using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class createGuardian : ScriptableObject
{
    private static readonly string fileName  = "EnemyGuardian2.asset";
    private const           string AssetPath = "Assets/Thuong/BattleEmulator/Datas/";
    private static readonly string CsvPath   = Application.dataPath + "/Thuong/BattleEmulator/CardMaster/EnemyGuardianMaster.csv";

    [MenuItem("ScriptableObjects/Create Guardian2")]
    private static void CreateCardMasterDataAsset()
    {
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", "EnemyGuardian2"));
        if (guids.Length > 0)
        {
            var emptyEnemyGuardianAsset = AssetDatabase.LoadAssetAtPath<EnemyGuardianModel>(AssetDatabase.GUIDToAssetPath(guids[0]));
            MakeCardBaseMasterData(emptyEnemyGuardianAsset);
            //  変更があり次第追加する
        }
        else
        {
            var emptyEnemyGuardianAsset = CreateInstance<EnemyGuardianModel>();
            var assetName               = $"{AssetPath}{fileName}";
            MakeCardBaseMasterData(emptyEnemyGuardianAsset);
            AssetDatabase.CreateAsset(emptyEnemyGuardianAsset, assetName);
            AssetDatabase.Refresh();
        }
    }
    
    private static void MakeCardBaseMasterData(EnemyGuardianModel enemyGuardian2)
    {
        enemyGuardian2.enemy_guardians.Clear();
        using (var str = new StreamReader(CsvPath))
        {
            var csvDatas = str.ReadToEnd();
            csvDatas = csvDatas.Replace("\r\n", "\n").Replace("\r", "\n");
            var csvLists = csvDatas.Split('\n').Where(data => !string.IsNullOrWhiteSpace(data)).ToList();
            csvLists.ForEach(csvData => {
                                 var csvSplitLists = csvData.Split(',').ToList();
                                 if (!csvSplitLists[0].Equals("guard_id")         && 
                                     !string.IsNullOrWhiteSpace(csvSplitLists[0]))
                                 {
                                    var enemyModel = new EnemyGuardian();
                                    enemyModel.guard_id                 = int.Parse(csvSplitLists[0]);             //  カードID
                                    foreach (var i in Enumerable.Range(0, 9))
                                    {
                                        enemyModel.enemy_params[i].enemy_id = int.Parse(csvSplitLists[i * 2 + 1]);
                                        
                                        enemyModel.enemy_params[i].enemy_level = int.Parse(csvSplitLists[i * 2 + 2]);
                                    }
                                    enemyGuardian2.enemy_guardians.Add(enemyModel);
                                 }
                             });
        }
    }

}

#endif