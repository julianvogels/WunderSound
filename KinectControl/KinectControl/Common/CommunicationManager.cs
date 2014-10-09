using System;
using System.IO.Ports;
using System.Diagnostics;
namespace KinectControl.Common
{
    public class CommunicationManager
    {
        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };
        public string msg;
        private string _baudRate = string.Empty;
        private string _portName = string.Empty;
        private static SerialPort port1;
        public event EventHandler<SerialDataReceivedEventArgs> DataRecieved;
        public string BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        public CommunicationManager(string baud)
        {
            DataRecieved += port1_DataReceived;
            _baudRate = baud;
            try
            {
                _portName = SerialPort.GetPortNames()[0];
                port1 = new SerialPort(_portName, Int32.Parse(baud));
                port1.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived);
            }
            catch (Exception)
            {
                _portName = "";
            }
        }

        public void WriteData(string msg)
        {
            if (!_portName.Equals(""))
            {
                if (!port1.IsOpen) 
                port1.Open();
                port1.Write(msg);
             //   port1.Close();
            }
        }

        public bool OpenPort()
        {
            try
            {
                if (!_portName.Equals(""))
                {
                    port1.Open();
                    Debug.WriteLine(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(MessageType.Error, ex.Message);
                return false;
            }
        }

        public bool ClosePort()
        {
            try
            {
                if ((port1.IsOpen == true) && (port1.BytesToRead == 0)) port1.Close();
                Debug.WriteLine(MessageType.Normal, "Port closed at " + DateTime.Now + "\n");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(MessageType.Error, ex.Message);
                return true;
            }
        }
        public void port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            msg = port1.ReadExisting();
            Debug.WriteLine(MessageType.Incoming, msg + "\n");
//            port1.Close();
        }
    }
}


