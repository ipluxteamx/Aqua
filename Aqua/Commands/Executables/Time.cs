﻿using System;

namespace Aqua.Commands.Executables
{
    public class Time : Command
    {
        public Time(string name, string description)
            : base(name, description) { }

        public override string Execute(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            return $"{Cosmos.HAL.RTC.Hour.ToString("D2")}:{Cosmos.HAL.RTC.Minute.ToString("D2")}:{Cosmos.HAL.RTC.Second.ToString("D2")}";
        }
    }
}
