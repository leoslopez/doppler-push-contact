using System;

namespace Doppler.PushContact.Transversal.Test
{
    public class EncryptionHelperTest
    {
        private const string TestKey = "5Rz2VJbnjbhPfEKn3Ryd0E+u7jzOT2KCBicmM5wUq5Y="; // Key in Base64, 32 bytes length
        private const string TestIV = "7yZ8kT8L7UeO8JpH3Ir6jQ=="; // IV in Base64, 16 bytes length

        public EncryptionHelperTest()
        {
            EncryptionHelper.Initialize(TestKey, TestIV);
        }

        [Fact]
        public void KeyAndIV_ShouldHaveCorrectLength()
        {
            byte[] keyBytes = Convert.FromBase64String(TestKey);
            byte[] ivBytes = Convert.FromBase64String(TestIV);

            // Assert
            // length of the Key and IV
            Assert.Equal(32, keyBytes.Length);
            Assert.Equal(16, ivBytes.Length);
        }

        [Theory]
        [InlineData("66291accdc3ab636288af4ab")] // a string representing a mongodb ObjectId
        [InlineData("df555721-5135-4b5d-9c6a-7db3565f22ae")] // a string representing valid Guid
        [InlineData("my hello world sentence to be encrypted")]
        public void Encrypt_ShouldEncryptText(string plainText)
        {
            // Arrange

            // Act
            string encryptedText = EncryptionHelper.Encrypt(plainText);

            // Assert
            Assert.False(string.IsNullOrEmpty(encryptedText));
            Assert.NotEqual(plainText, encryptedText);
        }

        [Theory]
        [InlineData("66291accdc3ab636288af4ab")] // a string representing a mongodb ObjectId
        [InlineData("df555721-5135-4b5d-9c6a-7db3565f22ae")] // a string representing valid Guid
        [InlineData("my hello world sentence to be decrypted")]
        public void Decrypt_ShouldReturnOriginalText(string originalText)
        {
            // Arrange

            // Act
            string encryptedText = EncryptionHelper.Encrypt(originalText);
            string decryptedText = EncryptionHelper.Decrypt(encryptedText);

            // Assert
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public void Decrypt_Base64Url_ShouldReturnOriginalText()
        {
            // Arrange
            string originalText = "Test string with special characters: /+=?&";

            // Act
            string encryptedText = EncryptionHelper.Encrypt(originalText, useBase64Url: true);
            string decryptedText = EncryptionHelper.Decrypt(encryptedText, useBase64Url: true);

            // Assert
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public void Encrypt_Base64Url_ShouldNotContainSpecialCharacters()
        {
            // Arrange
            string plainText = "Test string with special characters: /+=?&";

            // Act
            string encryptedText = EncryptionHelper.Encrypt(plainText, useBase64Url: true);

            // Assert
            Assert.DoesNotContain("/", encryptedText);
            Assert.DoesNotContain("+", encryptedText);
            Assert.DoesNotContain("=", encryptedText);
        }
    }
}
