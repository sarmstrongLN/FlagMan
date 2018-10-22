using System;

namespace FlagMan.DTOs
{
    public class GithubReviewDTO
    {
        public UserDTO user { get; set; }
        public string state { get; set; }
        public string submitted_at { get; set; }
    }
}
