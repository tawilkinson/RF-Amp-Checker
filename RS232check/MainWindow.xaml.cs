using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;

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

        public MainWindow()
        {
            InitializeComponent();

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

        private void SetMessage(string txtin)
        {
            string txtout = txtin + "\n";
            txtbInfo.Text = txtout;
        }

        private void btnGetSerialPorts_Click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            try
            {
                ArrayComPortsNames = SerialPort.GetPortNames();
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
                SetMessage(cboPorts.Text + " opened.");
            }
            else if (btnPortState.Content.ToString() == "Open")
            {
                btnPortState.Content = "Closed";
                ComPort.Close();
                SetMessage(cboPorts.Text + " closed.");
            }
        }

        private void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            // SetMessage("Data Received:\n" + indata);
            MessageBox.Show("Returned value: \n" + indata);
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

    }
}