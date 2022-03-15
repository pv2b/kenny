public static class Globals {
    public static ApiKeyring ApiKeyring;

    static Globals() {
        var apiKeyringPath = Path.Join(AppContext.BaseDirectory, "ApiKeyring.json");
        ApiKeyring = new ApiKeyring(apiKeyringPath);
    }
}