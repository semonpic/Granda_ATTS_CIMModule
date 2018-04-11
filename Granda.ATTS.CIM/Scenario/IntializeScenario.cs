﻿using Secs4Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Granda.ATTS.CIM.Extension;
using Granda.ATTS.CIM.Model;
using static Granda.ATTS.CIM.Extension.ExtensionHelper;
using static Granda.ATTS.CIM.Extension.SmlExtension;
using static Granda.ATTS.CIM.StreamType.Stream1_EquipmentStatus;
using static Granda.ATTS.CIM.StreamType.Stream2_EquipmentControl;
using static Granda.ATTS.CIM.StreamType.Stream6_DataCollection;
using static Secs4Net.Item;
using Granda.ATTS.CIM.Data;
using Granda.ATTS.CIM.Data.ENUM;
using Granda.ATTS.CIM.Data;
using Granda.ATTS.CIM.Data.Report;

namespace Granda.ATTS.CIM.Scenario
{
    /// <summary>
    /// 初始化场景处理类
    /// </summary>
    internal class InitializeScenario : BaseScenario, IScenario
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public InitializeScenario(IItializeScenario callBack) : base()
        {
            ScenarioType = Scenarios.Intialize_Scenario;
            itializeScenario = callBack;
        }
        /// <summary>
        /// 保存Equipment当前基本信息
        /// </summary>
        public EquipmentBaseInfo EquipmentBaseInfo { get; set; }
        /// <summary>
        /// 保存设备当前的状态信息
        /// </summary>
        public EquipmentStatus EquipmentStatusInfo { get; set; }

        private IItializeScenario itializeScenario = new DefaultIItializeScenario();

