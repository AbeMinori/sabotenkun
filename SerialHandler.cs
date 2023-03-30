using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialHandler : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    //ポート名
    //例
    //Linuxでは/dev/ttyUSB0
    //windowsではCOM1
    //Macでは/dev/tty.usbmodem1421など
    //string portName = "COM3";
    public string portName = "/dev/tty.usbmodem143403";

    public int baudRate = 115200;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    void Awake()
    {
        Open();
    }

    void Update()
    {
        if (isNewMessageReceived_)
        {
            OnDataReceived(message_);
        }
        isNewMessageReceived_ = false;
    }

    void OnDestroy()
    {
        Close();
    }

    private void Open()
    {
        try
        {
            serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        //または
        //serialPort_ = new SerialPort(portName, baudRate);
        try
        {
            serialPort_.Open();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();
        Debug.Log("SerialOpen");
    }

    private void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    private void Read()
    {
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        try
        {
            serialPort_.Write(buffer, offset, count);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}