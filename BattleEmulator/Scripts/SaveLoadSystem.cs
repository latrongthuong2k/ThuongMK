using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveLoadSystem
{
    private const string _saveFileName = "/Emu.data";
    
    public static void Save(BattleEmulatorManager battleEmulator, DataSave _dataSaveSecond , int Num, string name) 
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + _saveFileName ;

        if (!File.Exists(path + Num + name))
        {
            FileStream stream = new FileStream(path + Num + name, FileMode.Create);

            if (battleEmulator != null)
            {
                DataSave data = new DataSave(battleEmulator, null, name);
                formatter.Serialize(stream, data);
            }
            else
            {
                DataSave data = new DataSave(null, _dataSaveSecond, name);
                formatter.Serialize(stream, data);
            }
            stream.Close();
        }
        else
        {
            Debug.LogError("File is exists in " + path);
        }
       
    }
    public static DataSave LoadData(int value, string name)
    {
        string path = Application.persistentDataPath + _saveFileName;

        if (File.Exists(path + value + name))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path + value + name, FileMode.Open);

            DataSave data = formatter.Deserialize(stream) as DataSave;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
