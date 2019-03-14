using System;
using Microsoft.Diagnostics.Tracing;
using System.Management.Automation;

namespace TracePeek
{
    public class TracePeekEvent
    {
        private TracePeekEvent(TraceEvent traceEvent)
        {
            ProviderGuid = traceEvent.ProviderGuid;
            ProviderName = traceEvent.ProviderName;
            Levels = (ushort)traceEvent.Level;
            Keywords = (ulong)traceEvent.Keywords;
            ProcessId = traceEvent.ProcessID;
            ProcessName = traceEvent.ProcessName;
            Name = traceEvent.EventName;
            Version = traceEvent.Version;
            Message = traceEvent.FormattedMessage;
        }
        
        public readonly string ProviderName;
        public readonly Guid ProviderGuid;
        public readonly ushort Levels;
        public readonly UInt64 Keywords;
        public readonly string RelativeId;
        public readonly string ProcessName;
        public readonly Int32 ProcessId;
        public readonly int Version;
        public readonly string Message;
        public readonly string Name;

        public static TracePeekEvent CreateWithoutPayloadProperties(TraceEvent traceEvent) => new TracePeekEvent(traceEvent);
        public static PSObject CreateWithNamedPayloadProperties(TraceEvent traceEvent)
        {
            var eventProjection = new PSObject(new TracePeekEvent(traceEvent));
            for(var i=0; i < traceEvent.PayloadNames.Length; i++)
            {
                var psProperty = new PSNoteProperty(traceEvent.PayloadNames[i], traceEvent.PayloadValue(i));
                eventProjection.Properties.Add(psProperty);
            }
            return eventProjection;
        }

        public static PSObject CreateWithNumberedPayloadProperties(TraceEvent traceEvent, int maxFieldCount = 10)
        {
            var eventProjection = new PSObject(new TracePeekEvent(traceEvent));
            for(var i=0; i < maxFieldCount; i++)
            {
                var name = i < traceEvent.PayloadNames.Length ? traceEvent.PayloadNames[i] : String.Empty;
                var value = i < traceEvent.PayloadNames.Length ? traceEvent.PayloadString(i) : String.Empty;
                eventProjection.Properties.Add(new PSNoteProperty($"Field{i}Name", name));
                eventProjection.Properties.Add(new PSNoteProperty($"Field{i}Value", value));
            }
            return eventProjection;
        }

        public static PSObject CreateWithNumberedNestedPayloadProperties(TraceEvent traceEvent, int maxFieldCount = 10)
        {
            var eventProjection = new PSObject(new TracePeekEvent(traceEvent));
            for(var i=0; i < maxFieldCount; i++)
            {
                var name = i < traceEvent.PayloadNames.Length ? traceEvent.PayloadNames[i] : String.Empty;
                var value = i < traceEvent.PayloadNames.Length ? traceEvent.PayloadString(i) : String.Empty;
                eventProjection.Properties.Add(new PSNoteProperty($"Field{i}", $"{name} : {value}"));
            }
            return eventProjection;
        }
    }
}