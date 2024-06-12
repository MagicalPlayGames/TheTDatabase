using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UI;

public struct UpdateForm
{
    public Dropdown[][] dropLists;
    public int[] quantities;
}

public class AdvUpdateTables : MonoBehaviour
{
    public GrabValues grabBag;
    public InsertValues insertion;
    
    //Inventory Update Table
    public InvUpdateOCreateTable InventoryUpdateTable;

    //Fill Item Descrptions and Batches
    public void getIUList()
    {
        InventoryUpdateTable.showTable();
        List<string> itemList = new List<string>();
        List<string> batchList = new List<string>();
        IDataReader IUReader = grabBag.getInventoryReader();
        while(IUReader.Read())
        {
            string batchId = (string)IUReader["BatchId"];
            if (!batchList.Contains(batchId))
                batchList.Add(batchId);
        }
        IUReader.Close();
        IDataReader ITReader = grabBag.getItemReader();
        while(ITReader.Read())
        {
            string desc = (string)ITReader["ItemName"] + " " + (string)ITReader["Flavor"] + " " + (string)ITReader["Size"] + " " + (decimal)ITReader["Price"];
            if (!itemList.Contains(desc))
                itemList.Add(desc);
        }
        ITReader.Close();
        InventoryUpdateTable.fillList1(itemList);
        InventoryUpdateTable.fillList2(batchList);
        for (int i = 0; i < InventoryUpdateTable.quantities.Length;i++)
        {
            InventoryUpdateTable.setQuantities(i, 0);
        }
    }

    //Adds a new Item to inventory and batch if appropriate
    public void getInfo()
    {
        for (int i = 0; i < InventoryUpdateTable.dropList1.Length; i++)
        {
            string resultItem = InventoryUpdateTable.dropList1[i].value.ToString();
            string resultBatch = InventoryUpdateTable.dropList2[i].options[InventoryUpdateTable.dropList2[i].value].text;
            int quantity = InventoryUpdateTable.quantities[i];
            if (resultItem == "0")//No ItemId
            {
                continue;
            }
            else if(quantity==0)//ItemId with no Quantity
            {
                Debug.Log("Add Inventory Failed: 0 Quantity");
                this.gameObject.AddComponent<ExceptionPopUp>().Start();
                continue;
            }
            else if (resultBatch == "None")//ItemId with Quantity but no batch number
            {
                addBatchItem(resultItem, "");
                IDataReader reader = grabBag.getBatchReader();
                while(reader.Read())
                {
                    resultBatch = (string)reader["BatchId"];
                }
                reader.Close();
                for (int j = 1; j < quantity; j++)
                    addBatchItem(resultItem, resultBatch);
            }
            else
                for (int j = 0; j < quantity; j++)
                    addBatchItem(resultItem, resultBatch);

        }
    }
    public void addBatchItem(string itemText, string batchText)
    {
        if (batchText.Length > 0)
        {
            AddToBatch(batchText, itemText);
        }
        else
        {
            AddNewBatch(itemText);
        }
    }

    //Adds new Inventory Item to a new batch
    private void AddNewBatch(string itemId)
    {
        item i = grabBag.getItemById(int.Parse(itemId));
        item[] items = new item[1];
        items[0] = i;
        insertion.addNewBatch(items);
    }

    //Adds new Inventory Item to an exisiting batch
    private void AddToBatch(string batchId, string itemId)
    {
        item i = grabBag.getItemById(int.Parse(itemId));
        item[] items = new item[1];
        items[0] = i;
        string bId = batchId;
        if (!batchId.Contains("B"))
        {
            bId = "B";
            int parsedId = int.Parse(batchId);
            if (parsedId < 1000)
                bId += "0";
            if (parsedId < 100)
                bId += "0";
            if (parsedId < 10)
                bId += "0";
            bId += batchId;
        }
        insertion.addToBatch(bId, items);
    }

    //Change Quantities in Inventory Update Table
    public void upQuantity(int index)
    {
        InventoryUpdateTable.setQuantities(index, 1);
    }

    public void lowerQuantity(int index)
    {
        InventoryUpdateTable.setQuantities(index, -1);
    }
  
    //Order Fill and Status Update Table
    public OrderFillUpdateTable OrderFillTable;
    Dictionary<string, List<string>> orderItemList;
    Dictionary<string, List<string>> orderStatusList;
    Dictionary<string, int> orderQuantityList;
    
