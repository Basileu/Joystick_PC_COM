using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Text;

namespace USB_Joystick_v0_1_C_Sharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetSticks();
            Sticks = GetSticks();
            timer1.Enabled = true;
        }
        DirectInput Input = new DirectInput();
        SlimDX.DirectInput.Joystick stick;
        Joystick[] Sticks;
        bool mouseClicked = false;
        SerialPort ComPort = new SerialPort();

        int yValue = 0;
        int xValue = 0;
        int zValue = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern void mouse_event(uint flag, uint _x, uint _y, uint btn, uint exInfo);
        private const int MOUSEEVENT_LEFTDOWN = 0x2;
        private const int MOUSEEVENT_LEFTUP = 0x4;

        public Joystick[] GetSticks()
        {
            List<SlimDX.DirectInput.Joystick> sticks = new List<SlimDX.DirectInput.Joystick>();
            foreach(DeviceInstance device in Input.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    stick = new SlimDX.DirectInput.Joystick(Input, device.InstanceGuid);
                    stick.Acquire();

                    foreach (DeviceObjectInstance deviceObject in stick.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                        {
                            stick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100,100);
                        }
                    }
                    sticks.Add(stick);
                }
                catch(DirectInputException)
                {

                }
            }
            return sticks.ToArray();
        }

        void stickHandle(Joystick stick, int id)
        {
            JoystickState state = new JoystickState();
            state = stick.GetCurrentState();

            yValue = state.Y;
            xValue = state.X;
            zValue = state.Z;

            //Sending Joystick data over USB2UART
            //MouseMove(xValue, yValue);
            SendJoystickData(xValue, yValue);
            bool[] buttons = state.GetButtons();

            if (id == 0)
            {
                for (int i = 0; i< buttons.Length; i++)
                {
                    if (buttons[i])
                    {
                        SendButtonsData(i);
                        //MessageBox.Show(String.Format("Button '{0}' pressed", i));
                    }
                }
            //    if (buttons[0])
            //    {
            //        Do stuff
            //        if (mouseClicked == false)
            //        {
            //            mouse_event(MOUSEEVENT_LEFTDOWN, 0, 0, 0, 0);
            //            mouseClicked = true;
            //        }
            //    }
            //    else
            //    {
            //        if (mouseClicked == true)
            //        {
            //            mouse_event(MOUSEEVENT_LEFTUP, 0, 0, 0, 0);
            //            mouseClicked = false;

                //        }
                //    }
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Joystick[] joystick = GetSticks();
        }

        public void MouseMove(int posx, int posy)
        {
            //Cursor.Position = new Point(Cursor.Position.X + posx/3, Cursor.Position.Y + posy/3);
            //Cursor.Clip = new Rectangle(this.Location, this.Size);
        }
        //private void timer1_Tick(object sender, EventArgs e)
        //{

        //}

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            for (int i = 0; i < Sticks.Length; i++)
                stickHandle(Sticks[i], i);
        }

        private void btnGetSerialPorts_Click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            do
            {
                index += 1;
                rtbIncoming.Text += ArrayComPortsNames[index] + "\n";
            }
            while (!((ArrayComPortsNames[index] == ComPortName) ||
                                (index == ArrayComPortsNames.GetUpperBound(0))));
            if (index == ArrayComPortsNames.GetUpperBound(0))
            {
                ComPortName = ArrayComPortsNames[0];
            }

            //get first item print in text
            cboPorts.Text = ArrayComPortsNames[0];
            cboPorts.DataSource = ArrayComPortsNames;

            if (cboPorts.SelectedIndex > -1)
            {
                //MessageBox.Show(String.Format("You selected port '{0}'", cboports.SelectedItem));
                //Connect(cboPorts.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Please select a port first");
            }
        }

        private void rtbIncomingData_TextChanged(object sender, EventArgs e)
        {

        }

        private void SendButtonsData(int buttNr)
        {
            // Instantiate the communications
            // port with some basic settings

            //String portName = cboPorts.SelectedItem.ToString();
            if (cboPorts.SelectedItem == null)
            {
                //MessageBox.Show(String.Format("Port is not selected"));
                return;
            }

            SerialPort port = new SerialPort(cboPorts.SelectedItem.ToString(), 115200, Parity.None, 8, StopBits.One);

            port.Open();
            // Write a string
            //port.Write("Hello World");
            //port.Write(buttNr.ToString()  + "\r\n");

            String strToSend = buttNr.ToString();
            port.Write(0xFF + strToSend.Length.ToString() + strToSend);

            // Write a set of bytes
            //port.Write(new byte[] { 0x0A, 0xE2, 0xFF }, 0, 3);

            // Close the port
            port.Close();
        }
        private void SendJoystickData(int posx, int posy)
        {
            // Instantiate the communications
            // port with some basic settings

            //String portName = cboPorts.SelectedItem.ToString();
            if (cboPorts.SelectedItem == null)
            {
                //MessageBox.Show(String.Format("Port is not selected"));
                return;
            }

            SerialPort port = new SerialPort(cboPorts.SelectedItem.ToString(), 115200, Parity.None, 8, StopBits.One);

            // Open the port for communications
            if (port == null)
            {
                //MessageBox.Show(String.Format("Port is not selected"));
                return;
            }
            port.Open();

            // Write a string
            //port.Write("Hello World");
            int nrBytes = posx.ToString().Length + posy.ToString().Length;

            //port.Write("7"+ "8" + "\t" + posx.ToString() + "\t" + posy.ToString() + "\r\n");
            if (posx > 10)
                posx = 1;
            else if(posx < -10)
                posx = 2;
            else // Idle position
                posx = 0;
            if (posy < -10)
                posy = 1;
            else if(posy > 3)
                posy = 3;
            else
                posy = 0;

            //String strToSend = "8" + "\t" + posx.ToString() + "\t" + posy.ToString();
            //String strToSend = "8" + "\t" + posx.ToString() + "\t" + posy.ToString();
            String strToSend = "8" +  posx.ToString() + posy.ToString();

            //port.Write("8" + "\t" + posx.ToString() + "\t" + posy.ToString() + "\r\n");
            port.Write(0xFF + strToSend.Length.ToString() + strToSend);

            // Write a set of bytes
            //port.Write(new byte[] { 0x0A, 0xE2, 0xFF }, 0, 3);

            // Close the port
            port.Close();
        }

        private void SendString_Click(object sender, EventArgs e)
        {
            // Instantiate the communications
            // port with some basic settings

            //String portName = cboPorts.SelectedItem.ToString();
            if(cboPorts.SelectedItem == null)
            {
                MessageBox.Show(String.Format("Port is not selected"));
                return;
            }

            SerialPort port = new SerialPort(cboPorts.SelectedItem.ToString(), 115200, Parity.None, 8, StopBits.One);

            // Open the port for communications
            //if (port==null)
            //{
            //    MessageBox.Show(String.Format("Port is not selected"));
            //    return;
            //}
            port.Open();

            // Write a string
            port.Write("Hello World");

            // Write a set of bytes
            port.Write(new byte[] { 0x0A, 0xE2, 0xFF }, 0, 3);

            // Close the port
            port.Close();
        }
    }
}
