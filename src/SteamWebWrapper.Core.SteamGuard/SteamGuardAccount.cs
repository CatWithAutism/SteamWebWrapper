﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamWebWrapper.SteamGuard
{
    public class SteamGuardAccount
    {
        [JsonPropertyName("shared_secret")]
        public required string SharedSecret { get; set; }

        [JsonPropertyName("serial_number")]
        public required string SerialNumber { get; set; }

        [JsonPropertyName("revocation_code")]
        public required string RevocationCode { get; set; }

        [JsonPropertyName("uri")]
        public required string Uri { get; set; }

        [JsonPropertyName("server_time")]
        public required long ServerTime { get; set; }

        [JsonPropertyName("account_name")]
        public required string AccountName { get; set; }

        [JsonPropertyName("token_gid")]
        public required string TokenGid { get; set; }

        [JsonPropertyName("identity_secret")]
        public required string IdentitySecret { get; set; }

        [JsonPropertyName("secret_1")]
        public required string Secret1 { get; set; }

        [JsonPropertyName("status")]
        public required int Status { get; set; }

        [JsonPropertyName("device_id")]
        public required string DeviceId { get; set; }

        /// <summary>
        /// Set to true if the authenticator has actually been applied to the account.
        /// </summary>
        [JsonPropertyName("fully_enrolled")]
        public bool FullyEnrolled { get; set; }

        public SessionData Session { get; set; }
        
        private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

        /// <summary>
        /// Remove steam guard from this account
        /// </summary>
        /// <param name="scheme">1 = Return to email codes, 2 = Remove completley</param>
        /// <returns></returns>
        public async Task<bool> DeactivateAuthenticator(int scheme = 1)
        {
            var postBody = new NameValueCollection();
            postBody.Add("revocation_code", RevocationCode);
            postBody.Add("revocation_reason", "1");
            postBody.Add("steamguard_scheme", scheme.ToString());
            string response = await SteamWeb.PostRequest("https://api.steampowered.com/ITwoFactorService/RemoveAuthenticator/v1?access_token=" + Session.AccessToken, null, postBody);

            // Parse to object
            var removeResponse = JsonConvert.DeserializeObject<RemoveAuthenticatorResponse>(response);

            if (removeResponse == null || removeResponse.Response == null || !removeResponse.Response.Success) return false;
            return true;
        }

        public string GenerateSteamGuardCode()
        {
            return GenerateSteamGuardCodeForTime(TimeAligner.GetSteamTime());
        }

        public async Task<string> GenerateSteamGuardCodeAsync()
        {
            return GenerateSteamGuardCodeForTime(await TimeAligner.GetSteamTimeAsync());
        }

        public string GenerateSteamGuardCodeForTime(long time)
        {
            if (SharedSecret == null || SharedSecret.Length == 0)
            {
                return "";
            }

            string sharedSecretUnescaped = Regex.Unescape(SharedSecret);
            byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
            byte[] timeArray = new byte[8];

            time /= 30L;

            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            HMACSHA1 hmacGenerator = new HMACSHA1();
            hmacGenerator.Key = sharedSecretArray;
            byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
            byte[] codeArray = new byte[5];
            try
            {
                byte b = (byte)(hashedData[19] & 0xF);
                int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (int i = 0; i < 5; ++i)
                {
                    codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                    codePoint /= SteamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                return null; //Change later, catch-alls are bad!
            }
            return Encoding.UTF8.GetString(codeArray);
        }

        public Confirmation[] FetchConfirmations()
        {
            string url = GenerateConfirmationUrl();
            string response = SteamWeb.GetRequest(url, Session.GetCookies()).Result;
            return FetchConfirmationInternal(response);
        }

        public async Task<Confirmation[]> FetchConfirmationsAsync()
        {
            string url = GenerateConfirmationUrl();
            string response = await SteamWeb.GetRequest(url, Session.GetCookies());
            return FetchConfirmationInternal(response);
        }

        private Confirmation[] FetchConfirmationInternal(string response)
        {
            var confirmationsResponse = JsonConvert.DeserializeObject<ConfirmationsResponse>(response);

            if (!confirmationsResponse.Success)
            {
                throw new Exception(confirmationsResponse.Message);
            }

            if (confirmationsResponse.NeedAuthentication)
            {
                throw new Exception("Needs Authentication");
            }

            return confirmationsResponse.Confirmations;
        }

        /// <summary>
        /// Deprecated. Simply returns conf.Creator.
        /// </summary>
        /// <param name="conf"></param>
        /// <returns>The Creator field of conf</returns>
        public long GetConfirmationTradeOfferId(Confirmation conf)
        {
            if (conf.ConfType != Confirmation.EMobileConfirmationType.Trade)
                throw new ArgumentException("conf must be a trade confirmation.");

            return (long)conf.Creator;
        }

        public async Task<bool> AcceptMultipleConfirmations(Confirmation[] confs)
        {
            return await _sendMultiConfirmationAjax(confs, "allow");
        }

        public async Task<bool> DenyMultipleConfirmations(Confirmation[] confs)
        {
            return await _sendMultiConfirmationAjax(confs, "cancel");
        }

        public async Task<bool> AcceptConfirmation(Confirmation conf)
        {
            return await _sendConfirmationAjax(conf, "allow");
        }

        public async Task<bool> DenyConfirmation(Confirmation conf)
        {
            return await _sendConfirmationAjax(conf, "cancel");
        }

        private async Task<bool> _sendConfirmationAjax(Confirmation conf, string op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/ajaxop";
            string queryString = "?op=" + op + "&";
            // tag is different from op now
            string tag = op == "allow" ? "accept" : "reject";
            queryString += GenerateConfirmationQueryParams(tag);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            string response = await SteamWeb.GetRequest(url, Session.GetCookies());
            if (response == null) return false;

            SendConfirmationResponse confResponse = JsonConvert.DeserializeObject<SendConfirmationResponse>(response);
            return confResponse.Success;
        }

        private async Task<bool> _sendMultiConfirmationAjax(Confirmation[] confs, string op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/multiajaxop";
            // tag is different from op now
            string tag = op == "allow" ? "accept" : "reject";
            string query = "op=" + op + "&" + GenerateConfirmationQueryParams(tag);
            foreach (var conf in confs)
            {
                query += "&cid[]=" + conf.Id + "&ck[]=" + conf.Key;
            }

            string response;
            using (CookieAwareWebClient wc = new CookieAwareWebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.CookieContainer = Session.GetCookies();
                wc.Headers[HttpRequestHeader.UserAgent] = SteamWeb.MobileAppUserAgent;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
                response = await wc.UploadStringTaskAsync(new Uri(url), "POST", query);
            }
            if (response == null) return false;

            SendConfirmationResponse confResponse = JsonConvert.DeserializeObject<SendConfirmationResponse>(response);
            return confResponse.Success;
        }

        public string GenerateConfirmationUrl(string tag = "conf")
        {
            string endpoint = ApiEndpoints.CommunityBase + "/mobileconf/getlist?";
            string queryString = GenerateConfirmationQueryParams(tag);
            return endpoint + queryString;
        }

        public string GenerateConfirmationQueryParams(string tag)
        {
            if (String.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            var queryParams = GenerateConfirmationQueryParamsAsNvc(tag);

            return string.Join("&", queryParams.AllKeys.Select(key => $"{key}={queryParams[key]}"));
        }

        public NameValueCollection GenerateConfirmationQueryParamsAsNvc(string tag)
        {
            if (String.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            long time = TimeAligner.GetSteamTime();

            var ret = new NameValueCollection();
            ret.Add("p", DeviceId);
            ret.Add("a", Session.SteamId.ToString());
            ret.Add("k", _generateConfirmationHashForTime(time, tag));
            ret.Add("t", time.ToString());
            ret.Add("m", "react");
            ret.Add("tag", tag);

            return ret;
        }

        private string _generateConfirmationHashForTime(long time, string tag)
        {
            byte[] decode = Convert.FromBase64String(IdentitySecret);
            int n2 = 8;
            if (tag != null)
            {
                if (tag.Length > 32)
                {
                    n2 = 8 + 32;
                }
                else
                {
                    n2 = 8 + tag.Length;
                }
            }
            byte[] array = new byte[n2];
            int n3 = 8;
            while (true)
            {
                int n4 = n3 - 1;
                if (n3 <= 0)
                {
                    break;
                }
                array[n4] = (byte)time;
                time >>= 8;
                n3 = n4;
            }
            if (tag != null)
            {
                Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);
            }

            try
            {
                HMACSHA1 hmacGenerator = new HMACSHA1();
                hmacGenerator.Key = decode;
                byte[] hashedData = hmacGenerator.ComputeHash(array);
                string encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
                string hash = WebUtility.UrlEncode(encodedData);
                return hash;
            }
            catch
            {
                return null;
            }
        }

        public class WgTokenInvalidException : Exception
        {
        }

        public class WgTokenExpiredException : Exception
        {
        }

        private class RemoveAuthenticatorResponse
        {
            [JsonProperty("response")]
            public RemoveAuthenticatorInternalResponse Response { get; set; }

            internal class RemoveAuthenticatorInternalResponse
            {
                [JsonProperty("success")]
                public bool Success { get; set; }

                [JsonProperty("revocation_attempts_remaining")]
                public int RevocationAttemptsRemaining { get; set; }
            }
        }

        private class SendConfirmationResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
        }

        private class ConfirmationDetailsResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("html")]
            public string Html { get; set; }
        }
    }
}
