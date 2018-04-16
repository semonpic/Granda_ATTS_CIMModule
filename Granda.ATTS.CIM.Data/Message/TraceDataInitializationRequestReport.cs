﻿#region 文件说明
/*------------------------------------------------------------------------------
// Copyright © 2018 Granda. All Rights Reserved.
// 苏州广林达电子科技有限公司 版权所有
//------------------------------------------------------------------------------
// File Name: TraceDataInitializationRequest
// Author: Ivan JL Zhang    Date: 2018/4/13 17:26:04    Version: 1.0.0
// Description: 
//   
// 
// Revision History: 
// <Author>  		<Date>     	 	<Revision>  		<Modification>
// 	
//----------------------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using Secs4Net;
using static Granda.ATTS.CIM.Data.Helper;
using static Secs4Net.Item;
namespace Granda.ATTS.CIM.Data.Message
{
    /// <summary>
    /// Host requests Trace Data Initialization
    /// </summary>
    public struct TraceDataInitializationRequest : IMessage
    {
        /// <summary>
        /// Trace Data Id
        /// </summary>
        public string TRID { get; private set; }
        /// <summary>
        /// Data Sample Period
        /// Time Format: hhmmss
        /// </summary>
        public string DSPER { get; private set; }
        /// <summary>
        /// Total Samples to be made
        /// The maximum number of samples that this Trace Report will perform.
        ///-1 means infinite count.
        /// </summary>
        public string TOTSMP { get; private set; }
        /// <summary>
        /// Reporting Group Size.
        /// ex:
        /// <para>DSPER = 3 Seconds, REPGSZ = 1: Report S6F1 (1 group) every 3 seconds.</para>
        /// <para>DSPER = 3 Seconds, REPGSZ = 2: Report S6F1(2 group) every 6 seconds.</para>
        /// </summary>
        public string REPGSZ { get; private set; }
        /// <summary>
        /// Status Variable ID List
        /// <para>Status variables may include any parameter that can be sampled in time such as temperature or quantity of a consumable.</para>
        /// </summary>
        public IEnumerable<string> SVIDList;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Parse(Item item)
        {
            if (item.Items.Count == 5)
            {
                TRID = item.Items[0].Format == SecsFormat.ASCII ? item.Items[0].GetString() : String.Empty;
                DSPER = item.Items[1].Format == SecsFormat.ASCII ? item.Items[0].GetString() : String.Empty;
                TOTSMP = item.Items[2].Format == SecsFormat.ASCII ? item.Items[0].GetString() : String.Empty;
                REPGSZ = item.Items[3].Format == SecsFormat.ASCII ? item.Items[0].GetString() : String.Empty;

                List<string> svidList = new List<string>();
                if (item.Items[4].Format == SecsFormat.List)
                {
                    for (int index = 0; index < item.Items[4].Items.Count; index++)
                    {
                        svidList.Add(item.Items[4].Items[index].Format == SecsFormat.ASCII ? item.Items[4].Items[index].GetString() : String.Empty);
                    }
                }

                SVIDList = svidList;
            }
        }
    }
    /// <summary>
    /// trace data result report
    /// </summary>
    public struct TraceDataInitializationReport : IReport
    {
        /// <summary>
        /// Trace data ID
        /// </summary>
        public string TRID { get; set; }
        /// <summary>
        /// Sample Number
        /// <para>The order number of trace data.</para>
        /// <para>SMPLN will be increased sequentially.</para>
        /// </summary>
        public string SMPLN { get; set; }
        /// <summary>
        /// Sample Time
        /// <para>STIME FORMAT =’YYYYMMDDhhmmss’</para>
        /// </summary>
        public string STIME { get; set; }
        /// <summary>
        /// Status Variable ID List
        /// </summary>
        public SVIDS SVIDLIST { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Item SecsItem
        {
            get
            {
                var stack = new Stack<List<Item>>();
                stack.Push(new List<Item>() {
                    A(TRID ?? String.Empty),
                    A(SMPLN ?? String.Empty),
                    A(STIME ?? String.Empty)
                });
                stack.Push(new List<Item>());
                for (int index = 0; index < SVIDLIST.Count; index++)
                {
                    var svid = SVIDLIST[index];
                    stack.Push(new List<Item>() {
                        A(svid.SVID),
                        A(svid.SV),
                    });
                }

                return ParseItem(stack);
            }
        }
    }
    /// <summary>
    /// SVID List class
    /// </summary>
    public class SVIDS
    {
        /// <summary>
        /// Status Variable ID
        /// </summary>
        public string SVID { get; set; }
        /// <summary>
        /// Status Variable
        /// </summary>
        public string SV { get; set; }

        #region IList相关
        private SVIDS[] _items;
        private static readonly SVIDS[] emptyArray = new SVIDS[0];
        private int _size = 0;
        /// <summary>
        /// ctor
        /// </summary>
        public SVIDS()
        {
            _items = emptyArray;
        }
        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SVIDS this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }


        /// <summary>
        /// List的长度
        /// </summary>
        public int Count { get => _size; }

        /// <summary>
        /// 将新的item添加至列表末尾处
        /// </summary>
        /// <param name="item"></param>
        public void Add(SVIDS item)
        {
            this._items[this._size++] = item;
        }
        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            this._items = emptyArray;
            this._size = 0;
        }
        #endregion
    }
}
