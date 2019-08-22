

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VRIME2
{
    public sealed class BIHandler : IBIHandler
    {
        #region Constants

        private const string ImportName = "IMEBIClientDLL";
        private const string AppId = "com.htc.vive.vrime";

        #endregion

        #region Imports

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IMEBiLogger_Init(int version, [MarshalAs(UnmanagedType.LPStr)] string appId, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_SetEnvironment([MarshalAs(UnmanagedType.LPStr)] string env);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr IMEBiLogger_Log([MarshalAs(UnmanagedType.LPStr)] string category);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_AddDataInt(IntPtr logger, [MarshalAs(UnmanagedType.LPStr)] string key, int value);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_AddDataLong(IntPtr logger, [MarshalAs(UnmanagedType.LPStr)] string key, long value);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_AddDataBool(IntPtr logger, [MarshalAs(UnmanagedType.LPStr)] string key, bool value);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_AddDataDouble(IntPtr logger, [MarshalAs(UnmanagedType.LPStr)] string key, double value);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_AddDataString(IntPtr logger, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_SendAndFree(IntPtr logger);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_Flush();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_FlushAll();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void IMEBiLogger_Deinit();

        #endregion

        #region Dependencies

        
        #endregion

        #region Variables

        private bool _isStage = false;
        public bool StageLog {
            get { return _isStage;}
            set { _isStage = value; }
        }
        private IntPtr _currentLogger;
        
        #endregion

        #region Methods


        public bool Init(int version, string path)
        {
            var init = IMEBiLogger_Init(version, AppId, path);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _isStage = true; // TODO: get stage setting from VRIME_Manager
#endif
            if (_isStage) {
                IMEBiLogger_SetEnvironment("stage");
            }

            VRIME_Debugger.Log("BI State: "+ (_isStage ? "stage" : "production"));
#if !UNITY_EDITOR
            IMEBiLogger_FlushAll();
#endif
            return init;
        }

        public void SetEnvironment(string env)
        {
            IMEBiLogger_SetEnvironment(env);
        }

        public void StartLog(string category)
        {
            _currentLogger = IMEBiLogger_Log(category);
        }

        public void AddData(string key, int value)
        {
            IMEBiLogger_AddDataInt(_currentLogger, key, value);
        }

        public void AddData(string key, long value)
        {
            IMEBiLogger_AddDataLong(_currentLogger, key, value);
        }

        public void AddData(string key, bool value)
        {
            IMEBiLogger_AddDataBool(_currentLogger, key, value);
        }

        public void AddData(string key, double value)
        {
            IMEBiLogger_AddDataDouble(_currentLogger, key, value);
        }

        public void AddData(string key, string value)
        {
            IMEBiLogger_AddDataString(_currentLogger, key, value);
        }

        public void EndLog()
        {
            IMEBiLogger_SendAndFree(_currentLogger);
            _currentLogger = IntPtr.Zero;
        }

        public void Flush()
        {
            IMEBiLogger_Flush();
        }

        public void FlushAll()
        {
            IMEBiLogger_FlushAll();
        }

        public void DeInit()
        {
            IMEBiLogger_Deinit();
        }

        #endregion
    }

}
