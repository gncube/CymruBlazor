#!/usr/bin/env dotnet-script
/*
 * CymruBlazor CSS Bundler
 *
 * A lightweight utility that bundles CSS files in a defined order, removes
 * duplicate @layer declarations and @import directives, and generates a
 * single deterministic output stylesheet suitable for PWA asset manifests.
 *
 * Usage: dotnet run -- <output_file> <input_file_1> [<input_file_2> ...]
 *
 * Example:
 *   dotnet run -- src/CymruBlazor/wwwroot/css/cymrublazor.css \
 *     wwwroot/css/base/reset.css \
 *     wwwroot/css/tokens/colours.css \
 *     wwwroot/css/tokens/typography.css
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class CssBundler
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: dotnet run -- <output_file> <input_file_1> [<input_file_2> ...]");
            return 1;
        }

        var outputFile = args[0];
        var inputFiles = args.Skip(1).ToList();

        try
        {
            Console.WriteLine($"CymruBlazor CSS Bundler");
            Console.WriteLine($"Output: {outputFile}");
            Console.WriteLine($"Inputs: {string.Join(", ", inputFiles)}");

            var bundled = BundleCss(inputFiles);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputFile, bundled, Encoding.UTF8);
            Console.WriteLine($"✓ Bundle written to {outputFile}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Error: {ex.Message}");
            return 1;
        }
    }

    static string BundleCss(List<string> inputFiles)
    {
        var output = new StringBuilder();
        var layersSeen = new HashSet<string>();
        var globalLayerDeclaration = "@layer reset,tokens,base,layout,components,utilities,overrides;";

        // Add global layer declaration at the very top
        output.AppendLine(globalLayerDeclaration);
        output.AppendLine();

        foreach (var inputFile in inputFiles)
        {
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"⚠ Warning: Input file not found: {inputFile}");
                continue;
            }

            var content = File.ReadAllText(inputFile, Encoding.UTF8);

            // Remove all @import statements (they're development-only)
            content = Regex.Replace(content, @"@import\s+[^;]+;", "", RegexOptions.Multiline);

            // Skip @layer declarations since we have the global one
            // But preserve everything else
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var processedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip empty lines between removals
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                // Skip individual @layer declarations (we only want the global one)
                if (trimmed.StartsWith("@layer"))
                    continue;

                processedLines.Add(line);
            }

            if (processedLines.Count > 0)
            {
                // Add file boundary comment for debugging
                output.AppendLine($"/* ============================================ */");
                output.AppendLine($"/* {Path.GetFileName(inputFile)} */");
                output.AppendLine($"/* ============================================ */");
                output.AppendLine();

                foreach (var line in processedLines)
                {
                    output.AppendLine(line);
                }

                output.AppendLine();
            }
        }

        return output.ToString();
    }
}
