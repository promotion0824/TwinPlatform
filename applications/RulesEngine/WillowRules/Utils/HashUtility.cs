using System;
using System.Security.Cryptography;
using System.Text;

namespace WillowRules.Utils
{
	public static class HashUtility
	{
		/// <summary>
		/// Encode input into a textual format using a set of 64 characters
		/// </summary>
		public static string CalculateBase64Hash(string input)
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
			byte[] hashBytes = SHA256.HashData(inputBytes);

			// Encode the hash bytes in Base64
			string base64Hash = Convert.ToBase64String(hashBytes);

			return base64Hash;
		}

		/// <summary>
		/// Produces a fixed-size output (256 bits or 32 bytes) regardless of the input size
		/// </summary>
		private static readonly Lazy<SHA256> shaHash = new Lazy<SHA256>(() => SHA256.Create());
		public static string GetSha256Hash(string input)
		{
			// Convert the input string to a byte array and compute the hash.
			byte[] data = shaHash.Value.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new();

			// TODO: Base64 would be better
			// Loop through each byte of the hashed data
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}
	}
}
