using System.Text.RegularExpressions;

public class PmpCredentialId {
    public string ResourceGroupId { get; }
    public string ResourceId { get; }
    public string AccountId { get; }

    private static Regex s_parseRegex = new Regex(@"^Pmp_(\d+)_(\d+)_(\d+)$");

    public PmpCredentialId(string resourceGroupId, string resourceId, string accountId) {
        ResourceGroupId = resourceGroupId;
        ResourceId = resourceId;
        AccountId = accountId;
    }

    public PmpCredentialId(string s) {
        Match m = s_parseRegex.Match(s);
        if (!m.Success)
            throw new ArgumentException($"cannot parse {s} into PmpCredentialId");
        ResourceGroupId = m.Groups[1].Value;
        ResourceId = m.Groups[2].Value;
        AccountId = m.Groups[3].Value;
    }

    public override string ToString()
    {
        return $"Pmp_{ResourceGroupId}_{ResourceId}_{AccountId}";
    }
}