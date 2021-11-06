using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetLatestVersionURL(repo));
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                request.ContentType = "application/json; charset=utf-8";
                request.UserAgent = "UAssetGUI/" + UAGUtils._displayVersion;

                string newURL = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    newURL = response.Headers["location"];
                }

                if (string.IsNullOrEmpty(newURL)) return null;
                string[] splitURL = newURL.Split('/');

                string finalVersionBit = splitURL[splitURL.Length - 1];
                if (finalVersionBit[0] == 'v') finalVersionBit = finalVersionBit.Substring(1);
                finalVersionBit = finalVersionBit.Replace(".0-alpha.", ".");

                Version.TryParse(finalVersionBit, out Version foundVersion);
                return foundVersion;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is FormatException) return null;
                throw;
            }
        }
    }
}
