﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using DotSmart.LessCoffee;
using DotSmart.Properties;
using System.Diagnostics;

namespace DotSmart
{
    public class LessCssHandler : ScriptHandlerBase, IHttpHandler
    {
        static string _lesscompiler;
        static string _lessc;

        /// <summary>
        /// Initializes a new instance of the LessCssHandler class.
        /// </summary>
        static LessCssHandler()
        {
            _lessc = Path.Combine(TempDirectory, @"node_modules\less\bin\lessc");
        }

        internal static void AppStart()
        {

        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/css";

            context.Response.Write("/* Generated by DotSmart LessCoffee on " + DateTime.Now + " - http://dotsmart.net */ \r\n");
            context.Response.Write("/* Using LESS - Leaner CSS - http://lesscss.org - Copyright (c) 2009-2012, Alexis Sellier */ \r\n\r\n");

            string lessFile = context.Server.MapPath(context.Request.FilePath);

            bool success = renderStylesheet(lessFile, context.Response.Output);
            if (success)
            {
                // look for "@import" and add those to dependencies also
                var lessFiles = parseImports(lessFile).Concat(new[] { lessFile }).ToArray();
                SetCacheability(context.Response, lessFiles);
            }
        }

        static IEnumerable<string> parseImports(string lessFileName)
        {
            string dir = Path.GetDirectoryName(lessFileName);

            var importRegex = new Regex(@"@import\s+[""'](.*)[""'];");

            return (from line in File.ReadAllLines(lessFileName)
                    let match = importRegex.Match(line)
                    let file = match.Groups[1].Value
                    where match.Success
                      && !file.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    select Path.Combine(dir, Path.ChangeExtension(file, ".less"))
            );
        }

        bool renderStylesheet(string lessFilePath, TextWriter output)
        {
            try
            {
                RenderCss(lessFilePath, output, compress: !DebugMode);
                return true;
            }
            catch (Exception ex)
            {
                output.WriteLine("/* " + ex.Message + " */");
                return false;
            }
        }

        public static void RenderCss(string lessFilePath, TextWriter output, bool compress = true, string lessPrologue = null)
        {
            TextReader lessFile;
            if (lessPrologue != null)
                lessFile = new StringReader(lessPrologue + File.ReadAllText(lessFilePath));
            else
                lessFile = new StreamReader(lessFilePath);

            using (lessFile)
            using (var errors = new StringWriter())
            {
                int exitCode = ProcessUtil.Exec(NodeExe,
                    args: "\"" + _lessc + "\""
                        + " - " // read from stdidn
                        + (compress ? " --yui-compress" : "")
                        + " --include-path=\"" + Path.GetDirectoryName(lessFilePath) + "\""
                        + " --no-color",
                    stdIn: lessFile,
                    stdOut: output,
                    stdErr: errors);
                if (exitCode != 0)
                {
                    throw new ApplicationException(string.Format("Error {0} in '{1}': \r\n{2}",
                                                    exitCode,
                                                    lessFilePath,
                                                    errors.ToString().Trim()));
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

    }
}
