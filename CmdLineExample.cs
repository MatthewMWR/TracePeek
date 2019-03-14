using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace TracePeek
{
    class Program
    {   
        static int Main(string[] args)
        {
            if(args == null || args.Length == 0 || args[0].Contains("?"))
            {
                Console.WriteLine(CmdLineInfo);
                return 1;
            }
            var consoleCancellationRequested = false;
            var thisTracePeekSession = new TracePeekController();
            thisTracePeekSession.OnTracePeekEvent += (eventProjection) => 
            {
                Console.WriteLine(eventProjection);
            };
            thisTracePeekSession.EnableProviders(args);

            Console.WriteLine("Listener starting up...");

            thisTracePeekSession.StartPeek();
            Console.WriteLine("Listener running.");
            Console.WriteLine("Press ALT+s to stop");
            while(!consoleCancellationRequested)
            {
                var keyPress = Console.ReadKey(true);
                if (keyPress.Modifiers.HasFlag(ConsoleModifiers.Alt) && keyPress.KeyChar == 's')
                {
                    Console.WriteLine("Cleaning up...");
                    consoleCancellationRequested = true;
                    thisTracePeekSession.StopPeek();
                }
            }
            return 0;
        }

        private const string CmdLineInfo = @"
TracePeek.exe is a thin demo wrapper for TracePeek.dll.
For rich usage scenarios consider using the TracePeek powershell module or writing code against TracePeek.dll.
For a quick cmdline demo using TracePeek.exe, specify an ETW provider on the cndline, such as:
    TracePeek.exe Microsoft-Windows-Wordpad
and then launch Wordpad while the trace is running to see the events.
";

    }
}
