using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HIV_System_API_Services.Implements
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IMemoryCache _cache;
        private const int CODE_LENGTH = 6;
        private static readonly TimeSpan CODE_EXPIRY = TimeSpan.FromMinutes(4);

        public VerificationCodeService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateCode(string email)
        {
            var code = Random.Shared.Next(100000, 999999).ToString("D6");
            
            // Store in cache with 4-minute expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CODE_EXPIRY)
                .SetSize(256);
            
            _cache.Set($"verification_code_{email}", code, cacheOptions);
            
            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            var cacheKey = $"verification_code_{email}";
            
            if (!_cache.TryGetValue(cacheKey, out string? storedCode))
                return false;

            // Remove the code immediately after verification attempt
            _cache.Remove(cacheKey);

            return code == storedCode;
        }
    }
}
