using System;
using System.Diagnostics.CodeAnalysis;

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
        
        public static string IsNotNullOrEmpty(this string? src, Func<string, string> act)
        {
            if (!string.IsNullOrEmpty(src))
                return act(src!);

            return src;
        }
    }
}