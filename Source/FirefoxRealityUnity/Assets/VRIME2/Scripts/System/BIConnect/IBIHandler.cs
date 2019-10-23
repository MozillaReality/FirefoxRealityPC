

using System;
using UnityEngine;

namespace VRIME2
{
    public interface IBIHandler
    {
        bool Init(int version, string path);
        void SetEnvironment(string env);
        void StartLog(string category);
        void AddData(string key, int value);
        void AddData(string key, long value);
        void AddData(string key, bool value);
        void AddData(string key, double value);
        void AddData(string key, string value);
        void EndLog();
        void Flush();
        void FlushAll();
        void DeInit();
    }

    public sealed class BILogScope : IDisposable
    {
#if !UNITY_EDITOR
        private static int FLUSH_COUNT = 50;
        private static int mCount = 0;
#endif
        private static IBIHandler _handler;

        public static bool Init(bool iIsStage = false) {
            BIHandler aTmpHandler = new BIHandler();
            aTmpHandler.StageLog = iIsStage;
            _handler = aTmpHandler;
            return _handler.Init(1, ""); // TODO: set correct version
        }

        public BILogScope(string category)
        {            
            _handler.StartLog(category);
        }

        public void Dispose()
        {
            
            _handler.EndLog();
            VRIME_Debugger.Log("BILogScope.Dispose");
#if !UNITY_EDITOR
            mCount++;
            if (mCount % FLUSH_COUNT == 0)
            {
                _handler.FlushAll();
            }
#endif
        }

        public static IBIHandler BI
        {
            get { return _handler; }
        }
    }
}