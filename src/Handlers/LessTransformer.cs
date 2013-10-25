using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Optimization;

namespace DotSmart
{
    public class LessTransformer : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StringWriter())
                {
                    using (var sw = new StreamWriter(stream))
                    {
                        foreach (var file in response.Files)
                        {
                            LessCssHandler.RenderCss(context.HttpContext.Server.MapPath(file.IncludedVirtualPath), sw, false, null, null, null);
                        }

                        sw.Flush();
                        stream.Position = 0;
                        using (var streamReader = new StreamReader(stream))
                        {
                            response.Content = streamReader.ReadToEnd();
                            response.ContentType = "text/css";
                        }
                    }
                }
            }
        }

    }
}
