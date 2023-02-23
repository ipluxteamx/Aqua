﻿using System;

namespace Aqua.Commands.Executables
{
    public class Time : Command
    {
        public Time (string name, string description) : base (name, description) { }

        public override string Execute(string[] args)
        {
            switch (args[0])
            {
                case "compile":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return Miscellaneous.Compilation.Date + " | " + Miscellaneous.Compilation.Time;

                case "direct":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return $"{Cosmos.HAL.RTC.Hour.ToString("D2")}:{Cosmos.HAL.RTC.Minute.ToString("D2")}:{Cosmos.HAL.RTC.Second.ToString("D2")}";

                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  Next time, please specify an argument [\"compile\" or \"direct\"].");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    return $"{Cosmos.HAL.RTC.Hour.ToString("D2")}:{Cosmos.HAL.RTC.Minute.ToString("D2")}:{Cosmos.HAL.RTC.Second.ToString("D2")}";
            }
        }
    }
}
