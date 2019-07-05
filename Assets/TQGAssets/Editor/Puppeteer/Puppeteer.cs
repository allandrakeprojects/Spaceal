using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class Puppeteer : EditorWindow
{
    static Puppeteer()
    {
        if ( PlayerPrefs.GetInt("Puppeteer", 0) == 0 )    EditorApplication.update += Startup;
    }

    static void Startup()
    {
        EditorApplication.update -= Startup;

        EditorApplication.ExecuteMenuItem("Tools/More From Puppeteer");

        PlayerPrefs.SetInt("Puppeteer", 1);
    }
}
