using HIV_System_API_DTOs.VNPayPaymentDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HIV_System_API_Services.Implements
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _config;
        public VNPayService(IConfiguration config)
        {
            _config = config;
        }
        public string CreatePaymentUrl(VNPayPaymentRequestDTO request, string clientIp)
        {
            var vnp_Url = _config["VNPay:BaseUrl"];
            var vnp_Returnurl = request.ReturnUrl;
            var vnp_TmnCode = _config["VNPay:TmnCode"];
            var vnp_HashSecret = _config["VNPay:HashSecret"];

            var vnp_TxnRef = request.OrderId;
            var vnp_OrderInfo = request.OrderDescription;
            var vnp_Amount = ((int)(request.amount * 100)).ToString();
            var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            var vnp_Params = new Dictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", vnp_TmnCode },
        { "vnp_Amount", vnp_Amount },
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", vnp_TxnRef },
        { "vnp_OrderInfo", vnp_OrderInfo },
        { "vnp_OrderType", "other" },
        { "vnp_Locale", "vn" },
        { "vnp_ReturnUrl", vnp_Returnurl },
        { "vnp_IpAddr", clientIp },
        { "vnp_CreateDate", vnp_CreateDate }
    };

            // 1. Sort by key
            var ordered = vnp_Params.OrderBy(kvp => kvp.Key);

            // 2. Build signData with raw values (not URL-encoded)
            var signData = string.Join("&", ordered.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // 3. Build query with URL-encoded values
            var query = string.Join("&", ordered.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

            // 4. Hash
            var hash = HmacSHA512(vnp_HashSecret, signData);

            // 5. Build final URL
            var paymentUrl = $"{vnp_Url}?{query}&vnp_SecureHash={hash}";
            return paymentUrl;
        }

        public bool ValidateSignature(IQueryCollection query)
        {
            var vnp_HashSecret = _config["VNPay:HashSecret"];
            var vnp_SecureHash = query["vnp_SecureHash"].ToString();
            var vnp_Params = query
                .Where(kvp => kvp.Key.StartsWith("vnp_") && kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

            var signData = string.Join("&", vnp_Params.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var hash = HmacSHA512(vnp_HashSecret, signData);
            return hash.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
