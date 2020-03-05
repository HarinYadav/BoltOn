using System;
using System.Collections.Generic;
using System.Text;

namespace BoltOn.Data.Elasticsearch
{
    public class ElasticsearchModel
    {
        public int Size { get; set; }
        public int From { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
