using System.Security.Claims;

namespace HIV_System_API_Backend.Common
{
    public static class ClaimsHelper
    {
        public static int? ExtractAccountIdFromClaims(ClaimsPrincipal user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            var accIdClaim = user.Claims.FirstOrDefault(c => c.Type == "AccountId");

            if (accIdClaim == null)
                return null;

            if (int.TryParse(accIdClaim.Value, out int accId))
                return accId;

            return null;
        }
    }
}
