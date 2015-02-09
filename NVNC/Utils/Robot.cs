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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NVNC.Utils
{
    /// <summary>
    /// A clone of Java's Robot class.
    /// Used to handle keyboard and mouse events.
    /// </summary>
    public static class Robot
    {
        /*
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte vk, byte scan, int flags, IntPtr extrainfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        */

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            int dwFlags, // motion and click options
            int dx, // horizontal position or change
            int dy, // vertical position or change
            int dwData, // wheel movement
            IntPtr dwExtraInfo // application-defined information
        );

        private class KeyCode
        {
            public int key;
            public bool isShift;
            public bool isAlt;
            public bool isCtrl;
        }
        private static int mouseModifiers = 0;
        private static bool shift = false;
        private static bool alt = false;
        private static bool ctrl = false;


        public static void KeyEvent(bool pressed, int key)
        {
            try
            {
                string sCode = "";
                KeyCode localKeyCode = keysym.toVKCode(key);
                if (localKeyCode != null)
                {
                    sCode = keysym.vkToString(localKeyCode.key);
                    try
                    {
                        if (pressed)
                        {
                            if ((localKeyCode.key == (int)_KeyEvent.VK_SHIFT) || (localKeyCode.key == (int)_KeyEvent.VK_LSHIFT) || (localKeyCode.key == (int)_KeyEvent.VK_RSHIFT))
                                shift = true;
                            if ((localKeyCode.key == (int)_KeyEvent.VK_MENU) || (localKeyCode.key == (int)_KeyEvent.VK_LMENU) || (localKeyCode.key == (int)_KeyEvent.VK_RMENU))
                                alt = true;
                            if ((localKeyCode.key == (int)_KeyEvent.VK_CONTROL) || (localKeyCode.key == (int)_KeyEvent.VK_LCONTROL) || (localKeyCode.key == (int)_KeyEvent.VK_RCONTROL))
                                ctrl = true;
                        }
                        else
                        {
                            if ((localKeyCode.key == (int)_KeyEvent.VK_SHIFT) || (localKeyCode.key == (int)_KeyEvent.VK_LSHIFT) || (localKeyCode.key == (int)_KeyEvent.VK_RSHIFT))
                                shift = false;
                            if ((localKeyCode.key == (int)_KeyEvent.VK_MENU) || (localKeyCode.key == (int)_KeyEvent.VK_LMENU) || (localKeyCode.key == (int)_KeyEvent.VK_RMENU))
                                alt = false;
                            if ((localKeyCode.key == (int)_KeyEvent.VK_CONTROL) || (localKeyCode.key == (int)_KeyEvent.VK_LCONTROL) || (localKeyCode.key == (int)_KeyEvent.VK_RCONTROL))
                                ctrl = false;
                        }
                    }
                    catch (Exception localException1)
                    {
                        Console.WriteLine(localException1.Message);
                    }
                }
                if (!pressed)
                {
                    string keys = "";
                    if (shift && (sCode != "+"))
                        keys += "+";
                    if (ctrl && (sCode != "^"))
                        keys += "^";
                    if (alt && (sCode != "%"))
                        keys += "%";

                    keys += sCode;

                    Trace.WriteLine(keys);
                    try
                    {
                        System.Windows.Forms.SendKeys.SendWait(keys);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /*
        public static void keyPress(int keycode)
        {
            keybd_event(MapKeyCode(keycode), 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
        }
        public static void keyRelease(int keycode)
        {
            keybd_event(MapKeyCode(keycode), 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
        */

        /// <summary>
        /// Moves the mouse pointer to the specified location, and/or presses the specified mouse buttons.
        /// </summary>
        /// <param name="Mask">The button mask, depending on which mouse button was pressed</param>
        /// <param name="X">The X coordinate where the pointer should be moved.</param>
        /// <param name="Y">The Y coordinate where the pointer should be moved.</param>
        public static void PointerEvent(byte Mask, ushort X, ushort Y)
        {
            int i = 0;
            if ((Mask & 0x1) != 0)
                i |= 16;
            if ((Mask & 0x2) != 0)
                i |= 8;
            if ((Mask & 0x4) != 0)
                i |= 4;
            if (i != mouseModifiers)
            {
                if (mouseModifiers == 0)
                {
                    MouseMove(X, Y);
                    MousePress(i);
                }
                else
                    MouseRelease(mouseModifiers);
                mouseModifiers = i;
            }
            else
                MouseMove(X, Y);
        }
        /// <summary>
        /// Moves the cursor to the specified position on screen.
        /// </summary>
        /// <param name="x">The X coordinate where the cursor will be moved.</param>
        /// <param name="y">The Y coordinate where the cursor will be moved.</param>
        private static void MouseMove(int x, int y)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }
        /// <summary>
        /// Performs a mouse key press.
        /// </summary>
        /// <param name="button">The mouse button that should be pressed. Left, right, or middle</param>
        private static void MousePress(int button)
        {
            int dwFlags = 0;
            switch (button)
            {
                case (int)InputEvent.BUTTON1_MASK:
                    dwFlags |= MOUSEEVENTF_LEFTDOWN;
                    break;
                case (int)InputEvent.BUTTON2_MASK:
                    dwFlags |= MOUSEEVENTF_MIDDLEDOWN;
                    break;
                case (int)InputEvent.BUTTON3_MASK:
                    dwFlags |= MOUSEEVENTF_RIGHTDOWN;
                    break;
            }
            mouse_event(dwFlags, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Releases a pressed mouse button.
        /// </summary>
        /// <param name="button">The mouse button that should be released.</param>
        private static void MouseRelease(int button)
        {
            int dwFlags = 0;
            switch (button)
            {
                case (int)InputEvent.BUTTON1_MASK:
                    dwFlags |= MOUSEEVENTF_LEFTUP;
                    break;
                case (int)InputEvent.BUTTON2_MASK:
                    dwFlags |= MOUSEEVENTF_MIDDLEUP;
                    break;
                case (int)InputEvent.BUTTON3_MASK:
                    dwFlags |= MOUSEEVENTF_RIGHTUP;
                    break;
            }
            mouse_event(dwFlags, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Scrolls the mouse wheel
        /// </summary>
        /// <param name="wheel">The mouse wheel code.</param>
        private static void mouseWheel(int wheel)
        {
            mouse_event(0, 0, 0, wheel, IntPtr.Zero);
        }

        private enum InputEvent : int
        {
            BUTTON1_MASK = 16,
            BUTTON2_MASK = 8,
            BUTTON3_MASK = 4
        }

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;

        private enum _KeyEvent : int
        {
            VK_BACK = 0x08,
            VK_TAB = 0x09,

            /*
             * 0x0A - 0x0B : reserved
             */

            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D,

            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12,
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14,

            VK_KANA = 0x15,
            VK_HANGEUL = 0x15,   /* old name - should be here for compatibility */
            VK_HANGUL = 0x15,
            VK_JUNJA = 0x17,
            VK_readonly = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,

            VK_ESCAPE = 0x1B,

            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,

            VK_SPACE = 0x20,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C,
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_HELP = 0x2F,

            /*
             * VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
             * 0x40 : unassigned
             * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
             */

            VK_LWIN = 0x5B,
            VK_RWIN = 0x5C,
            VK_APPS = 0x5D,

            /*
             * 0x5E : reserved
             */

            VK_SLEEP = 0x5F,

            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A,
            VK_ADD = 0x6B,
            VK_SEPARATOR = 0x6C,
            VK_SUBTRACT = 0x6D,
            VK_DECIMAL = 0x6E,
            VK_DIVIDE = 0x6F,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,

            /*
             * 0x88 - 0x8F : unassigned
             */

            VK_NUMLOCK = 0x90,
            VK_SCROLL = 0x91,

            /*
             * NEC PC-9800 kbd definitions
             */
            VK_OEM_NEC_EQUAL = 0x92,    // '=' key on numpad

            /*
             * Fujitsu/OASYS kbd definitions
             */
            VK_OEM_FJ_JISHO = 0x92,    // 'Dictionary' key
            VK_OEM_FJ_MASSHOU = 0x93,    // 'Unregister word' key
            VK_OEM_FJ_TOUROKU = 0x94,    // 'Register word' key
            VK_OEM_FJ_LOYA = 0x95,    // 'Left OYAYUBI' key
            VK_OEM_FJ_ROYA = 0x96,    // 'Right OYAYUBI' key
            /*
             * 0xB8 - 0xB9 : reserved
             */

            VK_OEM_1 = 0xBA,   // ';:' for US
            VK_OEM_PLUS = 0xBB,   // '+' any country
            VK_OEM_COMMA = 0xBC,   // ',' any country
            VK_OEM_MINUS = 0xBD,   // '-' any country
            VK_OEM_PERIOD = 0xBE,   // '.' any country
            VK_OEM_2 = 0xBF,   // '/?' for US
            VK_OEM_3 = 0xC0,   // '`~' for US

            /*
             * 0xC1 - 0xD7 : reserved
             */

            /*
             * 0xD8 - 0xDA : unassigned
             */

            VK_OEM_4 = 0xDB,  //  '[{' for US
            VK_OEM_5 = 0xDC,  //  '\|' for US
            VK_OEM_6 = 0xDD,  //  ']}' for US
            VK_OEM_7 = 0xDE,  //  ''"' for US
            VK_OEM_8 = 0xDF,
            VK_OEM_CLEAR = 0xFE,
            /*
             * 0xE0 : reserved
             */

            /*
             * Various extended or enhanced keyboards
             */
            VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
            VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.

            /*
             * 0x97 - 0x9F : unassigned
             */

            /*
             * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
             * Used only as parameters to GetAsyncKeyState() and GetKeyState().
             * No other API or message will distinguish left and right keys in this way.
             */
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3,
            VK_LMENU = 0xA4,
            VK_RMENU = 0xA5,

            VK_BROWSER_BACK = 0xA6,
            VK_BROWSER_FORWARD = 0xA7,
            VK_BROWSER_REFRESH = 0xA8,
            VK_BROWSER_STOP = 0xA9,
            VK_BROWSER_SEARCH = 0xAA,
            VK_BROWSER_FAVORITES = 0xAB,
            VK_BROWSER_HOME = 0xAC,

            VK_VOLUME_MUTE = 0xAD,
            VK_VOLUME_DOWN = 0xAE,
            VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0,
            VK_MEDIA_PREV_TRACK = 0xB1,
            VK_MEDIA_STOP = 0xB2,
            VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4,
            VK_LAUNCH_MEDIA_SELECT = 0xB5,
            VK_LAUNCH_APP1 = 0xB6,
            VK_LAUNCH_APP2 = 0xB7,

            /*
             * VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
             * 0x40 : unassigned
             * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
             */
            VK_0 = 0x30,
            VK_1 = 0x31,
            VK_2 = 0x32,
            VK_3 = 0x33,
            VK_4 = 0x34,
            VK_5 = 0x35,
            VK_6 = 0x36,
            VK_7 = 0x37,
            VK_8 = 0x38,
            VK_9 = 0x39,

            VK_A = 65,
            VK_B = 66,
            VK_C = 67,
            VK_D = 68,
            VK_E = 69,
            VK_F = 70,
            VK_G = 71,
            VK_H = 72,
            VK_I = 73,
            VK_J = 74,
            VK_K = 75,
            VK_L = 76,
            VK_M = 77,
            VK_N = 78,
            VK_O = 79,
            VK_P = 80,
            VK_Q = 81,
            VK_R = 82,
            VK_S = 83,
            VK_T = 84,
            VK_U = 85,
            VK_V = 86,
            VK_W = 87,
            VK_X = 88,
            VK_Y = 89,
            VK_Z = 90
        }
        private static class keysym
        {
            const int DeadGrave = 0xFE50;
            const int DeadAcute = 0xFE51;
            const int DeadCircumflex = 0xFE52;
            const int DeadTilde = 0xFE53;

            const int BackSpace = 0xFF08;
            const int Tab = 0xFF09;
            const int Linefeed = 0xFF0A;
            const int Clear = 0xFF0B;
            const int Return = 0xFF0D;
            const int Pause = 0xFF13;
            const int ScrollLock = 0xFF14;
            const int SysReq = 0xFF15;
            const int Escape = 0xFF1B;

            const int Delete = 0xFFFF;

            const int Home = 0xFF50;
            const int Left = 0xFF51;
            const int Up = 0xFF52;
            const int Right = 0xFF53;
            const int Down = 0xFF54;
            const int PageUp = 0xFF55;
            const int PageDown = 0xFF56;
            const int End = 0xFF57;
            const int Begin = 0xFF58;

            const int Select = 0xFF60;
            const int Print = 0xFF61;
            const int Execute = 0xFF62;
            const int Insert = 0xFF63;

            const int Cancel = 0xFF69;
            const int Help = 0xFF6A;
            const int Break = 0xFF6B;
            const int NumLock = 0xFF6F;

            const int KpSpace = 0xFF80;
            const int KpTab = 0xFF89;
            const int KpEnter = 0xFF8D;

            const int KpHome = 0xFF95;
            const int KpLeft = 0xFF96;
            const int KpUp = 0xFF97;
            const int KpRight = 0xFF98;
            const int KpDown = 0xFF99;
            const int KpPrior = 0xFF9A;
            const int KpPageUp = 0xFF9A;
            const int KpNext = 0xFF9B;
            const int KpPageDown = 0xFF9B;
            const int KpEnd = 0xFF9C;
            const int KpBegin = 0xFF9D;
            const int KpInsert = 0xFF9E;
            const int KpDelete = 0xFF9F;
            const int KpEqual = 0xFFBD;
            const int KpMultiply = 0xFFAA;
            const int KpAdd = 0xFFAB;
            const int KpSeparator = 0xFFAC;
            const int KpSubtract = 0xFFAD;
            const int KpDecimal = 0xFFAE;
            const int KpDivide = 0xFFAF;

            const int KpF1 = 0xFF91;
            const int KpF2 = 0xFF92;
            const int KpF3 = 0xFF93;
            const int KpF4 = 0xFF94;

            const int Kp0 = 0xFFB0;
            const int Kp1 = 0xFFB1;
            const int Kp2 = 0xFFB2;
            const int Kp3 = 0xFFB3;
            const int Kp4 = 0xFFB4;
            const int Kp5 = 0xFFB5;
            const int Kp6 = 0xFFB6;
            const int Kp7 = 0xFFB7;
            const int Kp8 = 0xFFB8;
            const int Kp9 = 0xFFB9;

            const int F1 = 0xFFBE;
            const int F2 = 0xFFBF;
            const int F3 = 0xFFC0;
            const int F4 = 0xFFC1;
            const int F5 = 0xFFC2;
            const int F6 = 0xFFC3;
            const int F7 = 0xFFC4;
            const int F8 = 0xFFC5;
            const int F9 = 0xFFC6;
            const int F10 = 0xFFC7;
            const int F11 = 0xFFC8;
            const int F12 = 0xFFC9;
            const int F13 = 0xFFCA;
            const int F14 = 0xFFCB;
            const int F15 = 0xFFCC;
            const int F16 = 0xFFCD;
            const int F17 = 0xFFCE;
            const int F18 = 0xFFCF;
            const int F19 = 0xFFD0;
            const int F20 = 0xFFD1;
            const int F21 = 0xFFD2;
            const int F22 = 0xFFD3;
            const int F23 = 0xFFD4;
            const int F24 = 0xFFD5;

            const int ShiftL = 0xFFE1;
            const int ShiftR = 0xFFE2;
            const int ControlL = 0xFFE3;
            const int ControlR = 0xFFE4;
            const int CapsLock = 0xFFE5;
            const int ShiftLock = 0xFFE6;
            const int MetaL = 0xFFE7;
            const int MetaR = 0xFFE8;
            const int AltL = 0xFFE9;
            const int AltR = 0xFFEA;

            public static int toVK(int keysym)
            {
                switch (keysym)
                {
                    case BackSpace: return (int)_KeyEvent.VK_BACK;
                    case Tab: return (int)_KeyEvent.VK_TAB;

                    case Clear: return (int)_KeyEvent.VK_CLEAR;
                    case Return: return (int)_KeyEvent.VK_RETURN;
                    case Pause: return (int)_KeyEvent.VK_PAUSE;
                    case ScrollLock: return (int)_KeyEvent.VK_SCROLL;

                    case Escape: return (int)_KeyEvent.VK_ESCAPE;

                    case Delete: return (int)_KeyEvent.VK_DELETE;

                    case Home: return (int)_KeyEvent.VK_HOME;
                    case Left: return (int)_KeyEvent.VK_LEFT;
                    case Up: return (int)_KeyEvent.VK_UP;
                    case Right: return (int)_KeyEvent.VK_RIGHT;
                    case Down: return (int)_KeyEvent.VK_DOWN;
                    case PageUp: return (int)_KeyEvent.VK_PRIOR;
                    case PageDown: return (int)_KeyEvent.VK_NEXT;
                    case End: return (int)_KeyEvent.VK_END;
                    case Print: return (int)_KeyEvent.VK_SNAPSHOT;

                    case Insert: return (int)_KeyEvent.VK_INSERT;

                    //case Cancel: return (int)_KeyEvent.VK_CANCEL;
                    case Help: return (int)_KeyEvent.VK_HELP;

                    case NumLock: return (int)_KeyEvent.VK_NUMLOCK;

                    case KpSpace: return (int)_KeyEvent.VK_SPACE;
                    case KpTab: return (int)_KeyEvent.VK_TAB;
                    case KpEnter: return (int)_KeyEvent.VK_RETURN;

                    case KpHome: return (int)_KeyEvent.VK_HOME;
                    case KpLeft: return (int)_KeyEvent.VK_LEFT;
                    case KpUp: return (int)_KeyEvent.VK_UP;
                    case KpRight: return (int)_KeyEvent.VK_RIGHT;
                    case KpDown: return (int)_KeyEvent.VK_DOWN;
                    case KpPageUp: return (int)_KeyEvent.VK_PRIOR; // = KpPrior
                    case KpPageDown: return (int)_KeyEvent.VK_NEXT; // = KpNext
                    case KpEnd: return (int)_KeyEvent.VK_END;

                    case KpInsert: return (int)_KeyEvent.VK_INSERT;
                    case KpDelete: return (int)_KeyEvent.VK_DELETE;

                    case KpMultiply: return (int)_KeyEvent.VK_MULTIPLY;
                    case KpAdd: return (int)_KeyEvent.VK_ADD;
                    case KpSeparator: return (int)_KeyEvent.VK_SEPARATOR; // Sun should spellcheck...
                    case KpSubtract: return (int)_KeyEvent.VK_SUBTRACT;
                    case KpDecimal: return (int)_KeyEvent.VK_DECIMAL;
                    case KpDivide: return (int)_KeyEvent.VK_DIVIDE;

                    case KpF1: return (int)_KeyEvent.VK_F1;
                    case KpF2: return (int)_KeyEvent.VK_F2;
                    case KpF3: return (int)_KeyEvent.VK_F3;
                    case KpF4: return (int)_KeyEvent.VK_F4;

                    case Kp0: return (int)_KeyEvent.VK_NUMPAD0;
                    case Kp1: return (int)_KeyEvent.VK_NUMPAD1;
                    case Kp2: return (int)_KeyEvent.VK_NUMPAD2;
                    case Kp3: return (int)_KeyEvent.VK_NUMPAD3;
                    case Kp4: return (int)_KeyEvent.VK_NUMPAD4;
                    case Kp5: return (int)_KeyEvent.VK_NUMPAD5;
                    case Kp6: return (int)_KeyEvent.VK_NUMPAD6;
                    case Kp7: return (int)_KeyEvent.VK_NUMPAD7;
                    case Kp8: return (int)_KeyEvent.VK_NUMPAD8;
                    case Kp9: return (int)_KeyEvent.VK_NUMPAD9;

                    case F1: return (int)_KeyEvent.VK_F1;
                    case F2: return (int)_KeyEvent.VK_F2;
                    case F3: return (int)_KeyEvent.VK_F3;
                    case F4: return (int)_KeyEvent.VK_F4;
                    case F5: return (int)_KeyEvent.VK_F5;
                    case F6: return (int)_KeyEvent.VK_F6;
                    case F7: return (int)_KeyEvent.VK_F7;
                    case F8: return (int)_KeyEvent.VK_F8;
                    case F9: return (int)_KeyEvent.VK_F9;
                    case F10: return (int)_KeyEvent.VK_F10;
                    case F11: return (int)_KeyEvent.VK_F11;
                    case F12: return (int)_KeyEvent.VK_F12;
                    case F13: return (int)_KeyEvent.VK_F12;
                    case F14: return (int)_KeyEvent.VK_F12;
                    case F15: return (int)_KeyEvent.VK_F12;
                    case F16: return (int)_KeyEvent.VK_F12;
                    case F17: return (int)_KeyEvent.VK_F12;
                    case F18: return (int)_KeyEvent.VK_F12;
                    case F19: return (int)_KeyEvent.VK_F12;
                    case F20: return (int)_KeyEvent.VK_F12;
                    case F21: return (int)_KeyEvent.VK_F12;
                    case F22: return (int)_KeyEvent.VK_F12;
                    case F23: return (int)_KeyEvent.VK_F12;
                    case F24: return (int)_KeyEvent.VK_F12;

                    case CapsLock: return (int)_KeyEvent.VK_CAPITAL;
                    //No C# equivalent: case ShiftLock: return (int)_KeyEvent.;
                    default: return 0;
                }
            }
            public static KeyCode toVKCode(int key)
            {

                KeyCode localKeyCode = null;
                bool chk = false, altgr = false;
                int i = 0;
                Console.WriteLine(key);
                if ((key >= 65) && (key <= 90))
                {
                    i = key;
                    chk = true;
                }
                else if ((key >= 48) && (key <= 57))
                {
                    i = key;
                }
                else if ((key >= 97) && (key <= 122))
                {
                    i = key - 97 + 65;
                    chk = true;
                }
                else if (((key == 32)) || ((key >= 58) && (key <= 64)) || (key == 8364) || ((key >= 65360) && (key <= 65367)) || (key == 65535))
                {
                    i = key;
                    chk = true;
                }
                else if (((key >= 91) && (key <= 93)) || (key == 95) || (key == 96))
                {
                    i = key;
                    chk = true;
                }
                else if ((key >= 126) && (key <= 255))
                {
                    i = key;
                    chk = true;
                }
                else if ((key == 65507))
                {
                    altgr = !altgr;
                    if (altgr)
                    {
                        i = key;
                        chk = true;
                    }

                }
                else
                {
                    Console.WriteLine(key);
                    switch (key)
                    {
                        case 33:
                            i = 49;
                            chk = true;
                            break;
                        case 34:
                            i = 50;
                            chk = true;
                            break;
                        case 35:
                            i = 35;
                            chk = true;
                            break;
                        case 36:
                            i = 52;
                            chk = true;
                            break;
                        case 37:
                            i = 53;
                            chk = true;
                            break;
                        case 38:
                            i = 54;
                            chk = true;
                            break;
                        case 39:
                            i = 39;
                            chk = true;
                            break;
                        case 40:
                            i = 56;
                            chk = true;
                            break;
                        case 41:
                            i = 57;
                            chk = true;
                            break;
                        case 42:
                            i = 42;
                            chk = true;
                            break;
                        case 43:
                            i = (int)_KeyEvent.VK_OEM_PLUS;
                            chk = true;
                            break;
                        case 44:
                            i = 44;
                            chk = true;
                            break;
                        case 45:
                            i = 45;
                            chk = true;
                            break;
                        case 46:
                            i = 46;
                            chk = true;
                            break;
                        case 47:
                            i = 47;
                            chk = true;
                            break;
                        case 58:
                            i = 58;
                            chk = true;
                            break;
                        case 59:
                            i = 59;
                            chk = true;
                            break;
                        case 60:
                            i = 60;
                            chk = true;
                            break;
                        case 61:
                            i = 61;
                            chk = true;
                            break;
                        case 62:
                            i = 62;
                            chk = true;
                            break;
                        case 63:
                            i = 63;
                            chk = true;
                            break;
                        case 94:
                            i = 94;
                            chk = false;
                            break;
                        case 123:
                            i = 91;
                            chk = true;
                            break;
                        case 124:
                            i = 124;
                            chk = false;
                            break;
                        case 125:
                            i = 93;
                            chk = true;
                            break;
                        case 65429:
                            i = (int)_KeyEvent.VK_NUMPAD7;
                            chk = true;
                            break;
                        case 65430:
                            i = (int)_KeyEvent.VK_NUMPAD4;
                            chk = true;
                            break;
                        case 65431:
                            i = (int)_KeyEvent.VK_NUMPAD8;
                            chk = true;
                            break;
                        case 65432:
                            i = (int)_KeyEvent.VK_NUMPAD6;
                            chk = true;
                            break;
                        case 65433:
                            i = (int)_KeyEvent.VK_NUMPAD2;
                            chk = true;
                            break;
                        case 65434:
                            i = (int)_KeyEvent.VK_NUMPAD9;
                            chk = true;
                            break;
                        case 65435:
                            i = (int)_KeyEvent.VK_NUMPAD3;
                            chk = true;
                            break;
                        case 65436:
                            i = (int)_KeyEvent.VK_NUMPAD1;
                            chk = true;
                            break;
                        case 65437:
                            i = (int)_KeyEvent.VK_NUMPAD5;
                            chk = true;
                            break;
                        case 65438:
                            i = (int)_KeyEvent.VK_NUMPAD0;
                            chk = true;
                            break;
                        default:
                            i = toVKall(key);
                            break;
                    }
                }
                if (i != 0)
                {
                    localKeyCode = new KeyCode();
                    localKeyCode.key = i;
                    localKeyCode.isShift = chk;
                }
                return localKeyCode;
            }
            public static int toVKall(int keysym)
            {
                int key = toVK(keysym);
                if (key != 0)
                    return key;

                switch (keysym)
                {
                    case ShiftL: return (int)_KeyEvent.VK_LSHIFT;
                    case ShiftR: return (int)_KeyEvent.VK_RSHIFT;
                    case ControlL: return (int)_KeyEvent.VK_LCONTROL;
                    case ControlR: return (int)_KeyEvent.VK_RCONTROL;
                    case AltL: return (int)_KeyEvent.VK_LMENU;
                    case AltR: return (int)_KeyEvent.VK_RMENU;
                    default: return 0;
                }
            }
            public static string vkToString(int vk)
            {
                Console.WriteLine(vk);
                char[] c = new char[1];
                if (vk >= '0' && vk <= '9')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk >= 'A' && vk <= 'Z')
                {
                    c[0] = (char)vk;
                    return new String(c).ToLower();
                }
                if (vk == '.')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == ',')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 'è')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 'é')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '§')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '+')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '^')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '-')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '_')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 'ò')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '°')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 'ì')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '^')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '\\')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '(')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == ')')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '[')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == ']')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '%')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '&')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '#')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '$')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '\'')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '/')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '£')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '€')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 40)
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 41)
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == 94)
                {
                    return "^";
                }
                if (vk == 96)
                {
                    c[0] = '`';
                    return new String(c);
                }
                if (vk == 123)
                {
                    return "{";
                }
                if (vk == 124)
                {
                    return "|";
                }
                if (vk == 125)
                {
                    return "}";
                }
                if (vk == 126)
                {
                    return "~";
                }
                if (vk == 127)
                {
                    c[0] = '⌂';
                    return new String(c);
                }
                if (vk == 128)
                {
                    c[0] = 'Ç';
                    return new String(c);
                }
                if (vk == 129)
                {
                    c[0] = 'ü';
                    return new String(c);
                }
                if (vk == 130)
                {
                    c[0] = 'é';
                    return new String(c);
                }
                if (vk == 131)
                {
                    c[0] = 'â';
                    return new String(c);
                }
                if (vk == 132)
                {
                    c[0] = 'ä';
                    return new String(c);
                }
                if (vk == 133)
                {
                    c[0] = 'à';
                    return new String(c);
                }
                if (vk == 134)
                {
                    c[0] = 'å';
                    return new String(c);
                }
                if (vk == 135)
                {
                    c[0] = 'ç';
                    return new String(c);
                }
                if (vk == 136)
                {
                    c[0] = 'ê';
                    return new String(c);
                }
                if (vk == 137)
                {
                    c[0] = 'ë';
                    return new String(c);
                }
                if (vk == 138)
                {
                    c[0] = 'è';
                    return new String(c);
                }
                if (vk == 139)
                {
                    c[0] = 'ï';
                    return new String(c);
                }
                if (vk == 140)
                {
                    c[0] = 'î';
                    return new String(c);
                }
                if (vk == 141)
                {
                    c[0] = 'ì';
                    return new String(c);
                }
                if (vk == 142)
                {
                    c[0] = 'Ä';
                    return new String(c);
                }
                if (vk == 143)
                {
                    c[0] = 'Å';
                    return new String(c);
                }
                if (vk == 144)
                {
                    c[0] = 'É';
                    return new String(c);
                }
                if (vk == 145)
                {
                    c[0] = 'æ';
                    return new String(c);
                }
                if (vk == 146)
                {
                    c[0] = 'Æ';
                    return new String(c);
                }
                if (vk == 147)
                {
                    c[0] = 'ô';
                    return new String(c);
                }
                if (vk == 148)
                {
                    c[0] = 'ö';
                    return new String(c);
                }
                if (vk == 149)
                {
                    c[0] = 'ò';
                    return new String(c);
                }
                if (vk == 150)
                {
                    c[0] = 'û';
                    return new String(c);
                }
                if (vk == '!')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '+')
                {
                    c[0] = (char)vk;
                    return "+";
                }
                if (vk == 32)
                {
                    c[0] = (char)vk;
                    return new String(c);
                }
                if (vk == '\"')
                {
                    c[0] = (char)vk;
                    return new String(c);
                }

                if (vk == 65360)
                {
                    vk = (int)_KeyEvent.VK_HOME;
                }
                if (vk == 65361)
                {
                    vk = (int)_KeyEvent.VK_LEFT;
                }
                if (vk == 65363)
                {
                    vk = (int)_KeyEvent.VK_RIGHT;
                }
                if (vk == 65362)
                {
                    vk = (int)_KeyEvent.VK_UP;
                }
                if (vk == 65364)
                {
                    vk = (int)_KeyEvent.VK_DOWN;
                }
                if (vk == 65365)
                {
                    vk = (int)_KeyEvent.VK_PRIOR;
                }
                if (vk == 65366)
                {
                    vk = (int)_KeyEvent.VK_NEXT;
                }
                if (vk == 65367)
                {
                    vk = (int)_KeyEvent.VK_END;
                }
                if (vk == 65293)
                {
                    vk = (int)_KeyEvent.VK_RETURN;
                }
                if (vk == 65535)
                {
                    vk = (int)_KeyEvent.VK_DELETE;
                }
                switch (vk)
                {
                    case (int)_KeyEvent.VK_TAB:
                        return "{TAB}";
                    case (int)_KeyEvent.VK_RETURN:
                        return "{ENTER}";

                    //caseStringify(VK_CLEAR);
                    case (int)_KeyEvent.VK_LSHIFT:
                        return "+";
                    case (int)_KeyEvent.VK_RSHIFT:
                        return "+";
                    case (int)_KeyEvent.VK_SHIFT:
                        return "+";

                    //caseStringify(VK_CONTROL);
                    case (int)_KeyEvent.VK_LCONTROL:
                        return "^";
                    case (int)_KeyEvent.VK_RCONTROL:
                        return "^";
                    case (int)_KeyEvent.VK_CONTROL:
                        return "^";

                    case (int)_KeyEvent.VK_LMENU:
                        return "%";
                    case (int)_KeyEvent.VK_RMENU:
                        return "%";
                    case (int)_KeyEvent.VK_MENU:
                        return "%";

                    //caseStringify(VK_PAUSE);
                    case (int)_KeyEvent.VK_CAPITAL:
                        return "{CAPSLOCK}";
                    case (int)_KeyEvent.VK_ESCAPE:
                        return "{ESC}";
                    case (int)_KeyEvent.VK_SPACE:
                        return "{SPACE}";
                    case (int)_KeyEvent.VK_PRIOR:
                        return "{PGUP}";
                    case (int)_KeyEvent.VK_NEXT:
                        return "{PGDN}";
                    case (int)_KeyEvent.VK_END:
                        return "{END}";
                    case (int)_KeyEvent.VK_HOME:
                        return "{HOME}";
                    case (int)_KeyEvent.VK_LEFT:
                        return "{LEFT}";
                    case (int)_KeyEvent.VK_UP:
                        return "{UP}";
                    case (int)_KeyEvent.VK_RIGHT:
                        return "{RIGHT}";
                    case (int)_KeyEvent.VK_DOWN:
                        return "{DOWN}";

                    //caseStringify(VK_SELECT);
                    //caseStringify(VK_EXECUTE);

                    case (int)_KeyEvent.VK_SNAPSHOT:
                        return "{PRTSC}";
                    case (int)_KeyEvent.VK_INSERT:
                        return "{INSERT}";
                    case (int)_KeyEvent.VK_DELETE:
                        return "{DELETE}";
                    case (int)_KeyEvent.VK_HELP:
                        return "{HELP}";
                    case ((int)_KeyEvent.VK_LWIN | (int)_KeyEvent.VK_RWIN):
                        return "^{ESC}";

                    //caseStringify(VK_APPS);
                    //caseStringify(VK_SLEEP);

                    case (int)_KeyEvent.VK_NUMPAD0:
                        return "0";
                    case (int)_KeyEvent.VK_NUMPAD1:
                        return "1";
                    case (int)_KeyEvent.VK_NUMPAD2:
                        return "2";
                    case (int)_KeyEvent.VK_NUMPAD3:
                        return "3";
                    case (int)_KeyEvent.VK_NUMPAD4:
                        return "4";
                    case (int)_KeyEvent.VK_NUMPAD5:
                        return "5";
                    case (int)_KeyEvent.VK_NUMPAD6:
                        return "6";
                    case (int)_KeyEvent.VK_NUMPAD7:
                        return "7";
                    case (int)_KeyEvent.VK_NUMPAD8:
                        return "8";
                    case (int)_KeyEvent.VK_NUMPAD9:
                        return "9";

                    case (int)_KeyEvent.VK_MULTIPLY:
                        return "{MULTIPLY}";
                    case (int)_KeyEvent.VK_ADD:
                        return "{ADD}";
                    case (int)_KeyEvent.VK_SUBTRACT:
                        return "{SUBTRACT}";
                    case (int)_KeyEvent.VK_DIVIDE:
                        return "{DIVIDE}";

                    //caseStringify(VK_SEPARATOR);
                    //caseStringify(VK_DECIMAL);
                    case (int)_KeyEvent.VK_F1:
                        return "{F1}";
                    case (int)_KeyEvent.VK_F2:
                        return "{F2}";
                    case (int)_KeyEvent.VK_F3:
                        return "{F3}";
                    case (int)_KeyEvent.VK_F4:
                        return "{F4}";
                    case (int)_KeyEvent.VK_F5:
                        return "{F5}";
                    case (int)_KeyEvent.VK_F6:
                        return "{F6}";
                    case (int)_KeyEvent.VK_F7:
                        return "{F7}";
                    case (int)_KeyEvent.VK_F8:
                        return "{F8}";
                    case (int)_KeyEvent.VK_F9:
                        return "{F9}";
                    case (int)_KeyEvent.VK_F10:
                        return "{F10}";
                    case (int)_KeyEvent.VK_F11:
                        return "{F11}";
                    case (int)_KeyEvent.VK_F12:
                        return "{F12}";
                    case (int)_KeyEvent.VK_F13:
                        return "{F13}";
                    case (int)_KeyEvent.VK_F14:
                        return "{F14}";
                    case (int)_KeyEvent.VK_F15:
                        return "{F15}";
                    case (int)_KeyEvent.VK_F16:
                        return "{F16}";

                    case (int)_KeyEvent.VK_NUMLOCK:
                        return "{NUMLOCK}";
                    case (int)_KeyEvent.VK_SCROLL:
                        return "{SCROLLLOCK}";
                    case (int)_KeyEvent.VK_OEM_PLUS:
                        return "{+}";
                    case (int)_KeyEvent.VK_OEM_MINUS:
                        return "{-}";
                    case (int)_KeyEvent.VK_OEM_COMMA:
                        return ",";
                    case (int)_KeyEvent.VK_OEM_PERIOD:
                        return ".";
                    case (int)_KeyEvent.VK_OEM_2:
                        return "{DIVIDE}"; //or ?
                    case (int)_KeyEvent.VK_OEM_3:
                        return "{~}";
                    case (int)_KeyEvent.VK_OEM_4:
                        return "{[}";
                    case (int)_KeyEvent.VK_OEM_6:
                        return "{]}";

                    //caseStringify(VK_OEM_5);  //  '\|' for US
                    //caseStringify(VK_OEM_7);  //  ''"' for US
                    //caseStringify(VK_OEM_102);  //  "<>" or "\|" on RT 102-key kbd.
                }
                c[0] = (char)vk;
                return new String(c);
            }
        }
    }
}