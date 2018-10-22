using FlagMan.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlagMan.Services
{
    public class Flag
    {
        private List<string> _flags;
        private string _repo;
        private GithubApi _api;
        private FlowdockApi _flowdock;

        public Flag(IConfiguration config, List<string> flags, string repo, string baseBranch)
        {
            _flags = flags;
            _repo = repo;
            _api = new GithubApi(config, GetRepoOwnerFromUrl(), GetRepoNameFromUrl(), baseBranch);
            _flowdock = new FlowdockApi(config);
        }

        public async void Process()
        {
            var repoOwner = GetRepoOwnerFromUrl();
            var repoName = GetRepoNameFromUrl();
            _api.RepoOwner = repoOwner;
            _api.RepoName = repoName;
            var relevantPRs = await GetAllRemovalPRs();
            ProcessRelevantPRs(relevantPRs);
        }

        private string[] GetRepoParts()
        {
            var githubIndex = _repo.IndexOf("github.com") + "github.com".Length;
            if (githubIndex == -1)
            {
                throw new Exception("Not a github.com URL");
            }

            var prefixStripped = _repo.Substring(githubIndex);
            string[] sections = prefixStripped.Split("/");
            return sections;
        }

        private string GetRepoOwnerFromUrl()
        {
            return GetRepoParts()[1];
        }

        private string GetRepoNameFromUrl()
        {
            return GetRepoParts()[2];
        }

        private async Task<List<GithubPullRequestDTO>> GetAllRemovalPRs()
        {
            var allPRs = await _api.GetAllOpenPullRequests();
            string removalPattern = ".*Remove.*";
            List<GithubPullRequestDTO> retVal = new List<GithubPullRequestDTO>();
            foreach( var PR in allPRs)
            {
                if(Regex.IsMatch(PR.title, removalPattern, RegexOptions.IgnoreCase))
                {
                    retVal.Add(PR);
                }
            }

            return retVal;
        }

        private async void ProcessRelevantPRs(List<GithubPullRequestDTO> PRs)
        {
            foreach( var PR in PRs)
            {
                foreach( var flag in _flags)
                {
                    if(Regex.IsMatch(PR.title, $".* {flag}.*", RegexOptions.IgnoreCase))
                    {
                        await ProcessPRForFlag(PR, flag);
                    }
                }
            }
        }

        private async Task ProcessPRForFlag(GithubPullRequestDTO PR, string flag)
        {
            var ready = await _api.IsReadyToMerge(PR);
            if (ready)
            {
                var response = await _api.MergePR(PR);
                if (response.merged)
                {
                    _flowdock.alertFlowdock($"[PR]({PR.url}) for the removal of {flag} has been automatically merged");
                } else
                {
                    _flowdock.alertFlowdock($"[PR]({PR.url}) for the removal of {flag} failed to merge automatically - {response.message}");
                }
            }
        }
    }
}
