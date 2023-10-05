using System.Text.RegularExpressions;

namespace DataAccess.Utilities
{
    internal static class Sanitization
    {
        // both are similar ... for now !
        internal static string GenerateName(string name)
        {
            string sanitize = Regex.Replace(name.ToLower().Trim(), @"[^a-zA-Z0-9]+", "-");
            string sanitizedName = Regex.Replace(sanitize, @"-$", "");
            return sanitizedName;
        }

        internal static string GenerateSlug(string title)
        {
            string sanitize = Regex.Replace(title.ToLower().Trim(), @"[^a-zA-Z0-9]+", "-");
            string slug = Regex.Replace(sanitize, @"-$", "");
            return slug;
        }
    }
}