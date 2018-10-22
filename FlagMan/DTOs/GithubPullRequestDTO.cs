using System.Collections.Generic;

namespace FlagMan.DTOs
{
    public class GithubPullRequestDTO
    {
        public string url { get; set; }
        public long number { get; set; }
        public string state { get; set; }
        public bool locked { get; set; }
        public string title { get; set; }
        public UserDTO user {get;set;}
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string closed_at { get; set; }
        public string merged_at { get; set; }
        public List<LabelDTO> labels {get;set;}
        public HeadDTO head { get; set; }
        public BaseDTO @base { get; set; }
        public bool? mergeable { get; set; }
    }
}
