using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

public class IntermediateInsert : MonoBehaviour
{
    public InsertValues insertion;
    public GrabValues grab;
    public IntermediateTable tableScript;
    public InputField[] customerTexts;
    public InputField[] orderTexts;
    public InputField[] batchTexts;
    public InputField[] itemTexts;
    public InputField[] fillTexts;
    public InputField[] updateTexts;
    // Start is called before the first frame update
    public void AddCustomer()
    {
        customer c = new customer();
        c.name = customerTexts[0].text;
        c.address = customerTexts[1].text;
        c.phone = long.Parse(customerTexts[2].text);
        if (insertion.addCustomer(c) != 1)
        {
            Debug.Log("Add Customer Failed");
        }
        tableScript.generateTable();
    }

    public void AddOrderItem()
    {
        if(orderTexts[0].text.Length>0 && orderTexts[0].text!="-1")
        {
            addToOrder(orderTexts[0], orderTexts[1], orderTexts[2], orderTexts[3]);
        }
        else
        {
            AddOrder(orderTexts[2], orderTexts[1], orderTexts[3]);
        }
        tableScript.generateTable();
    }

    private void addToOrder(InputField orderId, InputField itemId, InputField customerId, InputField quantity)
    {
        order o = new order();
        o.orderId = int.Parse(orderId.text);
        o.ItemId = int.Parse(itemId.text);
        string cId = customerId.text;
        if (!customerId.text.Contains("C"))
        {
            cId = "C";
            int parsedId = int.Parse(customerId.text);
            if (parsedId < 1000)
                cId += "0";
            if (parsedId < 100)
                cId += "0";
            if (parsedId < 10)
                cId += "0";
            cId += customerId.text;
        }
        o.customerId = cId;
        item i = new item();
        i.itemId = int.Parse(itemId.text);
        if (insertion.addToOrder(o, i, int.Parse(quantity.text)).ItemId == 0)
        {
            Debug.Log("Add to Order Failed");
        }
    }

    private void AddOrder(InputField customerId, InputField itemId, InputField quantity)
    {
        customer c = new customer();
        string cId = customerId.text;
        if (!customerId.text.Contains("C"))
        {
            cId = "C";
            int parsedId = int.Parse(customerId.text);
            if (parsedId < 1000)
                cId += "0";
            if (parsedId < 100)
                cId += "0";
            if (parsedId < 10)
                cId += "0";
            cId += customerId.text;
        }
        c.customerId = cId;
        item i = new item();
        i.itemId = int.Parse(itemId.text);
        if (insertion.addOrder(c, i, int.Parse(quantity.text)).orderId == 0)
        {
            Debug.Log("Add Order Failed");
        }
    }

    public void addBatchItem()
    {

        if (batchTexts[0].text.Length > 0 && batchTexts[0].text != "-1")
        {
            AddToBatch(batchTexts[0], batchTexts[1]);
        }
        else
        {
            AddNewBatch(batchTexts[1]);
        }
        tableScript.generateTable();
    }

    private void AddNewBatch(InputField itemId)
    {
        item i = grab.getItemById(int.Parse(itemId.text));
        item[] items = new item[1];
                items[0] = i;
                insertion.addNewBatch(items);
    }

    private void AddToBatch(InputField batchId, InputField itemId)
    {
        item i = grab.getItemById(int.Parse(itemId.text));
        item[] items = new item[1];
        items[0] = i;
        string bId = batchId.text;
        if (!batchId.text.Contains("B"))
        {
            bId = "B";
            int parsedId = int.Parse(batchId.text);
            if (parsedId < 1000)
                bId += "0";
            if (parsedId < 100)
                bId += "0";
            if (parsedId < 10)
                bId += "0";
            bId += batchId.text;
        }
        insertion.addToBatch(bId, items);
    }



    private void AddItemType(InputField nameText, InputField flavorText, InputField sizeText, InputField priceText)
    {
        item i = new item();
        i.itemName = nameText.text;
        i.flavor = flavorText.text;
        i.size = sizeText.text;
        i.price = decimal.Parse(priceText.text);
        if (insertion.addItemType(i) != 1)
        {
            Debug.Log("Add Item Type Failed");
        }
    }

    public void AddItemType()
    {
        AddItemType(itemTexts[0], itemTexts[1], itemTexts[2], itemTexts[3]);
        tableScript.generateTable();
    }

    private void fill(InputField orderText, InputField itemId)
    {
        IDataReader reader = grab.getOrderItemsById(orderText.text);
        //orderId
        //orderQuantity
        //itemId
        while (reader.Read())
        {
            if ((long)reader["ItemId"] == int.Parse(itemId.text))
            {
                order o = new order();
                item i = new item();
                i.itemId = int.Parse(itemId.text);
                o.orderId = int.Parse(orderText.text);
                o.Quantity = (int)(long)reader["Quantity"];
                reader.Close();
                insertion.fillOrder(o, i);
                return;
            }
        }
        Debug.Log("Fill Failed");
    }

    public void fill()
    {
        fill(fillTexts[0], fillTexts[1]);
    }



    private void UpdateStatus(InputField orderIDText, InputField orderStatusText)
    {
        insertion.updateStatus(unstringStatus(orderStatusText.text), orderIDText.text);
    }

    public void UpdateStatus()
    {
        UpdateStatus(updateTexts[0], updateTexts[1]);
    }
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
}
