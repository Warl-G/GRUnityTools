using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.PackageManager;

namespace GRTools.PackageResolver
{
    internal static class Resolver
    {
        const StringComparison Ordinal = StringComparison.Ordinal;
        const string k_LogHeader = "<b><color=#2E8B57>[GitResolver]</color></b> ";

        [System.Diagnostics.Conditional("GDR_LOG")]
        static void Log(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(k_LogHeader + format, args);
        }

        static void Error(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(k_LogHeader + format, args);
        }

        public static bool IsInstalled(string name)
        {
            foreach (var package in GetInstalledPackages())
            {
                if (package.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static string[] UsedBy(string name)
        {
            List<string> used = new List<string>();
            foreach (var package in GetInstalledPackages())
            {
                foreach (var pack in  package.dependencyPackages)
                {
                    if (pack.name == name)
                    {
                        used.Add(package.name);
                        break;
                    }
                }
            }
            return used.ToArray();
        } 

        private static PackageMeta[] GetInstalledPackages()
        {
            return Directory.GetDirectories("./Library/PackageCache")
                .Concat(Directory.GetDirectories("./Packages"))
                .Select(PackageMeta.FromPackageDir) // Convert to PackageMeta
                .Concat(new[] {PackageMeta.FromPackageJson("./Packages/manifest.json")})
                .Where(x => x != null) // Skip null
                .ToArray();
        }

        /// <summary>
        /// Uninstall unused packages (for auto-installed packages)
        /// </summary>
        private static void UninstallUnusedPackages()
        {
            Log("Find for unused automatically installed packages");
            var needToCheck = true;
            while (needToCheck)
            {
                needToCheck = false;

                // Collect all dependencies.
                var allDependencies = GetInstalledPackages()
                    .SelectMany(x => x.GetAllDependencies()) // Get all dependencies
                    .ToArray();

                PackageMeta[] autoInstalledPackages = Directory.GetDirectories("./Packages")
                    .Where(x => Path.GetFileName(x).StartsWith(".", Ordinal)) // Directory name starts with '.'. This is 'auto-installed package'
                    .Select(PackageMeta.FromPackageDir) // Convert to PackageMeta
                    .Where(x => x != null) // Skip null
                    .ToArray();

                var used = autoInstalledPackages
                    .Where(x => allDependencies.Any(y => y.name == x.name)) // Depended from other packages
                    .GroupBy(x => x.name) // Grouped by package name
                    .Select(x => x.OrderByDescending(y => y.version).First()) // Latest package
                    .ToArray();

                // Collect unused pakages.
                var unused = autoInstalledPackages
                    .Except(used) // Exclude used packages
                    .ToArray();

                var sb = new StringBuilder();
                sb.AppendLine("############## UninstallUnusedPackages ##############");
                sb.AppendLine("\n[ allDependencies ] ");
                allDependencies.ToList().ForEach(p => sb.AppendLine(p.ToString()));

                sb.AppendLine("\n[ autoInstalledPackages ] ");
                autoInstalledPackages.ToList().ForEach(p => sb.AppendLine(p.ToString()));

                sb.AppendLine("\n[ unusedPackages ] ");
                unused.ToList().ForEach(p => sb.AppendLine(p.ToString()));
                Log(sb.ToString());

                // Uninstall unused packages and re-check.
                foreach (var p in unused)
                {
                    needToCheck = true;
                    Log("Uninstall the unused package '{0}@{1}'", p.name, p.version);
                    Client.Remove(p.name);
                }
            }
        }
    }
}
