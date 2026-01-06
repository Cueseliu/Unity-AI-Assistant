using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace AIOperator.LLM
{
    /// <summary>
    /// AWS Signature Version 4 签名工具
    /// 用于 AWS Bedrock API 请求签名
    /// </summary>
    public class AWSSignatureV4
    {
        private const string Algorithm = "AWS4-HMAC-SHA256";
        private const string ServiceName = "bedrock";
        private const string TerminationString = "aws4_request";

        public static Dictionary<string, string> SignRequest(
            string method,
            string url,
            string region,
            string accessKey,
            string secretKey,
            string payload,
            DateTime timestamp)
        {
            // 解析 URL
            Uri uri = new Uri(url);
            string host = uri.Host;
            string canonicalUri = uri.AbsolutePath;
            string canonicalQueryString = uri.Query.TrimStart('?');

            // 日期格式
            string amzDate = timestamp.ToString("yyyyMMddTHHmmssZ");
            string dateStamp = timestamp.ToString("yyyyMMdd");

            // 创建规范化请求头
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "host", host },
                { "x-amz-date", amzDate }
            };

            if (!string.IsNullOrEmpty(payload))
            {
                headers["content-type"] = "application/json";
            }

            // 排序并构建规范化请求头
            var sortedHeaders = headers.OrderBy(h => h.Key).ToList();
            string canonicalHeaders = string.Join("\n", sortedHeaders.Select(h => $"{h.Key}:{h.Value}")) + "\n";
            string signedHeaders = string.Join(";", sortedHeaders.Select(h => h.Key));

            // 计算 payload hash
            string payloadHash = string.IsNullOrEmpty(payload) 
                ? Hash(Encoding.UTF8.GetBytes(""))
                : Hash(Encoding.UTF8.GetBytes(payload));

            // 构建规范化请求
            string canonicalRequest = $"{method}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

            // 构建签名字符串
            string credentialScope = $"{dateStamp}/{region}/{ServiceName}/{TerminationString}";
            string stringToSign = $"{Algorithm}\n{amzDate}\n{credentialScope}\n{Hash(Encoding.UTF8.GetBytes(canonicalRequest))}";

            // 计算签名
            byte[] signingKey = GetSignatureKey(secretKey, dateStamp, region, ServiceName);
            string signature = ToHex(HmacSha256(signingKey, Encoding.UTF8.GetBytes(stringToSign)));

            // 构建 Authorization header
            string authorization = $"{Algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

            // 返回所需的 headers
            Dictionary<string, string> result = new Dictionary<string, string>
            {
                { "Authorization", authorization },
                { "x-amz-date", amzDate },
                { "host", host }
            };

            if (!string.IsNullOrEmpty(payload))
            {
                result["content-type"] = "application/json";
            }

            return result;
        }

        private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
        {
            byte[] kSecret = Encoding.UTF8.GetBytes("AWS4" + key);
            byte[] kDate = HmacSha256(kSecret, Encoding.UTF8.GetBytes(dateStamp));
            byte[] kRegion = HmacSha256(kDate, Encoding.UTF8.GetBytes(regionName));
            byte[] kService = HmacSha256(kRegion, Encoding.UTF8.GetBytes(serviceName));
            byte[] kSigning = HmacSha256(kService, Encoding.UTF8.GetBytes(TerminationString));
            return kSigning;
        }

        private static byte[] HmacSha256(byte[] key, byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        private static string Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return ToHex(hash);
            }
        }

        private static string ToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}