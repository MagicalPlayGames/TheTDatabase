using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UI;

public class ControlQuery : MonoBehaviour
{
    int tempPwd = 1234;
    //Connection
    IDbConnection dbcon;
    static string connection;

    public InputField request;
    public InputField pwd;
    private void Start()
    {
        connection = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
    }
    public long customScalar(int password, string query)
    {
        long output = -99;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            if(password!=tempPwd)
            {
                throw new System.Exception("Go Away");
            }
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = query;
            output = (long)dbcmd.ExecuteScalar();

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            output = -1;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    public IDataReader customReader(int password, string query)
       {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            if(password!=tempPwd)
            {
                throw new System.Exception("Go Away");
            }
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = query;
            reader = dbcmd.ExecuteReader();

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
        }
        finally
        {
            dbcon.Close();
        }
        return reader;
       }


    public void submitQuery()
    {
        customNonQuery(int.Parse(pwd.text), request.text);
    }
    public long customNonQuery(int password,string query)
    {
        long output = -99;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            if(password!=tempPwd)
            {
                throw new System.Exception("Go Away");
            }
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = query;
            output = (long)dbcmd.ExecuteNonQuery();

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            output = -1;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }
     
}
