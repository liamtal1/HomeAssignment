using System;
using System.Threading.Tasks;
using System.Linq;

namespace GrepTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string[] input = Array.FindAll(args, arg => arg != "-v" && arg != "-i");
            string[] rawOptions = Array.FindAll(args, arg => arg == "-v" || arg == "-i");
            
            ErrorCode error = CheckError(input, rawOptions);
            if (error != ErrorCode.None)
            {
                PrintError(error);
                return;
            }

            var options = ParseOptions(input, rawOptions);

            var searcher = new ContentSearcher(options);
            await searcher.PerformSearchAsync(input.Skip(1));
        }

        private static void PrintError(ErrorCode error)
        {
            Console.WriteLine(error switch
            {
                ErrorCode.MissingArguments => "Usage: grep [options] <search-text> <file-path|directory-path|url>...",
                _ => ""
            });
        }

        private static ErrorCode CheckError(string[] input, string[] options)
        {
            if (input.Length < 2)
            {
                return ErrorCode.MissingArguments;
            }

            // More errors can be added here, but I checked the real 'grep' function and it doesn't test further
            return ErrorCode.None;
        }

        static SearchOptions ParseOptions(string[] input, string[] rawOptions)
        {
            return new SearchOptions
            {
                SearchText = input[0],
                CaseSensitive = rawOptions.Contains("-i"),
                ReverseSearch = rawOptions.Contains("-v")
            };
        }
    }
}