﻿using Aqua.Terminal;
using Cosmos.Core;
using Cosmos.System.ScanMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Aqua.Commands.Executables
{
    public class Set : Command
    {
        public Set(string name, string description) : base(name, description) { }

        public override string Execute(string[] args)
        {
            switch (args[0])
            {
                case "bg":
                    switch (args[1])
                    {
                        case "blue":
                            Kernel.bgColor = ConsoleColor.Blue;
                            break;

                        case "green":
                            Kernel.bgColor = ConsoleColor.Green;
                            break;

                        case "red":
                            Kernel.bgColor = ConsoleColor.Red;
                            break;

                        case "orange":
                            Kernel.bgColor = ConsoleColor.Yellow;
                            break;

                        case "black":
                            Kernel.bgColor = ConsoleColor.Black;
                            break;

                        case "white":
                            Kernel.bgColor = ConsoleColor.White;
                            break;

                        case "cyan":
                            Kernel.bgColor = ConsoleColor.Cyan;
                            break;

                        default:
                            return "Please select a correct color.";
                    }
                    Filesystem.Utilities.WriteLine(@"0:\AquaSys\Config\Colors.acf", args[1], false);
                    Console.Clear();
                    return null;

                case "fg":
                    switch (args[1])
                    {
                        case "blue":
                            Kernel.fgColor = ConsoleColor.Blue;
                            break;

                        case "green":
                            Kernel.fgColor = ConsoleColor.Green;
                            break;

                        case "red":
                            Kernel.fgColor = ConsoleColor.Red;
                            break;

                        case "orange":
                            Kernel.fgColor = ConsoleColor.Yellow;
                            break;

                        case "black":
                            Kernel.fgColor = ConsoleColor.Black;
                            break;

                        case "white":
                            Kernel.fgColor = ConsoleColor.White;
                            break;

                        case "cyan":
                            Kernel.fgColor = ConsoleColor.Cyan;
                            break;

                        default:
                            return Terminal.Terminal.DebugWrite("Please select a correct color.", 4);
                    }
                    Filesystem.Utilities.WriteLine(@"0:\AquaSys\Config\Colors.acf", args[1], true);
                    return null;

                case "keymap":
                    switch (args[1])
                    {
                        case "us":
                            Cosmos.System.KeyboardManager.SetKeyLayout(new US_Standard());
                            break;

                        case "fr":
                            Cosmos.System.KeyboardManager.SetKeyLayout(new FR_Standard());
                            break;

                        case "de":
                            Cosmos.System.KeyboardManager.SetKeyLayout(new DE_Standard());
                            break;

                        default:
                            return Terminal.Terminal.DebugWrite("Please select a correct key mapping.", 4);
                    }
                    System.IO.File.WriteAllText(@"0:\AquaSys\Config\KeyMap.acf", args[1]);
                    return Terminal.Terminal.DebugWrite($"Successfully set the keyboard to \"{args[1]}\".", 2);

                default:
                    return Terminal.Terminal.DebugWrite("Specify a correct argument.\n", 4);
            }
        }
    }

    public class Get : Command
    {
        public Get(string name, string description) : base(name, description) { }

        public override string Execute(string[] args)
        {
            switch (args[0])
            {
                case "ram":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    double ramUsage = (double)(Cosmos.Core.GCImplementation.GetUsedRAM() / 1024 / 1024) / (double)Cosmos.Core.CPU.GetAmountOfRAM();
                    return "RAM usage : " + ((int)ramUsage * 100).ToString() + "%";

                default:
                    return Terminal.Terminal.DebugWrite("Specify a correct argument.", 4);
            }
        }
    }
}
