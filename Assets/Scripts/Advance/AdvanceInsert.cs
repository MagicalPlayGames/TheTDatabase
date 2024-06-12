using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Insertion for Items, and Customers
public class AdvanceInsert : MonoBehaviour
{
    public InsertValues insertion;
    public InputField[] itemTexts;
    public InputField[] customerTexts;
    public AdvanceTables tables;
    public void AddItemType()
    {
        item i = new item();
        i.itemName = itemTexts[0].text;
        i.flavor = itemTexts[1].text;
        i.size = itemTexts[2].text;
        decimal p = 0;
        if(!decimal.TryParse(itemTexts[3].text, out p))
        {
            Debug.Log("Add Item Type Failed: Bad Price");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            return;
        }
        else
        {
            i.price = p;
        }

        if (insertion.addItemType(i) != 1)
        {
            Debug.Log("Add Item Type Failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        else
        {
            tables.generateTable(CurrentDisplayTable.Items);
        }
    }
    public void AddCustomer()
    {
        customer c = new customer();
        if(customerTexts[0].text.Length<=0)
        {
            Debug.Log("Add Customer Failed: No Name");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
            return;
        }
        c.name = customerTexts[0].text;
        c.address = customerTexts[1].text;
        long phone = -1;
        if (!long.TryParse(customerTexts[2].text,out phone))
        {
            Debug.Log("Add Customer Continued: Bad Phone Number");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        c.phone = phone;
        if (insertion.addCustomer(c) != 1)
        {
            Debug.Log("Add Customer Failed");
            this.gameObject.AddComponent<ExceptionPopUp>().Start();
        }
        else
        {
            tables.generateTable(CurrentDisplayTable.Customers);
        }
    }
}
