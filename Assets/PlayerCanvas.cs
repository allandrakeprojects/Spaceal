using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Data;
using UnityEngine.UI;
using System.IO;

public class PlayerCanvas : MonoBehaviour
{
    string databaseName = "SpacealDam.s3db";

    public GameObject About;
    public Transform Menu;
    public Transform Player;
    public Transform PlayerList;
    public Transform Leaderboard;
    public Transform LeaderboardNoRecord;
    public GameObject buttonPrefab;
    public RectTransform buttonParentPanel;
    List<GameObject> generatedButtonObject = new List<GameObject>();
    public InputField PlayerInputField;
    public InputField PlayerListInputField;
    public Dropdown dropdownSubject;
    public Dropdown dropdownLevel;

    public GameObject highscorePrefab;
    public RectTransform highScoreParentPanel;
    List<GameObject> generatedHighscoreObject = new List<GameObject>();

    // Start is called before the first frame update
    //void Start()
    //{


    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private string conn, sqlQuery;
    private IDataReader reader;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    private string dropdownSubjectSelected;
    private string dropdownLevelSelected;

    void Start()
    {
        dropdownSubject.onValueChanged.AddListener(delegate {
            DropdownValueChangedSubject(dropdownSubject);
        });

        dropdownSubjectSelected = "English";

        dropdownLevel.onValueChanged.AddListener(delegate {
            DropdownValueChangedLevel(dropdownLevel);
        });

        dropdownLevelSelected = "Level 1";

#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", databaseName);
#else
            // check if file exists in Application.persistentDataPath
            var filepath = string.Format("{0}/{1}", Application.persistentDataPath, databaseName);
       
            if (!File.Exists(filepath))
            {
                Debug.Log("Database not in Persistent path");
                // if it doesn't ->
                // open StreamingAssets directory and load the db ->
           
#if UNITY_ANDROID
                var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + databaseName);  // this is the path to your StreamingAssets in android
                while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
                // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                var loadDb = Application.dataPath + "/Raw/" + databaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
           
#elif UNITY_WINRT
                var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#endif
           
                Debug.Log("Database written");
            }
       
