using System;

namespace ViewSource
{
    class Program
    {
        static int Main(string[] aArgs)
        {
            var program = new ViewSource();

            try
            {
                program.Run(aArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine("vs [filename] [width] [depth]");
                Console.WriteLine("[filename] = audio file to process");
                Console.WriteLine("[width] = number of shapes across");
                Console.WriteLine("[height] = number of shapes down");
                Console.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }
    }
}