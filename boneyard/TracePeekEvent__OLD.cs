/* 
using System;
using Microsoft.Diagnostics.Tracing;
using System.Collections.Generic;
using System.Dynamic;

namespace TracePeek
{
    public class TracePeekEvent
    {
        public TracePeekEvent(TraceEvent traceEvent)
        {
            ThisTraceEvent = traceEvent;
        }
        public TraceEvent ThisTraceEvent {get;}

        private List<KeyValuePair<string,string>> thisPayload;

        public IEnumerable<KeyValuePair<string,string>> Payload
        {
            get
            {
                if(thisPayload == null) populatePayload();
                return thisPayload;
            }
        }

        private void populatePayload()
        {
            thisPayload = new List<KeyValuePair<string,string>>();
            for(var i=0; i < ThisTraceEvent.PayloadNames.Length; i++)
            {
                thisPayload.Add(new KeyValuePair<string, string>(ThisTraceEvent.PayloadNames[i], ThisTraceEvent.PayloadString(i)));
            }
        }
    }
} */
