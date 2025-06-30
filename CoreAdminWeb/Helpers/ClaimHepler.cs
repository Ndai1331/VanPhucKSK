using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace CoreAdminWeb.Helpers
{
     public static class ClaimHepler
    {
        public static List<string> GetListClaim(string tokenStr)
        {
            tokenStr.Replace("Bearer ", "");
            JwtSecurityToken token;
            token = new JwtSecurityTokenHandler().ReadJwtToken(tokenStr);
            return token
                    .Claims.Select(c => c.Type.Trim())
                    .ToList();
        }
    }
}