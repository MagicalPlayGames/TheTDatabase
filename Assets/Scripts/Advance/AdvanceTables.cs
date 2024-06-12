using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

[System.Serializable]
public enum CurrentDisplayTable {Items,Inventory,Batches, Orders, Customers};
public class AdvanceTables : MonoBehaviour
{
    public CurrentDisplayTable tableEnum;
    public GrabValues grabBag;

    [System.Serializable]
    public struct itemRows
    {
        //5 fields
        public Text[] fields;
        public Text this[int x]
        {
            get
            {
                return fields[x];
            }
            set
            {
                fields[x] = value;
            }
        }
    }

    [System.Serializable]
    public struct itemUpdateRow
    {
        public Button updateButton;
        //4 fields
        public InputField[] fields;
        public InputField this[int x]
        {
            get
            {
                return fields[x];
            }
            set
            {
                fields[x] = value;
            }
        }
    }

    [System.Serializable]
    public struct displayTable
    {
        public string tableInfo;
        public int startRow;
        public string[] items;
        public GameObject tableObj;
        //1 updateRow
        public itemUpdateRow updateRow;

        //7 itemRows
        public itemRows[] rows;
        public itemRows this[int x]
        {
            get
            {
                return rows[x];
            }
            set
            {
                rows[x] = value;
            }
        }

        public void SetActive(bool active)
        {
            tableObj.SetActive(active);
        }

        public void split()
        {
            int fieldLength = rows[0].fields.Length;
            for (int i = startRow; i < rows.Length+startRow; i ++)
            {
                int w = i * fieldLength;
                for(int j = 0;j<fieldLength; j++)
                {
                    if (w + j >= items.Length)
                        return;
                    rows[i-startRow][j].text = items[w + j];
                }
            }
        }
        public void increaseRows()
        {
            if (startRow >= (items.Length/rows[0].fields.Length)-rows.Length)
                return;
            startRow++;
        }
        public void decreaseRows()
        {
            if (startRow <= 0)
                return;
            startRow--;
        }
    }

    //Display Tables
    public displayTable itemTable;
    public displayTable inventoryTable;
    public displayTable batchTable;
    public displayTable orderTable;
    public displayTable customerTable;

    private displayTable selectedTable;

    public void goUp()
    {
        displayTable t = getTable(tableEnum);
        t.increaseRows();
        t.split();
        setTable(t, tableEnum);

    }

    public void goDown()
    {
        displayTable t = getTable(tableEnum);
        t.decreaseRows();
        t.split();
        setTable(t, tableEnum);
    }

    private IDataReader getReader(CurrentDisplayTable tableSelection)
    {
        switch (tableSelection)
        {
            case CurrentDisplayTable.Items:
                return grabBag.getItemReader();
            case CurrentDisplayTable.Inventory:
                return grabBag.getInventoryReader();
            case CurrentDisplayTable.Batches:
                return grabBag.getBatchReader();
            case CurrentDisplayTable.Orders:
                return grabBag.getOrderItemsReader();
            case CurrentDisplayTable.Customers:
                return grabBag.getCustomerReader();
            default:
                return grabBag.getCustomerReader();
        }
    }

    private displayTable getTable(CurrentDisplayTable tableSelection)
    {
        switch (tableSelection)
        {
            case CurrentDisplayTable.Items:
                return itemTable;
            case CurrentDisplayTable.Inventory:
                return inventoryTable;
            case CurrentDisplayTable.Batches:
                return batchTable;
            case CurrentDisplayTable.Orders:
                return orderTable;
            case CurrentDisplayTable.Customers:
                return customerTable;
            default:
                return itemTable;
        }
    }

    private void setTable(displayTable table, CurrentDisplayTable tableSelection)
    {
        switch (tableSelection)
        {
            case CurrentDisplayTable.Items:
                itemTable = table;
                break;
            case CurrentDisplayTable.Inventory:
                inventoryTable = table;
                break;
            case CurrentDisplayTable.Batches:
                batchTable = table;
                break;
            case CurrentDisplayTable.Orders:
                orderTable = table;
                break;
            case CurrentDisplayTable.Customers:
                customerTable = table;
                break;
        }
    }
    //Shows table depedent on enum CurrentDisplayTable
    public void generateTable(CurrentDisplayTable tableSelection)
    {
        tableEnum = tableSelection;
        if ((int)tableSelection > 4)
        {
            Debug.Log("Other Table");
                return;
        }
        displayTable table = getTable(tableSelection);
        table.SetActive(true);
        IDataReader reader = getReader(tableSelection);
        table.tableInfo = "";
        int max = table[0].fields.Length;
        while (reader.Read())
        {
            for (int i = 0; i < max; i++)
            {
                table.tableInfo += reader.GetValue(i).ToString() + "?";
            }
        }
        reader.Close();
        table.items = table.tableInfo.Split("?");
        table.split();
        setTable(table, tableSelection);
    }


    //Hides All Tables
    public void hideAllTables()
    {
    itemTable.SetActive(false);
    inventoryTable.SetActive(false);
    batchTable.SetActive(false);
    orderTable.SetActive(false);
    customerTable.SetActive(false);
    }

    //USE TO SHOW ONE TABLE AND HIDE OTEHR TABLES AT ONCE
    public void genTable(int tS)
    {
        hideAllTables();
        generateTable((CurrentDisplayTable)tS);
    }
}
