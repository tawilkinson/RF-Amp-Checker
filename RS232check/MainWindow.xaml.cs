using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace RS232check
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort ComPort = new SerialPort();
        bool PortFlag = new bool();
        Dictionary<string, string> Commands = new Dictionary<string, string>();
        public Message messenger = new Message();
        string currentPort;

        public MainWindow()
        {

            InitializeComponent();
            DataContext = messenger;
            SetMessage("Click Ports to load...");

            Commands.Add("Enable Remote Control", "SPC:CTL 1;");
            Commands.Add("Enable RF power", "RF 1;");
            Commands.Add("Disable RF power", "RF 0;");
            Commands.Add("Get status", "STA?;");
            Commands.Add("Get forward power", "PWR:PF?;");
            Commands.Add("Get temperature", "SPC:T?;");

            foreach (KeyValuePair<string, string> entry in Commands)
            {
                cboCommand.Items.Add(entry.Key);
            }
        }

        public class Message : INotifyPropertyChanged
        {
            string textb = "";
            public string Textb {
                get {
                    return textb;
                }
                set {
                    textb = value;
                    OnPropertyChanged("Textb");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string property)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void SetMessage(string txtin)
        {
            messenger.Textb = txtin;
        }

        private void AppendMessage(string txtin)
        {
            messenger.Textb = messenger.Textb + "\n" + txtin;
        }

        private void btnGetSerialPorts_Click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;
            cboPorts.Items.Clear();

            try
            {
                ArrayComPortsNames = SerialPort.GetPortNames();
                Array.Sort(ArrayComPortsNames);
                do
                {
                    index += 1;
                    cboPorts.Items.Add(ArrayComPortsNames[index]);
                }
                while (!((ArrayComPortsNames[index] == ComPortName) ||
                (index == ArrayComPortsNames.GetUpperBound(0))));

                if (index == ArrayComPortsNames.GetUpperBound(0))
                {
                    ComPortName = ArrayComPortsNames[0];
                }
                cboPorts.Text = ArrayComPortsNames[0];
                PortFlag = true;
                SetMessage((index + 1) + " COM devices detected.");
            }
            catch
            {
                SetMessage("No COM devices detected...");
                PortFlag = false;
            }
        }

        public void btnPortStatus_Click(object sender, EventArgs e)
        {
            if (PortFlag == false)
            {
                SetMessage("First get system ports or choose a port.");
            }
            else if (btnPortState.Content.ToString() == "Closed")
            {
                btnPortState.Content = "Open";
                ComPort.PortName = Convert.ToString(cboPorts.Text);
                ComPort.BaudRate = Convert.ToInt32(19200);
                ComPort.DataBits = Convert.ToInt16(8);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
                ComPort.DataReceived += new SerialDataReceivedEventHandler(DataRecievedHandler);
                ComPort.Open();
                currentPort = cboPorts.Text;
                SetMessage(cboPorts.Text + " opened.");
            }
            else if (btnPortState.Content.ToString() == "Open")
            {
                btnPortState.Content = "Closed";
                ComPort.Close();
                currentPort = "No Port";
                SetMessage(cboPorts.Text + " closed.");
            }
        }

        private void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();

            indata = Regex.Replace(indata, @"[^\u0020-\u007E]+", string.Empty);

            switch (indata)
            {
                case "11;":
                    SetMessage(currentPort + "\nData Received:\n11;\nRF Enabled.");
                    break;
                case "9;":
                    SetMessage(currentPort + "\nData Received:\n11;\nRF Disabled.");
                    break;
                case ";":
                    SetMessage(currentPort + "\nData Received:\n;\nSuccess");
                    break;
                default:
                    SetMessage(currentPort + "\nData Received:\n" + indata);
                    break;
            }
        }

        private void CheckHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();

            AppendMessage(indata);

            indata = Regex.Replace(indata, @"[^\u0020-\u007E]+", string.Empty);

            AppendMessage(indata);

            switch (indata)
            {
                case "11;":
                    AppendMessage(currentPort + " Barthel amp - Active.");
                    break;
                case "10;":
                    AppendMessage(currentPort + " Barthel amp - Start.");
                    break;
                case "9;":
                    AppendMessage(currentPort + " Barthel amp - Inactive.");
                    break;
                case "1;":
                    AppendMessage(currentPort + " Barthel amp - Error state.");
                    break;
                default:
                    AppendMessage(currentPort + " unknown device.");
                    break;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cboCommand.SelectedValue == null)
            {
                SetMessage("First select a command to send.");
            }
            else if (PortFlag == false)
            {
                SetMessage("First get system ports.");
            }
            else if (btnPortState.Content.ToString() == "Closed")
            {
                SetMessage("First choose a port.");
            }
            else
            {
                string val = "";
                Commands.TryGetValue(cboCommand.SelectedValue.ToString(), out val);
                SetMessage("Sending \"" + val + "\"");
                ComPort.Write(val);
            }
        }

        private void BtnPortScan_Click(object sender, RoutedEventArgs e)
        {
            string[] ArrayComPortsNames = null;
            // string[] ArrayRFPortsNames = null;
            int index = -1;
            string ComPortName = null;
            cboPorts.Items.Clear();

            try
            {
                ArrayComPortsNames = SerialPort.GetPortNames();
                Array.Sort(ArrayComPortsNames);
                do
                {
                    index += 1;
                    cboPorts.Items.Add(ArrayComPortsNames[index]);
                }
                while (!((ArrayComPortsNames[index] == ComPortName) ||
                (index == ArrayComPortsNames.GetUpperBound(0))));

                if (index == ArrayComPortsNames.GetUpperBound(0))
                {
                    ComPortName = ArrayComPortsNames[0];
                }
                cboPorts.Text = ArrayComPortsNames[0];
                PortFlag = true;
                SetMessage((index + 1) + " COM devices detected.\nDetermining amps");
            }
            catch
            {
                PortFlag = false;
            }

            if (PortFlag == false)
            {
                SetMessage("No COM devices detected...");
            }
            else
            {
                index = 0;
                do
                {
                    btnPortState.Content = "Open";
                    ComPort.PortName = Convert.ToString(ArrayComPortsNames[index]);
                    ComPort.BaudRate = Convert.ToInt32(19200);
                    ComPort.DataBits = Convert.ToInt16(8);
                    ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
                    ComPort.DataReceived += new SerialDataReceivedEventHandler(CheckHandler);
                    ComPort.Open();
                    currentPort = ArrayComPortsNames[index];
                    AppendMessage(ArrayComPortsNames[index] + " opened.");
                    ComPort.Write("STA?;");
                    AppendMessage(ArrayComPortsNames[index] + " written.");
                    ComPort.Close();
                    btnPortState.Content = "Closed";
                    index++;
                }
                while (!(index == ArrayComPortsNames.GetUpperBound(0)));
                
            }
        }
    }
}