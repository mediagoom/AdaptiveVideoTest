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
    class MpdExtraction: ExtractionRule
    {
        string _ctx_name = "MPX";
        string _prefix = "";
        string _prefix_remove = "/index.mpd";
    
        [Obsolete]
        public override string RuleName
        {
            get
            {
                return "MPD Extraction";
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
                return "perform mpd extraction";
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

        
        private void Push(List<string> container,string url,int k,long b,long t)
        {
            string dest = url.Replace("$Bandwidth$",b.ToString()).Replace("$Time$",t.ToString());

            dest = Prefix + dest;

            container.Add(dest);
        }

        private int process_stream(XmlNode st, List<string> container)
        {
            try
            {
                //string type = st.Attributes["Type"].Value;
                string init = st.Attributes["initialization"].Value;
                string url  = st.Attributes["media"].Value;

                XmlNodeList nl = st.SelectNodes("*[local-name()='SegmentTimeline']/*[local-name()='S']");

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

                    long r = 1;

                    if(null != c.Attributes["r"])
                    {
                        r = long.Parse(c.Attributes["r"].Value);
                        r++;
                    }
                    

                    long d = 0;

                    for(int j = 0;j < r;j++)
                    {
                        d = long.Parse(c.Attributes["d"].Value);

                        if(j < (r - 1))
                        {
                            time += d;
                            times.Add(time);
                        }
                    }

                    time += d;
                }

                XmlNodeList quality = st.SelectNodes("../*[local-name()='Representation']");

                foreach(XmlNode q in quality)
                {
                    long b = long.Parse(q.Attributes["bandwidth"].Value);

                    bitrates.Add(b);
                }

                int k = 0;// start;

                foreach(long b in bitrates)
                {
                    Push(container, init, k++, b, 0);


                    foreach(long t in times)
                    {
                        Push(container, url, k++, b, t);
                    }
                }

                return k;

            }catch(Exception ex)
            {
                throw ex;
            }

        }

        private int maxs(List<List<string> > streams_containers)
        {
            int k = 0;

            foreach(List<string> s in streams_containers)
            {
                if(k < s.Count)
                    k = s.Count;
            }

            return k;
        }

      

        public override void Extract(object sender,ExtractionEventArgs e)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            doc.LoadXml(e.Response.BodyString);

            XmlNodeList nl = doc.SelectNodes("//*[local-name()='SegmentTemplate']");
            
            int k = 0;

            List<List<string> > streams_containers = new List<List<string>>(nl.Count);
            
            
            foreach(XmlNode n in nl)
            {
                int j = 0;

                streams_containers.Add(new List<string>());

                j = process_stream(n, streams_containers[k++]);
            }

            int m = maxs(streams_containers);

            int idx = 0;

            for(int i = 0;i < m; i++)
            {
                for(int h = 0 ; h < streams_containers.Count; h++)
                {
                    if(i < streams_containers[h].Count)
                    {
                        e.WebTest.Context.Add(ContextParameterName + (idx++).ToString(), streams_containers[h][i]);
                    }
                }
            }

                e.WebTest.Context.Add(ContextParameterName + "TOT", idx.ToString());

            e.WebTest.Context.Add(ContextParameterName, e.WebTest.Context[ContextParameterName + "0"]);

            e.WebTest.Context.Add(ContextParameterName + "CURRENT", "0");
        }
    }
}
