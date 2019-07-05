using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ShowPuppeteer : EditorWindow
{
    Texture2D image;

    [MenuItem("Tools/More From Puppeteer")]
    static void Init()
    {
        ShowPuppeteer window = ScriptableObject.CreateInstance<ShowPuppeteer>();
        window.position = new Rect(0, 0, 750, 450);
        window.ShowPopup();
    }

    void Awake()
    {
        image = Resources.Load<Texture2D>("Images/MoreFromPuppeteer");
    }

    void OnGUI()
    {
        GUILayout.Label("Thanks for the download, try it out and don't forget to give a review/rating!", EditorStyles.boldLabel);

        GUILayout.Space(5);

        GUILayout.Label("Trivia Quiz - Update 1.99f2(18.02.2018)", EditorStyles.label);
        GUILayout.Label("- You can enter a URL for an image or a sound, and they will be dynamically loaded at runtime.So no more need for putting any images or sounds in the Resources folder!", EditorStyles.miniLabel);

        GUILayout.Space(5);

        //GUILayout.Label("Thanks for the download! Try it out and don't forget to rate!", EditorStyles.boldLabel);

        if (GUILayout.Button(image))
        {
            Application.OpenURL("https://goo.gl/FkU6Rn");

            this.Close();
        }

        if (GUILayout.Button("Click here for more assets from Puppeteer!"))
        {
            Application.OpenURL("https://goo.gl/FkU6Rn");

            this.Close();
        }


        if (GUILayout.Button("Continue"))
        {
            this.Close();
        }
    }
}