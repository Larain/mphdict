﻿using mphdict;
using mphdict.Models.morph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mphweb.Models
{
    public class dictParams
    {
        public filter f { get; set; }
        public incParams incp { get; set; }
        public word_param[] page { get; set; }
        public int maxpage { get; set; }
        public int count { get; set; }
    }
}