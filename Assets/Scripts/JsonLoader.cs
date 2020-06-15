using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Boomlagoon.JSON;
using UnityEngine;

public static class JsonLoader
{
    private static string _path;
    private static JSONObject _jsonObject;
    
    public static JSONObject LoadJsonFromFile(string name)
    {
        _path = $"{Application.dataPath}/{name}";
        try
        {
            var jsonTextAsset = Resources.Load<TextAsset>("test");
            if (jsonTextAsset != null)
            {
                var contents = jsonTextAsset.text;
                _jsonObject = JSONObject.Parse(contents);
            }
            else
            {
                Debug.LogError($"File {_path} does not exist.");
                _jsonObject = new JSONObject();
            }

            return _jsonObject;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static Vector3 GetVector3(JSONObject obj)
    {
        return new Vector3(
            (float)obj["x"].Number, 
            (float)obj["y"].Number,
            (float)obj["z"].Number);
    }
}
