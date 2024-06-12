using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;

public class CreateDB : MonoBehaviour
{
    IDbConnection dbcon;
    // Start is called before the first frame update
    void Awake()
    {
        string connection = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //Batches should be included in inventory
            /*dbcmd.CommandText = "DROP TABLE Items";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "DROP TABLE Batches";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "DROP TABLE Inventory";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "DROP TABLE Customers";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "DROP TABLE Orders";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "DROP TABLE OPS";
            dbcmd.ExecuteNonQuery();*/
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS Items(ItemId INTEGER IDENTITY(1,1) PRIMARY KEY,ItemName VARCHAR(50) NOT NULL, Flavor VARCHAR(50), Size VARCHAR(20),PRICE DECIMAL(5,2))";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS Batches(BatchId VARCHAR(5),ItemId INTEGER REFERENCES Items(ItemId), Quantity INTEGER, Description VARCHAR(255), PRIMARY KEY (BatchId,ItemId))";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS Inventory(ItemId INTEGER REFERENCES Items(ItemId), BatchId VARCHAR(5) REFERENCES Batches(BatchId),Quantity INTEGER,Description VARCHAR(255), PRIMARY KEY (ItemId,BatchId))";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS Customers(CustomerId VARCHAR(5) CONSTRAINT PK_CID PRIMARY KEY,QuantityOfOrders INTEGER, Name VARCHAR(255), Address VARCHAR(255), Phone INTEGER(10), Total DECIMAL(6,2))";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS Orders(OrderId INTEGER, BatchId VARCHAR(5),ItemId INTEGER REFERENCES Items(ItemId), CustomerId VARCHAR(5), Quantity INTEGER, Total DECMIAL(5,2), STATUS CONSTRAINT S_CHK CHECK(STATUS = 'Unfilled' OR STATUS='Filled' OR STATUS ='Cancelled'), PRIMARY KEY(ItemId,OrderId,BatchId))";
            dbcmd.ExecuteNonQuery();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS OPS(OrderId INTEGER REFERENCES Orders(OrderId), Total DECMIAL(5,2),STATUS CONSTRAINT S_CHK CHECK(STATUS = 'Unfilled' OR STATUS='Ordered' OR STATUS='Paid' OR STATUS = 'Delivered' OR STATUS = 'Cancelled' OR STATUS='Refunded'))";
            dbcmd.ExecuteNonQuery();
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            dbcon.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
