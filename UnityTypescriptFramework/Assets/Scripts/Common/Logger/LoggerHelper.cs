﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class LoggerHelper : MonoSingleton<LoggerHelper>
{
    public enum LOG_TYPE
    {
        LOG = 0,
        LOG_ERR,
    }

    struct log_info
    {
        public LOG_TYPE type;
        public string msg;

        public log_info(LOG_TYPE type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }

    private static LoggerHelper _instance = null;
    private List<log_info> backList = new List<log_info>(100);
    private List<log_info> frontList = new List<log_info>(100);

    protected override void Init()
    {
        if (!Application.isEditor && !String.IsNullOrEmpty(URLSetting.REPORT_ERROR_URL))
        {
            Application.logMessageReceived += (LogHandler);
            InvokeRepeating("CheckReport", 1f, 1f);
        }
    }

    private void LogHandler(string condition, string stackTrace, LogType type)
    {
        if (Application.isEditor)
        {
            return;
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            CS.Logger.LogError(condition + " \n" + stackTrace);
        }
    }

    private void CheckReport()
    {
        CS.Logger.CheckReportError();
    }

    private void Update()
    {
        lock (backList)
        {
            if (backList.Count > 0)
            {
                List<log_info> tmp = frontList;
                frontList = backList;
                backList = tmp;
            }
        }

        if (frontList.Count > 0)
        {
            for (int i = 0; i < frontList.Count; i++)
            {
                var logInfo = frontList[i];
                switch (logInfo.type)
                {
                    case LOG_TYPE.LOG:
                    {
                        CS.Logger.Log(logInfo.msg, null);
                        break;
                    }
                    case LOG_TYPE.LOG_ERR:
                    {
                        CS.Logger.LogError(logInfo.msg, null);
                        break;
                    }
                }
            }

            frontList.Clear();
        }
    }

    public override void Dispose()
    {
        lock (backList)
        {
            backList.Clear();
        }

        frontList.Clear();
        base.Dispose();
    }

    public void LogToMainThread(LOG_TYPE type, string msg)
    {
        lock (backList)
        {
            backList.Add(new log_info(type, msg));
        }
    }
}