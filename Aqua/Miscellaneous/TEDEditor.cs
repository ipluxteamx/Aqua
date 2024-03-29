﻿using Cosmos.Core;
using System;
using System.IO;
using System.Linq;

namespace Aqua.Miscellaneous
{
    /*
     * Project Avalanche
     * Source code by ipluxteamx.
     * Software for Aqua System 0.2.1
     *
     * Version 0.1.2
     * Milestone 2
     */

    // clipboard class that copies the line from the cursorY position
    public class Clipboard
    {
        public static string line;

        public Clipboard()
        {
            line = "";
        }

        public void Copy(string[] lines, int cursorY)
        {
            Clipboard.line = lines[cursorY - 2];
        }

        public void Cut(string[] lines, int cursorY) 
        {
            Clipboard.line = lines[cursorY - 2];
            lines[cursorY - 2] = "";
        }

        public void Paste(string[] lines, int cursorY)
        {
            lines[cursorY - 2] = Clipboard.line;
        }
    }

    public class TEDEditor
    {
        public static string Run(string[] args)
        {
            try
            {
                Load(args);
            }
            catch (Exception ex)
            {
                Terminal.Screen.DebugWrite(ex.ToString(), 4);
            }
            return null;
        }

        public static void Load(string[] args)
        {
            string path = null;

            if (!Directory.Exists("0:\\AquaSys\\Config"))
                Directory.CreateDirectory("0:\\AquaSys\\Config");

            if (!Directory.Exists("0:\\AquaSys\\Config\\TED"))
                Directory.CreateDirectory("0:\\AquaSys\\Config\\TED");

            for (int i = 0; i < args.Length; i++)
            {
                // Debugging only.
                // This is only used for debugging purposes.
                // Console.WriteLine(args[i]);

                if (args[i] == "-h") { }
                else if (path == null)
                {
                    path = args[i];

                    if (!path.Contains("\\"))
                    {
                        // Set the path to (for example) : "0:\AquaSys\" + "file.txt".
                        path = $"{Kernel.currentDirectory}{path}";

                        // Replace all the "\\" to "\".
                        path = path.Replace("\\\\", "\\");
                    }
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                        File.WriteAllText(
                            path,
                            "This is the default TED Editor message.\nTo remove every line, simply press CTRL+K."
                        );
                    }

                    Console.Clear();
                    Editor(path);

                    // Console.WriteLine($"Path : {path}");
                }
                else
                    Terminal.Screen.DebugWrite("Unknown argument : " + args[i], 4);
            }
        }

