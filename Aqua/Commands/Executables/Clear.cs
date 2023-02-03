﻿using System;

namespace Aqua.Commands.Executables
{
    public class Clear : Command
    {
        public Clear(String name, String description) : base(name, description) { }

        public override string Execute(string[] args)
        {
            Console.Clear();
            return "";
        }
    }
}
