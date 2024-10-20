using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Willow.Common
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts stream to byte array
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>The resulting array</returns>
        public static byte[] ToArray(this Stream stream)
        {
            if(stream is MemoryStream memStream)
                return memStream.ToArray();

            using(var mem = new MemoryStream())
            { 
                stream.CopyTo(mem);

                return mem.ToArray();
            }
        }    

        /// <summary>
        /// Converts stream to an object
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>The resulting object</returns>
        public static async Task<T> ReadObject<T>(this Stream stream)
        {
            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using(var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                
                return JsonConvert.DeserializeObject<T>(json);
            }
        }   
        
        /// <summary>
        /// Read the contents of a stream and convert to string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Defaults to UTF8Encoding</param>
        /// <returns>String result</returns>
        public static async Task<string> ReadStringAsync(Stream stream, Encoding encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream is MemoryStream memStream)
            { 
                var array  = memStream.ToArray();
                var arrLen = array.Length;
                var str    = encoder.GetString(memStream.ToArray());
                var atrLen = str.Length;

                return str;
            }

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    await stream.CopyToAsync(mem).ConfigureAwait(false);

                    return encoder.GetString(mem.ToArray());
                }
            }
            finally
            { 
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
            }
        }    

        /// <summary>
        /// Writes a string to the stream
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="data">The string to write</param>
        /// <param name="encoder">Text encoder. Will default to UTF8Encoding</param>
        public static Task WriteStringAsync(this Stream stream, string data, Encoding encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            byte[] bytes = encoder.GetBytes(data);
       
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
