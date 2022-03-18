using System.Text.RegularExpressions;

public class PmpCredentialId {
    public string ResourceId { get; }
    public string AccountId { get; }

    private static Regex s_parseRegex = new Regex(@"^Pmp_(\d+)_(\d+)$");

    public PmpCredentialId(string resourceId, string accountId) {
        ResourceId = resourceId;
        AccountId = accountId;
    }

    public PmpCredentialId(string s) {
        Match m = s_parseRegex.Match(s);
        if (!m.Success)
            throw new ArgumentException("cannot parse {s} into PmpCredentialId");
        ResourceId = m.Groups[1].Value;
        AccountId = m.Groups[2].Value;
    }

    public override string ToString()
    {
        return $"Pmp_{ResourceId}_{AccountId}";
    }
}