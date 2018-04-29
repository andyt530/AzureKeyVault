﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureKeyVault.PasswordProtection
{
    public class KeyVault : KeyVaultBase, IKeyVault
    {
        public KeyVault()
        {
            var clientId = "922e5bfc-98b3-4f23-bcbb-8a12439ebbfb";
            var clientSecret = "PsAJTPBeG1WwFq+hH+ZQAh9xadcNI60cep8FYPpt3Ew=";
            VaultAddress = "https://RIKeyVault.vault.azure.net/";

            ClientCredential = new ClientCredential(clientId, clientSecret);
            KeyVaultClient = new KeyVaultClient(GetAccessTokenAsync, GetHttpClient());
        }


		public KeyVault(string clientId, string clientSecret, string vaultAddress )
		{
			VaultAddress = vaultAddress;

			ClientCredential = new ClientCredential(clientId, clientSecret);
			KeyVaultClient = new KeyVaultClient(GetAccessTokenAsync, GetHttpClient());
		}

        public async Task<string> CreateKeyAsync(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException(keyName);
            }

            var keyBundle = GetKeyBundle();
            var createdKey = await KeyVaultClient.CreateKeyAsync(VaultAddress, keyName, keyBundle.Key.Kty, keyAttributes: keyBundle.Attributes, tags: GetKeyTags());

            return createdKey.KeyIdentifier.Identifier;
        }

        public async Task DeleteKeyAsync(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException(keyName);
            }

            await KeyVaultClient.DeleteKeyAsync(VaultAddress, keyName);
        }

        public async Task<byte[]> EncryptAsync(string keyId, byte[] dataToEncrypt)
        {
            if (string.IsNullOrEmpty(keyId))
            {
                throw new ArgumentNullException(keyId, "Key Id is Null.");
            }

            if (dataToEncrypt == null)
            {
                throw new ArgumentNullException(nameof(dataToEncrypt), "Data to Encrypt is Null.");
            }

            if (dataToEncrypt.Length == 0)
            {
                throw new ArgumentNullException(nameof(dataToEncrypt), "Data to Encrypt is Empty.");
            }

            var operationResult = await KeyVaultClient.EncryptAsync(keyId, JsonWebKeyEncryptionAlgorithm.RSAOAEP, dataToEncrypt);

            return operationResult.Result;
        }

        public async Task<byte[]> DecryptAsync(string keyId, byte[] dataToDecrypt)
        {
            if (string.IsNullOrEmpty(keyId))
            {
                throw new ArgumentNullException(keyId, "Key NickName is Null.");
            }

            if (dataToDecrypt == null)
            {
                throw new ArgumentNullException(nameof(dataToDecrypt), "Data to Derypt is Null.");
            }

            if (dataToDecrypt.Length == 0)
            {
                throw new ArgumentNullException(nameof(dataToDecrypt), "Data to Encrypt is Empty.");
            }

            var operationResult = await KeyVaultClient.DecryptAsync(keyId, JsonWebKeyEncryptionAlgorithm.RSAOAEP, dataToDecrypt);

            return operationResult.Result;
        }

        public async Task<string> SetSecretAsync(string secretName, string secretValue)
        {
			if (string.IsNullOrEmpty(secretName))
			{
                throw new ArgumentNullException(nameof(secretName));
			}

			if (string.IsNullOrEmpty(secretValue))
			{
				throw new ArgumentNullException(nameof(secretValue));
			}
                    
            var bundle = await KeyVaultClient.SetSecretAsync(VaultAddress, secretName, secretValue, null, "plaintext");
            return bundle.Id;
        }

		public async Task<string> GetSecretAsync(string secretName)
		{
			if (string.IsNullOrEmpty(secretName))
			{
				throw new ArgumentNullException(nameof(secretName));
			}

            try
            {
                var bundle = await KeyVaultClient.GetSecretAsync(VaultAddress, secretName);
                return bundle.Value;
            }
            catch (KeyVaultErrorException)
            {
                return string.Empty;
            }
		}
    }
}
