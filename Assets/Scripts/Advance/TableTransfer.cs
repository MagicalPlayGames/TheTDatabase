using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using SimpleFileBrowser;
using System;

public class TableTransfer : MonoBehaviour
{
    //Transfers Table Info
    private string path;
    private string conneciton;
    private DataSet set;

    public GrabValues grabBag;
    void Start()
    {
        set = new DataSet();
        conneciton = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, Application.persistentDataPath, null, "Load HRS XML", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            byte[] bytes2 = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            Debug.Log(FileBrowser.Result[0]);
            // Or, copy the first file to persistentDataPath
            set.ReadXmlSchema(FileBrowser.Result[1]);
            set.ReadXml(FileBrowser.Result[0]);
            loadFile();
        }
        else
        {
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
    }

    //Opens File Explorer to choose location //Can be updated
    public void openFileExplorer()
    {
#if UNITY_EDITOR
        path = UnityEditor.EditorUtility.SaveFolderPanel("Select A Folder", "", "");
        saveFile();
#endif
#if UNITY_ANDROID
        path = Application.persistentDataPath;
        saveFile();
        //string FileType = NativeFilePicker.ConvertExtensionToFileType(".xml");
        //NativeFilePicker.Permission permission = NativeFilePicker.PickFile(getFile, new string[] { FileType });
#endif
    }

    private void getFile(string path)
    {
        if (path == null)
        {
            Debug.Log("File Upload Error");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        else
        {
            this.path = path;
            saveFile();
        }
    }

    //fills the dataset with tables
    public void saveFile()
    {
        set.Clear();
        itemAdapter();
        batchAdapter();
        inventoryAdapter();
        CustomerAdapter();
        orderAdapter();
        OPSAdapter();
        write();
    }

    //Adapters that fill the dataset
    private void itemAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM Items", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "Items";
    }

    private void batchAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM Batches", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "Batches";
    }

    private void inventoryAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM Inventory", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "Inventory";
    }

    private void CustomerAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM Customers", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "Customers";
    }

    private void orderAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM Orders", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "Orders";
    }

    private void OPSAdapter()
    {
        IDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM OPS", conneciton);
        adapter.Fill(set);
        set.Tables[set.Tables.Count - 1].TableName = "OPS";
    }

    //Writes out the XML for Schema and Data
    private void write()
    {
#if UNITY_EDITOR
        if (path.LastIndexOf(".") > path.Length - 5)
        {
            set.WriteXml(path + "/HRS.xml");
            set.WriteXmlSchema((path+ "/HRSSchema.xml"));
        }
        else
        {
            set.WriteXml(path + "/HRS.xml");
            set.WriteXmlSchema((path + "/HRSSchema.xml"));
        }
#endif
#if UNITY_ANDROID
        set.WriteXml(path.Substring(0, path.LastIndexOf("/") + 1) + "HRS.xml");
        set.WriteXmlSchema((path.Substring(0, path.LastIndexOf("/") + 1) + "HRSSchema.xml"));
        //NativeFilePicker.ExportFile((path.Substring(0, path.LastIndexOf("/") + 1) + "/HRS.xml"));
        //NativeFilePicker.ExportFile((path.Substring(0, path.LastIndexOf("/") + 1) + "/HRSSchema.xml"));
#endif
    }

    public void selectFile()
    {
        set.Clear();
#if UNITY_EDITOR
        path = UnityEditor.EditorUtility.OpenFilePanel("Select HRS.xml", "", "xml");
        set.ReadXmlSchema(path.Substring(0, path.Length - 4) + "Schema.xml");
        set.ReadXml(path);
        loadFile();
#endif
#if UNITY_ANDROID
        //string FileType = NativeFilePicker.ConvertExtensionToFileType("");
        //NativeFilePicker.Permission permission = NativeFilePicker.PickFile(getFile, new string[] { FileType });
        StartCoroutine(ShowLoadDialogCoroutine());
#endif
    }

    //Loads in HRS and HRSSchema files to tables
    //For every SELECT Count(*) < 1 INSERT
    public void loadFile()
    {
        SqliteConnection dbcon = new SqliteConnection(conneciton);
        dbcon.Open();
        foreach (DataTable table in set.Tables)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                try
                {
                    IDbCommand dbcmd = dbcon.CreateCommand();
                    string query = "SELECT Count(*) FROM " + table.TableName + " WHERE ";
                    foreach (DataColumn column in table.Columns)
                    {
                        if (column != table.Columns[0])
                        {
                            query += " AND ";
                        }
                        query += column.ColumnName + " = @" + column.ColumnName;
                        string columnType = column.DataType.ToString();
                        if (columnType.Contains("tring"))
                        {
                            dbcmd.Parameters.Add(new SqliteParameter("@" + column.ColumnName, (string)table.Rows[i][column.ColumnName]));
                        }
                        else if(columnType.Contains("ecimal"))
                        {
                            dbcmd.Parameters.Add(new SqliteParameter("@" + column.ColumnName, (decimal)table.Rows[i][column.ColumnName]));
                        }
                        else if (columnType.Contains("ouble"))
                        {
                            dbcmd.Parameters.Add(new SqliteParameter("@" + column.ColumnName, (double)table.Rows[i][column.ColumnName]));
                        }
                        else
                        {
                            dbcmd.Parameters.Add(new SqliteParameter("@" + column.ColumnName, (long)table.Rows[i][column.ColumnName]));
                        }
                    }
                    dbcmd.CommandText = query;
                    Debug.Log(dbcmd.ExecuteScalar());
                    if(dbcmd.ExecuteScalar().GetType().ToString().Contains("System.DBNull"))
                    {
                        Debug.Log("Yes");
                        query = "INSERT INTO " + table.TableName + "(";
                        string cloumnStrings = "(";
                        foreach (DataColumn column in table.Columns)
                        {
                            if (column != table.Columns[0])
                            {
                                query += ",";
                                cloumnStrings += ", ";
                            }
                            query += column.ColumnName;
                            cloumnStrings += "@" + column.ColumnName;
                        }
                        query += ")VALUES" + cloumnStrings + ")";
                        try
                        {
                            dbcmd.CommandText = query;
                            dbcmd.ExecuteNonQuery();
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("Insert Error");
                            Debug.Log(e.Message);
                        }
                    }
                    if ((long)dbcmd.ExecuteScalar() < 1)
                    {
                        query = "INSERT INTO " + table.TableName + "(";
                        string cloumnStrings = "(";
                        foreach (DataColumn column in table.Columns)
                        {
                            if (column != table.Columns[0])
                            {
                                query += ",";
                                cloumnStrings += ", ";
                            }
                            query += column.ColumnName;
                            cloumnStrings += "@" + column.ColumnName;
                        }
                        query += ")VALUES" + cloumnStrings + ")";
                        try
                        {
                            dbcmd.CommandText = query;
                            dbcmd.ExecuteNonQuery();
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("Insert Error");
                            Debug.Log(e.Message);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("Select Row Error");
                    Debug.Log(e.Message);
                }
            }
        }
        dbcon.Close();
    }
}
