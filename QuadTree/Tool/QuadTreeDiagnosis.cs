using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树调试相关
    /// </summary>
    public readonly struct QuadTreeDiagnosis
    {
        public static bool EnableLog = false;
        public static int[] TreeIds; // 需要检测的TreeId，null/Empty代表不限制检测
        public static int[] Indexes; // 需要检测的Index，null/Empty代表不限制检测

        [Conditional(Macro.Editor)]
        internal static void Log<TArgs>(LogType logType, string message, TArgs args = default) where TArgs : struct, IDiagnosisArgs
        {
            if (!EnableLog)
                return;
            Log(logType, args.BuildMessage(message));
        }
        [Conditional(Macro.Editor)]
        internal static void LogTreeId<TArgs>(int treeId, LogType logType, string format, TArgs args = default) where TArgs : struct, IDiagnosisArgs
        {
            if (!EnableLog)
                return;
            if (!CheckTargets(TreeIds, treeId))
                return;
            Log(logType, args.BuildMessage(format));
        }
        [Conditional(Macro.Editor)]
        internal static void LogIndex<TArgs>(int index, LogType logType, string format, TArgs args = default) where TArgs : struct, IDiagnosisArgs
        {
            if (!EnableLog)
                return;
            if (!CheckTargets(Indexes, index))
                return;
            Log(logType, args.BuildMessage(format));
        }
        [Conditional(Macro.Editor)]
        internal static void LogIndex<TArgs>(int treeId, int index, LogType logType, string format, TArgs args = default) where TArgs : struct, IDiagnosisArgs
        {
            if (!EnableLog)
                return;
            if (!CheckTargets(TreeIds, treeId))
                return;
            if (!CheckTargets(Indexes, index))
                return;
            Log(logType, args.BuildMessage(format));
        }

        private static bool CheckTargets(ICollection<int> targets, int id) => targets.IsNullOrEmpty() || targets.Contains(id);
        [Conditional(Macro.Editor)]
        private static void Log(LogType logType, string message)
        {
            switch (logType)
            {
                case LogType.Trace: LogRelay.Trace(message); break;
                case LogType.Debug: LogRelay.Debug(message); break;
                case LogType.Info: LogRelay.Info(message); break;
                case LogType.Warn: LogRelay.Warn(message); break;
                case LogType.Error: LogRelay.Error(message); break;
                case LogType.Fail: LogRelay.Fail(message); break;
            }
        }
    }
}