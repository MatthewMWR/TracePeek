using System;

namespace TracePeek
{
    public static class Utility
    {
        public static void HandleConsoleCancelKeyPress(this TracePeekController controller)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Console.WriteLine("Cleaning up...");
                controller.StopPeek();
                Console.WriteLine("Cleanup done.");
            };
        }
    } 
}