using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace AdaptiveVideoTest
{
    public class IsmExtraction: ExtractionRule
    {
        string _ctx_name = "VDX";
    
        [Obsolete]
        public override string RuleName
        {
            get
            {
                return "ISM Extraction";
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
                return "perform ism extraction";
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

        private int process_stream(XmlNode st, WebTestContext ctx, int start)
        {
            string type = st.Attributes["Type"].Value;
            string url  = st.Attributes["Url"].Value;
            
            XmlNodeList nl = st.SelectNodes("c");

            long time = 0;

            List<long> times    = new List<long>();
            List<long> bitrates = new List<long>();

            foreach(XmlNode c in nl)
            {
                if(null != c.Attributes["t"])
                {
                    time = long.Parse(c.Attributes["t"].Value);
                }

                times.Add(time);

                long d = long.Parse(c.Attributes["d"].Value);

                time += d;
            }

            XmlNodeList quality = st.SelectNodes("QualityLevel");

            foreach(XmlNode q in quality)
            {
                long b = long.Parse(q.Attributes["Bitrate"].Value);

                bitrates.Add(b);
            }

            int k = start;

            foreach(long b in bitrates)
            {
                foreach(long t in times)
                {
                    string dest = url.Replace("{bitrate}", b.ToString()).Replace("{start time}", t.ToString());

                    ctx.Add(ContextParameterName + (k++).ToString(),dest);

                }
            }

            return k;

        }

        public override void Extract(object sender,ExtractionEventArgs e)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            doc.LoadXml(e.Response.BodyString);

            XmlNodeList nl = doc.SelectNodes("//StreamIndex");
            
            int k = 0;

            foreach(XmlNode n in nl)
            {
                k = process_stream(n, e.WebTest.Context, k);
            }

            e.WebTest.Context.Add(ContextParameterName + "TOT",k.ToString());

            e.WebTest.Context.Add(ContextParameterName, e.WebTest.Context[ContextParameterName + "0"]);

            e.WebTest.Context.Add(ContextParameterName + "CURRENT", "0");
        }
    }


    public class IsmCounter:ExtractionRule
    {
        string _ctx_name = "VDX";

        [Obsolete]
        public override string RuleName
        {
            get
            {
                return "ISM Counter";
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
                return "perform ism extraction";
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

        public override void Extract(object sender,ExtractionEventArgs e)
        {
            string v = e.WebTest.Context[ContextParameterName + "CURRENT"].ToString();

            long k = long.Parse(v);

            string tot = e.WebTest.Context[ContextParameterName + "TOT"].ToString();


            long t = long.Parse(tot);

            k++;

            if(k >= t)
            {
                e.WebTest.Context[ContextParameterName + "TOT"] = "0";
                return;
            }


            e.WebTest.Context.Add(ContextParameterName,e.WebTest.Context[ContextParameterName + k.ToString()]);
            e.WebTest.Context.Add(ContextParameterName + "CURRENT",k.ToString());


        }
    }
}
