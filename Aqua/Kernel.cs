/* The kernel of Aqua. */
using Aqua.Commands;
using Aqua.Terminal.Accounts;

using Cosmos.HAL.Drivers.PCI.Audio;
using Cosmos.System;
using Cosmos.System.Audio;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

using System;
using Console = System.Console;
using Sys = Cosmos.System;

using term = Aqua.Terminal.Terminal;
using Aqua.ErrorHandler;
using Cosmos.System.Graphics.Fonts;

using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using IL2CPU.API.Attribs;
using Cosmos.System.ScanMaps;
using System.Collections.Generic;

using PrismTools;
using System.IO;

namespace Aqua
{
    public class Tab
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public List<string> Screen { get; set; }

        public Tab(int tabNumber, string name)
        {
            this.Index = tabNumber;
            this.Name = name;
        }

        public static void Add(string name)
        {
            Kernel.tabs.Add(new Tab(Kernel.tabs.Count, name));
        }

        public static void Change(int index)
        {
            Type type = Type.GetType("Aqua.Tab");
            if (Kernel.tabs[index].GetType() == type)
            {
                try
                {
                    Kernel.tabs[Kernel.currentTabIndex].Screen = Kernel.textStorage;

                    Kernel.currentTabIndex = index;
                    Kernel.textStorage.Clear();
                    Console.Clear();

                    if (Kernel.tabBarVisible)
                        Console.WriteLine();

                    for (int i = 0; i < Kernel.tabs[index].Screen.Count; i++)
                        Console.WriteLine(Kernel.tabs[index].Screen[i]);
                }
                catch (Exception ex)
                {
                    term.DebugWrite(ex.ToString(), 4);
                }
            }
            else
                term.DebugWrite("Index not valid.", 4);
        }

