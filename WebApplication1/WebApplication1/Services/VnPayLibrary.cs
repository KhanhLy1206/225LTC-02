using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication1.Services
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public SortedList<string, string> GetRequestData()
        {
            return _requestData;
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var val) ? val : string.Empty;
        }

        private string VnpayUrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var encoded = WebUtility.UrlEncode(value);
            return Regex.Replace(encoded, @"%[0-9a-f]{2}", m => m.Value.ToUpper());
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var queryString = string.Join("&", _requestData.Select(kv => $"{VnpayUrlEncode(kv.Key)}={VnpayUrlEncode(kv.Value)}"));
            var secureHash = HmacSha512(vnpHashSecret, queryString);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var queryString = string.Join("&", _responseData.Select(kv => $"{VnpayUrlEncode(kv.Key)}={VnpayUrlEncode(kv.Value)}"));
            var checkHash = HmacSha512(secretKey, queryString);
            return checkHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("X2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}
