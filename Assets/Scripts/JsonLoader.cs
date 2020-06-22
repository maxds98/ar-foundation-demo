using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Boomlagoon.JSON;
using UnityEngine;

public static class JsonLoader
{
    /// <summary>
    ///  Path to the JSON file.
    /// </summary>
    private static string _path;
    
    /// <summary>
    /// JSON object from the file.
    /// </summary>
    private static JSONObject _jsonObject;
    
    /// <summary>
    /// Loading JSON from the file.
    /// </summary>
    /// <param name="name">File name.</param>
    /// <returns>JSON object.</returns>
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

    /// <summary>
    /// Parse vector3 from the JSON object.
    /// </summary>
    /// <param name="obj">Passed JSON object.</param>
    /// <returns>Vector3 from the JSONObject.</returns>
    public static Vector3 GetVector3(JSONObject obj)
    {
        return new Vector3(
            (float)obj["x"].Number, 
            (float)obj["y"].Number,
            (float)obj["z"].Number);
    }
}
