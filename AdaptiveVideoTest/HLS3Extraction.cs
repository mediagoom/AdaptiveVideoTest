using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.WebTesting;
using System.Xml;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace AdaptiveVideoTest
{
    class HLS3Extraction: ExtractionRule
    {
        string _ctx_name = "MPX";
        string _prefix = "";
        string _prefix_remove = "/main.m3u8";
    
        [Obsolete]
        public override string RuleName
        {
            get
            {
                return "HLS3 Extraction";
            }
        }

        [DisplayNameAttribute]
        public string DisplayName
        {
            get
            {
                return RuleName;
            }
        }

        [Obsolete]
        public override string RuleDescription
        {
            get
            {
                return "perform hls3 extraction";
            }
        }

        public override string ContextParameterName
        {
            get
            {
                return _ctx_name;
            }
            set
            {
               _ctx_name = value;
            }
        }

        [Description("PropertyDescriptionPrefix"), DisplayName("PropertyNamePrefix")]
        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; }
        }

        [Description("PropertyDescriptionPrefixRemove"), DisplayName("PropertyNamePrefixRemove")]
        public string PrefixRemove
        {
            get { return _prefix_remove; }
            set { _prefix_remove = value; }
        }

      

        public override void Extract(object sender,ExtractionEventArgs e)
        {
            int idx = 0;

            using(System.IO.MemoryStream m = new System.IO.MemoryStream( e.Response.BodyBytes))
            {
                using(System.IO.StreamReader r = new System.IO.StreamReader(m))
                {
                       
                       while(!r.EndOfStream)
                       {
                           string line = r.ReadLine();

                           if(line.StartsWith("#EXT-X-STREAM-INF:"))
                           {
                               string stream = Prefix + "/" + r.ReadLine();

                               e.WebTest.Context.Add(ContextParameterName + (idx++).ToString(), stream);
                           }
                       }
                }

            }

            e.WebTest.Context.Add(ContextParameterName + "TOT", idx.ToString());

            e.WebTest.Context.Add(ContextParameterName, e.WebTest.Context[ContextParameterName + "0"]);

            e.WebTest.Context.Add(ContextParameterName + "CURRENT", "0");
        }
    }

    class HLS3TSExtraction: ExtractionRule
    {
        string _ctx_name = "HLS";
        string _prefix = "";
        string _prefix_remove = "/main.m3u8";
    
        [Obsolete]
        public override string RuleName
        {
            get
            {
                return "HLS3 TS Extraction";
            }
        }

        [DisplayNameAttribute]
        public string DisplayName
        {
            get
            {
                return RuleName;
            }
        }

        [Obsolete]
        public override string RuleDescription
        {
            get
            {
                return "perform hls3 ts extraction";
            }
        }

        public override string ContextParameterName
        {
            get
            {
                return _ctx_name;
            }
            set
            {
               _ctx_name = value;
            }
        }

        [Description("PropertyDescriptionPrefix"), DisplayName("PropertyNamePrefix")]
        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; }
        }

        [Description("PropertyDescriptionPrefixRemove"), DisplayName("PropertyNamePrefixRemove")]
        public string PrefixRemove
        {
            get { return _prefix_remove; }
            set { _prefix_remove = value; }
        }

      

        public override void Extract(object sender,ExtractionEventArgs e)
        {
            int idx = 0;

            using(System.IO.MemoryStream m = new System.IO.MemoryStream( e.Response.BodyBytes))
            {
                using(System.IO.StreamReader r = new System.IO.StreamReader(m))
                {
                       while(!r.EndOfStream)
                       {
                           string line = r.ReadLine();

                           if(line.StartsWith("#EXTINF:"))
                           {
                               string stream = Prefix + "/" + r.ReadLine();

                               e.WebTest.Context.Add(ContextParameterName + (idx++).ToString(), stream);
                           }
                       }
                }

            }

            e.WebTest.Context.Add(ContextParameterName + "TOT", idx.ToString());

            e.WebTest.Context.Add(ContextParameterName, e.WebTest.Context[ContextParameterName + "0"]);

            e.WebTest.Context.Add(ContextParameterName + "CURRENT", "0");
        }
    }
}
