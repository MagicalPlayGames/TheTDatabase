using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LabelShot : MonoBehaviour
{ 
    [SerializeField]
    public int ScreenSaveNum;
        void Start()
        {
            //yield return SaveScreenPNG();
        }

        public IEnumerator SaveScreenPNG()
        {
            // Read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();

        // Create a texture in RGB24 format the size of the screen
        int width = 1318;
            int height = 1706;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB48,false);

            // Read the screen contents into the texture
            tex.ReadPixels(new Rect(0,0, 1078, 1541),0,0,true);
            tex.Apply();

            // Encode the bytes in PNG format
            byte[] bytes = ImageConversion.EncodeArrayToPNG(tex.GetRawTextureData(), tex.graphicsFormat, (uint)width, (uint)height);
            Object.Destroy(tex);

            // Write the returned byte array to a file in the project folder
            File.WriteAllBytes(Application.persistentDataPath + "/../SavedScreen" + (ScreenSaveNum++) + ".png", bytes);
        }
}
