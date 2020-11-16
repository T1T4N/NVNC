// NVNC - .NET VNC Server Library
// Copyright (C) 2014 T!T@N
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Drawing;
using System.Threading;

namespace NVNC
{
    /// <summary>
    /// A wrapper class that should be used. It represents a VNC Server, and handles all the RFB procedures and communication.
    /// </summary>
    public class VncServer
    {
        private VncHost host;
        private Framebuffer fb;

        
        public int Port { get; private set; }
        public string Password { get; private set; }

        private string _repeaterHost;
        /// <summary>
        /// The Repeater Host and port
        /// <remarks>
        /// Format as hostname[:port] or ipaddress[:port].
        /// If no port is specified then it will use the default 5500 port
        /// </remarks>
        /// </summary>
        public string RepeaterHost { get; set; }

        /// <summary>
        /// The server's Repeater Mode 2 ID. Must be a numerical value
        /// </summary>
        public string RepeaterID { get; set; }

        /// <summary>
        /// The VNC Server name.
        /// <remarks>The variable value should be non-null.</remarks>
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The default constructor using the default values for the parameters.
        /// Port is set to 5900, the Name is set to Default, and there is no password.
        /// </summary>
        public VncServer()
            : this("", 5900, "Default")
        { }

        public VncServer(string password, int port, string name)
        {
            Password = password;
            Port = port;
            Name = name;

            Init();
        }

        public VncServer(string password, int port, string name, string repeaterHost, string repeaterID)
        {
            Password = password;
            Port = port;
            Name = name;
            RepeaterHost = repeaterHost;
            RepeaterID = repeaterID;

            Init();
        }

        private void Init()
        {
            Size screenSize = ScreenSize();
            fb = new Framebuffer(screenSize.Width, screenSize.Height)
            {
                BitsPerPixel = 32,
                Depth = 24,
                BigEndian = false,
                TrueColor = true,
                RedShift = 16,
                GreenShift = 8,
                BlueShift = 0,
                BlueMax = 0xFF,
                GreenMax = 0xFF,
                RedMax = 0xFF,
                DesktopName = Name
            };
        }