        public static bool DrawUpperBar(int x, int y, string path, string oldC, string newC)
        {
            try
            {
                Console.SetCursorPosition(x, y);

                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;

                for (int screenX = x; screenX < Console.WindowWidth; screenX++)
                    Console.Write(' ');

                string programName = "Project Avalanche | version 0.1.2";
                Console.SetCursorPosition(x, y);
                Console.Write(programName);

                Console.SetCursorPosition(Console.WindowWidth - path.Length, y);
                Console.Write(path);

                if (newC != oldC)
                    Console.BackgroundColor = ConsoleColor.Red;
                else
                    Console.BackgroundColor = ConsoleColor.DarkGreen;

                Console.ForegroundColor = ConsoleColor.White;

                Console.SetCursorPosition(x, y + 1);
                for (int screenX = x; screenX < Console.WindowWidth; screenX++)
                    Console.Write(' ');

                Console.SetCursorPosition(x, y + 1);
                if (newC.Length != 0)
                {
                    int chars = newC.Length;
                    chars--;
                    Console.Write("Characters : [" + chars + "]");
                }
                else
                    Console.Write("Characters : [No characters yet]");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(x, y + 2);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string[] lines;
        private static string lineToInsert,
            newLine;

        public static void Editor(string path)
        {
            string fileContents = File.ReadAllText(path),
                oldFC = File.ReadAllText(path);
            int defaultYPos = 2;

            Clipboard clipboard = new Clipboard();

            // Draw the information and title bar.
            DrawUpperBar(0, 0, path, oldFC, fileContents);

            Console.SetCursorPosition(0, defaultYPos);
            Console.Write(fileContents);

            int cursorX = Console.CursorLeft,
                cursorY = Console.CursorTop;
            Console.SetCursorPosition(cursorX, cursorY);

            while (true)
            {
                // Draw the information and title bar.
                DrawUpperBar(0, 0, path, oldFC, fileContents);

                Console.SetCursorPosition(0, defaultYPos);
                Console.Write(fileContents);

                Console.SetCursorPosition(cursorX, cursorY);

                var input = Console.ReadKey(true);
                /* It's adding a new line to the fileContents string. */
                switch (input.Key)
                {
                    case ConsoleKey.LeftArrow:
                        TEDEditor.lines = fileContents.Split('\n');
                        if (cursorX > 0)
                            cursorX--;
                        else if (cursorY > defaultYPos)
                        {
                            cursorX = TEDEditor.lines[cursorY - defaultYPos].Length;
                            cursorY--;
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        // Split the input string into lines
                        TEDEditor.lines = fileContents.Split('\n');
                        if (
                            cursorY - defaultYPos < TEDEditor.lines.Length - 1
                            && cursorX < TEDEditor.lines[cursorY - defaultYPos].Length
                        )
                            cursorX++;
                        else if (cursorY - defaultYPos < TEDEditor.lines.Length - 1)
                        {
                            cursorX = 0;
                            cursorY++;
                        }
                        break;

                    case ConsoleKey.UpArrow:
                        if (cursorY > defaultYPos)
                        {
                            cursorY--;
                            if (cursorX > fileContents.Split('\n')[cursorY - 2].Length)
                                cursorX = fileContents.Split('\n')[cursorY - 2].Length;
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (cursorY < fileContents.Split('\n').Length + 1)
                        {
                            cursorY++;
                            if (cursorX > fileContents.Split('\n')[cursorY - 2].Length)
                                cursorX = fileContents.Split('\n')[cursorY - 2].Length;
                        }
                        break;

                    case ConsoleKey.Home:
                        cursorX = 0;
                        break;

                    case ConsoleKey.End:
                        // Split the input string into lines
                        TEDEditor.lines = fileContents.Split('\n');

                        // Determine the line where the new text should be inserted
                        var line = TEDEditor.lines[cursorY - defaultYPos];
                        cursorX = line.Length;
                        break;

                    case ConsoleKey.Enter:
                        // Split the input string into lines
                        TEDEditor.lines = fileContents.Split('\n');
                        TEDEditor.lines[cursorY - defaultYPos] += "\n";

                        fileContents = string.Join('\n', TEDEditor.lines);
                        cursorX = 0;
                        cursorY++;
                        break;

                    case ConsoleKey.Backspace:
                        if (fileContents.Length != 0 || cursorX > 1)
                        {
                            // Split the input string into lines
                            TEDEditor.lines = fileContents.Split('\n');

                            // Determine the line where the new text should be inserted
                            TEDEditor.lineToInsert = TEDEditor.lines[cursorY - defaultYPos];

                            if (cursorX > 0) // Check if cursorX is greater than 0
                            {
                                TEDEditor.newLine =
                                    TEDEditor.lineToInsert.Substring(0, cursorX - 1)
                                    + TEDEditor.lineToInsert.Substring(cursorX);
                                TEDEditor.lines[cursorY - defaultYPos] = TEDEditor.newLine;

                                cursorX--;
                                Console.SetCursorPosition(cursorX, cursorY);
                                Console.Write(' ');

                                var PHxPos = Console.CursorLeft;
                                var PHyPos = Console.CursorTop;

                                Console.SetCursorPosition(
                                    TEDEditor.lineToInsert.Length - 1,
                                    cursorY
                                );
                                Console.Write(' ');

                                Console.SetCursorPosition(PHxPos, PHyPos);
                            }
                            else if (cursorY > defaultYPos)
                            {
                                // Remove the last "\n" from the file contents
                                int lastNewLineIndex = fileContents.LastIndexOf(
                                    '\n',
                                    fileContents.Length - 2
                                );
                                if (lastNewLineIndex >= 0)
                                    fileContents = fileContents.Substring(0, lastNewLineIndex);

                                TEDEditor.lines = fileContents.Split('\n');
                                cursorY--;
                                if (cursorY - defaultYPos >= 0)
                                {
                                    if (TEDEditor.lines[cursorY - defaultYPos] == "")
                                        cursorX = TEDEditor.lines[cursorY - defaultYPos].Length;
                                    else
                                        cursorX = TEDEditor.lines[cursorY - defaultYPos].Length;
                                }
                                else
                                    cursorX = 0;

                                Console.SetCursorPosition(cursorX, cursorY);
                            }

                            fileContents = string.Join('\n', TEDEditor.lines);
                        }
                        break;

                    case ConsoleKey.Escape:
                        Console.Clear();
                        return;

                    default:
                        if ((input.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            switch (input.Key)
                            {
                                case ConsoleKey.Q:
                                    Console.Clear();
                                    return;

                                case ConsoleKey.S:
                                    if ((input.Modifiers & ConsoleModifiers.Shift) != 0)
                                        File.WriteAllText(path, Decryption.Encrypt(fileContents));
                                    else
                                        File.WriteAllText(path, fileContents);
                                    oldFC = fileContents;
                                    break;

                                case ConsoleKey.C:
                                    // Split the input string into lines
                                    TEDEditor.lines = fileContents.Split('\n');
                                    clipboard.Copy(TEDEditor.lines, cursorY);
                                    fileContents = string.Join('\n', TEDEditor.lines);
                                    break;

                                case ConsoleKey.Z:
                                    fileContents = oldFC;
                                    break;
                                
                                case ConsoleKey.X:
                                    // Split the input string into lines
                                    TEDEditor.lines = fileContents.Split('\n');
                                    clipboard.Cut(TEDEditor.lines, cursorY);
                                    fileContents = string.Join('\n', TEDEditor.lines);
                                    break;

                                case ConsoleKey.V:
                                    // Split the input string into lines
                                    TEDEditor.lines = fileContents.Split('\n');
                                    clipboard.Paste(TEDEditor.lines, cursorY);
                                    fileContents = string.Join('\n', TEDEditor.lines);
                                    break;

                                case ConsoleKey.K:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    fileContents = "";

                                    cursorX = 0;
                                    cursorY = defaultYPos;
                                    Console.Write(' ');
                                    cursorX = 0;

                                    Console.Clear();
                                    Console.SetCursorPosition(0, defaultYPos);
                                    break;
                            }
                        }
                        else if (
                            cursorY > defaultYPos || cursorY < defaultYPos + TEDEditor.lines.Length
                        )
                        {
                            // Split the input string into lines
                            TEDEditor.lines = fileContents.Split('\n');

                            // Determine the line where the new text should be inserted
                            TEDEditor.lineToInsert = TEDEditor.lines[cursorY - defaultYPos];

                            if (lineToInsert.Length > 0)
                                TEDEditor.newLine =
                                    TEDEditor.lineToInsert.Substring(0, cursorX)
                                    + input.KeyChar.ToString()
                                    + TEDEditor.lineToInsert.Substring(cursorX);
                            else
                                TEDEditor.newLine = input.KeyChar.ToString();

                            TEDEditor.lines[cursorY - defaultYPos] = TEDEditor.newLine;
                            fileContents = string.Join('\n', lines);
                            cursorX++;
                        }
                        break;
                }
            }
        }
    }
}
