using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;


//Can Add types of items
//Can add orders
//Can add customers
//can filled order
//can cancel order


public enum OrderStatus {Unfilled,Ordered,Paid,Delivered,Cancelled,Refund };
public struct item
{
    public int itemId { get; set; }
    public string itemName { get; set; }
    public string flavor { get; set; }
    public string size { get; set; }
    public decimal price { get; set; }
};

public struct customer
{
    public string customerId { get; set; }
    public int QuantityOfOrders { get; set; }
    public string name { get; set; }
    public string address { get; set; }
    public long phone { get; set; }
    public decimal total { get; set; }
};

public struct order
{
    public int orderId { get; set; }
    public string customerId { get; set; }
    public string batchId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public OrderStatus status { get; set; }
    //OrderId,CustomerId,BatchNo,ItemId, Quantity, Status
}
public class InsertValues : MonoBehaviour
{
    //Connection
    IDbConnection dbcon;
    static string connection;
    public GrabValues grab;
    private void Start()
    {
        connection = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
    }

    //Adds 1 Item to the Shelf
    //Returns -1 if Error, returns 1 if completed
    private int addToInventory(int itemId, string batchId)
    {
        int output = 0;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Inventory WHERE ItemId = @ItemId AND BatchId = @BatchId";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", itemId));
            dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchId));
            //If Item in that batch is found, increment the Quantity
            if (substitueDBNull(dbcmd.ExecuteScalar())>0)
            {
                dbcmd.CommandText = "UPDATE Inventory SET Quantity=Quantity+1 WHERE ItemId=@ItemId AND BatchId = @BatchId";
                output = dbcmd.ExecuteNonQuery();
            }//else add into inventory and add description
            else
            {
                dbcmd.CommandText = "Select ItemName, Flavor, Size FROM Items WHERE ItemId = @ItemId";
                IDataReader reader = dbcmd.ExecuteReader();
                string desc;
                reader.Read();
                desc = (string)reader[0] + "<>" + (string)reader[1] + "<>" + (string)reader[2];
                reader.Close();
                dbcmd.CommandText = "INSERT INTO Inventory(ItemId,BatchId,Quantity,Description) VALUES(@ItemId,@BatchId,1,@Desc)";
                dbcmd.Parameters.Add(new SqliteParameter("@Desc", desc));
                output = dbcmd.ExecuteNonQuery();
            }
            
        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log(e.Message);
            Debug.Log("addToInventory from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    //Takes 1 Item from the shelf
    //Returns how many rows are affected, -2 if it can't find, or -1 if there is an error
    private int takeOneFromInventory(int itemId, out string batchId)
    {
        int output = 0;
        batchId = "B999";
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT SUM(Quantity) FROM Inventory WHERE ItemId = @ItemId";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", itemId));
            long count = substitueDBNull(dbcmd.ExecuteScalar());
            //If ItemId has multiple in one BATCH
            if (count > 1)//Then get that batch item, and update that inventory at that batch
            {
                dbcmd.CommandText = "SELECT BatchId FROM Inventory WHERE ItemId=@ItemId";
                IDataReader reader = dbcmd.ExecuteReader();
                reader.Read();
                batchId = (string)reader["BatchId"];
                reader.Close();
                dbcmd.CommandText = "UPDATE Inventory SET Quantity=Quantity-1 WHERE ItemId=@ItemId AND BatchId = @BatchId";
                dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchId));
                output = dbcmd.ExecuteNonQuery();
            }
            else if(count==1)//Else if there is only one, delete from inventory where that batch is
            {
                dbcmd.CommandText = "SELECT BatchId FROM Inventory WHERE ItemId=@ItemId";
                batchId = (string)dbcmd.ExecuteScalar();
                dbcmd.CommandText = "DELETE FROM Inventory WHERE ItemId=@ItemId AND BatchId= @BatchId";
                dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchId));
                output = dbcmd.ExecuteNonQuery();
            }
            else//Else return -2
            {
                output = -2;
            }

        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log(e.Message);
            Debug.Log("takeOneFromInventory from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    //Check to see if customer exists
    private bool checkCustomer(string customerId)
    {
        bool successful = false;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Customers WHERE CustomerId = @customerId";
            dbcmd.Parameters.Add(new SqliteParameter("@customerId", customerId));
            if (substitueDBNull(dbcmd.ExecuteScalar()) > 0)
            {
                successful = true;
            }
            else
            {
                successful = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("checkCustomer from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            successful = false;
        }
        finally
        {
            dbcon.Close();
        }
        return successful;
    }

    //Checks to see if an Order exists
    private bool checkOrder(string orderId)
    {
        bool successful = false;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Orders WHERE OrderId = @OrderId";
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", orderId));
            if (substitueDBNull(dbcmd.ExecuteScalar()) > 0)
            {
                successful = true;
            }
            else
            {
                successful = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("checkOrder from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            successful = false;
        }
        finally
        {
            dbcon.Close();
        }
        return successful;
    }

    //Check if Item Type exists
    private bool checkItem(string itemId)
    {
        bool successful = false;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemId = @ItemId";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", itemId));
            if (substitueDBNull(dbcmd.ExecuteScalar()) > 0)
            {
                successful = true;
            }
            else
            {
                successful = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("checkItem from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            successful = false;
        }
        finally
        {
            dbcon.Close();
        }
        return successful;
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
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", long.Parse(itemId)));
            output = substitueDBNull(dbcmd.ExecuteScalar());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("numOnShelf from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            output = -1;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }


    //Adds a type of Item
    //-1 if there is an error
    //-2 if the item exists already
    public int addItemType(item i)
    {
        int output = 0;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            if (i.itemName.Length <= 0)
                throw new System.Exception("No Item Name");
            if (i.price <= 0)
                throw new System.Exception("No Item Price");
            IDbCommand dbcmd = dbcon.CreateCommand();

            SqliteParameter name = new SqliteParameter("@ItemName", i.itemName);
            dbcmd.Parameters.Add(name);

            SqliteParameter flavor = new SqliteParameter("@Flavor", i.flavor);
            dbcmd.Parameters.Add(flavor);

            SqliteParameter size = new SqliteParameter("@Size", i.size);
            dbcmd.Parameters.Add(size);

            SqliteParameter price = new SqliteParameter("@Price", i.price);
            dbcmd.Parameters.Add(price);
            dbcmd.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemName = @ItemName AND Size=@Size AND Flavor=@Flavor AND Price=@Price";
            if (substitueDBNull(dbcmd.ExecuteScalar()) > 0)//If the item is there, return -2
            {
                output = -2;
                Debug.Log("There is an existing Item with the same name, size, flavor, and price");
            }
            else//Else add item type
            {
                dbcmd.CommandText = "SELECT MAX(ItemId) FROM Items";

                long id = substitueDBNull(dbcmd.ExecuteScalar())+1;
                dbcmd.CommandText = "INSERT INTO Items(ItemId,ItemName, Flavor, Size, Price) VALUES(@ItemId, @ItemName,@Flavor,@Size,@Price)";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", id));
                output = dbcmd.ExecuteNonQuery();
            }
        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log(e.Message);
            Debug.Log("addItemType from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    public long substitueDBNull(object db)
    {
        if (db.GetType().ToString() == "System.DBNull")
            return 0;
        else
            return (long)db;
    }

    //Adds a new Batch and items to that batch
    //Returns -1 if there is an error, or the output from addToBatch
    public long addNewBatch(item[] items)
    {
        long output = 0;
        long size = 0;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Batches";
            size = substitueDBNull(dbcmd.ExecuteScalar())+1;
        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log("addNewBatch from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            Debug.Log(e.Message);
        }
        finally
        {
            dbcon.Close();
        }
        if (output == -1)
            return output;

        string num = "B";
        if (size < 1000)
            num += "0";
        if (size < 100)
            num += "0";
        if (size < 10)
            num += "0";
        num += size;
        output = addToBatch(num, items);
        return output;
    }

    //Adds items to a batch and to Inventory
    //Returns how many rows are affected
    //Or -1 if there is an error
    public long addToBatch(string batchId, item[] items)
    {
        long output = 0;
        try
        {
            IDbCommand dbcmd;
            foreach (item i in items)
            {
                dbcon = new SqliteConnection(connection);
                long itemId = -1;
                dbcon.Open();
                dbcmd = dbcon.CreateCommand();
                dbcmd.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemName=@ItemName AND Flavor=@Flavor AND Size=@Size AND PRICE=@PRICE";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemName", i.itemName));
                dbcmd.Parameters.Add(new SqliteParameter("@Flavor", i.flavor));
                dbcmd.Parameters.Add(new SqliteParameter("@Size", i.size));
                dbcmd.Parameters.Add(new SqliteParameter("@PRICE", i.price));
                if (substitueDBNull(dbcmd.ExecuteScalar()) < 1)//Adds new Item type if need be
                {
                    dbcon.Close();
                    addItemType(i);
                    dbcon.Open();
                    dbcmd = dbcon.CreateCommand();
                    dbcmd.Parameters.Add(new SqliteParameter("@ItemName", i.itemName));
                    dbcmd.Parameters.Add(new SqliteParameter("@Flavor", i.flavor));
                    dbcmd.Parameters.Add(new SqliteParameter("@Size", i.size));
                    dbcmd.Parameters.Add(new SqliteParameter("@PRICE", i.price));
                }
                dbcmd.CommandText = "SELECT ItemId FROM Items WHERE ItemName=@ItemName AND Flavor=@Flavor AND Size=@Size AND PRICE = @PRICE";

                itemId = substitueDBNull(dbcmd.ExecuteScalar());
                dbcmd.CommandText = "SELECT COUNT(*) FROM Batches WHERE ItemId=@ItemId AND BatchId=@BatchId";
                dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchId));
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", itemId));
                long count = substitueDBNull(dbcmd.ExecuteScalar());
                if (count > 0)//Adds to quantity in batch if Item is there
                {
                    dbcmd.CommandText = "UPDATE Batches SET Quantity=Quantity+1 WHERE ItemId=@ItemId AND BatchId=@BatchId";
                }
                else//Adds Item to batch if it is not
                {
                    dbcmd.CommandText = "INSERT INTO Batches(BatchId,ItemId,Quantity,Description) VALUES(@BatchId,@ItemId,1,@Desc)";
                    dbcmd.Parameters.Add(new SqliteParameter("@Desc", i.itemName + "<>" + i.flavor + "<>" + i.size));
                }
                output += (long)dbcmd.ExecuteNonQuery();
                dbcon.Close();
                if (itemId!=-1)
                    addToInventory((int)itemId, batchId);
            }
        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log("addToBatch from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            Debug.Log(e.Message);
        }
        return output;
    }

    //Adds a new customer
    //Returns -2 if Customer Exists
    //Returns -1 if there is an error
    //Else returns 1 if completed
    public int addCustomer(customer c)
    {
        int output = 0;
        string newId = "C";
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            if (c.customerId != "C9999")
            {
                dbcmd.CommandText = "SELECT COUNT(*) FROM Customers";
                long size = substitueDBNull(dbcmd.ExecuteScalar());
                if (size < 1000)
                    newId += "0";
                if (size < 100)
                    newId += "0";
                if (size < 10)
                    newId += "0";
                newId += (size + 1);
                c.customerId = newId;
            }
                dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", c.customerId));
                dbcmd.CommandText = "INSERT INTO Customers(CustomerId,QuantityOfOrders,Name,Address,Phone,Total)Values(@CustomerId,@Quantity,@Name,@Address,@Phone,@Total)";
                dbcmd.Parameters.Add(new SqliteParameter("@Quantity", 0));
                dbcmd.Parameters.Add(new SqliteParameter("@Name", c.name));
                dbcmd.Parameters.Add(new SqliteParameter("@Address", c.address));
                dbcmd.Parameters.Add(new SqliteParameter("@Phone", c.phone));
                dbcmd.Parameters.Add(new SqliteParameter("@Total", 0));
                output = dbcmd.ExecuteNonQuery();
        }
        catch (System.Exception e)
        {
            output = -1;
            Debug.Log(e.Message);
            Debug.Log("addCustomer from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    //!!!TODO!!!
    //Add Quantity of Orders and Spent to customer


    //Creates a new order with 1 item and returns it
    //Returns null order if it didn't work
    public order addOrder(customer c, item i,int quantity)
    {
        if (!checkCustomer(c.customerId))//If customer doesn't exist, add them
        {
            if(addCustomer(c)==-1)//If it is Unsuccessful
            {
                return new order();
            }
        }
            //Create new unfilled order
            order newestOrder = new order();
            newestOrder.ItemId = i.itemId;
            newestOrder.customerId = c.customerId;
            newestOrder.Quantity = quantity;
            newestOrder.status = OrderStatus.Unfilled;
            newestOrder.batchId = "B9999";
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //Give a new OrderId
            dbcmd.CommandText = "SELECT MAX(OrderId) FROM Orders";
            newestOrder.orderId = (int)substitueDBNull(dbcmd.ExecuteScalar())+1;
            //Insert new Order
            dbcmd.CommandText = "SELECT Price FROM Items WHERE ItemId=@ItemId";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", newestOrder.ItemId));
            decimal price = (decimal)dbcmd.ExecuteScalar();
            price *= newestOrder.Quantity;
            dbcmd.CommandText = "INSERT INTO Orders(OrderId,BatchId, ItemId, CustomerId, Quantity, Total, STATUS) VALUES(@OrderId,@Batch,@ItemId,@CustomerId,@Quantity,@Price, 'Unfilled')";
            dbcmd.Parameters.Add(new SqliteParameter("@Quantity", newestOrder.Quantity));
            dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", newestOrder.customerId));
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", newestOrder.orderId));
            dbcmd.Parameters.Add(new SqliteParameter("@Price", price));
            dbcmd.Parameters.Add(new SqliteParameter("@Batch", "B9999"));
            dbcmd.Parameters.Add(new SqliteParameter("@Status", "Unfilled"));
            if ((int)(dbcmd.ExecuteNonQuery())<1)//If it is Unsuccessful
            {
                newestOrder = new order();
            }
            else//Update OPS (Order, Price, Status)
            {
                dbcmd.CommandText = "INSERT INTO OPS(OrderId,Total,STATUS)VALUES(@OrderId,(SELECT SUM(Total) FROM Orders WHERE OrderId = @OrderId),'Unfilled')";
                dbcmd.ExecuteNonQuery();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("addOrder from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            newestOrder = new order();
        }
        finally
        {
            dbcon.Close();
        }
        return newestOrder;
    }

    //Returns a null order if unsuccessful, else returns a new unfilled order item
    public order addToOrder(order o, item i, int quantity)
    {
        if (!checkOrder(o.orderId.ToString()))//check to see if OrderId Exists
        {
            return new order();
        }
        //Create new Order with new itemId and Quantity
        order newestOrder = o;
        newestOrder.ItemId = i.itemId;
        newestOrder.customerId = o.customerId;
        newestOrder.Quantity = quantity;
        newestOrder.status = OrderStatus.Unfilled;
        newestOrder.batchId = "B9999";
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //Insert new Order Item
            dbcmd.CommandText = "SELECT COUNT(*) FROM Orders WHERE OrderId=@OrderId AND ItemId=@ItemId AND BatchId = 'B9999'";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", newestOrder.ItemId));
            dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", newestOrder.customerId));
            dbcmd.Parameters.Add(new SqliteParameter("@Quantity", newestOrder.Quantity));
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", newestOrder.orderId));
            if (substitueDBNull(dbcmd.ExecuteScalar()) < 1)
                dbcmd.CommandText = "INSERT INTO Orders(OrderId,BatchId, ItemId, CustomerId, Quantity, Total, STATUS) VALUES(@OrderId,'B9999',@ItemId,@CustomerId,@Quantity,((SELECT Price FROM Items WHERE ItemId=@ItemId)*@Quantity), 'Unfilled')";
            else
            {
                dbcmd.CommandText = "SELECT Quantity, CustomerId FROM Orders WHERE OrderId=@OrderId AND ItemId=@ItemId AND BatchId='B9999'";
                IDataReader reader = dbcmd.ExecuteReader();
                reader.Read();
                dbcmd.Parameters.Add(new SqliteParameter("@QuantityQ", (substitueDBNull(reader["Quantity"]) + newestOrder.Quantity)));
                if ((string)reader["CustomerId"] == "C9999")
                {
                    reader.Close();
                    dbcmd.CommandText = "UPDATE Orders SET CustomerId=@CustomerId WHERE  OrderId=@OrderId";
                    dbcmd.ExecuteNonQuery();
                }
                else
                {
                    reader.Close();
                }
                dbcmd.CommandText = "UPDATE Orders SET Quantity=@QuantityQ WHERE  OrderId=@OrderId AND ItemId=@ItemId AND BatchId='B9999'";
            }
            if (dbcmd.ExecuteNonQuery() < 1)
            {
                newestOrder = new order();
            }
            else//Update OPS (OrderId, Price, Status)
            {
                dbcmd.CommandText = "UPDATE OPS SET Total = (SELECT SUM(Total) FROM Orders WHERE OrderId = @OrderId), STATUS = 'Unfilled' WHERE OrderId = @OrderId";
                dbcmd.ExecuteNonQuery();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("addToOrder from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            newestOrder = new order();
        }
        finally
        {
            dbcon.Close();
        }
        return newestOrder;
    }

    //Returns true if Successful. Fills Order Items, and whole Order if need be
    public bool fillOrder(order o, item i)
    {
        bool output = false;
        //Create a dictionary in case there are multiple batches needed
        Dictionary<string, int> batches = new Dictionary<string, int>();
        string[] batchNames = new string[o.Quantity];
        if (numOnShelf(i.itemId.ToString()) < o.Quantity)//If we don't have enough items, return false
        {
            return false;
        }
        if (!checkOrder(o.orderId.ToString()))//If the order doesn't exist, return false
        {
            return false;
        }
        for (int index = 0; index < o.Quantity; index++)//For every item in the order, take off shelf and record batch
        {
            string batchNum;
            takeOneFromInventory(i.itemId, out batchNum);
            if (batches.ContainsKey(batchNum))
            {
                batches[batchNum]++;
            }
            else
            {
                batchNames[batches.Count] = batchNum;
                batches[batchNum] = 1;
            }
        }
        if (batches.Count == 1)//If there is only one batch, set item order status to filled
        {
            try
            {
                dbcon = new SqliteConnection(connection);
                dbcon.Open();
                IDbCommand dbcmd = dbcon.CreateCommand();
                dbcmd.CommandText = "UPDATE Orders SET STATUS = 'Filled', BatchId = @BatchId WHERE OrderId = @OrderId AND ItemId = @ItemId";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", i.itemId));
                dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchNames[0]));
                dbcmd.Parameters.Add(new SqliteParameter("@OrderId", o.orderId));
                dbcmd.ExecuteNonQuery();
                output = true;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("fillOrder from InsertValues failed");
                this.gameObject.AddComponent<ExceptionPopUp>().Start();
                output = false;
            }
            finally
            {
                dbcon.Close();
            }
        }
        else//If more than one batch, delete item order where there is no batch
        {
            try
            {
                dbcon = new SqliteConnection(connection);
                dbcon.Open();
                IDbCommand dbcmd = dbcon.CreateCommand();
                dbcmd.CommandText = "DELETE FROM Orders WHERE BatchId = B999 AND OrderId = @OrderId AND ItemId = @ItemId";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", i.itemId));
                dbcmd.Parameters.Add(new SqliteParameter("@OrderId", o.orderId));
                dbcmd.ExecuteNonQuery();

                for (int index = 0; index < batches.Count; index++)//Insert all batches needed
                {
                    dbcmd.CommandText = "INSERT INTO Orders(OrderId,BatchId, ItemId, CustomerId, Quantity, Total, STATUS) VALUES(@OrderId,@BatchId,@ItemId,@CustomerId,@Quantity,(SELECT Price FROM Items WHERE ItemId=@ItemId)*@Quantity,'Filled'";
                    dbcmd.Parameters.Add(new SqliteParameter("@ItemId", i.itemId));
                    dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", o.customerId));
                    dbcmd.Parameters.Add(new SqliteParameter("@Quantity", batches[batchNames[index]]));
                    dbcmd.Parameters.Add(new SqliteParameter("@OrderId", o.orderId));
                    dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchNames[index]));
                }
                output = true;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("fillOrder multiBatches from InsertValues failed");
                this.gameObject.AddComponent<ExceptionPopUp>().Start();
                output = false;
            }
            finally
            {
                dbcon.Close();
            }
        }
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();

            for (int index = 0; index < batches.Count; index++)//Insert every Item and coresponding batchId
            {
                dbcmd.CommandText = "INSERT INTO Orders(OrderId,BatchId, ItemId, CustomerId, Quantity, Total, STATUS) VALUES(@OrderId,@BatchId,@ItemId,@CustomerId,@Quantity,(SELECT Price FROM Items WHERE ItemId=@ItemId)*@Quantity,'Filled'";
                dbcmd.Parameters.Add(new SqliteParameter("@ItemId", o.ItemId));
                dbcmd.Parameters.Add(new SqliteParameter("@CustomerId", o.customerId));
                dbcmd.Parameters.Add(new SqliteParameter("@Quantity", batches[batchNames[index]]));
                dbcmd.Parameters.Add(new SqliteParameter("@OrderId", o.orderId));
                dbcmd.Parameters.Add(new SqliteParameter("@BatchId", batchNames[index]));
            }

            dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Orders WHERE OrderId = @OrderId AND STATUS = 'Unfilled'";
            dbcmd.Parameters.Add(new SqliteParameter("@ItemId", o.ItemId));
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", o.orderId));
            if (substitueDBNull(dbcmd.ExecuteScalar()) == 0)//If the whole order is filled, update OPS
            {
                dbcmd.CommandText = "UPDATE OPS SET STATUS = 'Ordered' WHERE OrderId = @OrderId";
                dbcmd.ExecuteNonQuery();
            }
            output = true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("fillOrder multiBatches 2 from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            output = false;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    //Returns true if Successful. Puts items back on shelf and changes order to Cancelled
    public bool cancelOrder(int orderId)
    {
        bool output = false;
        if (!checkOrder(orderId.ToString()))//If the order doesn't exist, return false
        {
            return false;
        }

        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT STATUS FROM OPS WHERE OrderId = @OrderId";
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", orderId));
            IDataReader reader = dbcmd.ExecuteReader();
            reader.Read();
            if ((string)reader["STATUS"] == "Cancelled" || (string)reader["STATUS"]=="Unfilled")
            {
                reader.Close();
                return false;
            }
            reader.Close();
            //For Each Order Item, put it back on the shelf and include Quantity, ItemId, and BatchId
            dbcmd.CommandText = "SELECT Quantity, ItemId, BatchId FROM Orders WHERE OrderId = @OrderId";
            reader = dbcmd.ExecuteReader();
            int[] quantities = new int[reader.GetSchemaTable().Rows.Count];
            int[] itemIds = new int[reader.GetSchemaTable().Rows.Count];
            string[] batchIds = new string[reader.GetSchemaTable().Rows.Count];
            int index = 0;
            while (reader.Read())
            {
                quantities[index] = (int)(long)reader[0];
                itemIds[index] = (int)(long)reader[1];
                batchIds[index] = (string)reader[2];
                index++;
            }
            reader.Close();
            dbcon.Close();
            for (int i = 0; i < index; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                {
                    addToInventory(itemIds[i], batchIds[i]);
                }
            }
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            dbcmd = dbcon.CreateCommand();
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", orderId));
            //Cancel Order
            dbcmd.CommandText = "UPDATE OPS SET STATUS = 'Cancelled' WHERE OrderId = @OrderId";
            dbcmd.ExecuteNonQuery();

            dbcmd.CommandText = "UPDATE Orders SET BatchId = 'B9999', STATUS = 'Cancelled' WHERE OrderId = @OrderId";
            dbcmd.ExecuteNonQuery();
            output = true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("cancelOrder from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            output = false;
        }
        finally
        {
            dbcon.Close();
        }
        return output;
    }

    private string stringStatus(OrderStatus status)
    {
        switch(status)
        {
            case OrderStatus.Unfilled:
                return "Unfilled";
            case OrderStatus.Ordered:
                return "Ordered";
            case OrderStatus.Paid:
                return "Paid";
            case OrderStatus.Delivered:
                return "Delivered";
            case OrderStatus.Cancelled:
                return "Cancelled";
            case OrderStatus.Refund:
                return "Refunded";
            default:
                return "Unfilled";
        }
    }

    public void updateStatus(OrderStatus status, string orderId)
    {
        try
        {
            if ((int)status == 4)
            {

                cancelOrder(int.Parse(orderId));
            }
                dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            //ItemId, ItemName, Flavor, Size, PRICE
            dbcmd.CommandText = "UPDATE OPS SET STATUS = @Status WHERE OrderId = @OrderId";
            dbcmd.Parameters.Add(new SqliteParameter("@Status", stringStatus(status)));
            dbcmd.Parameters.Add(new SqliteParameter("@OrderId", orderId));
            dbcmd.ExecuteNonQuery();
            if ((int)status==0)
            {
                dbcmd.CommandText = "UPDATE Orders SET STATUS = 'Unfilled' WHERE OrderId = @OrderId";
                dbcmd.ExecuteNonQuery();
            }

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("updateStatus from InsertValues failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        finally
        {
            dbcon.Close();
        }
    }


    
}
