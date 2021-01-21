using System.Security.Cryptography;
using System.IO;

namespace CoreRemoting.Encryption
{
    /// <summary>
    /// Provides methods to perform a RSA key exchange.
    /// </summary>
    public static class RsaKeyExchange
    {
        /// <summary>
        /// Encrypts a secret with assymetric RSA algorithm.
        /// </summary>
        /// <param name="keySize">Key size (1024, 2048, 4096, ...)</param>
        /// <param name="receiversPublicKeyBlob">Public key of the receiver</param>
        /// <param name="secretToEncrypt">Secret to encrypt</param>
        /// <returns>Encrypted secret</returns>
        public static EncryptedSecret EncryptSecret(int keySize, byte[] receiversPublicKeyBlob, byte[] secretToEncrypt)
        {
            using var receiversPublicKey = new RSACryptoServiceProvider(dwKeySize: keySize);
            receiversPublicKey.ImportCspBlob(receiversPublicKeyBlob);
            
            using Aes aes = new AesCryptoServiceProvider();

            // Encrypt the session key
            var keyFormatter = new RSAPKCS1KeyExchangeFormatter(receiversPublicKey);
            
            // Encrypt the seceret
            using MemoryStream ciphertext = new MemoryStream();
            using CryptoStream stream = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write);
            
            stream.Write(secretToEncrypt, 0, secretToEncrypt.Length);
            stream.Close();
            
            return new EncryptedSecret(
                encryptedSessionKey: keyFormatter.CreateKeyExchange(aes.Key, typeof(Aes)),
                iv: aes.IV, 
                encryptedMessage: ciphertext.ToArray());
        }

        /// <summary>
        /// Decrypts a secret with assymetric RSA alogrithm.
        /// </summary>
        /// <param name="keySize">Key size (1024, 2048, 4096, ...)</param>
        /// <param name="receiversPrivateKeyBlob">Private key of the receiver</param>
        /// <param name="encryptedSecret">Encrypted secret</param>
        /// <returns>Decrypted secret</returns>
        public static byte[] DecrpytSecret(int keySize, byte[] receiversPrivateKeyBlob, EncryptedSecret encryptedSecret)
        {
            using var receiversPrivateKey = new RSACryptoServiceProvider(dwKeySize: keySize);
            receiversPrivateKey.ImportCspBlob(receiversPrivateKeyBlob);

            using Aes aes = new AesCryptoServiceProvider();
            aes.IV = encryptedSecret.Iv;

            // Decrypt the session key
            RSAPKCS1KeyExchangeDeformatter keyDeformatter = new RSAPKCS1KeyExchangeDeformatter(receiversPrivateKey);
            aes.Key = keyDeformatter.DecryptKeyExchange(encryptedSecret.EncryptedSessionKey);

            // Decrypt the message
            using MemoryStream stream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(encryptedSecret.EncryptedMessage, 0, encryptedSecret.EncryptedMessage.Length);
            cryptoStream.Close();

            return stream.ToArray();
        }
    }
}