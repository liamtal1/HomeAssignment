using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrepTool
{
    class ContentSearcher
    {
        private readonly SearchOptions _options;

        public ContentSearcher(SearchOptions options)
        {
            _options = options;
        }

        public async Task PerformSearchAsync(IEnumerable<string> searchPaths)
        {
            foreach (var path in searchPaths)
            {
                if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                {
                    await SearchInUrlAsync(path);
                }
                else if (Directory.Exists(path))
                {
                    await SearchInDirectoryAsync(path);
                }
                else if (File.Exists(path))
                {
                    await SearchInFileAsync(path);
                }
                else
                {
                    Console.WriteLine($"Invalid search path: {path}");
                }
            }
        }

        private async Task SearchInUrlAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                await PrintMatchesAsync(reader, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve content from URL: {url}. Error: {ex.Message}");
            }
        }

        private async Task SearchInDirectoryAsync(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, ".", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                await SearchInFileAsync(file);
            }
        }

        private async Task SearchInFileAsync(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                using var reader = new StreamReader(stream);
                await PrintMatchesAsync(reader, filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read file: {filePath}. Error: {ex.Message}");
            }
        }

        private async Task PrintMatchesAsync(StreamReader reader, string source)
        {
            string line;
            int lineNumber = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (MatchCondition(line))
                {
                    Console.WriteLine($"{source}: Line {lineNumber}: {line}");
                }
            }
        }

        private bool MatchCondition(string line)
        {
            var comparison = _options.CaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var containsSearchText = line.IndexOf(_options.SearchText, comparison) >= 0;

            return _options.ReverseSearch ? !containsSearchText : containsSearchText;
        }
    }
}