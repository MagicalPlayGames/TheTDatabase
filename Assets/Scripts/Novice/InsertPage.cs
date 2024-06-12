using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

public class InsertPage : MonoBehaviour
{
    [System.Serializable]
    public struct arr
    {
        public InputField[] f;
        public InputField this[int x]
        {
            get
            {
                return f[x];
            }
            set
            {
                f[x] = value;
            }
        }
    }
    public InsertValues insertion;
    public GrabValues grab;
    //2 Fill: orderId, ItemId
    //4 AddToOrder: orderId, customerId, itemId, Quantity
    //3 AddOder: customerId, itemId, Quantity
    //1 CancelOrder: orderId
    //4 Add Item Type: name, flavor, size, price
    //3 Add Customer: name, address, phone
    //2 Update Status: orderId, status
    //4 Add New Batch: Name, Flavor, Size, Price
    //5 Add To Batch: batchId, name, flavor, size, price
    
    public arr[] fields;
    public void fill()
    {
        fill(fields[0][0], fields[0][1]);
    }
    public void addToOrder()
    {
        addToOrder(fields[1][0], fields[1][1], fields[1][2], fields[1][3]);
    }
    public void AddOrder()
    {
        AddOrder(fields[2][0], fields[2][1], fields[2][2]);
    }
    public void CancelOrder()
    {
        CancelOrder(fields[3][0]);
    }
    public void AddItemType()
    {
        AddItemType(fields[4][0], fields[4][1], fields[4][2], fields[4][3]);
    }
    public void AddCustomer()
    {
        AddCustomer(fields[5][0], fields[5][1], fields[5][2]);
    }
    public void UpdateStatus()
    {
        UpdateStatus(fields[6][0], fields[6][1]);
    }
    public void AddNewBatch()
    {
        AddNewBatch(fields[7][0], fields[7][1], fields[7][2], fields[7][3]);
    }
    public void AddToBatch()
    {
        AddToBatch(fields[8][0], fields[8][1], fields[8][2], fields[8][3], fields[8][4]);
    }
    private void fill(InputField orderText,InputField itemId)
    {
        IDataReader reader = grab.getOrderItemsById(orderText.text);
        //orderId
        //orderQuantity
        //itemId
        while (reader.Read())
        {
            if ((long)reader["ItemId"]==int.Parse(itemId.text))
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
        if(insertion.addToOrder(o, i, int.Parse(quantity.text)).ItemId==0)
        {
            Debug.Log("Add to Order Failed");
        }
    }

    private void AddOrder(InputField customerId,InputField itemId, InputField quantity)
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
        if(insertion.addOrder(c, i, int.Parse(quantity.text)).orderId==0)
        {
            Debug.Log("Add Order Failed");
        }
    }
 
    private void CancelOrder(InputField text)
    {
        if(!insertion.cancelOrder(int.Parse(text.text)))
        {
            Debug.Log("Cancellation Failed");
        }
    }

    private void AddItemType(InputField nameText,InputField flavorText,InputField sizeText,InputField priceText)
    {
        item i = new item();
        i.itemName = nameText.text;
        i.flavor = flavorText.text;
        i.size = sizeText.text;
        i.price = decimal.Parse(priceText.text);
        if(insertion.addItemType(i)!=1)
        {
            Debug.Log("Add Item Type Failed");
        }
    }

    private void AddCustomer(InputField nameText, InputField AddressText, InputField PhoneText)
    {
        customer c = new customer();
        c.name = nameText.text;
        c.address = AddressText.text;
        c.phone = int.Parse(PhoneText.text);
        if(insertion.addCustomer(c)!=1)
        {
            Debug.Log("Add Customer Failed");
        }
    }

    private void UpdateStatus(InputField orderIDText, InputField orderStatusText)
    {
        if(orderStatusText.text=="Cancelled")
        {
            CancelOrder(orderIDText);
            return;
        }
        insertion.updateStatus(unstringStatus(orderStatusText.text), orderIDText.text);
    }

    private void AddNewBatch(InputField nameText, InputField flavorText, InputField sizeText, InputField priceText)
    {
        item i = new item();
        i.itemName = nameText.text;
        i.flavor = flavorText.text;
        i.size = sizeText.text;
        i.price = decimal.Parse(priceText.text);
        IDataReader reader = grab.getItemsCalled(i.itemName);
        while(reader.Read())
        {
            if (i.flavor.CompareTo(reader["Flavor"]) == 0 && i.size.CompareTo(reader["Size"])==0)//ADD PRICE AND ROUND
            {
                i.itemId = (int)(long)reader["ItemId"];
                item[] items = new item[1];
                items[0] = i;
                reader.Close();
                insertion.addNewBatch(items);
                return;
            }
        }
        Debug.Log("Add New Batch Failed");
    }

    private void AddToBatch(InputField batchId,InputField nameText, InputField flavorText, InputField sizeText, InputField priceText)
    {
        item i = new item();
        i.itemName = nameText.text;
        i.flavor = flavorText.text;
        i.size = sizeText.text;
        i.price = decimal.Parse(priceText.text);
        IDataReader reader = grab.getItemsCalled(i.itemName);
        while (reader.Read())
        {
            if ((string)reader["Flavor"] == i.flavor && (string)reader["Size"] == i.size)//AND PRICE AND ROUND
            {
                i.itemId = (int)(long)reader["ItemId"];
                item[] items = new item[1];
                items[0] = i;
                reader.Close();
                string bId = batchId.text;
                if(!batchId.text.Contains("B"))
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
                insertion.addToBatch(bId,items);
                return;
            }
        }
        AddNewBatch(nameText, flavorText, sizeText, priceText);
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
