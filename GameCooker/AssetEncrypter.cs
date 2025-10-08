using System;
using System.IO;
using System.Security.Cryptography;

namespace GameCooker
{
    public static class AssetEncrypter
    {
        private const int KeySize = 32;       // AES-256
        private const int IvSize = 16;        // AES block size
        private const int SaltSize = 16;      // PBKDF2 salt
        private const int HmacSize = 32;      // SHA256 HMAC
        private const int Iterations = 100_000;
        private const int BufferSize = 81920; // 80 KB

        public static byte[] EncryptBytes(byte[] data, string password)
        {
            using var memStream = EncryptToStream(new MemoryStream(data), password);
            return memStream.ToArray();
        }

        public static byte[] DecryptBytes(byte[] encryptedData, string password)
        {
            using var input = new MemoryStream(encryptedData);
            using var output = DecryptFromStream(input, password);
            return output.ToArray();
        }

        public static MemoryStream EncryptToStream(Stream assetStream, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

            DeriveKeys(password, salt, out byte[] aesKey, out byte[] hmacKey);

            var output = new MemoryStream();
            output.Write(salt, 0, salt.Length);
            output.Write(iv, 0, iv.Length);

            using var aes = CreateAes(aesKey, iv);
            using var encryptor = aes.CreateEncryptor();
            using var hmac = new HMACSHA256(hmacKey);

            // Stream wrapper for HMAC computation
            using (var cryptoStream = new CryptoStream(new HmacStream(output, hmac), encryptor, CryptoStreamMode.Write))
            {
                CopyStream(assetStream, cryptoStream);
                cryptoStream.FlushFinalBlock();
            }

            // Finalize HMAC after all encryption
            hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            output.Write(hmac.Hash, 0, HmacSize);
            output.Position = 0;
            return output;
        }

        public static MemoryStream DecryptFromStream(Stream encryptedStream, string password)
        {
            byte[] salt = ReadBytes(encryptedStream, SaltSize);
            byte[] iv = ReadBytes(encryptedStream, IvSize);

            DeriveKeys(password, salt, out byte[] aesKey, out byte[] hmacKey);

            long encryptedContentLength = encryptedStream.Length - SaltSize - IvSize - HmacSize;
            byte[] storedHmac = ReadBytesAt(encryptedStream, encryptedStream.Length - HmacSize, HmacSize);

            encryptedStream.Position = SaltSize + IvSize;

            using var hmac = new HMACSHA256(hmacKey);
            using var hmacVerifyingStream = new HmacVerifyingStream(new SubStream(encryptedStream, encryptedContentLength), hmac);

            using var aes = CreateAes(aesKey, iv);
            using var cryptoStream = new CryptoStream(hmacVerifyingStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

            var output = new MemoryStream();
            CopyStream(cryptoStream, output);

            // Finalize HMAC after all ciphertext has been read
            hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            if (!CryptographicOperations.FixedTimeEquals(hmac.Hash, storedHmac))
                throw new CryptographicException("Asset authentication failed: HMAC mismatch.");

            output.Position = 0;
            return output;
        }

        private static void DeriveKeys(string password, byte[] salt, out byte[] aesKey, out byte[] hmacKey)
        {
            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            aesKey = derive.GetBytes(KeySize);
            hmacKey = derive.GetBytes(KeySize);
        }

        private static Aes CreateAes(byte[] key, byte[] iv)
        {
            return new AesCryptoServiceProvider
            {
                KeySize = KeySize * 8,
                BlockSize = IvSize * 8,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = key,
                IV = iv
            };
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[BufferSize];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, read);
        }

        private static byte[] ReadBytes(Stream stream, long count)
        {
            byte[] buffer = new byte[count];
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, totalRead, (int)(count - totalRead));
                if (read == 0) throw new EndOfStreamException();
                totalRead += read;
            }
            return buffer;
        }

        private static byte[] ReadBytesAt(Stream stream, long position, int count)
        {
            stream.Position = position;
            return ReadBytes(stream, count);
        }

        private class SubStream : Stream
        {
            private readonly Stream _base;
            private readonly long _length;
            private long _position;

            public SubStream(Stream baseStream, long length)
            {
                _base = baseStream;
                _length = length;
                _position = 0;
            }

            public override bool CanRead => _base.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _length;
            public override long Position { get => _position; set => throw new NotSupportedException(); }
            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                long remaining = _length - _position;
                if (remaining <= 0) return 0;
                if (count > remaining) count = (int)remaining;

                int read = _base.Read(buffer, offset, count);
                _position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        private class HmacStream : Stream
        {
            private readonly Stream _base;
            private readonly HMAC _hmac;

            public HmacStream(Stream baseStream, HMAC hmac)
            {
                _base = baseStream;
                _hmac = hmac;
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _base.Length;
            public override long Position { get => _base.Position; set => throw new NotSupportedException(); }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _base.Write(buffer, offset, count);
                _hmac.TransformBlock(buffer, offset, count, null, 0);
            }

            public override void Flush() => _base.Flush();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        private class HmacVerifyingStream : Stream
        {
            private readonly Stream _base;
            private readonly HMAC _hmac;

            public HmacVerifyingStream(Stream baseStream, HMAC hmac)
            {
                _base = baseStream;
                _hmac = hmac;
            }

            public override bool CanRead => _base.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _base.Length;
            public override long Position { get => _base.Position; set => throw new NotSupportedException(); }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = _base.Read(buffer, offset, count);
                if (read > 0)
                    _hmac.TransformBlock(buffer, offset, read, null, 0);
                return read;
            }

            public override void Flush() => _base.Flush();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