        #region Initialize Scenario: 
        /// <summary>
        /// handle online/offline request by host
        /// </summary>
        public void HandleSecsMessage(SecsMessage secsMessage)
        {
            PrimaryMessage = secsMessage;
            switch (PrimaryMessage.GetSFString())
            {
                case "S1F1":// are you there request
                    SubScenarioName = Resource.Intialize_Scenario_1;
                    secsMessage.S1F2(this.EquipmentBaseInfo.MDLN, this.EquipmentBaseInfo.SOFTREV);// 作为unit端， 只考虑online的选项
                    break;
                case "S1F13":// estublish communication request
                    handleS1F13();
                    break;
                case "S1F17":// request online by host
                    SubScenarioName = Resource.Intialize_Scenario_3;
                    secsMessage.S1F18("0");
                    if (LaunchControlStateProcess((int)ControlState.R, this.EquipmentStatusInfo))
                    {
                        if (LaunchDateTimeUpdateProcess())
                        {
                            LaunchControlStateProcess((int)ControlState.EQT_STATUS_CHANGE, this.EquipmentStatusInfo);
                        }
                    }
                    break;
                case "S1F15":// request offline by host
                    SubScenarioName = Resource.Intialize_Scenario_4;
                    switch (this.EquipmentStatusInfo.CRST)
                    {
                        case ControlState.O:
                            //send equipment denies requests
                            S1F0();
                            break;
                        case ControlState.L:
                        case ControlState.R:
                            // send OFF_LINE Acknowledge
                            secsMessage.S1F16("1");
                            // send Control State Change(OFF_LINE)
                            LaunchControlStateProcess((int)ControlState.O, this.EquipmentStatusInfo);
                            break;
                        default:
                            break;
                    }
                    break;
                case "S2F17":// Date and Time Request
                    string dataTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    secsMessage.S2F18(dataTime);
                    break;
                case "S6F11":// Event Report Send (ERS)
                    secsMessage.S6F12("0");
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 启动建立连接进程 online by Unit
        /// </summary>
        /// <returns></returns>
        public bool LaunchOnlineProcess(IDataItem eqtBaseInfo, IDataItem eqtStatus)
        {
            SubScenarioName = Resource.Intialize_Scenario_1;
            this.EquipmentBaseInfo = (EquipmentBaseInfo)eqtBaseInfo;
            this.EquipmentStatusInfo = (EquipmentStatus)eqtStatus;
            // send estublish communication request
            var replyMsg = S1F13(EquipmentBaseInfo.SecsItem);

            if (!(replyMsg != null && replyMsg.GetSFString() == "S1F14"))
            {
                return false;
            }

            replyMsg = S1F1();
            if (replyMsg == null || replyMsg.F == 0)
            {// host denies online request
                return false;
            }
            var equipmentStatus = (eqtStatus as EquipmentStatus);
            equipmentStatus.CRST = ControlState.R;
            if (LaunchControlStateProcess((int)ControlState.R, equipmentStatus))
            {
                if (LaunchDateTimeUpdateProcess())
                {
                    return LaunchControlStateProcess((int)ControlState.EQT_STATUS_CHANGE, equipmentStatus);
                }
            }
            return false;
        }

        /// <summary>
        /// 启动Offline进程 Offline by Unit
        /// </summary>
        /// <returns></returns>
        public bool LaunchOfflineProcess(IDataItem eqtStatus)
        {
            SubScenarioName = Resource.Intialize_Scenario_2;
            var equipmentStatus = (eqtStatus as EquipmentStatus);
            equipmentStatus.CRST = ControlState.O;
            var result = LaunchControlStateProcess((int)ControlState.O, equipmentStatus);
            return result;
        }
        /// <summary>
        /// 启动online by host 进程
        /// </summary>
        /// <returns></returns>
        public bool LaunchOnlineByHostProcess()
        {
            SubScenarioName = Resource.Intialize_Scenario_3;
            // send estublish communication request
            var replyMsg = S1F13(this.EquipmentBaseInfo.SecsItem);
            if (replyMsg != null && replyMsg.GetSFString() == "S1F14")
            {
                var ack = replyMsg.GetCommandValue();
                if (ack == 0)
                {
                    // send ON_LINE request
                    replyMsg = S1F17(ControlState.R.ToString());
                    if (replyMsg != null && replyMsg.GetSFString() == "S1F18")
                    {
                        ack = replyMsg.GetCommandValue();
                        if (ack == 0)
                        {
                            this.EquipmentStatusInfo.CRST = ControlState.R;
                            itializeScenario?.UpdateControlState(ControlState.R);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 启动offline by host 进程
        /// </summary>
        /// <returns></returns>
        public bool LaunchOfflineByHostProcess()
        {
            SubScenarioName = Resource.Intialize_Scenario_4;
            // send Off-line request
            var replyMsg = S1F15();
            if (replyMsg != null && replyMsg.GetSFString() == "S1F0")
            {// equipment denies requests

            }
            else if (replyMsg.GetSFString() == "S1F16")
            {
                var ack = replyMsg.GetCommandValue();
                if (ack == 1)
                {
                    this.EquipmentStatusInfo.CRST = ControlState.O;
                    itializeScenario?.UpdateControlState(ControlState.O);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 其他可公开的控制进程
        /// <summary>
        /// 设置Control State状态：
        /// CEID 113==>ONLINE REMOTE
        ///      112==>ONLINE LOCAL
        ///      111==>OFFLINE
        ///      114==>EQUIPMENT STATUS CHANGE
        /// </summary>
        /// <param name="ceid"></param>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        public bool LaunchControlStateProcess(int ceid, IDataItem dataItem)
        {
            if (ceid >= 111 && ceid <= 114)
            {// control state change
                var stack = new Stack<List<Item>>();
                stack.Push(new List<Item>());
                stack.Peek().Add(A("0"));// DATAID 始终设为0
                stack.Peek().Add(A($"{ceid}"));
                stack.Push(new List<Item>());
                stack.Push(new List<Item>());
                stack.Peek().Add(A("100"));// RPTID 设为100
                stack.Push(new List<Item>(dataItem.SecsItem.Items));
                var item = ParseItem(stack);
                var replyMsg = S6F11(item, (int)ceid);
                if (replyMsg != null && replyMsg.GetSFString() == "S6F12")
                {
                    try
                    {
                        int ack = replyMsg.GetCommandValue();
                        if (ack == 0)
                        {
                            EquipmentStatusInfo = (EquipmentStatus)dataItem;
                            itializeScenario?.UpdateControlState((ControlState)ceid);
                            return true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 启动date and time更新请求
        /// </summary>
        /// <returns></returns>
        public bool LaunchDateTimeUpdateProcess()
        {
            var replyMsg = S2F17();
            if (replyMsg != null && replyMsg.GetSFString() == "S2F18")
            {
                var dateTimeStr = replyMsg.SecsItem.GetString();
                itializeScenario?.UpdateDateTime(dateTimeStr);
                return true;
            }
            return false;
        }
        #endregion


        void handleS1F13()
        {
            SubScenarioName = Resource.Intialize_Scenario_3;
            EquipmentBaseInfo message = new EquipmentBaseInfo();
            message.Parse(PrimaryMessage.SecsItem);
            this.EquipmentBaseInfo = message;
            PrimaryMessage.S1F14(message.MDLN, message.SOFTREV, "0");
        }
        public interface IItializeScenario
        {
            void UpdateControlState(ControlState controlState);
            void UpdateDateTime(string dateTimeStr);
        }

        private class DefaultIItializeScenario : IItializeScenario
        {
            public void UpdateControlState(ControlState controlState)
            {
                Debug.WriteLine("Control State Changed: " + controlState.ToString());
            }

            public void UpdateDateTime(string dateTimeStr)
            {
                Debug.WriteLine("date and time update: " + dateTimeStr);
            }
        }
    }
}