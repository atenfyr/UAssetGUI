using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace UAssetGUI
{
    public static class GitHubAPI
    {
        public static string CombineURI(params string[] uris)
        {
            string output = "";
            foreach (string uriBit in uris)
            {
                output += uriBit.Trim('/') + "/";
            }
            return output.TrimEnd('/');
        }

        public static string GetLatestVersionURL(string repo)
        {
            return CombineURI("https://github.com", repo, "releases", "latest");
        }

        public static Version GetLatestVersionFromGitHub(string repo)
        {
            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.AllowAutoRedirect = false;

                    using (HttpClient client = new HttpClient(httpClientHandler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "UAssetGUI/" + UAGUtils._displayVersion);
                        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, GetLatestVersionURL(repo)));

                        string newURL = response.Headers.GetValues("location").First();

                        if (string.IsNullOrEmpty(newURL)) return null;
                        string[] splitURL = newURL.Split('/');

                        string finalVersionBit = splitURL[splitURL.Length - 1];
                        if (finalVersionBit[0] == 'v') finalVersionBit = finalVersionBit.Substring(1);
                        finalVersionBit = finalVersionBit.Replace(".0-alpha.", ".");

                        if (Version.TryParse(finalVersionBit, out Version foundVersion))
                        {
                            return foundVersion;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is FormatException) return null;
                throw;
            }
        }
    }
}
