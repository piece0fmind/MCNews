﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Shared
{
    public class ApiResponse 
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
       
}
