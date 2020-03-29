using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace HelmPreprocessor.Extensions
{
    public static class ObjectExtensions
    {
        // public static T? IsNotNull<T>(this T? src, Func<T, T> act)
        //     where T: class
        // {
        //     if (src != null) act(src);
        //     return src;
        // }
        
        public static void IsNotNull<T>(this T src, Action<T> act)
            where T: class?
        {
            if (src != null) act(src);
        }

        public static T? Is<T>(this object src)
            where T: class
        {
            if (src is T targetType)
            {
                return targetType;
            }

            return null;
        }

        public static void Do<T>(this T? src, Action<T> act)
            where T: class
        {
            if (src != null)
            {
                act(src!);
            }
        }
    }

    public static class StringExtensions
    {
        public static string? IsNotNullOrWhitespace(this string? src, Action<string> act)
        {
            if (!string.IsNullOrWhiteSpace(src))
                act(src!);

            return src;
        }
        
        public static string? IsNotNullOrEmpty(this string? src, Action<string> act)
        {
            if (!string.IsNullOrEmpty(src))
                act(src!);

            return src;
        }
        
        public static string? IsNotNullOrEmpty(this string? src, Func<string, string> act)
        {
            return !string.IsNullOrEmpty(src) ? act(src!) : src;
        }
    }

    public static class DirectoryInfoExtensions
    {
        public static FileInfo GetFilePath(this DirectoryInfo directoryInfo, params string[] paths)
        {
            return new FileInfo(BuildAndCombinePath(directoryInfo, paths));
        }

        private static string BuildAndCombinePath(DirectoryInfo directoryInfo, string[] paths)
        {
            var finalListOfPaths = new string[paths.Length + 1];
            finalListOfPaths[0] = directoryInfo.FullName;
            Array.Copy(paths, 0, finalListOfPaths, 1, paths.Length);
            var finalPath = Path.Combine(finalListOfPaths);
            return finalPath;
        }

        public static DirectoryInfo GetSubDirectoryPath(this DirectoryInfo directoryInfo, params string[] paths)
        {
            return new DirectoryInfo(BuildAndCombinePath(directoryInfo, paths));
        }

        public static void CopyRecursive(this DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
            {
                target.Create();
            }

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
 
            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyRecursive(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}