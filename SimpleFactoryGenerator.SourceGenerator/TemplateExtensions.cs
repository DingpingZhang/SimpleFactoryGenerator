using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleFactoryGenerator.SourceGenerator
{
    public static class TemplateExtensions
    {
        public const string RemoveLineIfEmpty = " ";
        public const string KeepLineIfEmpty = "";
        public const string LineTrimmedFlag = "#\"#";

        private static readonly int NewLineLength = Environment.NewLine.Length;
        private static readonly Regex WhiteSpaceLineRegex = new($@"{Environment.NewLine}( +?{Environment.NewLine})+", RegexOptions.Compiled);

        public static string For<T>(this IEnumerable<T> self, Func<T, string> callback, string fallbackIfEmpty = RemoveLineIfEmpty)
        {
            return self.For((item, _) => callback(item), fallbackIfEmpty);
        }

        public static string For<T>(this IEnumerable<T> self, Func<T, int, string> callback, string fallbackIfEmpty = RemoveLineIfEmpty)
        {
            var list = self.ToList();
            if (list.Count == 0)
            {
                return fallbackIfEmpty;
            }

            return string.Join(Environment.NewLine, list.Select((item, index) => callback(item, index).TrimNewLine()));
        }

        public static string Join<T>(this IEnumerable<T> self, string separator = "", Func<T, string>? callback = null)
        {
            return string.Join(separator, self.Select(item => callback?.Invoke(item) ?? item?.ToString() ?? KeepLineIfEmpty));
        }

        public static string If(this string? self, Func<string, string> ifTrueCallback, string? ifFalseText = KeepLineIfEmpty)
        {
            return string.IsNullOrEmpty(self)
                ? ifFalseText?.TrimNewLine() ?? string.Empty
                : ifTrueCallback(self!.TrimNewLine()).TrimNewLine();
        }

        public static string FormatCode(this string text)
        {
            text = text
                .Replace(LineTrimmedFlag, string.Empty)
                .TrimStart(Environment.NewLine.ToCharArray());

            return WhiteSpaceLineRegex.Replace(text, Environment.NewLine);
        }

        public static string Text(string? text) => text.TrimNewLine();

        private static string TrimNewLine(this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int start = text!.StartsWith(Environment.NewLine) ? NewLineLength : 0;
            int length = text.EndsWith(Environment.NewLine)
                ? text.Length - start - NewLineLength
                : text.Length - start;

            return $"{LineTrimmedFlag}{text.Substring(start, length)}{LineTrimmedFlag}";
        }
    }
}
