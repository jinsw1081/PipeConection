using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;

public class NamePipeLine:MonoBehaviour
{
    string url= "https://localhost:44355/api/CustomWebAPI/pipe";
    string path;
    Thread clientWriteThread;
    Stopwatch stopwatch=new Stopwatch();

    Camera mainCam;
    bool booCaptureOn = false;
    private void Start()
    {
        mainCam = Camera.main;
        path =Environment.GetFolderPath(  Environment.SpecialFolder.MyDocuments)+ "\\Images1.png";
        //StartCoroutine(ActivePipeLine(url));    

        //clientWriteThread = new Thread(ClientThread_Write);
        //ThreadStart();
        ClientThread_Write();
        ScreenShootSingleThread(mainCam);

    }


    void ThreadStart()
    {
        clientWriteThread.Start();
    }

    


    void ClientThread_Write()
    {

        NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", "ServerRead_ClientWrite",
            PipeDirection.In);
        
        namedPipeClientStream.Connect();
        stopwatch.Start();
        Debug.Log("Client Wirte Connected");

        StreamString streamString = new StreamString(namedPipeClientStream);
        string message = streamString.ReadString();
        booCaptureOn = true;
        namedPipeClientStream.Close();

    }



    byte[] ScreenShootSingleThread(Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();
        File.WriteAllBytes(path + "2.png", image.EncodeToPNG());

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image.EncodeToPNG();
    }

    //byte[] ScreenShootSingleThread()
    //{

    //    RenderTexture renderTexture = Camera.main.targetTexture;
    //    Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
    //    RenderTexture.active = renderTexture;
    //    texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //    texture2D.Apply();
    //    File.WriteAllBytes(path + "2.png", texture2D.EncodeToPNG());

    //    return texture2D.EncodeToPNG();

    //}

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();

        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

}
