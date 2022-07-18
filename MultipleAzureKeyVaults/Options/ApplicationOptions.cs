namespace MultipleAzureKeyVaults.Options
{
    public class ApplicationOptions
    {
        public KeyVaultOne KeyVaultOne { get; set; }
        public KeyVaultTwo KeyVaultTwo { get; set; }
        public string SecretName { get; set; }
    }
}