using System.Security.Claims;
using PmpSqlClient;
using System.Text.RegularExpressions;

public class ResourceGroupAce {
    public enum AceAction { ALLOW, DENY }
    public string? GroupName { get; set; }
    public string? GroupPattern { get; set; }
    public bool Recursive { get; set; } = false;
    public IEnumerable<string>? Users { get; set; }
    public IEnumerable<string>? Groups { get; set; }
    public AceAction? Action { get; set; }

    private bool IsGroupMatch(string groupName) {
        if (GroupName != null && GroupName.Equals(groupName, StringComparison.CurrentCultureIgnoreCase)) {
            return true;
        }
    
        if (GroupPattern != null && Regex.IsMatch(input: groupName, pattern: GroupPattern)) {
            return true;
        }

        return false;
    }

    public AceAction? Check(ClaimsPrincipal user, ResourceGroup rg, Dictionary<long, ResourceGroup> rgs) {
        while (!IsGroupMatch(rg.Name)) {
            if (!Recursive)
                return null;

            if (!rgs.ContainsKey(rg.ParentId))
                return null;

            rg = rgs[rg.ParentId];
        }

        if (Users != null) {
            var username = user.Identity?.Name;
            if (username != null) {
                foreach (var aceUser in Users) {
                    if (username.Equals(aceUser, StringComparison.CurrentCultureIgnoreCase)) {
                        return Action;
                    }
                }
            }
        }

        if (Groups != null) {
            foreach (var aceGroup in Groups) {
                if (user.IsInRole(aceGroup)) {
                    return Action;
                }
            }
        }

        return null;
    }
}