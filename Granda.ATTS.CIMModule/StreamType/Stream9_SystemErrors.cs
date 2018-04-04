﻿using Secs4Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Granda.ATTS.CIMModule.CimModuleBase;
using static Granda.ATTS.CIMModule.Extension.SmlExtension;
using static Secs4Net.Item;
namespace Granda.ATTS.CIMModule.StreamType
{
    internal class Stream9_SystemErrors
    {
        /// <summary>
        /// Unrecognized Device ID
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F1()
        {
            return SendMessage(9, 1, false, null);
        }

        /// <summary>
        /// Unrecognized Stream Type
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F3()
        {
            return SendMessage(9, 3, false, null);
        }

        /// <summary>
        /// Unrecognized Function Type
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F5()
        {
            return SendMessage(9, 5, false, null);
        }

        /// <summary>
        /// Illegal Data
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F7()
        {
            return SendMessage(9, 7, false, null);
        }

        /// <summary>
        /// Transaction Timer Timeout
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F9()
        {
            return SendMessage(9, 9, false, null);
        }

        /// <summary>
        /// Conversation Timeout
        /// </summary>
        /// <returns></returns>
        public static SecsMessage S9F13()
        {
            return SendMessage(9, 13, false, null);
        }
    }
}