    //Fill order itmes ids and statuses
    //Only Unfilled Items will show in top of the table
    public void getOFList()
    {
        OrderFillTable.showTable();
        orderItemList = new Dictionary<string, List<string>>();
        orderStatusList = new Dictionary<string, List<string>>();
        orderQuantityList = new Dictionary<string, int>();

        IDataReader IUReader = grabBag.getOrderItemsReader();
        while (IUReader.Read())
        {
                string oId = ((long)IUReader["OrderId"]).ToString();
                string iId = ((long)IUReader["ItemId"]).ToString();
                long quantity = (long)IUReader["Quantity"];
            if (!orderStatusList.ContainsKey(oId))
            {
                orderStatusList.Add(oId, new List<string> { iId });
            }
            else
            {
                List<string> tempList = orderStatusList[oId];
                tempList.Add(iId);
                orderStatusList[oId] = tempList;
            }

            if ((string)IUReader["STATUS"] == "Unfilled")
            {
                if (!orderItemList.ContainsKey(oId))
                {
                    orderItemList.Add(oId, new List<string> { iId });
                    orderQuantityList.Add(oId + " " + iId, (int)quantity);
                }
                else
                {
                    List<string> tempList = orderItemList[oId];
                    tempList.Add(iId);
                    orderItemList[oId] = tempList;
                    if (!orderQuantityList.ContainsKey(oId + " " + iId))
                        orderQuantityList[oId + " " + iId] = (int)quantity;
                    else
                        orderQuantityList[oId + " " + iId]++;
                }
            }
        }
        IUReader.Close();
        reloadOFOrderList();
    }

    //reload the orderIds
    public void reloadOFOrderList()
    {
        List<string> keys = new List<string>();
        foreach(string key in orderItemList.Keys)
        {
            keys.Add(key);
        }
        OrderFillTable.fillList1(keys);
    }
    //reload the items of the orderId
    public void reloadOFItemList(int j)
    {
        foreach(string key in orderItemList.Keys)
        {
            if(key==OrderFillTable.dropList1[j].options[OrderFillTable.dropList1[j].value].text)
            {
                OrderFillTable.dropList2[j].options.Clear();
                OrderFillTable.dropList2[j].options.Add(new Dropdown.OptionData("None"));
                foreach (string str in orderItemList[key])
                    OrderFillTable.dropList2[j].options.Add(new Dropdown.OptionData(str));
                reloadOFQuantityList(j);
            }
        }
    }

    //reload the Quantity of the Order Id and Item Id
    public void reloadOFQuantityList(int i)
    {
        string OValue = OrderFillTable.dropList1[i].options[OrderFillTable.dropList1[i].value].text;
        string IValue = OrderFillTable.dropList2[i].options[OrderFillTable.dropList2[i].value].text;
        if (OValue == "None" || IValue == "None")
            OrderFillTable.quantities[i].text = "N/A";
        else
            OrderFillTable.quantities[i].text = (orderQuantityList[OValue + " " + IValue].ToString());
    }

    //Fill Order by table values
    public void getOFInfo()
    {
        UpdateForm form = OrderFillTable.submitTable1();
        for (int j = 0; j < form.dropLists[0].Length; j++)
        {
            if (form.dropLists[0][j].options[form.dropLists[0][j].value].text == "None" || form.dropLists[0][j].value == 0)
                continue;
            if (form.dropLists[1][j].options.Count <= 0 || form.dropLists[1][j].options[form.dropLists[1][j].value].text == "None" || form.dropLists[1][j].value==0)
                continue;
            string OValue = form.dropLists[0][j].options[form.dropLists[0][j].value].text;
            string IValue = form.dropLists[1][j].options[form.dropLists[1][j].value].text;
            fill(OValue, IValue);
        }
    }

    //Fill Status Lists
    public void fillStatus()
    {

        List<string> keys = new List<string>();
        foreach (string key in orderStatusList.Keys)
        {
            keys.Add(key);
        }
        List<string> k = new List<string> {"Unfilled","Filled","Ordered","Paid","Delivered","Cancelled","Refunded"};
        OrderFillTable.fillList3(keys);
        OrderFillTable.fillList4(k);
    }