            var dbPath = filepath;
#endif
        //_connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        conn = "URI=file:" + dbPath;
        readers();
    }

    void DropdownValueChangedSubject(Dropdown change)
    {
        dropdownSubjectSelected = change.value.ToString();
        if (dropdownSubjectSelected == "0")
        {
            dropdownSubjectSelected = "English";
        } else if (dropdownSubjectSelected == "1")
        {
            dropdownSubjectSelected = "Science";
        }
        else if (dropdownSubjectSelected == "2")
        {
            dropdownSubjectSelected = "Math";
        }

        leaderboardGet();
    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChangedLevel(Dropdown change)
    {
        dropdownLevelSelected = change.value.ToString();
        if (dropdownLevelSelected == "0")
        {
            dropdownLevelSelected = "Level 1";
        }
        else if (dropdownLevelSelected == "1")
        {
            dropdownLevelSelected = "Level 2";
        }
        else if (dropdownLevelSelected == "2")
        {
            dropdownLevelSelected = "Level 3";
        }

        leaderboardGet();
    }

    private bool isInsertStudent(string nameGet)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = "SELECT name " + "FROM sys_students";// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                if (name.ToLower().Trim() == nameGet.ToLower().Trim())
                {
                    return false;
                }
            }

            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }

        return true;
    }

    private void studentsList()
    {
        foreach (var obj in generatedButtonObject)
        {
            Destroy(obj);
        }

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = "SELECT name FROM sys_students ORDER BY name";// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int i = 0;
            while (reader.Read())
            {
                string name = reader.GetString(0);

                GameObject goButton = (GameObject)Instantiate(buttonPrefab);
                generatedButtonObject.Add(goButton);
                goButton.GetComponentInChildren<Text>().text = name;
                goButton.transform.SetParent(buttonParentPanel, false);
                goButton.transform.localScale = new Vector3(1, 1, 1);

                Button tempButton = goButton.GetComponent<Button>();
                string tempName = name;
                i++;
                tempButton.onClick.AddListener(() => ButtonClicked(tempName));
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }

    void ButtonClicked(string name)
    {
        PlayerPrefs.SetString("CURRENT_PLAYER", name);
        Player.Find("ErrorMessage").GetComponent<Text>().text = "";
        PlayerInputField.text = "";
        PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "";
        PlayerListInputField.text = "";
        Menu.Find("SideMenu/PlayerName").GetComponent<Text>().text = name;
        Menu.gameObject.SetActive(true);
        Player.gameObject.SetActive(false);
        PlayerList.gameObject.SetActive(false);

        setLevel(name);
    }

    private void setLevel(string name)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("SELECT level FROM sys_students WHERE name=\"{0}\"", name);// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            string levelGet = "";
            while (reader.Read())
            {
                string level = reader.GetString(0);
                String[] combinationArray = level.ToString().Split(new string[] { "," }, StringSplitOptions.None);
                for (int index = 0; index < combinationArray.Length; index++)
                {
                    if (index == 0)
                    {
                        levelGet += combinationArray[index] + ",";
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 1 (Placement Test)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 1 (Placement Test)Completed", 1);
                        }
                    }
                    else if (index == 1)
                    {
                        levelGet += combinationArray[index] + ",";
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 2 (Vocabulary Words)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 2 (Vocabulary Words)Completed", 1);
                        }
                    }
                    else if (index == 2)
                    {
                        levelGet += combinationArray[index] + ",";
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 1 (Human Body)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 1 (Human Body)Completed", 1);
                        }
                    }
                    else if (index == 3)
                    {
                        levelGet += combinationArray[index] + ",";
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 2 (Animals)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 2 (Animals)Completed", 1);
                        }
                    }
                    else if (index == 4)
                    {
                        levelGet += combinationArray[index] + ",";
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 1 (Place Value)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 1 (Place Value)Completed", 1);
                        }
                    }
                    else if (index == 5)
                    {
                        levelGet += combinationArray[index];
                        if (Convert.ToInt32(combinationArray[index]) == 0)
                        {
                            PlayerPrefs.DeleteKey("Level 2 (Computation)Completed");
                        }
                        else
                        {
                            PlayerPrefs.SetInt("Level 2 (Computation)Completed", 1);
                        }
                    }
                }

                PlayerPrefs.SetString("CURRENT_LEVEL", levelGet);

                break;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }

    private void insertStudent(string name)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("insert into sys_students (name) values (\"{0}\")", name);// table name
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }

        setLevel(name);
    }

    private void Deletvalue(int id)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("Delete from Usersinfo WHERE ID=\"{0}\"", id);// table name
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
    }

    private void Updatevalue(string name, string email, string address, int id)
    {
        using (dbconn = new SqliteConnection(conn))
        {

            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("UPDATE Usersinfo set Name=\"{0}\", Email=\"{1}\", Address=\"{2}\" WHERE ID=\"{3}\" ", name, email, address, id);// table name
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
    }

    private void readers()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            IDbCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sys_students";
            var count = (Int64)cmd.ExecuteScalar();
            if (count > 0)
            {
                if (PlayerPrefs.GetString("CURRENT_PLAYER") == "")
                {
                    PlayerList.Find("ButtonBack").gameObject.SetActive(false);
                    PlayerList.gameObject.SetActive(true);
                    Player.gameObject.SetActive(false);

                    // Show data in player list
                    studentsList();
                }
                else
                {
                    setLevel(PlayerPrefs.GetString("CURRENT_PLAYER"));

                    Menu.Find("SideMenu/PlayerName").GetComponent<Text>().text = PlayerPrefs.GetString("CURRENT_PLAYER");
                    GOHome();
                }
            }
            //dbcmd = dbconn.CreateCommand();
            //sqlQuery = "SELECT * " + "FROM sys_students";// table name
            //dbcmd.CommandText = sqlQuery;
            //IDataReader reader = dbcmd.ExecuteReader();
            //while (reader.Read())
            //{
            //    int id = reader.GetInt32(0);
            //    string name = reader.GetString(1);

            //    Debug.Log("value= " + id + "  name =" + name);
            //}
            //reader.Close();
            //reader = null;
            //dbcmd.Dispose();
            dbcmd = null;
            dbconn = null;
        }
    }

    [Obsolete]
    public void GO()
    {
        string username = "";
        if (Player.gameObject.active)
        {
            username = Player.Find("PlayerUsername/Text").GetComponent<Text>().text.Trim();
        }
        else
        {
            username = PlayerList.Find("PlayerUsername/Text").GetComponent<Text>().text.Trim();
        }

        if (username.Length > 0)
        {
            if (username.Length <= 10)
            {
                if (isInsertStudent(username))
                {
                    PlayerPrefs.SetString("CURRENT_PLAYER", username);
                    insertStudent(username);
                    Player.Find("ErrorMessage").GetComponent<Text>().text = "";
                    PlayerInputField.text = "";
                    PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "";
                    PlayerListInputField.text = "";
                    Menu.Find("SideMenu/PlayerName").GetComponent<Text>().text = username;
                    Menu.gameObject.SetActive(true);
                    Player.gameObject.SetActive(false);
                    PlayerList.gameObject.SetActive(false);
                }
                else
                {
                    if (Player.gameObject.active)
                    {
                        Player.Find("ErrorMessage").GetComponent<Text>().text = "Name exists";
                    }
                    else
                    {
                        PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "Name exists";
                    }
                }
            }
            else
            {
                if (Player.gameObject.active)
                {
                    Player.Find("ErrorMessage").GetComponent<Text>().text = "Name too long";
                }
                else
                {
                    PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "Name too long";
                }
            }
        }
        else
        {
            if (Player.gameObject.active)
            {
                Player.Find("ErrorMessage").GetComponent<Text>().text = "Provide name";
            }
            else
            {
                PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "Provide name";
            }
        }
    }

    public void GOAbout()
    {
        About.SetActive(true);
        Menu.gameObject.SetActive(false);
    }

    public void GOPlayerList()
    {
        PlayerList.Find("ButtonBack").gameObject.SetActive(true);
        Menu.gameObject.SetActive(false);
        PlayerList.gameObject.SetActive(true);

        // Show data in player list
        studentsList();
    }

    public void GOHome()
    {
        Menu.gameObject.SetActive(true);
        About.SetActive(false);
        PlayerList.gameObject.SetActive(false);
        Player.gameObject.SetActive(false);
        Leaderboard.gameObject.SetActive(false);
    }

    public void GOLeaderboard()
    {
        Leaderboard.gameObject.SetActive(true);
        Menu.gameObject.SetActive(false);

        leaderboardGet();
    }

    private void leaderboardGet()
    {
        foreach (var obj in generatedHighscoreObject)
        {
            Destroy(obj);
        }

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            IDbCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = string.Format("SELECT count(*) FROM sys_leaderboard WHERE subject=\"{0}\" AND level=\"{1}\"", dropdownSubjectSelected, dropdownLevelSelected);// table name
            var count = (Int64)cmd.ExecuteScalar();
            if (count > 0)
            {
                LeaderboardNoRecord.gameObject.SetActive(false);
                leaderboardShow();
            }
            else
            {
                LeaderboardNoRecord.gameObject.SetActive(true);
            }
            dbcmd = null;
            dbconn = null;
        }
    }

    private void leaderboardShow()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("SELECT * FROM sys_leaderboard WHERE subject=\"{0}\" AND level=\"{1}\" ORDER BY stars desc, timespent asc, no_attempts asc, name asc", dropdownSubjectSelected, dropdownLevelSelected);// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int count = 0;
            while (reader.Read())
            {
                count++;
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string subject_ = reader.GetString(2);
                string level_ = reader.GetString(3);
                string timespent = reader.GetString(4);
                int noOfAttempts = reader.GetInt32(5);
                int stars = reader.GetInt32(6);
                string remarks = reader.GetString(7);

                if (count == 1)
                {
                    // First
                    GameObject goHighscore = (GameObject)Instantiate(highscorePrefab);
                    generatedHighscoreObject.Add(goHighscore);
                    if (PlayerPrefs.GetString("CURRENT_PLAYER") == name)
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = "-" + name + "-";
                    }
                    else
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = name;
                    }
                    goHighscore.transform.Find("subjectText").GetComponent<Text>().text = subject_;
                    goHighscore.transform.Find("levelText").GetComponent<Text>().text = level_;
                    goHighscore.transform.Find("timeSpentText").GetComponent<Text>().text = timespent;
                    goHighscore.transform.Find("noOfAttempts").GetComponent<Text>().text = noOfAttempts.ToString();
                    if (stars == 3)
                    {
                        // 3 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 3/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 2)
                    {
                        // 2 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 1)
                    {
                        // 1 star
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                    }
                    goHighscore.transform.Find("remarksText").GetComponent<Text>().text = remarks;
                    goHighscore.transform.SetParent(highScoreParentPanel, false);
                }
                else if (count == 2)
                {
                    // Second
                    GameObject goHighscore = (GameObject)Instantiate(highscorePrefab);
                    generatedHighscoreObject.Add(goHighscore);
                    goHighscore.transform.Find("trophy").GetComponent<Image>().color = new Color32(182, 179, 180, 255);
                    if (PlayerPrefs.GetString("CURRENT_PLAYER") == name)
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = "-" + name + "-";
                    }
                    else
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = name;
                    }
                    goHighscore.transform.Find("subjectText").GetComponent<Text>().text = subject_;
                    goHighscore.transform.Find("levelText").GetComponent<Text>().text = level_;
                    goHighscore.transform.Find("timeSpentText").GetComponent<Text>().text = timespent;
                    goHighscore.transform.Find("noOfAttempts").GetComponent<Text>().text = noOfAttempts.ToString();
                    if (stars == 3)
                    {
                        // 3 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 3/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 2)
                    {
                        // 2 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 1)
                    {
                        // 1 star
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                    }
                    goHighscore.transform.Find("remarksText").GetComponent<Text>().text = remarks;
                    goHighscore.transform.SetParent(highScoreParentPanel, false);
                }
                else if (count == 3)
                {
                    // Third
                    GameObject goHighscore = (GameObject)Instantiate(highscorePrefab);
                    generatedHighscoreObject.Add(goHighscore);
                    goHighscore.transform.Find("trophy").GetComponent<Image>().color = new Color32(184, 121, 100, 255);
                    if (PlayerPrefs.GetString("CURRENT_PLAYER") == name)
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = "-" + name + "-";
                    }
                    else
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = name;
                    }
                    goHighscore.transform.Find("subjectText").GetComponent<Text>().text = subject_;
                    goHighscore.transform.Find("levelText").GetComponent<Text>().text = level_;
                    goHighscore.transform.Find("timeSpentText").GetComponent<Text>().text = timespent;
                    goHighscore.transform.Find("noOfAttempts").GetComponent<Text>().text = noOfAttempts.ToString();
                    if (stars == 3)
                    {
                        // 3 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 3/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 2)
                    {
                        // 2 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 1)
                    {
                        // 1 star
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                    }
                    goHighscore.transform.Find("remarksText").GetComponent<Text>().text = remarks;
                    goHighscore.transform.SetParent(highScoreParentPanel, false);
                }
                else
                {
                    GameObject goHighscore = (GameObject)Instantiate(highscorePrefab);
                    generatedHighscoreObject.Add(goHighscore);
                    goHighscore.transform.Find("trophy").gameObject.SetActive(false);
                    if (PlayerPrefs.GetString("CURRENT_PLAYER") == name)
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = "-" + name + "-";
                    }
                    else
                    {
                        goHighscore.transform.Find("nameText").GetComponent<Text>().text = name;
                    }
                    goHighscore.transform.Find("subjectText").GetComponent<Text>().text = subject_;
                    goHighscore.transform.Find("levelText").GetComponent<Text>().text = level_;
                    goHighscore.transform.Find("timeSpentText").GetComponent<Text>().text = timespent;
                    goHighscore.transform.Find("noOfAttempts").GetComponent<Text>().text = noOfAttempts.ToString();
                    if (stars == 3)
                    {
                        // 3 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 3/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 2)
                    {
                        // 2 stars
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                        goHighscore.transform.Find("starsObject/Star 2/StarCollected").gameObject.SetActive(true);
                    }
                    else if (stars == 1)
                    {
                        // 1 star
                        goHighscore.transform.Find("starsObject/Star 1/StarCollected").gameObject.SetActive(true);
                    }
                    goHighscore.transform.Find("remarksText").GetComponent<Text>().text = remarks;
                    goHighscore.transform.SetParent(highScoreParentPanel, false);
                }
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
