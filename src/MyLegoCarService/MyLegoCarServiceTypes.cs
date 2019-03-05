using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

namespace MyLegoCarService
{
    /// <summary>
    /// MyLegoCarService contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// DSS contract identifer for MyLegoCarService
        /// </summary>
        [DataMember]
        public const string Identifier = "http://schemas.tempuri.org/2010/09/mylegocarservice.html";
    }

    /// <summary>
    /// MyLegoCarService state
    /// </summary>
    [DataContract]
    public class MyLegoCarServiceState
    {
    }

    /// <summary>
    /// MyLegoCarService main operations port
    /// </summary>
    [ServicePort]
    public class MyLegoCarServiceOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, TimerTick>
    {
    }

    /// <summary>
    /// MyLegoCarService get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<MyLegoCarServiceState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Get(GetRequestType body, PortSet<MyLegoCarServiceState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// MyLegoCarService subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    public class TimerTick : Update<TimerTickRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public TimerTick()
            : base(new TimerTickRequest())
        {
        }
    }


    [DataContract]
    public class TimerTickRequest
    {
    }

}


