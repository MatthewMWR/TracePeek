using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing;
using System.Threading.Tasks;
using System.Threading;
using System.Management.Automation;
using System.Diagnostics.Tracing;

namespace TracePeek
{
    public class TracePeekController : IDisposable
    {
        public TracePeekController(string sessionName)
        {
            if(String.IsNullOrEmpty(sessionName))
                sessionName = "TracePeek_DefaultSession";
            SessionName = sessionName;
            ThisTraceEventSession = new TraceEventSession(sessionName);
        }

        private static EventSource log = new EventSource("TracePeek");

        public TraceEventSession ThisTraceEventSession {get;}

        public string SessionName{ get;}
        public bool IsCancellationRequested { get => isCancellationRequested; }
        private bool isCancellationRequested = false;

        public void EnableProviders(ICollection<string> providers)
        {
            foreach(var providerIdentifier in providers)
            {
                var tokens = providerIdentifier.Split(':');
                var keywords = tokens.Length > 1 ? Convert.ToUInt64(tokens[1], 16) : ulong.MaxValue;
                var level = tokens.Length > 2 ? (TraceEventLevel)Convert.ToUInt16(tokens[2], 16) : TraceEventLevel.Always;
                ThisTraceEventSession.EnableProvider(tokens[0],  level, keywords);
            }
        }

        public delegate void OnEvent(PSObject tracePeekEventProjection);
        public event OnEvent OnTracePeekEvent;
        public void Dispose()
        {
            StopPeek();
        }
        public Task StartPeek(TracePeekProjectionStyle projectionStyle = TracePeekProjectionStyle.NamedPayloadProperties)
        {
            if(projectionStyle == TracePeekProjectionStyle.NamedPayloadProperties)
                ThisTraceEventSession.Source.Dynamic.All += traceEvent => {
                    if(OnTracePeekEvent != null) OnTracePeekEvent(TracePeekEvent.CreateWithNamedPayloadProperties(traceEvent));};

            else if(projectionStyle == TracePeekProjectionStyle.NumberedPayloadProperties)
                ThisTraceEventSession.Source.Dynamic.All += traceEvent => {
                    if(OnTracePeekEvent != null) OnTracePeekEvent(TracePeekEvent.CreateWithNumberedPayloadProperties(traceEvent));};

            else if(projectionStyle == TracePeekProjectionStyle.NumberedNestedPayloadProperties)
                ThisTraceEventSession.Source.Dynamic.All += traceEvent => {
                    if(OnTracePeekEvent != null) OnTracePeekEvent(TracePeekEvent.CreateWithNumberedNestedPayloadProperties(traceEvent));};

            else if(projectionStyle == TracePeekProjectionStyle.WithoutPayloadProperties)
                ThisTraceEventSession.Source.Dynamic.All += traceEvent => {
                    if(OnTracePeekEvent != null) OnTracePeekEvent(new PSObject(TracePeekEvent.CreateWithoutPayloadProperties(traceEvent)));};

            else throw new ArgumentOutOfRangeException(nameof(projectionStyle));

            return Task.Run( () => {
                Task.Run(() => {
                    while(!isCancellationRequested)
                    {
                        log.Write("Hearbeat", new { timestampAtCallsite = DateTime.UtcNow });
                        Task.Delay(5000).Wait();
                    }
                });
                ThisTraceEventSession.Source.Process();
            });
        }
        public void StopPeek()
        {
            isCancellationRequested = true;
            if(ThisTraceEventSession != null) 
                ThisTraceEventSession.StopOnDispose = true;
                ThisTraceEventSession.Dispose();
        }
    }

    public enum TracePeekProjectionStyle{
        None = 0,
        WithoutPayloadProperties,
        NamedPayloadProperties,
        NumberedPayloadProperties,
        NumberedNestedPayloadProperties
    }
}
