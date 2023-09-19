using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EMUHELP.Extensions;

namespace Helper.Data.AutoSplitter
{
    internal static partial class Actions
    {
        private static readonly object _aslMethods;
        private static readonly Type _aslMethodType;

        static Actions()
        {
            _aslMethods = Script.ASLScript.GetFieldValue<object>("_methods");
            _aslMethodType = (_aslMethods as IEnumerable<object>).FirstOrDefault()?.GetType();
        }

        public static string Current
        {
            get
            {
                foreach (string trace in Trace.Reverse())
                {
                    if (trace.StartsWith("ASLScript.Do"))
                    {
                        return trace.Substring(12).ToLower();
                    }

                    if (trace.StartsWith("ASLScript.Run"))
                    {
                        return trace.Substring(13).ToLower();
                    }
                }

                return null;
            }
        }

#pragma warning disable IDE1006
        public static Action startup { get; set; } = new(nameof(startup));
        public static Action shutdown { get; set; } = new(nameof(shutdown));
        public static Action init { get; set; } = new(nameof(init));
        public static Action exit { get; set; } = new(nameof(exit));
        public static Action update { get; set; } = new(nameof(update));
        public static Action start { get; set; } = new(nameof(start));
        public static Action split { get; set; } = new(nameof(split));
        public static Action reset { get; set; } = new(nameof(reset));
        public static Action gameTime { get; set; } = new(nameof(gameTime));
        public static Action isLoading { get; set; } = new(nameof(isLoading));
        public static Action onStart { get; set; } = new(nameof(onStart));
        public static Action onSplit { get; set; } = new(nameof(onSplit));
        public static Action onReset { get; set; } = new(nameof(onReset));
#pragma warning restore IDE1006

        private static IEnumerable<string> Trace
        {
            get
            {
                StackFrame[] frames = new StackTrace().GetFrames();

                for (int i = 0; i < frames.Length; i++)
                {
                    MethodBase method = frames[i].GetMethod();
                    Type decl = method.DeclaringType;
                    string ret = decl is null ? method.Name : $"{decl.Name}.{method.Name}";

                    yield return ret;
                }
            }
        }
    }
}
