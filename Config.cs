//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

// Created by Petr Bena benapetr@gmail.com

using System.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace wmib
{
    /// <summary>
    /// Configuration
    /// </summary>
    public partial class config
    {
        /// <summary>
        /// Network the bot is connecting to
        /// </summary>
        public static string network = "irc.freenode.net";

        /// <summary>
        /// Nick name
        /// </summary>
        public static string username = "wm-bot";

        /// <summary>
        /// Uptime
        /// </summary>
        public static DateTime UpTime;

        /// <summary>
        /// Debug channel (doesn't need to exist)
        /// </summary>
        public static string DebugChan = null;

        /// <summary>
        /// Link to css
        /// </summary>
        public static string css = null;

        public static string MysqlUser = null;

        public static string MysqlPw = null;

        public static string MysqlHost = null;

        public static string Mysqldb = null;

        public static int MysqlPort = 3306;

        /// <summary>
        /// Login name
        /// </summary>
        public static string login = null;

        /// <summary>
        /// Login pw
        /// </summary>
        public static string password = "";

        /// <summary>
        /// Whether the bot is using external network module
        /// </summary>
        public static bool UsingNetworkIOLayer = false;

        /// <summary>
        /// The webpages url
        /// </summary>
        public static string WebpageURL = "";

        /// <summary>
        /// Dump
        /// </summary>
        public static string DumpDir = "dump";

        /// <summary>
        /// This is a log where network log is dumped to
        /// </summary>
        public static string TransactionLog = "transaction.dat";

        /// <summary>
        /// Path to txt logs
        /// </summary>
        [Obsolete]
        public static string path_txt = "log";

        /// <summary>
        /// Network traffic is logged
        /// </summary>
        public static bool Logging = false;

        /// <summary>
        /// Path to html which is generated by this process
        /// </summary>
        [Obsolete]
        public static string path_htm = "html";

        /// <summary>
        /// Version
        /// </summary>
        public static string version = "wikimedia bot v. 1.20.1.0";

        /// <summary>
        /// Separator for system db
        /// </summary>
        public static string separator = "|";

        /// <summary>
        /// User name
        /// </summary>
        public static string name = "wm-bot";

        /// <summary>
        /// This is a port for network bouncer
        /// </summary>
        public static int BouncerPort = 6667;

        /// <summary>
        /// This is a port which system console listen on
        /// </summary>
        public static int SystemPort = 2020;

        /// <summary>
        /// List of channels the bot is in, you should never need to use this, use ChannelList instead
        /// </summary>
        public static List<channel> channels = new List<channel>();

        public static int Interval = 800;

        /// <summary>
        /// List of all channels the bot is in, thread safe
        /// </summary>
        public static List<channel> ChannelList
        {
            get
            {
                List<channel> list = new List<channel>();
                lock (channels)
                {
                    list.AddRange(channels);
                }
                return list;
            }
        }

        /// <summary>
        /// This is a string which commands are prefixed with
        /// </summary>
        public const string CommandPrefix = "@";

        /// <summary>
        /// If colors are in terminal
        /// </summary>
        public static bool Colors = true;

        /// <summary>
        /// Add line to the config file
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void AddConfig(string a, string b, StringBuilder text)
        {
            if (b != null)
            {
                text.Append(a + "=" + b + ";\n");
            }
        }

        /// <summary>
        /// How verbose the debugging is
        /// </summary>
        public static int SelectedVerbosity = 0;

        /// <summary>
        /// Save a channel config
        /// </summary>
        public static void Save()
        {
            StringBuilder text = new StringBuilder("");
            AddConfig("username", username, text);
            AddConfig("password", password, text);
            AddConfig("web", WebpageURL, text);
            AddConfig("serverIO", UsingNetworkIOLayer.ToString(), text);
            AddConfig("debug", DebugChan, text);
            AddConfig("network", network, text);
            AddConfig("bouncerp", BouncerPort.ToString(), text);
            AddConfig("style_html_file", css, text);
            AddConfig("interval", Interval.ToString(), text);
            AddConfig("nick", login, text);
            AddConfig("mysql_user", MysqlUser, text);
            AddConfig("mysql_host", MysqlHost, text);
            AddConfig("mysql_pw", MysqlPw, text);
            AddConfig("mysql_db", Mysqldb, text);
            AddConfig("mysql_port", MysqlPort.ToString(), text);
            text.Append("\nchannels=");

            lock (channels)
            {
                foreach (channel current in channels)
                {
                    text.Append(current.Name + ",\n");
                }
            }
            if (text.ToString().EndsWith(",\n"))
            {
                string x = text.ToString();
                x = x.Substring(0, x.Length - 2);
                text = new StringBuilder(x);
            }
            text.Append(";\n");
            lock (core.Instances)
            {
                int current = 0;
                foreach (Instance blah in core.Instances.Values)
                {
                    if (blah.Nick != username)
                    {
                        AddConfig("instancename" + current.ToString(), blah.Nick, text);
                        AddConfig("instanceport" + current.ToString(), blah.Port.ToString(), text);
                        current++;
                    }
                }
            }
            File.WriteAllText(variables.config + Path.DirectorySeparatorChar + "wmib", text.ToString());
        }

        /// <summary>
        /// Return a temporary name
        /// </summary>
        /// <param name="file">File you need to have temp for</param>
        /// <returns></returns>
        public static string tempName(string file)
        {
            return (file + "~");
        }

        private static Dictionary<string, string> File2Dict()
        {
            Dictionary<string, string> Values = new Dictionary<string, string>();
            string[] xx = File.ReadAllLines(variables.config + Path.DirectorySeparatorChar + "wmib");
            string LastName = null;
            foreach (string line in xx)
            {
                string content = null;
                if (line == "")
                {
                    continue;
                }
                if (line.StartsWith("//"))
                {
                    continue;
                }
                core.DebugWrite("Parsing line: " + line, 8);
                if (LastName == null && line.Contains("="))
                {
                    LastName = line.Substring(0, line.IndexOf("="));
                    if (Values.ContainsKey(LastName))
                    {
                        throw new Exception("You can't redefine same value in configuration multiple times, error reading: " + LastName);
                    }
                    content = line.Substring(line.IndexOf("=") + 1);
                    if (content.Contains(";"))
                    {
                        content = content.Substring(0, content.IndexOf(";"));
                    }
                    Values.Add(LastName, content);
                    core.DebugWrite("Stored config value: " + LastName + ": " + content);
                    if (line.Contains(";"))
                    {
                        LastName = null;
                    }
                    continue;
                }
                if (LastName != null)
                {
                    content = line;
                    if (!content.Contains(";"))
                    {
                        core.DebugWrite("Append config value: " + LastName + ": " + content);
                        Values[LastName] += "\n" + content;
                    }
                    else
                    {
                        content = content.Substring(0, content.IndexOf(";"));
                        Values[LastName] += "\n" + content;
                        core.DebugWrite("Append config value: " + LastName + ": " + content);
                        LastName = null;
                    }
                    continue;
                }
                Program.WriteNow("Invalid configuration line: " + line, true);
            }
            return Values;
        }

        /// <summary>
        /// Load config of bot
        /// </summary>
        public static int Load()
        {
            if (Directory.Exists(variables.config) == false)
            {
                Directory.CreateDirectory(variables.config);
            }
            if (!File.Exists(variables.config + Path.DirectorySeparatorChar + "wmib"))
            {
                Console.WriteLine("Error: unable to find config file in configuration/wmib");
                return 2;
            }
            Dictionary<string, string> Configuration = File2Dict();
            if (Configuration.ContainsKey("username"))
            {
                username = Configuration["username"];
            }
            if (Configuration.ContainsKey("network"))
            {
                network = Configuration["network"];
            }
            if (Configuration.ContainsKey("nick"))
            {
                login = Configuration["nick"];
            }
            if (Configuration.ContainsKey("debug"))
            {
                DebugChan = Configuration["debug"];
            }
            if (Configuration.ContainsKey("bouncerp"))
            {
                BouncerPort =  int.Parse(Configuration["bouncerp"]);
            }
            if (Configuration.ContainsKey("style_html_file"))
            {
                css = Configuration["style_html_file"];
            }
            if (Configuration.ContainsKey("web"))
            {
                WebpageURL = Configuration["web"];
            }
            if (Configuration.ContainsKey("password"))
            {
                password = Configuration["password"];
            }
            if (Configuration.ContainsKey("mysql_pw"))
            {
                MysqlPw = Configuration["mysql_pw"];
            }
            if (Configuration.ContainsKey("mysql_db"))
            {
                Mysqldb = Configuration["mysql_db"];
            }
            if (Configuration.ContainsKey("mysql_user"))
            {
                MysqlUser = Configuration["mysql_user"];
            }
            if (Configuration.ContainsKey("interval"))
            {
                Interval = int.Parse(Configuration["interval"]);
            }
            if (Configuration.ContainsKey("mysql_port"))
            {
                MysqlPort = int.Parse(Configuration["mysql_port"]);
            }
            if (Configuration.ContainsKey("mysql_host"))
            {
                MysqlHost = Configuration["mysql_host"];
            }
            if (string.IsNullOrEmpty(login))
            {
                Console.WriteLine("Error there is no login for bot");
                return 1;
            }
            if (string.IsNullOrEmpty(network))
            {
                Console.WriteLine("Error irc server is wrong");
                return 4;
            }
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Error there is no username for bot");
                return 6;
            }
            if (Configuration.ContainsKey("serverIO"))
            {
                UsingNetworkIOLayer = bool.Parse(Configuration["serverIO"]);
            }
            core.Log("Creating instances");
            core.CreateInstance(username, BouncerPort); // primary instance
            int CurrentInstance = 0;
            while (CurrentInstance < 20)
            {
                if (!Configuration.ContainsKey("instancename" + CurrentInstance.ToString()))
                {
                    break;
                }
                string InstanceName = Configuration["instancename" + CurrentInstance.ToString()];
                core.DebugLog("Instance found: " + InstanceName);
                if (UsingNetworkIOLayer)
                {
                    core.DebugLog("Using bouncer, looking for instance port");
                    if (!Configuration.ContainsKey("instanceport" + CurrentInstance.ToString()))
                    {
                        Program.WriteNow("Instance " + InstanceName + " has invalid port, not using", true);
                        continue;
                    }
                    string InstancePort = Configuration["instanceport" + CurrentInstance.ToString()];
                    int port = int.Parse(InstancePort);
                    core.CreateInstance(InstanceName, port);
                }
                else
                {
                    core.CreateInstance(InstanceName);
                }
                CurrentInstance++;
            }
            foreach (string x in Configuration["channels"].Replace("\n", "").Replace(" ", "").Split(','))
            {
                string name = x.Replace(" ", "").Replace("\n", "");
                if (name != "")
                {
                    lock (channels)
                    {
                        channels.Add(new channel(name));
                    }
                }
            }
            Program.WriteNow("Channels were all loaded");

            // Now when all chans are loaded let's link them together
            foreach (channel ch in channels)
            {
                ch.Shares();
            }

            Program.WriteNow("Channel db's working");
            if (!Directory.Exists(DumpDir))
            {
                Directory.CreateDirectory(DumpDir);
            }
            return 0;
        }
    }
}
