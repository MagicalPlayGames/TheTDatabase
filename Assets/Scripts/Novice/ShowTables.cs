using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;

public class ShowTables : MonoBehaviour
{
    public GrabValues grabBag;
    public Text CustomerTable;
    public Text ItemTable;
    public Text InventoryTable;
    public Text OrderTable;
    public Text BatchTable;
    // Start is called before the first frame update
    public void generate()
    {
        IDataReader CustomerReader = grabBag.getCustomerReader();
        CustomerTable.text = "CustomerId \t Quantity of Orders Name \t Address \t Phone \t Total\n";
        ItemTable.text = "ItemId \t ItemName \t Flavor \t Size \t Price\n";
        InventoryTable.text = "ItemId \t BatchId \t Quantity \t Description\n";
        OrderTable.text = "OrderId \t BatchId \t ItemId \t CustomerId \t Total \t Status \t OrderId \t Total \t Status\n";
        BatchTable.text = "BatchId \t ItemId \t Quantity \t Descriptiopn \n";
        while (CustomerReader.Read())
        {
            for(int i =0;i<CustomerReader.FieldCount;i++)
            {
                CustomerTable.text += CustomerReader.GetValue(i).ToString() + " ";
            }
            CustomerTable.text += "\n";
        }
        CustomerReader.Close();

        IDataReader ItemReader = grabBag.getItemReader();
        while (ItemReader.Read())
        {
            for (int i = 0; i < ItemReader.FieldCount; i++)
            {
                ItemTable.text += ItemReader.GetValue(i).ToString() + " ";
            }
            ItemTable.text += "\n";
        }
        ItemReader.Close();

        IDataReader InventoryReader = grabBag.getInventoryReader();
        while (InventoryReader.Read())
        {
            for (int i = 0; i < InventoryReader.FieldCount; i++)
            {
                InventoryTable.text += InventoryReader.GetValue(i).ToString() + " ";
            }
            InventoryTable.text += "\n";
        }
        InventoryReader.Close();

        IDataReader OrderReader = grabBag.getOrderItemsReader();
        while (OrderReader.Read())
        {
            for (int i = 0; i < OrderReader.FieldCount; i++)
            {
                OrderTable.text += OrderReader.GetValue(i).ToString() + " ";
            }
            OrderTable.text += "\n";
        }
        OrderReader.Close();

        IDataReader BatchReader = grabBag.getBatchReader();
        while (BatchReader.Read())
        {
            for (int i = 0; i < BatchReader.FieldCount; i++)
            {
                BatchTable.text += BatchReader.GetValue(i).ToString() + " ";
            }
            BatchTable.text += "\n";
        }
        BatchReader.Close();
    }
}
