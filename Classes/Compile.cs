﻿// Copyright (C) 2011 Iker Ruiz Arnauda (Wesko)
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
using System.IO;
using Nini.Config;
using Microsoft.Build.Evaluation;
using System.Threading.Tasks;

namespace MadCow
{
    class Compile
    {
        //This paths may change depending on which repository ur trying to retrieve, they are set over ParseRevision.cs
        public static String currentMooegeExePath = "";
        public static String currentMooegeDebugFolderPath = "";
        public static String mooegeINI = "";
        //This paths dont change.
        public static String madcowINI = Program.programPath + @"\Tools\\Settings.ini";

        public static void compileSource()
        {
            var libmoonetPath = Program.programPath + @"\" + @"Repositories\" + ParseRevision.developerName + "-" + ParseRevision.branchName + "-" + ParseRevision.lastRevision + @"\src\LibMooNet\LibMooNet.csproj";
            var mooegePath = Program.programPath + @"\" + @"Repositories\" + ParseRevision.developerName + "-" + ParseRevision.branchName + "-" + ParseRevision.lastRevision + @"\src\Mooege\Mooege-VS2010.csproj";

            var compileLibMooNetTask = Task<bool>.Factory.StartNew(() => CompileLibMooNet(libmoonetPath));
            var compileMooegeTask = compileLibMooNetTask.ContinueWith<bool>((x) => CompileMooege(mooegePath, x.Result));

            Task.WaitAll(compileMooegeTask);

            if (compileMooegeTask.Result == false)
                Console.WriteLine("[Fatal] Failed to compile.");
            else
                Console.WriteLine("Compiling Complete.");
        }

        private static bool CompileLibMooNet(string libmoonetPath)
        {
            Console.WriteLine("Compiling LibMoonet...");
            var libmoonetProject = new Project(libmoonetPath);
            return libmoonetProject.Build(new Microsoft.Build.Logging.FileLogger());
        }

        private static bool CompileMooege(string mooegePath, bool LibMooNetStatus)
        {
            if (LibMooNetStatus)
            {
                Console.WriteLine("Compiling Mooege...");
                var mooegeProject = new Project(mooegePath);
                return mooegeProject.Build(new Microsoft.Build.Logging.FileLogger());
            }
            return false;
        }

        public static void ModifyMooegeINI()
        {
            try
            {
                //After compiling we modify Mooege INI config file with the correct Storage path.
                IConfigSource source = new IniConfigSource(Compile.mooegeINI);
                string fileName = source.Configs["Storage"].Get("MPQRoot");
                if (fileName.Contains("${Root}"))
                {
                    Console.WriteLine("Modifying Mooege MPQ storage folder...");
                    IConfig config = source.Configs["Storage"];
                    config.Set("MPQRoot", Program.programPath + "\\MPQ");
                    source.Save();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Modifying Mooege MPQ storage folder Complete.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Fatal] Could not modify Mooege INI config file.");
            }
        }
    }
}
