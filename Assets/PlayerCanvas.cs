using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    public Transform Menu;
    public Transform Player;
    public Transform PlayerList;
    public Transform Leaderboard;
    public GameObject buttonPrefab;
    public RectTransform ParentPanel;
    List<GameObject> generatedButtonObject = new List<GameObject>();
    public InputField PlayerInputField;
    public InputField PlayerListInputField;

    // Start is called before the first frame update
    //void Start()
    //{


    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private string conn, sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    // Use this for initialization
    void Start()
    {
        PlayerPrefs.SetString("CURRENT_PLAYER", "");
        conn = "URI=file:" + Application.dataPath + "/Plugins/SpacealDam.s3db"; //Path to database.
        //Deletvalue(6);
        //insertStudent("ahmedm", "ahmedm@gmail.com", "sss"); 
        //Updatevalue("a", "w@gamil.com", "1st", 1);
        readers();
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
            sqlQuery = "SELECT name " + "FROM sys_students";// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int i = 0;
            while (reader.Read())
            {
                string name = reader.GetString(0);

                GameObject goButton = (GameObject)Instantiate(buttonPrefab);
                generatedButtonObject.Add(goButton);
                goButton.GetComponentInChildren<Text>().text = name;
                goButton.transform.SetParent(ParentPanel, false);
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
        Menu.Find("PlayerName").GetComponent<Text>().text = name;
        Menu.gameObject.SetActive(true);
        Player.gameObject.SetActive(false);
        PlayerList.gameObject.SetActive(false);
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
                PlayerList.Find("ButtonBack").gameObject.SetActive(false);
                PlayerList.gameObject.SetActive(true);
                Player.gameObject.SetActive(false);

                // Show data in player list
                studentsList();
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
            if (username.Length < 15)
            {
                if (isInsertStudent(username))
                {
                    PlayerPrefs.SetString("CURRENT_PLAYER", username);
                    insertStudent(username);
                    Player.Find("ErrorMessage").GetComponent<Text>().text = "";
                    PlayerInputField.text = "";
                    PlayerList.Find("ErrorMessage").GetComponent<Text>().text = "";
                    PlayerListInputField.text = "";
                    Menu.Find("PlayerName").GetComponent<Text>().text = username;
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
        PlayerList.gameObject.SetActive(false);
        Leaderboard.gameObject.SetActive(false);
    }

    public void GOLeaderboard()
    {
        Leaderboard.gameObject.SetActive(true);
        Menu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
