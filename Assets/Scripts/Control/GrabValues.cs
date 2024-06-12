using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class GrabValues : MonoBehaviour
{
    //Connection
    IDbConnection dbcon;
    static string connection;
    private void Start()
    {
        connection = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
    }

    //Gets how many Items are on the Shelf by ItemId
    //-1 if there is an error, 0 if there are none
    private long numOnShelf(string itemId)
    {

        long output = 0;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT SUM(Quantity) FROM Inventory WHERE ItemId = @ItemId";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", itemId));
            output = (long)dbcmd.ExecuteScalar();
        }
        catch (System.Exception e)
        {
            Debug.Log("numOnShelf from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            Debug.Log(e.Message);
            output = -1;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    public customer getCustomer(string customerName)
    {
        customer c = new customer();
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT customerId FROM Customers WHERE Name LIKE @customerName";
            dbcmd.Parameters.Add(new SqliteParameter("@customerName", "%" + customerName + "%"));
            string cId = (string)dbcmd.ExecuteScalar();
            dbcon.Close();
            c = getCustomerById(cId);
        }
        catch (System.Exception e)
        {
            dbcon.Close();
            Debug.Log("getCustomer from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            Debug.Log(e.Message);
            c = new customer();
        }
        return c;
    }
    public customer getCustomerById(string customerId)
    {
        customer c = new customer();
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT QuantityOfOrders,Name,Address,Phone,Total FROM Customers WHERE CustomerId = @CustomerId";
            dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", customerId));
            IDataReader cReader = dbcmd.ExecuteReader();
            cReader.Read();
            c.QuantityOfOrders = (int)substitueDBNull(cReader["QuantityOfOrders"]);
            c.customerId = customerId;
            c.name = (string)cReader["Name"];
            c.address = (string)cReader["Address"];
            c.phone = (long)cReader["Phone"];
            if (substitueDBNull(cReader["Total"]) == 0)
                c.total = 0;
            else
                c.total = (decimal)cReader["Total"];

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("getCustomerById from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            c = new customer();
            c.customerId = "C9999";
        }
        finally
        {
            dbcon.Close();
        }
        return c;
    }

    private long substitueDBNull(object db)
    {
        if (db.GetType().ToString() == "System.DBNull")
            return 0;
        else
            return (long)db;
    }

    public IDataReader getCustomerReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT * FROM Customers";
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
            Debug.Log("getCustomerReader from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getItemsCalled(string itemName)
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT * FROM Items WHERE ItemName LIKE @ItemName";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemName", "%" + itemName + "%"));
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("getItemsCalled from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            reader = null;
            dbcon.Close();
        }
        finally
        {
        }
        return reader;

    }

    public item getItemById(int id)
    {
        item i = new item();
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT ItemName, Flavor, Size, PRICE FROM Items WHERE ItemId = @ItemId";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", id));
            IDataReader reader = dbcmd.ExecuteReader();
            reader.Read();
            i.itemId = id;
            i.itemName = (string)reader["ItemName"];
            i.flavor = (string)reader["Flavor"];
            i.size = (string)reader["Size"];
            i.price = (decimal)reader["PRICE"];

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("getItemById from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            i = new item();
        }
        finally
        {
            dbcon.Close();
        }
        return i;
    }

    public IDataReader getItemReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT * FROM Items";
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
            Debug.Log("getItemReader from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getOrderItemsById(string orderId)
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "SELECT * FROM Orders AS o JOIN OPS AS ops ON o.OrderId = ops.OrderId WHERE o.OrderId = @OrderId";
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", orderId));
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("getOrderItemsById from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            reader = null;
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getOrderItemsReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "SELECT o.OrderId, o.BatchId, o.ItemId, o.CustomerId, o.Quantity, o.STATUS , ops.Total, ops.STATUS FROM Orders AS o JOIN OPS AS ops ON o.OrderId = ops.OrderId";
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
            Debug.Log("getOrderItemsReader from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getBatchReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "SELECT * FROM Batches";
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
            Debug.Log("getBatchReader from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getBatchById(string batchId)
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "SELECT * FROM Batches WHERE BatchId = @BatchId";
            dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchId));
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            dbcon.Close();
            Debug.Log("getBatchById from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            reader = null;
        }
        finally
        {
        }
        return reader;
    }

    public IDataReader getInventoryReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "SELECT * FROM Inventory";
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            reader = null;
            Debug.Log("getInventoryReader from GrabValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            dbcon.Close();
        }
        finally
        {
        }
        return reader;
    }

    public long getStockQuantityById(string itemId)
    {
        return numOnShelf(itemId);
    }
}
