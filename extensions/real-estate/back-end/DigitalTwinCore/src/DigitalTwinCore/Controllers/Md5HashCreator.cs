using System;
using System.IO;
using System.Security.Cryptography;

namespace DigitalTwinCore.Controllers
{
    public interface IHashCreator
    {
        byte[] Create(Stream stream);
    }

    public class Md5HashCreator : IHashCreator
    {
        public byte[] Create(Stream stream)
        {
            return MD5.Create().ComputeHash(stream);
        }
    }
}