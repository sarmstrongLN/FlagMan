using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlagMan.DTOs
{
    public class FlagRequestDTO
    {
        public List<string> Flags { get; set; }
        public string RepoUrl { get; set; }
        public string BaseBranch { get; set; }
    }
}
