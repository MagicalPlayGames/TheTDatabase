using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UI;

public class LabelMaker : MonoBehaviour
{
    // Start is called before the first frame update
    IDbConnection dbcon;
    static string connection;
    public LabelShot screenShot;
    public GameObject background;

    [System.Serializable]
    public struct label
    {
        public GameObject labelLarge;
        public GameObject labelSmall;
        public Text nameS;
        public Text flavorS;
        public Text idS;
        public Text nameL;
        public Text flavorL;
        public Text idL;
    }

    public label[] labels;
    void Start()
    {
        connection = "URI=file:" + Application.persistentDataPath + "/" + "HRS_DB";
    }

    // Update is called once per frame
    public void getLabels()
    {

        StartCoroutine(labelReader());
    }
    public  IEnumerator labelReader()
    {
        IDataReader reader = null;
        try
        {
            dbcon = new SqliteConnection(connection);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT i.ItemName, b.ItemId, b.BatchId, i.Flavor, i.Size, b.Quantity FROM ((Inventory AS inv INNER JOIN Items AS i ON inv.ItemId = i.ItemId) INNER JOIN Batches AS b ON i.ItemId = b.ItemId)";
            //dbcmd.Parameters.Add(new SqliteParameter("@ItemName", "%" + itemName + "%"));
            reader = dbcmd.ExecuteReader(CommandBehavior.CloseConnection);
            Debug.Log("Yes");
            int labelNum = 0;
            background.SetActive(true);
            while (reader.Read())
            {
                int quantity = (int)(long)reader["Quantity"];
                for (int quantityIndex = 0; quantityIndex < quantity; quantityIndex++)
                {
                    if (labelNum == 5)
                    {
                        StartCoroutine(screenShot.SaveScreenPNG());
                        yield return new WaitForSeconds(3.0f);
                        for (int resetLabelIndex = 0; resetLabelIndex < 5; resetLabelIndex++)
                        {
                            labels[resetLabelIndex].labelSmall.SetActive(false);
                            labels[resetLabelIndex].labelLarge.SetActive(false);
                        }
                        labelNum = 0;
                    }
                    if ((string)reader["Size"] == "Small")
                    {
                        labels[labelNum].labelSmall.SetActive(true);
                    }
                    else
                    {
                        labels[labelNum].labelLarge.SetActive(true);
                    }
                    yield return new WaitForEndOfFrame();

                    string itemId = ((long)reader["ItemId"]) + "";
                    string quant = (quantityIndex + 1) + "";
                    for (int j = itemId.Length; j < 4; j++)
                    {
                        itemId = "0" + itemId;
                    }
                    itemId = "I" + itemId;
                    for (int j = quant.Length; j < 4; j++)
                    {
                        quant = "0" + quant;
                    }
                    quant = "Q" + quant;
                    string fullId = (string)reader["BatchId"] + itemId + quant;

                    if ((string)reader["Size"] == "Small")
                    {
                        labels[labelNum].nameS.text = (string)reader["ItemName"];
                        labels[labelNum].flavorS.text = (string)reader["Flavor"];
                        labels[labelNum].idS.text = fullId;
                    }
                    else
                    {
                        labels[labelNum].nameL.text = (string)reader["ItemName"];
                        labels[labelNum].flavorL.text = (string)reader["Flavor"];
                        labels[labelNum].idL.text = fullId;
                    }
                    labelNum++;
                }
            }
            reader.Close();
            StartCoroutine(screenShot.SaveScreenPNG());
            yield return new WaitForSeconds(3.0f);
        }
        finally
        {
            for (int resetLabelIndex = 0; resetLabelIndex < 5; resetLabelIndex++)
            {
                labels[resetLabelIndex].labelSmall.SetActive(false);
                labels[resetLabelIndex].labelLarge.SetActive(false);
            }
            background.SetActive(false);
        }

    }
}
