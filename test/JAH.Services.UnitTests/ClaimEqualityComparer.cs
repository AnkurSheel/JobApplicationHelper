using System.Collections.Generic;
using System.Security.Claims;

public sealed class ClaimEqualityComparer : IEqualityComparer<Claim>
{
    public bool Equals(Claim x, Claim y)
    {
        return string.Equals(x.Type, y.Type) && string.Equals(x.Value, y.Value);
    }

    public int GetHashCode(Claim obj)
    {
        unchecked
        {
            return ((obj.Type != null ? obj.Type.GetHashCode() : 0) * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
        }
    }
}
