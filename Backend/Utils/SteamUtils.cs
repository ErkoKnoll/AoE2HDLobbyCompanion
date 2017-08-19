using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Commons.Models;
using Backend.Global;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Aoe2LobbyListFetcherService.Models;
using Newtonsoft.Json;
using Database;
using Commons.Extensions;

namespace Backend.Utils {
    public class SteamUtils {
        public static void CheckNethookPaths() {
            Variables.NethookDumpDir = SteamUtils.GetLatestNethookDumpDirectory();
            var rootDir = Path.GetFullPath(Path.Combine(Variables.NethookDumpDir, @"..\"));
            var dirFolder = Path.GetFileName(Variables.NethookDumpDir);
            Variables.NethookDumpDirParsed = Path.Combine(rootDir, dirFolder + "_parsed");
            Directory.CreateDirectory(Variables.NethookDumpDirParsed);
        }

        private static string GetLatestNethookDumpDirectory() {
            var steamDirectory = GetSteamDirectory();
            if (steamDirectory == null) {
                throw new Exception("Cannot find steam path in registry: " + RegistryPathToSteam);
            }

            var nethookDirectory = Path.Combine(steamDirectory, "nethook");

            if (!Directory.Exists(nethookDirectory)) {
                return steamDirectory;
            }

            var nethookDumpDirs = Directory.GetDirectories(nethookDirectory).Where(d => {
                return long.TryParse(d.Replace(Path.GetDirectoryName(d) + Path.DirectorySeparatorChar, ""), out long n);
            });
            var latestDump = nethookDumpDirs.OrderBy(d => int.Parse(d.Replace(Path.GetDirectoryName(d) + Path.DirectorySeparatorChar, ""))).LastOrDefault();
            if (latestDump == null) {
                return nethookDirectory;
            }

            return Path.Combine(nethookDirectory, latestDump);
        }

        public static string GetSteamDirectory() {
            return (string)Registry.GetValue(RegistryPathToSteam, "InstallPath", null);
        }

        public static string RegistryPathToSteam {
            get { return RuntimeInformation.OSArchitecture == Architecture.X64 ? @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Valve\Steam" : @"HKEY_LOCAL_MACHINE\Software\Valve\Steam"; }
        }
    }
}
