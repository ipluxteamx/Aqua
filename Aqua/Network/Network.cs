﻿using Aqua.Commands;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Text;
using term = Aqua.Terminal.Screen;
using Cosmos.System.Network.IPv4.UDP.DNS;
using System.IO;

namespace Aqua.Network
{
    public class Network
    {
        public Network() { }
        public static DHCPClient xClient = new DHCPClient();
        public static void Setup()
        {
            /** Send a DHCP Discover packet **/
            //This will automatically set the IP config after DHCP response
            xClient.SendDiscoverPacket();

            Address dnsAddress = new Address(8, 8, 8, 8);
            DNSConfig.Add(dnsAddress);

            Kernel.isNetworkConnected = true;
        }
    }

    public class PackageManager : Command
    {
        public PackageManager(string name, string description) : base(name, description) { }

        public override string Execute(string[] args)
        {
            try
            {
                /*WebClient wc = new WebClient(args[0]);
                Random random = new Random();
                string path = $"0:\\temp-{random.Next(100, 999)}.txt";

                byte[] fileData = wc.DownloadFile();

                using (FileStream fileStream = new(path, FileMode.Create))
                    fileStream.Write(fileData, 0, fileData.Length);*/
                string path = "not used";
                Console.WriteLine("NOT FINISHED, DO NOT USE THIS");

                return term.DebugWrite($"File downloaded successfully at \"{path}\".", 4);
            }
            catch (Exception e)
            {
                return term.DebugWrite("Error downloading file: " + e.Message, 4);
            }
        }
    }

    public class Commands : Command
    {
        public Commands(String name, String description) : base(name, description) { }

        public override string Execute(string[] args)
        {
            switch (args[0])
            {
                case "get":
                    if (args[1] == "ip")
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return NetworkConfiguration.CurrentAddress.ToString();
                    }
                    else
                        return term.DebugWrite("Invalid argument.", 4);

                default:
                    return term.DebugWrite("Invalid argument.", 4);
            }
        }
    }
}
