using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ResourceDetails {

    public class CustomField {
        public CustomField(string value, string type, string label, string columnName) {
            Value = value;
            Type = type;
            Label = label;
            ColumnName = columnName;
        }


        [JsonPropertyName("CUSTOMFIELDVALUE")]
        public string Value { get; }

        [JsonPropertyName("CUSTOMFIELDTYPE")]
        public string Type { get; }

        [JsonPropertyName("CUSTOMFIELDLABEL")]
        public string Label { get; }

        [JsonPropertyName("CUSTOMFIELDCOLUMNNAME")]
        public string ColumnName { get; }
    }

    public class Account {
        public Account(string isFavPass, string name, string passwordId, string passwordStatus, string id) {
            IsFavPass = isFavPass;
            Name = name;
            PasswordId = passwordId;
            PasswordStatus = passwordStatus;
            Id = id;
        }
        [JsonPropertyName("ISFAVPASS")]
        public string IsFavPass { get; }

        [JsonPropertyName("ACCOUNT NAME")]
        public string Name { get; }

        [JsonPropertyName("PASSWDID")]
        public string PasswordId { get; }

        [JsonPropertyName("PASSWORD STATUS")]
        public string PasswordStatus { get; }

        [JsonPropertyName("ACCOUNT ID")]
        public string Id { get; }
    }

    public ResourceDetails(string id, string name, string description, string type, string dnsName, string passwordPolicy, string department, string location, string url, string owner, IEnumerable<CustomField> customFields, IEnumerable<Account> accounts)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        DnsName = dnsName;
        PasswordPolicy = passwordPolicy;
        Department = department;
        Location = location;
        Url = url;
        Owner = owner;
        CustomFields = customFields;
        Accounts = accounts;
    }
    [JsonPropertyName("RESOURCE ID")]
    public string Id { get; }

    [JsonPropertyName("RESOURCE NAME")]
    public string Name { get; }

    [JsonPropertyName("RESOURCE DESCRIPTION")]
    public string Description { get; }

    [JsonPropertyName("RESOURCE TYPE")]
    public string Type { get; }

    [JsonPropertyName("DNS NAME")]
    public string DnsName { get; }

    [JsonPropertyName("PASSWORD POLICY")]
    public string PasswordPolicy { get; }

    [JsonPropertyName("DEPARTMENT")]
    public string Department { get; }

    [JsonPropertyName("LOCATION")]
    public string Location { get; }

    [JsonPropertyName("RESOURCE URL")]
    public string Url { get; }

    [JsonPropertyName("RESOURCE OWNER")]
    public string Owner { get; }

    [JsonPropertyName("CUSTOM FIELD")]
    public IEnumerable<CustomField> CustomFields { get; }

    [JsonPropertyName("ACCOUNT LIST")]
    [JsonConverter(typeof(ResourceAccountListJsonConverter))]
    public IEnumerable<Account> Accounts { get; }

}
