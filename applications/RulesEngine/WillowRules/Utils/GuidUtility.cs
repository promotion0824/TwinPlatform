using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WillowRules.Utils
{
	public static class GuidUtility
	{
		/// <summary>
		/// Creates a name-based UUID using the algorithm from RFC 4122 �4.3.
		/// </summary>
		/// <returns>A UUID derived from the namespace and name.</returns>
		/// <remarks>See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>.</remarks>
		public static Guid Create(string input)
		{
			// convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
			// ASSUME: UTF-8 encoding is always appropriate
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);

			// convert the namespace UUID to network order (step 3)
			byte[] baseIDBytes = BaseID.ToByteArray();
			SwapByteOrder(baseIDBytes);

			// comput the hash of the name space ID concatenated with the name (step 4)
			byte[] hash;
			using (HashAlgorithm algorithm = SHA256.Create())
			{
				algorithm.TransformBlock(baseIDBytes, 0, baseIDBytes.Length, null, 0);
				algorithm.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
				hash = algorithm.Hash!;
			}

			// most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
			byte[] newGuid = new byte[16];
			Array.Copy(hash, 0, newGuid, 0, 16);

			// set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
			newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

			// set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
			newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

			// convert the resulting UUID to local byte order (step 13)
			SwapByteOrder(newGuid);
			return new Guid(newGuid);
		}

		private static readonly int version = 5;

		/// <summary>
		/// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
		/// </summary>
		public static readonly Guid BaseID = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

		// Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
		internal static void SwapByteOrder(byte[] guid)
		{
			SwapBytes(guid, 0, 3);
			SwapBytes(guid, 1, 2);
			SwapBytes(guid, 4, 5);
			SwapBytes(guid, 6, 7);
		}

		private static void SwapBytes(byte[] guid, int left, int right)
		{
			byte temp = guid[left];
			guid[left] = guid[right];
			guid[right] = temp;
		}
	}
}