    //Updates the status
    public void updateStatus()
    {
        UpdateForm form = OrderFillTable.submitTable2();
        string OValue = form.dropLists[0][0].value.ToString();
        string SValue = form.dropLists[1][0].options[form.dropLists[1][0].value].text;
        if(SValue!="None" && OValue!="None" && OValue!="0")
            insertion.updateStatus(unstringStatus(SValue), OValue);
        else
        {
            Debug.Log("Update Order Failed: No Order Id, And Or No Status Selected");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
    }

    //For updateStatus()
    private OrderStatus unstringStatus(string status)
    {
        switch (status)
        {
            case "Unfilled":
                return OrderStatus.Unfilled;
            case "Ordered":
                return OrderStatus.Ordered;
            case "Paid":
                return OrderStatus.Paid;
            case "Delivered":
                return OrderStatus.Delivered;
            case "Cancelled":
                return OrderStatus.Cancelled;
            case "Refunded":
                return OrderStatus.Refund;
            default:
                return OrderStatus.Unfilled;
        }
    }
    //Fill an Order by orderId and ItemId
    private void fill(string orderText, string itemId)
    {
        IDataReader reader = grabBag.getOrderItemsById(orderText);
        while (reader.Read())
        {
            if ((long)reader["ItemId"] == int.Parse(itemId))
            {
                order o = new order();
                item i = new item();
                i.itemId = int.Parse(itemId);
                o.orderId = int.Parse(orderText);
                o.Quantity = (int)(long)reader["Quantity"];
                reader.Close();
                if(!insertion.fillOrder(o, i))
                {
                    Debug.Log("Fill Failed: Not Enough Items");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                }
                return;
            }
        }
        Debug.Log("Fill Failed");
        this.gameObject.AddComponent<ExceptionPopUp>().Start();
    }

    //Order Create Table
    public InvUpdateOCreateTable OrderCreateTable;
    public InputField[] OrderCID;
    List<string> itemList;
    List<string> itemNames;
    List<string> orderList;

    //Fill item Descriptions and Order Ids
    public void getOCTLists()
    {
        OrderCreateTable.showTable();
        itemList = new List<string>();
        itemNames = new List<string>();
        orderList = new List<string>();
        IDataReader ItemReader = grabBag.getItemReader();
        while(ItemReader.Read())
        {
            if (!itemList.Contains(((long)ItemReader["ItemId"]).ToString()))
            {
                string itemDesc = (string)ItemReader["ItemName"] + " " + (string)ItemReader["Flavor"] + " " + (string)ItemReader["Size"];
                itemList.Add(((long)ItemReader["ItemId"]).ToString());
                itemNames.Add(itemDesc);
            }
        }
        ItemReader.Close();
        IDataReader orderReader = grabBag.getOrderItemsReader();
        while(orderReader.Read())
        {
            if (!orderList.Contains(((long)orderReader["OrderId"]).ToString()))
            {
                orderList.Add(((long)orderReader["OrderId"]).ToString());
            }
        }
        orderReader.Close();
        OrderCreateTable.fillList1(itemNames);
        OrderCreateTable.fillList2(orderList);
        for(int i =0;i<OrderCreateTable.quantities.Length;i++)
            OrderCreateTable.setQuantities(i, 0);
    }

    //Submit Order
    public void submitAddToOrders()
    {
        UpdateForm form = OrderCreateTable.submitTable();
        Dropdown[] dropDown1 = form.dropLists[0];
        Dropdown[] dropDown2 = form.dropLists[1];
        for (int i =0;i<dropDown1.Length;i++)
        {
            string option1 = dropDown1[i].options[dropDown1[i].value].text;
            if (option1=="None")//No ItemId
            {
                continue;
            }
            string option2 = dropDown2[i].options[dropDown2[i].value].text;
            item item = grabBag.getItemById(int.Parse(itemList[dropDown1[i].value-1]));
            if (option2=="None" && OrderCID[i].text.Length<=0)//No OrderId or CustomerId
            {
                Debug.Log("Add Order Failed: No Order Id or Customer Id");
                this.gameObject.AddComponent<ExceptionPopUp>().Start();
                continue;
            }
            else if(option2!="None" && OrderCID[i].text.Length>0)//OrderId and CustomerId
            {
                if(form.quantities[i]<=0)
                {
                    Debug.Log("Add Order Failed:0 Quantity");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                    continue;
                }
                string cId = OrderCID[i].text;
                if (!OrderCID[i].text.Contains("C"))
                {
                    cId = "C";
                    int parsedId = int.Parse(OrderCID[i].text);
                    if (parsedId < 1000)
                        cId += "0";
                    if (parsedId < 100)
                        cId += "0";
                    if (parsedId < 10)
                        cId += "0";
                    cId += OrderCID[i].text;
                }
                order o = new order();
                o.customerId = cId;
                o.orderId = int.Parse(option2);
                insertion.addToOrder(o,item,form.quantities[i]);
            }
            else if(option2!="None")//Order Id
            {
                if (form.quantities[i] <= 0)
                {
                    Debug.Log("Add Order Failed:0 Quantity");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                    continue;
                }
                order o = new order();
                IDataReader reader = grabBag.getOrderItemsById(option2);
                reader.Read();
                o.customerId = (string)reader["CustomerId"];
                reader.Close();
                if (o.customerId.Length<=0)
                {
                    Debug.Log("Add Order Failed: No Customer Id Matches the OrderId");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                    continue;
                }
                o.orderId = int.Parse(option2);
                insertion.addToOrder(o, item, form.quantities[i]);
            }
            else if(OrderCID[i].text.Length > 0)//CustomerId
            {
                if (form.quantities[i] <= 0)
                {
                    Debug.Log("Add Order Failed:0 Quantity");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                    continue;
                }
                string cId = OrderCID[i].text;
                if (!OrderCID[i].text.Contains("C"))
                {
                    cId = "C";
                    int parsedId = int.Parse(OrderCID[i].text);
                    if (parsedId < 1000)
                        cId += "0";
                    if (parsedId < 100)
                        cId += "0";
                    if (parsedId < 10)
                        cId += "0";
                    cId += OrderCID[i].text;
                }
                customer c = grabBag.getCustomerById(cId);
                if(c.customerId!="C9999")
                    insertion.addOrder(c, item, form.quantities[i]);
                else
                {
                    Debug.Log("Add Order Failed: No Customer Id Matches the OrderId");
                    this.gameObject.AddComponent<ExceptionPopUp>().Start();
                }
            }
        }
    }

    //Changes Quantities in OrderCreateTable
    public void upQuantityO(int index)
    {
        OrderCreateTable.setQuantities(index, 1);
    }

    public void lowerQuantityO(int index)
    {
        OrderCreateTable.setQuantities(index, -1);
    }

    //Hides all tables
    public void hideTables()
    {
        InventoryUpdateTable.hideTable();
        OrderCreateTable.hideTable();
        OrderFillTable.hideTable();
    }
}

[System.Serializable]
public class OrderFillUpdateTable : UpdateTable
{
    public Dropdown[] dropList3;
    public Dropdown[] dropList4;
    public Text[] quantities;
    public sealed override void fillList1(List<string> list)
    {
        base.fillList1(list);
    }

