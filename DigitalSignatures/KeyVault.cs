﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureKeyVault.DigitalSignatures
{
    public class KeyVault : KeyVaultBase, IKeyVault
    {
        public KeyVault()
        {
            var clientId = "fc25ed2c-43e4-479d-a550-7baf3a172bbb";
            var clientSecret = "ItQBEzyIR+baZPn5VVSBmXf6pa5bZlPuyYrXIcc3lGU=";
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
            var keyBundle = GetKeyBundle();
            var createdKey = await KeyVaultClient.CreateKeyAsync(VaultAddress, keyName, keyBundle.Key.Kty, keyAttributes: keyBundle.Attributes, tags: GetKeyTags());

            return createdKey.KeyIdentifier.Identifier;
        }

        public async Task DeleteKeyAsync(string keyName)
        {
            await KeyVaultClient.DeleteKeyAsync(VaultAddress, keyName);
        }

        public async Task<byte[]> EncryptAsync(string keyId, byte[] dataToEncrypt)
        {
            var operationResult = await KeyVaultClient.EncryptAsync(keyId, JsonWebKeyEncryptionAlgorithm.RSAOAEP, dataToEncrypt);

            return operationResult.Result;
        }

        public async Task<byte[]> DecryptAsync(string keyId, byte[] dataToDecrypt)
        {
            var operationResult = await KeyVaultClient.DecryptAsync(keyId, JsonWebKeyEncryptionAlgorithm.RSAOAEP, dataToDecrypt);

            return operationResult.Result;
        }

        public async Task<string> SetSecretAsync(string secretName, string secretValue)
        {     
            var bundle = await KeyVaultClient.SetSecretAsync(VaultAddress, secretName, secretValue, null, "plaintext");
            return bundle.Id;
        }

		public async Task<string> GetSecretAsync(string secretName)
		{
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

        public async Task<byte[]> Sign(string keyId, byte[] hash)
        {
            var bundle = await KeyVaultClient.SignAsync(keyId, "RS256", hash);
            return bundle.Result;
        }

        public async Task<bool> Verify(string keyId, byte[] hash, byte[] signature)
        {
            var result = await KeyVaultClient.VerifyAsync(keyId, "RS256", hash, signature);
            return result;
        }
    }
}
