using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;


public static class DataUtils
{
    public static void SaveVector3Array(Vector3[] data, string savePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));

        PreSerializableVector4[] serData = Array.ConvertAll(data, v3 => (PreSerializableVector4)v3);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath);
        bf.Serialize(file, serData);
        file.Close();
    }

    public static bool LoadVector3Array(out Vector3[] data, string savePath)
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            PreSerializableVector4[] serData = (PreSerializableVector4[])bf.Deserialize(file);
            file.Close();

            data = DeserializeVector3Array(serData);

            return true;
        }
        else
        {
            data = null;
            return false;
        }

    }

    static void ResetPipeData()
    {
        if (File.Exists(Application.persistentDataPath
          + "/MySaveData.dat"))
        {
            File.Delete(Application.persistentDataPath
              + "/MySaveData.dat");
            //points = new Vector3[0];
            Debug.Log("Data reset complete!");
        }
        else
            Debug.LogError("No save data to delete.");
    }

    public static Vector3[] DeserializeVector3Array(PreSerializableVector4[] serData)
    {
        Vector3[] data = new Vector3[serData.Length];
        for (int i = 0; i < serData.Length; i++)
        {
            data[i] = serData[i].ToVector3();
        }

        return data;
    }
}
