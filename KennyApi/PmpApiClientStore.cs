using PmpApiClient;

public static class PmpApiClientStore {
    private static PmpApiClientMock s_pmpApiClientMock = new PmpApiClientMock();
    public static BasePmpApiClient GetClient(string apiUser) {
        return s_pmpApiClientMock;
    }
}