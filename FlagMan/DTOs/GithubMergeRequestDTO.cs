using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlagMan.DTOs
{
    public class GithubMergeRequestDTO
    {
        public string commit_title { get; set; }
        public string commit_message { get; set; }
        public string sha { get; set; }
        public string merge_method { get; set; } = "squash";
    }
}
