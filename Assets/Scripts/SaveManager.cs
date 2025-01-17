using System.IO;
using System.Linq;
using MessagePack;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveManager
{
    static string savePath = Path.Combine(Application.persistentDataPath, "SaveData");
    static IObjectWithData[] objectsWithData;
    static private GameData gameData;

    static public void Setup(IObjectWithData[] objects)
    {
        objectsWithData = objects;
    }
    static public void Load()
    {
        if (File.Exists(savePath))
        {
            using (FileStream stream = new(savePath, FileMode.Open))
                gameData = MessagePackSerializer.Deserialize<GameData>(stream);
        }
        else
            gameData = new();

        foreach (IObjectWithData obj in objectsWithData)
            obj.LoadData(gameData);
    }
    static public void Save()
    {
        foreach (IObjectWithData obj in objectsWithData)
            obj.FetchData(gameData);

        using (FileStream stream = new(savePath, FileMode.Create))
            stream.Write(MessagePackSerializer.Serialize(gameData));
    }
}