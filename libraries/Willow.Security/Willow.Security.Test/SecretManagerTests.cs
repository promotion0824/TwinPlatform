// -----------------------------------------------------------------------
// <copyright file="SecretManagerTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.Security.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Security.KeyVault.Secrets;
    using Moq;
    using Willow.Security.KeyVault;
    using Xunit;
    using Xunit.Abstractions;

    public class SecretManagerTests
    {
        private readonly ITestOutputHelper output;

        public SecretManagerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GetSecret_ReturnsPrimarySecret_WithValidSecretInKeyVault()
        {
            var mockSecretClient = new Mock<SecretClient>();
            var primaryKeyVaultSecret = new KeyVaultSecret("Test-Primary", "PrimarySecret");
            var secondaryKeyVaultSecret = new KeyVaultSecret("Test-Secondary", "SecondarySecret");

            var primaryResponse = Response.FromValue(primaryKeyVaultSecret, Mock.Of<Response>());
            var secondaryResponse = Response.FromValue(secondaryKeyVaultSecret, Mock.Of<Response>());

            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Primary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(primaryResponse);
            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Secondary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(secondaryResponse);

            var semaphore = new Semaphore(1, 1);
            var secretManager = new SecretManager(mockSecretClient.Object, semaphore);

            var secret = await secretManager.GetSecretAsync("Test");

            Assert.NotNull(secret);
            Assert.Equal(primaryKeyVaultSecret.Name, secret.Name);
            Assert.Equal(primaryKeyVaultSecret.Value, secret.Value);
        }

        [Fact]
        public async Task GetSecret_ReturnsSecondarySecret_WithPrimaryFail()
        {
            var mockSecretClient = new Mock<SecretClient>();
            var primaryKeyVaultSecret = new KeyVaultSecret("Test-Primary", "PrimarySecret");
            var secondaryKeyVaultSecret = new KeyVaultSecret("Test-Secondary", "SecondarySecret");

            var primaryResponse = Response.FromValue(primaryKeyVaultSecret, Mock.Of<Response>());
            var secondaryResponse = Response.FromValue(secondaryKeyVaultSecret, Mock.Of<Response>());

            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Primary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(primaryResponse);
            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Secondary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(secondaryResponse);

            var semaphore = new Semaphore(1, 1);
            var secretManager = new SecretManager(mockSecretClient.Object, semaphore);

            // Load the original secrets
            _ = await secretManager.GetSecretAsync("Test");

            // Fail the primary
            await secretManager.IncrementFailureAsync("Test");

            var secret = await secretManager.GetSecretAsync("Test");

            Assert.NotNull(secret);
            Assert.Equal(secondaryKeyVaultSecret.Name, secret.Name);
            Assert.Equal(secondaryKeyVaultSecret.Value, secret.Value);
        }

        [Fact]
        public async Task GetSecret_ReturnsPrimarySecret_AfterReload()
        {
            var mockSecretClient = new Mock<SecretClient>();
            var primaryKeyVaultSecret = new KeyVaultSecret("Test-Primary", "PrimarySecret");
            var secondaryKeyVaultSecret = new KeyVaultSecret("Test-Secondary", "SecondarySecret");

            var primaryResponse = Response.FromValue(primaryKeyVaultSecret, Mock.Of<Response>());
            var secondaryResponse = Response.FromValue(secondaryKeyVaultSecret, Mock.Of<Response>());

            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Primary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(primaryResponse);
            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Secondary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(secondaryResponse);

            var semaphore = new Semaphore(1, 1);
            var secretManager = new SecretManager(mockSecretClient.Object, semaphore);

            // Load the original secrets
            _ = await secretManager.GetSecretAsync("Test");

            // Fail the primary
            await secretManager.IncrementFailureAsync("Test");

            // Fail the backup. Should force reload
            await secretManager.IncrementFailureAsync("Test");

            var secret = await secretManager.GetSecretAsync("Test");

            Assert.NotNull(secret);
            Assert.Equal(primaryKeyVaultSecret.Name, secret.Name);
            Assert.Equal(primaryKeyVaultSecret.Value, secret.Value);
        }

        [Fact]
        public async Task GetSecret_ThrowsException_After3FailedReloads()
        {
            var mockSecretClient = new Mock<SecretClient>();
            var primaryKeyVaultSecret = new KeyVaultSecret("Test-Primary", "PrimarySecret");
            var secondaryKeyVaultSecret = new KeyVaultSecret("Test-Secondary", "SecondarySecret");

            var primaryResponse = Response.FromValue(primaryKeyVaultSecret, Mock.Of<Response>());
            var secondaryResponse = Response.FromValue(secondaryKeyVaultSecret, Mock.Of<Response>());

            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Primary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(primaryResponse);
            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Secondary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(secondaryResponse);

            var semaphore = new Semaphore(1, 1);
            var secretManager = new SecretManager(mockSecretClient.Object, semaphore);

            // Load the original secrets
            _ = await secretManager.GetSecretAsync("Test");

            // Fail the primary (1st fail)
            await secretManager.IncrementFailureAsync("Test");

            // Fail the backup. (1st fail) Should force reload
            await secretManager.IncrementFailureAsync("Test");

            // Fail the primary (2nd fail)
            await secretManager.IncrementFailureAsync("Test");

            // Fail the backup. (2nd fail) Should force reload
            await secretManager.IncrementFailureAsync("Test");

            // Fail the primary (3rd fail)
            await secretManager.IncrementFailureAsync("Test");

            // Fail the backup. (3rd fail) Should throw an exception
            await secretManager.IncrementFailureAsync("Test");

            // Fail the primary (4th fail)
            await secretManager.IncrementFailureAsync("Test");

            // Fail the backup. (4th fail) Should throw an exception
            await Assert.ThrowsAsync<SecretReloadException>(async () => await secretManager.IncrementFailureAsync("Test"));
        }

        [Fact]
        public async Task GetSecret_ResetAfter3FailedReloads_Success()
        {
            var mockSecretClient = new Mock<SecretClient>();
            var primaryKeyVaultSecret = new KeyVaultSecret("Test-Primary", "PrimarySecret");
            var secondaryKeyVaultSecret = new KeyVaultSecret("Test-Secondary", "SecondarySecret");

            var primaryResponse = Response.FromValue(primaryKeyVaultSecret, Mock.Of<Response>());
            var secondaryResponse = Response.FromValue(secondaryKeyVaultSecret, Mock.Of<Response>());

            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Primary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(primaryResponse);
            mockSecretClient.Setup(x => x.GetSecretAsync("Test-Secondary", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(secondaryResponse);

            var semaphore = new Semaphore(1, 1);
            var secretManager = new SecretManager(mockSecretClient.Object, semaphore);

            // Load the original secrets
            _ = await secretManager.GetSecretAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));

            // Fail the primary (1st fail)
            await secretManager.IncrementFailureAsync("Test");
            Assert.False(secretManager.IsPrimaryActive("Test"));

            // Fail the backup. (1st fail) Should force reload
            await secretManager.IncrementFailureAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));

            // Fail the primary (2nd fail)
            await secretManager.IncrementFailureAsync("Test");
            Assert.False(secretManager.IsPrimaryActive("Test"));

            // Fail the backup. (2nd fail) Should force reload
            await secretManager.IncrementFailureAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));

            // Fail the primary (3rd fail)
            await secretManager.IncrementFailureAsync("Test");
            Assert.False(secretManager.IsPrimaryActive("Test"));

            // Fail the backup. (3rd fail) Should force reload
            await secretManager.IncrementFailureAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));

            // Fail the primary (4th fail)
            await secretManager.IncrementFailureAsync("Test");
            Assert.False(secretManager.IsPrimaryActive("Test"));

            // Fail the backup. (4th fail) Should throw an exception
            await Assert.ThrowsAsync<SecretReloadException>(async () => await secretManager.IncrementFailureAsync("Test"));

            await secretManager.ResetFailureAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));
            await secretManager.GetSecretAsync("Test");
            Assert.True(secretManager.IsPrimaryActive("Test"));

            // Fail the primary (Again. Should not throw exception)
            await secretManager.IncrementFailureAsync("Test");
            Assert.False(secretManager.IsPrimaryActive("Test"));
        }
    }
}