        public void Start()
        {

            if (String.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (Port == 0)
                throw new ArgumentNullException("Port", "The VNC Server port cannot be zero.");
            Console.WriteLine("Started VNC Server at port: " + Port);

            host = new VncHost(Port, Name);
            host.Start();

            host.WriteProtocolVersion();
            Console.WriteLine("Wrote Protocol Version");

            host.ReadProtocolVersion();
            Console.WriteLine("Read Protocol Version");

            Console.WriteLine("Awaiting Authentication");
            if (!host.WriteAuthentication(Password))
            {
                Console.WriteLine("Authentication failed !");
                host.Close();
                //Start();
            }
            else
            {
                Console.WriteLine("Authentication successfull !");

                bool share = host.ReadClientInit();
                Console.WriteLine("Share: " + share);

                Console.WriteLine("Server name: " + fb.DesktopName);
                host.WriteServerInit(fb);

                while ((host.isRunning))
                {
                    switch (host.ReadServerMessageType())
                    {
                        case VncHost.ClientMessages.SetPixelFormat:
                            Console.WriteLine("Read SetPixelFormat");
                            Framebuffer f = host.ReadSetPixelFormat(fb.Width, fb.Height);
                            if (f != null)
                                fb = f;
                            break;
                        case VncHost.ClientMessages.ReadColorMapEntries:
                            Console.WriteLine("Read ReadColorMapEntry");
                            host.ReadColorMapEntry();
                            break;
                        case VncHost.ClientMessages.SetEncodings:
                            Console.WriteLine("Read SetEncodings");
                            host.ReadSetEncodings();
                            break;
                        case VncHost.ClientMessages.FramebufferUpdateRequest:
                            Console.WriteLine("Read FrameBufferUpdateRequest");
                            host.ReadFrameBufferUpdateRequest(fb);
                            break;
                        case VncHost.ClientMessages.KeyEvent:
                            Console.WriteLine("Read KeyEvent");      
                            host.ReadKeyEvent();
                            break;
                        case VncHost.ClientMessages.PointerEvent:
                            Console.WriteLine("Read PointerEvent");
                            host.ReadPointerEvent();
                            break;
                        case VncHost.ClientMessages.ClientCutText:
                            Console.WriteLine("Read CutText");
                            host.ReadClientCutText();
                            break;
                    }
                }
                //if (!host.isRunning)
                    //Start();
            }
        }

        /// <summary>
        /// Starts a Slightly Modified version of the Ultra's VNC's repeater Mode 2.
        /// Normally when a UltraVNC server connects to the repeater. It would not ask 
        /// for a password. It would just use the ID as the password. This is not 
        /// something I liked. So this server will ask for a password. Suprisingly 
        /// this is compatible with the Unix 1 repeater located:
        /// 
        /// http://www.uvnc.eu/download/repeater/uvncrepeater.tar.gz
        /// 
        /// I have not tested this compatiblity with any other UltraVNC repeater.
        /// 
        /// </summary>
        public void StartRepeater()
        {
            if (String.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (Port == 0)
                throw new ArgumentNullException("Port", "The VNC Server Port cannot be zero.");

            if (String.IsNullOrEmpty(RepeaterHost))
                throw new ArgumentNullException("RepeaterHost", "The Repeater Host can not be empty.");

            if (String.IsNullOrEmpty(RepeaterID))
                throw new ArgumentNullException("RepeaterID", "The Repeater ID can not be empty.");

            Console.WriteLine("Started VNC Server at port: " + Port);

            host = new VncHost(Port, Name, RepeaterHost, RepeaterID);
            host.StartRepeater();

            host.WriteRepeaterID();
            Console.WriteLine("Wrote repeater ID");

            host.WriteProtocolVersion();
            Console.WriteLine("Wrote Protocol Version");

            host.ReadProtocolVersion();
            Console.WriteLine("Read Protocol Version");

            Console.WriteLine("Awaiting Authentication");
            if (!host.WriteAuthentication(Password))
            {
                Console.WriteLine("Authentication failed !");
                host.Close();
                StartRepeater();
            }
            else
            {
                Console.WriteLine("Authentication successfull !");

                bool share = host.ReadClientInit();
                Console.WriteLine("Share: " + share.ToString());

                Console.WriteLine("Server name: " + fb.DesktopName);
                host.WriteServerInit(this.fb);

                while ((host.isRunning))
                {
                    switch (host.ReadServerMessageType())
                    {
                        case VncHost.ClientMessages.SetPixelFormat:
                            Console.WriteLine("Read SetPixelFormat");
                            Framebuffer f = host.ReadSetPixelFormat(fb.Width, fb.Height);
                            if (f != null)
                                fb = f;
                            break;
                        case VncHost.ClientMessages.ReadColorMapEntries:
                            Console.WriteLine("Read ReadColorMapEntry");
                            host.ReadColorMapEntry();
                            break;
                        case VncHost.ClientMessages.SetEncodings:
                            Console.WriteLine("Read SetEncodings");
                            host.ReadSetEncodings();
                            break;
                        case VncHost.ClientMessages.FramebufferUpdateRequest:
                            Console.WriteLine("Read FrameBufferUpdateRequest");
                            host.ReadFrameBufferUpdateRequest(fb);
                            break;
                        case VncHost.ClientMessages.KeyEvent:
                            Console.WriteLine("Read KeyEvent");
                            host.ReadKeyEvent();
                            break;
                        case VncHost.ClientMessages.PointerEvent:
                            Console.WriteLine("Read PointerEvent");
                            host.ReadPointerEvent();
                            break;
                        case VncHost.ClientMessages.ClientCutText:
                            Console.WriteLine("Read CutText");
                            host.ReadClientCutText();
                            break;
                    }
                }
                if (!host.isRunning)
                    StartRepeater();
            }
        }

        /// <summary>
        /// Closes all active connections, and stops the VNC Server from listening on the specified port.
        /// </summary>
        public void Stop()
        {
            host.Close();
        }
        private Size ScreenSize()
        {
            Size s = new Size();
            s.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            s.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            return s;
        }
    }
}
