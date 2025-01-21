using System.IO;
using System.Linq;
using MessagePack;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveManager
{
    static string savePath = Path.Combine(Application.persistentDataPath, "SaveData");
    static IDataLoader[] dataLoaders;
    static IDataFetcher[] dataFetchers;
    static private GameData gameData;

    static public void Setup(IDataLoader[] loaders, IDataFetcher[] fetchers)
    {
        dataLoaders = loaders;
        dataFetchers = fetchers;
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

        foreach (IDataLoader obj in dataLoaders)
            obj.LoadData(gameData);
    }
    static public void Save()
    {
        foreach (IDataFetcher obj in dataFetchers)
            obj.FetchData(gameData);

        using (FileStream stream = new(savePath, FileMode.Create))
            stream.Write(MessagePackSerializer.Serialize(gameData));
    }
}