using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;

public class IntermediateTable : MonoBehaviour
{
    private int start = 0;
    [System.Serializable]
    public struct fieldChart
    {
        public Text[] column;

        public Text this[int x]
        {
            get
            {
                return column[x];
            }
            set
            {
                column[x] = value;
            }
        }
    }

    [System.Serializable]
    public struct table
    {
        public string tableInfo;
        public GameObject tableObj;
        public Text[] rows;
        public int rowCount;
        public int colCount;
    }
    public GrabValues grabBag;
    public table[] tables;
    int tableChoice = 0;
    private string[] ItemArr;
    // Start is called before the first frame update
    public void goUp()
    {
        if (start <= 0)
            start = 1;
        start--;
        generateTable(tableChoice);
    }
    public void goDown()
    {
        if (start >= (ItemArr.Length / tables[tableChoice].rowCount) - 7)
            start = ItemArr.Length / tables[tableChoice].rowCount - 8;
        start++;
        generateTable(tableChoice);
    }
    public void displayTable(int i)
    {
        start = 0;
        for(int j =0;j<5;j++)
        {
            if(j==i)
            {
                tables[i].tableObj.SetActive(true);
            }
            else if (tables[j].tableObj!=null)
            {
                tables[j].tableObj.SetActive(false);
            }
        }
        generateTable(i);
    }

    public void generateTable()
    {
        generateTable(tableChoice);
    }

    private IDataReader getReader(int tableChoice)
    {
        this.tableChoice = tableChoice;
        switch(tableChoice)
        {
            case 0:
                return grabBag.getCustomerReader();
            case 1:
                return grabBag.getItemReader();
            case 2:
                return grabBag.getInventoryReader();
            case 3:
                return grabBag.getOrderItemsReader();
            case 4:
                return grabBag.getBatchReader();
            default:
                return grabBag.getCustomerReader();
        }
    }
    public void generateTable(int tableChoice)
    {
        table curTable = tables[tableChoice];
        curTable.tableInfo = "";
        IDataReader reader = getReader(tableChoice);
        while (reader.Read())
        {
            for (int i = 0; i < curTable.colCount; i++)
            {
                curTable.tableInfo += reader.GetValue(i).ToString() + "?";
            }
        }
        reader.Close();
        ItemArr = curTable.tableInfo.Split("?");
        for (int j = start *curTable.colCount; j < ItemArr.Length-1 && j-(start*curTable.colCount) <curTable.rows.Length; j += curTable.colCount)
        {
            for (int i = 0; i < curTable.colCount; i++)
            {
                curTable.rows[j-(start*curTable.colCount) +i].text = ItemArr[i + j];
            }
        }
    }
}
