using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[InitializeOnLoad]
public class UERP
{

    private static string applicationId = "509465267630374935";
    private static DiscordRpc.EventHandlers handlers;
    private static int callbackCalls;


    [MenuItem("UERP/Enable Rich Presence")]
    private static void Enable()
    {
        if (!EditorPrefs.GetBool("enabled"))
        {
            //If "enabled" is faulse, this will enable it
            EditorPrefs.SetBool("enabled", true);
            Debug.Log("Unity Editor Rich Presence : Enabled");
            Init();
        }
        else
        {
            Debug.Log("Unity Editor Rich Presence is already enabled");
        }
    }

    [MenuItem("UERP/Disable Rich Presence")]
    private static void Disable()
    {
        if (EditorPrefs.GetBool("enabled"))
        {
            //If "enabled" is true that means we need to shut it down
            EditorPrefs.SetBool("enabled", false);
            DiscordRpc.Shutdown();
            Debug.Log("Unity Editor Rich Presence has been disabled");
        }
    }

    //The Github button that leads to the original GitHub page.
    [MenuItem("UERP/Original GitHub")]
    private static void Github()
    {
        Application.OpenURL("https://github.com/MarshMello0/UERP");
    }
    //The Github button that leads to the forked GitHub page.
    [MenuItem("UERP/Forked GitHub")]
    private static void Github()
    {
        Application.OpenURL("https://github.com/reithegoat/UERP");
    }
    //This button is if for some reason they want to reset they prefs to the default
    [MenuItem("UERP/Reset to default")]
    private static void ResetPrefs()
    {
        EditorPrefs.DeleteAll();
        EditorPrefs.SetBool("enabled", true);
        AssetDatabase.Refresh();
    }

    //This gets called once everytime it recompiles / loads up
    static UERP() 
    {
        //Checking if the key "enabled" is true and if it doesn't exit it will output true
        if (EditorPrefs.GetBool("enabled", true)) 
        {
            Init();
            EditorApplication.update += Update;
            //This is here incase it didn't exist
            EditorPrefs.SetBool("enabled", true);
        }
        //This is true if the currentScene hasn't been set
        if (EditorPrefs.GetString("currentScene", "") == "")
        {
            EditorPrefs.SetString("currentScene", EditorSceneManager.GetActiveScene().name);
            EditorPrefs.SetString("timestamp", GetTimestamp());
        }
    }

    //This runs every frame like the runtime Update()
    static void Update()
    {
        DiscordRpc.RunCallbacks();
    }


    private static void Init()
    {
        try
        {
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorApplication.playModeStateChanged += LogPlayModeState;

            callbackCalls = 0;
            handlers = new DiscordRpc.EventHandlers();
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(applicationId, ref handlers, true, "1234");
        }
        catch(Exception e)
        {
            AssetDatabase.Refresh();
        }
        UpdatePresence();
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        //Make sure whenever the Editor starts/stops playing a scene, to update the presence.
        UpdatePresence();
    }
    
    private static void SceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        EditorPrefs.SetString("currentScene", EditorSceneManager.GetActiveScene().name);
        EditorPrefs.SetString("timestamp", GetTimestamp());
        //Every time a new scene is opened, it will call the update presence
        UpdatePresence();
    }
    
    
    
    //This updates the presence
    static void UpdatePresence()
    {
        DiscordRpc.RichPresence discordPresence = new DiscordRpc.RichPresence();
        if(EditorApplication.isPlaying) //If we're in play mode, then we're playtesting a scene.
            discordPresence.state = "Playtesting scene: " + EditorSceneManager.GetActiveScene().name;
        else //If the two above is false, then say we're editing a scene.
            discordPresence.state = "Editing scene: " + EditorSceneManager.GetActiveScene().name;
        discordPresence.details = "Project: " + GetProjectName();
        discordPresence.startTimestamp = long.Parse(EditorPrefs.GetString("timestamp",GetTimestamp()));
        discordPresence.largeImageKey = "logo";
        discordPresence.largeImageText = "Running Unity v" + Application.unityVersion;
        DiscordRpc.UpdatePresence(discordPresence);
    }


    static string GetProjectName()
    {
        string path = Application.dataPath;
        string[] folders = path.Split("/"[0]);
        return folders[folders.Length - 2];
    }

    private static void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        throw new NotImplementedException();
    }

    private static void ErrorCallback(int errorCode, string message)
    {
        Debug.LogError(errorCode + " | " + message);
    }

    private static void DisconnectedCallback(int errorCode, string message)
    {
        Debug.Log("Unity Editor Rich Presence has been disconnected \n " + errorCode + " | " + message);
    }

    public static String GetTimestamp()
    {
        long unixTimestamp = (long)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        return unixTimestamp.ToString();
    }
}