    public sealed override void fillList2(List<string> list)
    {
        base.fillList2(list);
    }
    public void fillList3(List<string> list)
    {
        foreach (Dropdown dropDown in dropList3)
        {
            dropDown.options.Clear();
            dropDown.options.Add(new Dropdown.OptionData("None"));
            foreach (string str in list)
                dropDown.options.Add(new Dropdown.OptionData(str));
        }
    }

    public void fillList4(List<string> list)
    {
        foreach (Dropdown dropDown in dropList4)
        {
            dropDown.options.Clear();
            dropDown.options.Add(new Dropdown.OptionData("None"));
            foreach (string str in list)
                dropDown.options.Add(new Dropdown.OptionData(str));
        }
    }

    public UpdateForm submitTable1()
    {
        UpdateForm form = new UpdateForm();
        form.dropLists = new Dropdown[][] { dropList1, dropList2 };
        return form;
    }

    public UpdateForm submitTable2()
    {
        UpdateForm form = new UpdateForm();
        form.dropLists = new Dropdown[][] { dropList3, dropList4 };
        return form;
    }
}

[System.Serializable]
public class InvUpdateOCreateTable : UpdateTable
{
    public int[] quantities;
    public Text[] quants;
    public sealed override void fillList1(List<string> list)
    {
        base.fillList1(list);
    }

    public sealed override void fillList2(List<string> list)
    {
        base.fillList2(list);
    }

    public void setQuantities(int index,int value)
    {
        quantities[index] += value;
        if (quantities[index] < 0)
            quantities[index] = 0;
        quants[index].text = quantities[index].ToString();
    }

    public sealed override UpdateForm submitTable()
    {
        UpdateForm form = new UpdateForm();
        form.dropLists = new Dropdown[][]{ dropList1,dropList2};
        form.quantities = quantities;
        return form;
    }
}

[System.Serializable]
public abstract class UpdateTable
{
    public Dropdown[] dropList1;
    public Dropdown[] dropList2;
    public GameObject tableObj;
    public virtual void hideTable()
    {
        tableObj.SetActive(false);
    }
    public virtual void showTable()
    {
        tableObj.SetActive(true);
    }
    public virtual UpdateForm submitTable()
    {
        return new UpdateForm();
    }

    public virtual void fillList1(List<string> list)
    {
        foreach (Dropdown dropDown in dropList1)
        {
            dropDown.options.Clear();
            dropDown.options.Add(new Dropdown.OptionData("None"));
            foreach (string str in list)
                dropDown.options.Add(new Dropdown.OptionData(str));
        }
    }

    public virtual void fillList2(List<string> list)
    {
        foreach (Dropdown dropDown in dropList2)
        {
            dropDown.options.Clear();
            dropDown.options.Add(new Dropdown.OptionData("None"));
            foreach (string str in list)
                dropDown.options.Add(new Dropdown.OptionData(str));
        }
    }
}