        public static void Delete(int index)
        {
            if (Kernel.tabs[index] != null && Kernel.currentTabIndex != index)
            {
                try
                {
                    string name = Kernel.tabs[index].Name;
                    Kernel.tabs.Remove(Kernel.tabs[index]);

                    term.DebugWrite($"Successfully deleted the tab: {name}.", 2);
                }
                catch (Exception e)
                {
                    term.DebugWrite(e.ToString(), 4);
                }
            }
        }
    }

    public class Kernel : Sys.Kernel
    {
        private Manager _commandManager = new Manager();
        public static CosmosVFS fs;

        public static bool isRoot, guiStarted, isNetworkConnected, tabBarVisible = false;
        public static string currentDirectory = "0:\\";

        public static AudioMixer mixer;
        public static AC97 audioDriver;
        public static AudioManager audioManager;

        public static ConsoleColor bgColor = ConsoleColor.Black, fgColor = ConsoleColor.White;

        public static List<Tab> tabs = new List<Tab>();
        public static int currentTabIndex;

        public static Canvas canvas;
        public static string time;

        [ManifestResourceStream(ResourceName = "Aqua.Fonts.zap-ext-vga09.psf")] public static byte[] font;

        //public static PCScreenFont Font = PCScreenFont.Default;
        //public static byte[] FontVga;

        public static string version = "0.3.1";

        public static (int Left, int Top) cursorPos;
        public static List<string> textStorage = new List<string>();

        // This is the "Setup" function, executed if the run is the first run ever.
        public void FirstRun()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Setting up things for you...");

            // Detect if the emulator is NOT VirtualBox or QEMU.
            // These emulators/virtualizers cannot support the filesystem completely for now. 
            if (!VMTools.IsVirtualBox || !VMTools.IsQEMU)
            {
                // Create the AquaSys directory.
                // THIS IS OBLIGATORY FOR MOST SYSTEM APPS.
                if (!System.IO.Directory.Exists(@"0:\AquaSys"))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(@"0:\AquaSys");

                        term.DebugWrite("Successfully made the AquaSys directory.\n", 1);

                        Cosmos.HAL.Global.PIT.Wait(500);
                    }
                    catch (Exception ex)
                    {
                        CrashHandler.Handle(ex);
                    }
                }

                // Create the one-time Setup folder.
                // This folder plays a role in First Time-detecting.
                if (!System.IO.Directory.Exists(@"0:\AquaSys\Setup"))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(@"0:\AquaSys\Setup");

                        term.DebugWrite("Successfully made the Setup directory.\n", 1);

                        Cosmos.HAL.Global.PIT.Wait(500);
                    }
                    catch (Exception ex)
                    {
                        CrashHandler.Handle(ex);
                    }
                }

                // Make the FirstRun.acf file.
                try
                {
                    System.IO.File.Create(@"0:\AquaSys\Setup\FirstRun.acf");
                    System.IO.File.WriteAllText("0:\\AquaSys\\Setup\\FirstRun.acf", "true");

                    term.DebugWrite("Successfully made the FirstRun.acf file.\n", 1);
                }
                catch (Exception ex)
                {
                    term.DebugWrite(ex.ToString(), 4);
                }

                /* Calling the WriteReadme method of the Readme class in the Miscellaneous namespace. */
                Miscellaneous.Readme.WriteReadme();
            }
            else
            {
                term.DebugWrite("Your emulator/virtualizer does not support the VFS.", 4);
            }

            /* Clearing the screen. */
            Cosmos.HAL.Global.PIT.Wait(750);
            Console.Clear();
        }

        /// <summary>
        /// Checks if a file exists, if it does, it reads the content of the file and sets the
        /// keyboard layout accordingly
        /// </summary>
        public void CheckKeyboardLayout()
        {
            /* Checking if the file exists. */
            if (System.IO.File.Exists(@"0:\AquaSys\Config\KeyMap.acf"))
            {
                /* Reading the content of the file KeyMap.acf and then it is setting the keyboard
                layout based on the content of the file. */
                var content = System.IO.File.ReadAllText(@"0:\AquaSys\Config\KeyMap.acf");

                switch (content)
                {
                    /* Setting the keyboard layout to US_Standard. */
                    case "us":
                        KeyboardManager.SetKeyLayout(new US_Standard());
                        break;

                    /* Setting the keyboard layout to the French keyboard layout. */
                    case "fr":
                        KeyboardManager.SetKeyLayout(new FR_Standard());
                        break;

                    /* Setting the keyboard layout to the German Standard layout. */
                    case "de":
                        KeyboardManager.SetKeyLayout(new DE_Standard());
                        break;
                }
            }
            else
            {
                KeyboardManager.SetKeyLayout(new US_Standard());
            }
        }

        // This function runs before the main loop, which is Run().
        // It is used to initiate drivers, the filesystem, check if this run is the first run, and set settings.
        protected override void BeforeRun()
        {
            /* Setting the size of the console window to 90x60. */
            Console.SetWindowSize(90, 60);

            // /* Loading a font into the operating system. */
            var f = PCScreenFont.LoadFont(font);
            VGAScreen.SetFont(f.CreateVGAFont(), f.Height);

            try
            {
                /* Registering the CosmosVFS with the VFSManager. */
                fs = new CosmosVFS();
                VFSManager.RegisterVFS(fs);
                System.IO.Directory.SetCurrentDirectory(currentDirectory);

                // fs.Disks[0].FormatPartition(0, "FAT32");

                // Check for format input.
                // Note : Replaced by the Disk Utility/Recovery Disk
                // FormatCheck();
            }
            catch (Exception ex)
            {
                CrashHandler.Handle(ex);
            }

            // Give time for the initialization to succeed
            Cosmos.HAL.Global.PIT.Wait(250);
            Console.Clear();

            CheckKeyboardLayout();

            if (!System.IO.File.Exists("0:\\AquaSys\\Setup\\FirstRun.acf") || System.IO.File.ReadAllText("0:\\AquaSys\\Setup\\FirstRun.acf") != "true" || fs.Disks[0].Partitions == null)
                FirstRun();

            isNetworkConnected = false;

            currentTabIndex = 0;
            tabs.Add(new Tab(0, "Default"));

            Cosmos.HAL.Global.PIT.Wait(750);
            LoginSystem.Start();
        }
        
        protected override void Run()
        {
            //Console.WriteLine($"Collected : {Heap.Collect()} objects.");
            Heap.Collect();

            if (guiStarted)
            {
                Graphics.Graphics.Draw(canvas);
            }
            else
            {
                // Draw the upper bar, with the time, name of the OS and version.
                // I need to find a way to update it while still being able to use the shell, and vice-versa.
                DrawBar();

                // Draw the upper bar, but this time, it shows the current tabs that are open.
                if (tabBarVisible)
                    DrawTabBar();

                // The main shell, source of almost all my problems.
                AquaShell();

                // Set the current colors to the specified colors.
                // Can set them using the "set" command.
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
            }
        }

        private void AquaShell()
        {
            // Change the text color to Dark Green.
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"{currentDirectory}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("$=> ");

            // Set the current colors to the specified colors, again.
            // Can set them using the "set" command.
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;

            // Listen for input, then respond to that input accordingly using the command manager.
            string response;
            string input = Console.ReadLine();

            /*if (tabs[currentTabIndex] != null)
                tabs[currentTabIndex].AddLine($"{currentDirectory}$=> {input}");*/

            response = _commandManager.ProcessInput(input);

            Console.WriteLine($"  {response}\n");
            textStorage.Add($"  {response}\n");
        }

        public static void DrawBar()
        {
            //var time = DateTime.Now.ToString("HH:mm");
            /* Drawing the top bar of the console. */
            var developmentStatus = "Alpha | Milestone 2";

            time = $"{Cosmos.HAL.RTC.Hour.ToString("D2")}:{Cosmos.HAL.RTC.Minute.ToString("D2")}";
            //time = DateTime.Now.ToString("t");
            cursorPos = Console.GetCursorPosition();

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;

            /* Setting the cursor position to the top left of the console window, then it is writing a
            space to the console window for the width of the console window. */
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Console.WindowWidth; i++) Console.Write(' ');

            Console.SetCursorPosition(0, 0);
            Console.Write("Aqua | Version " + version);

            DrawTime(time);

            /* Setting the cursor position to the bottom right of the console window and writing the
            development status. */
            Console.SetCursorPosition(Console.WindowWidth - developmentStatus.Length, 0);
            Console.Write(developmentStatus);

            Console.SetCursorPosition(cursorPos.Left, cursorPos.Top);
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;
        }

        /// <summary>
        /// It draws a tab bar.
        /// </summary>
        public static void DrawTabBar()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.SetCursorPosition(0, 1);
            for (int i = 0; i < Console.WindowWidth; i++) Console.Write(' ');

            Console.SetCursorPosition(0, 1);
            foreach (Tab tab in tabs)
            {
                if (tab.Index == currentTabIndex)
                    Console.BackgroundColor = ConsoleColor.Green;
                else
                    /* Changing the background color of the console to gray. */
                    Console.BackgroundColor = ConsoleColor.Gray;

                Console.Write($"[ {tab.Index} | {(tab.Index == currentTabIndex ? "> " : "")}{tab.Name} ]");
            }

            Console.SetCursorPosition(cursorPos.Left, cursorPos.Top);
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = fgColor;
        }

        public static void DrawTime(string time)
        {
            var oldCurPos = Console.GetCursorPosition();

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition((Console.WindowWidth / 2 + 1) - (time.Length / 2) - 2, 0);
            Console.Write(time);

            Console.SetCursorPosition(oldCurPos.Left, oldCurPos.Top);
        }

        public static void StartUp()
        {
            try
            {
                term.DebugWrite("Setting up the network...", 1);
                // Setup the network / Generate an IP address dynamically
                if (!isNetworkConnected) Network.Network.Setup();

                // Network.Network.DownloadFile("http://info.cern.ch/hypertext/WWW/TheProject.html", @"0:\File.txt");
                if (!VMTools.IsVMWare)
                {
                    term.DebugWrite("Setting up the audio drivers...", 0);

                    /* Initializing the audio driver and mixer. */
                    audioDriver = AC97.Initialize(bufferSize: 4096);
                    mixer = new AudioMixer();

                    audioManager = new AudioManager()
                    {
                        Stream = mixer,
                        Output = audioDriver
                    };

                    audioManager.Enable();
                }

                Cosmos.HAL.Global.PIT.Wait(750);
                Console.Clear();
            }
            catch (Exception e)
            {
                CrashHandler.Handle(e);
            }

            DrawBar();
            // DrawTabBar();

            Console.SetCursorPosition(0, 1);

            // Introduction
            string welcome = "Welcome to the world of Aqua.";

            Console.WriteLine();
            // Center the cursor position
            Console.SetCursorPosition((Console.WindowWidth / 2) - (welcome.Length / 2) - 1, Console.CursorTop);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(welcome + "\n");

            string versionInfo = @"Aqua | Version " + version + " | Codename 'Paris'";
            string[] devStatus =
            {
                "Current development status : ",
                "Alpha | Milestone 2\n"
            };

            /* Setting the cursor position to the center of the console window, then writing the
            version info. */
            Console.SetCursorPosition((Console.WindowWidth / 2) - (versionInfo.Length / 2) - 1, Console.CursorTop);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(versionInfo);

            Console.SetCursorPosition((Console.WindowWidth / 2) - ((devStatus[0].Length + devStatus[1].Length) / 2) - 1, Console.CursorTop);
            Console.Write(devStatus[0]);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(devStatus[1]);

            cursorPos = Console.GetCursorPosition();

            /* Checking if the machine is not a VMWare virtual machine. If it isn't, it will play the logon
            sound. */
            if (!VMTools.IsVMWare)
                Sounds.Sounds.LogonSound();
        }
    }
}
