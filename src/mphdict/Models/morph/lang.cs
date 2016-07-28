﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mphdict.Models.morph
{
    public class langid
    {
        [StringLength(255)]
        public string lang { get; set; }
        [Key]
        [StringLength(255)]
        public string pref { get; set; }
        [Required]
        public int id_lang { get; set; }

    }
}