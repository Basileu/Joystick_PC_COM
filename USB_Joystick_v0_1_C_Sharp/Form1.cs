﻿using System;
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

            MouseMove(xValue, yValue);
            bool[] buttons = state.GetButtons();

            if (id == 0)
            {
                if (buttons[0])
                {
                    // Do stuff
                    if (mouseClicked == false)
                    {
                        mouse_event(MOUSEEVENT_LEFTDOWN, 0, 0, 0, 0);
                        mouseClicked = true;
                    }
                }
                else
                {
                    if (mouseClicked == true)
                    {
                        mouse_event(MOUSEEVENT_LEFTUP, 0, 0, 0, 0);
                        mouseClicked = false;

                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Joystick[] joystick = GetSticks();
        }

        public void MouseMove(int posx, int posy)
        {
            Cursor.Position = new Point(Cursor.Position.X + posx/3, Cursor.Position.Y + posy/3);
            Cursor.Clip = new Rectangle(this.Location, this.Size);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            for (int i = 0; i < Sticks.Length; i++)
                stickHandle(Sticks[i], i);
        }
    }
}
