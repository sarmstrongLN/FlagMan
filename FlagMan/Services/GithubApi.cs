using FlagMan.DTOs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlagMan.Services
{
    public class GithubApi
    {
        private const string API_BASE = "api.github.com/repos";
        private WebClient _client;
        private FlowdockApi _logger;
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
        public string BaseBranch { get; set; }

        public GithubApi(IConfiguration config, string owner, string name, string baseBranch)
        {
            RepoOwner = owner;
            RepoName = name;
            BaseBranch = baseBranch;
            _client = new WebClient(config);
            _logger = new FlowdockApi(config);

        }

        public async Task<List<GithubPullRequestDTO>> GetAllOpenPullRequests()
        {
            var url = $"https://{API_BASE}/{RepoOwner}/{RepoName}/pulls";
            if (BaseBranch.Length > 0)
            {
                url += $"?base={BaseBranch}";
            }
            var response =  await _client.Get(url);
            var content = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<GithubPullRequestDTO>>(content);
            
        }

        public async Task<GithubPullRequestDTO> GetPullRequestByNum(long num)
        {
            var url = $"https://{API_BASE}/{RepoOwner}/{RepoName}/pulls/{num}";
            var response = await _client.Get(url);
            var content = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GithubPullRequestDTO>(content);
        }

        public async Task<List<GithubReviewDTO>> GetPullRequestReviews(long num)
        {
            var url = $"https://{API_BASE}/{RepoOwner}/{RepoName}/pulls/{num}/reviews";
            var response = await _client.Get(url);
            var content = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<GithubReviewDTO>>(content);
        }

        public async Task<GithubMergeResponseDTO> MergePR(GithubPullRequestDTO PR)
        {
            return new GithubMergeResponseDTO
            {
                merged = false,
                message = "Still in dry run mode, no merged will be performed"
            };

            var url = $"https://{API_BASE}/{RepoOwner}/{RepoName}/pulls/{PR.number}/merge";
            var reqDTO = new GithubMergeRequestDTO
            {
                commit_title = $"Automatic merging of approved flag removal branch {PR.head.@ref}",
                merge_method = "squash"
            };

            var json = JsonConvert.SerializeObject(reqDTO);

            var response = await _client.Put(url, json);
            var content = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GithubMergeResponseDTO>(content);
        }



        public async Task<bool> IsReadyToMerge(GithubPullRequestDTO PR)
        {
            if(PR.state == "open" )
            {
                if (await IsApproved(PR))
                {
                    if (await IsMergeable(PR))
                    {
                        if (SatisfiesLabels(PR))
                        {
                            return true;
                        } else
                        {
                            var message = $"Flag removal PR opened by {PR.user.login} is open, approved, mergeable but does not satisfy Date restrictions: [Link]({PR.url})";
                            _logger.alertFlowdock(message);
                        }
                    } else
                    {
                        var message = $"Flag removal PR opened by {PR.user.login} is open, and approved but not mergeable: [Link]({PR.url})";
                        _logger.alertFlowdock(message);
                    }
                } else
                {
                    var message = $"Flag removal PR opened by {PR.user.login} is open but not approved: [Link]({PR.url})";
                    _logger.alertFlowdock(message);
                }
            }
            return false;
        }


        public async Task<bool> IsApproved(GithubPullRequestDTO PR)
        {
            var reviews = await GetPullRequestReviews(PR.number);

            Dictionary<string, ReviewStateDTO> userToReviewStateMap = new Dictionary<string, ReviewStateDTO>();
            foreach(var review in reviews)
            {
                string[] relevantStates = { "APPROVED", "CHANGES_REQUESTED" };
                if(!relevantStates.Contains(review.state))
                {
                    continue;
                }

                var reviewer = review.user.login;
                if(!userToReviewStateMap.ContainsKey(reviewer))
                {
                    userToReviewStateMap.Add(reviewer, new ReviewStateDTO
                    {
                        submitted = DateTime.Parse(review.submitted_at),
                        state = review.state
                    });
                } else
                {
                    var newSubmissionDate = DateTime.Parse(review.submitted_at);
                    if( newSubmissionDate > userToReviewStateMap[reviewer].submitted)
                    {
                        userToReviewStateMap[reviewer].submitted = newSubmissionDate;
                        userToReviewStateMap[reviewer].state = review.state;
                    }  
                }
            }

            return userToReviewStateMap.Count > 0 && userToReviewStateMap.Values.All(o => o.state == "APPROVED");
        }

        public async Task<bool> IsMergeable(GithubPullRequestDTO PR)
        {
            var fullPR = await GetPullRequestByNum(PR.number);
            return fullPR.mergeable == true;
        }

        public bool SatisfiesLabels(GithubPullRequestDTO PR)
        {
            var labels = PR.labels;

            foreach(var label in PR.labels)
            {
                DateTime threshold;
                var success = DateTime.TryParse(label.name, out threshold);
                if (success)
                {
                    return DateTime.Now > threshold;
                }
            }
            return true;
        }

        
    }
}
