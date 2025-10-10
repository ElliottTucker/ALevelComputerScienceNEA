using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; //binary formater library 

public static class SaveSystem
{
    public static void SaveData(PlayerData playerData)
    {
        BinaryFormatter formatter = new BinaryFormatter(); //starts binary formatter
        string path = Application.persistentDataPath + "/playerData.dat"; //assign the formatter a path

        FileStream stream = new FileStream(path, FileMode.Create); //opens a file stream at said path
        formatter.Serialize(stream, playerData); //turns data to binary then saves it

        stream.Close();
    }

    public static PlayerData LoadData()
    {
        string path = Application.persistentDataPath + "/playerData.dat";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData playerData = (PlayerData)formatter.Deserialize(stream);

            stream.Close();
            return playerData;
        }
        else
        {
            return null;
        }
    }

    public static void DeleteData()
    {
        string path = Application.persistentDataPath + "/playerData.dat";

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}


