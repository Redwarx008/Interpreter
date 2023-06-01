using System;

namespace Interpreter
{
    internal class Lox
    {
        static bool _hadError = false;
        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                System.Environment.Exit(64);
            }
            else if(args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();    
            }
        }

        private static void RunFile(string path)
        {
            string data = File.ReadAllText(path);
            Run(data);
            // Indicate an error in the exit code.
            if(_hadError)
            {
                System.Environment.Exit(65);
            }
        }
        private static void RunPrompt()
        {
            while(true)
            {
                Console.Write(">");
                string? line = Console.ReadLine();
                if(line == null)
                    break;
                Run(line);
                _hadError = false;
            }
        }

        private static void Run(String source)
        {
            //Scanner scanner = new Scanner(source);
            //List<Token> tokens = scanner.scanTokens();

            // For now, just print the tokens.
        }
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }
        private static void Report(int line, String where,
                             String message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where} : {message}");
            _hadError = true;   
        }
    }
}