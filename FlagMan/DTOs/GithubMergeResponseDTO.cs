using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlagMan.DTOs
{
    public class GithubMergeResponseDTO
    {
        public string sha { get; set; }
        public bool merged { get; set; }
        public string message { get; set; }
    }
}
