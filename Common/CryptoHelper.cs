﻿using NPOI.Util;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Common
{
    public class CryptoHelper
    {


        /// <summary>
        /// 使用 Aes 加密
        /// </summary>
        /// <param name="text">待加密文本</param>
        /// <param name="privateKey">密钥 16位采用Aes128、24位采用Aes192、32位采用Aes256</param>
        /// <param name="stringType">返回的字符串编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static string AesEncrypt(string text, string privateKey, string stringType)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(privateKey);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            byte[] inputBuffers = Encoding.UTF8.GetBytes(text);
            ICryptoTransform cryptoTransform = aes.CreateEncryptor();
            byte[] results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);

            switch (stringType)
            {
                case "base64":
                    {
                        return Convert.ToBase64String(results);
                    }
                case "hex":
                    {
                        return Convert.ToHexString(results);
                    }
                default:
                    {
                        throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                    }
            }
        }



        /// <summary>
        /// 使用 Aes 解密
        /// </summary>
        /// <param name="cipherText">加密文本</param>
        /// <param name="privateKey">密钥 16位采用Aes128、24位采用Aes192、32位采用Aes256</param>
        /// <param name="stringType">加密文本的字符串编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static string AesDecrypt(string cipherText, string privateKey, string stringType)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(privateKey);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            byte[] inputBuffers;

            switch (stringType)
            {
                case "base64":
                    {
                        inputBuffers = Convert.FromBase64String(cipherText);
                        break;
                    }
                case "hex":
                    {
                        inputBuffers = Convert.FromHexString(cipherText);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                    }
            }

            ICryptoTransform cryptoTransform = aes.CreateDecryptor();
            byte[] results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
            return Encoding.UTF8.GetString(results);
        }



        /// <summary>
        /// 使用 AesGcm 解密
        /// </summary>
        /// <param name="cipherText">加密文本</param>
        /// <param name="privateKey">密钥 16位采用Aes128、24位采用Aes192、32位采用Aes256</param>
        /// <param name="nonce">随机值 长度必须是12</param>
        /// <param name="associatedText">相关文本</param>
        /// <param name="stringType">加密文本的字符串编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static string AesGcmDecrypt(string cipherText, string privateKey, string nonce, string? associatedText, string stringType)
        {
            var keyBytes = Encoding.UTF8.GetBytes(privateKey);
            var nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedBytes = associatedText == null ? null : Encoding.UTF8.GetBytes(associatedText);

            byte[] encryptedBytes;

            switch (stringType)
            {
                case "base64":
                    {
                        encryptedBytes = Convert.FromBase64String(cipherText);
                        break;
                    }
                case "hex":
                    {
                        encryptedBytes = Convert.FromHexString(cipherText);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                    }
            }

            var cipherBytes = encryptedBytes[..^16];
            var tag = encryptedBytes[^16..];
            var decryptedData = new byte[cipherBytes.Length];
            using AesGcm cipher = new(keyBytes);
            cipher.Decrypt(nonceBytes, cipherBytes, tag, decryptedData, associatedBytes);
            return Encoding.UTF8.GetString(decryptedData);
        }



        /// <summary>
        /// 使用 AesGcm 加密
        /// </summary>
        /// <param name="text">待加密文本</param>
        /// <param name="privateKey">密钥 16位采用Aes128、24位采用Aes192、32位采用Aes256</param>
        /// <param name="nonce">随机值 长度必须是12</param>
        /// <param name="associatedText">附加数据</param>
        /// <param name="stringType">返回字符串编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static string AesGcmEncrypt(string text, string privateKey, string nonce, string? associatedText, string stringType)
        {
            var keyBytes = Encoding.UTF8.GetBytes(privateKey);
            var nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedBytes = associatedText == null ? null : Encoding.UTF8.GetBytes(associatedText);

            var plainBytes = Encoding.UTF8.GetBytes(text);
            var cipherBytes = new byte[plainBytes.Length];

            var tag = new byte[16];
            using AesGcm cipher = new(keyBytes);
            cipher.Encrypt(nonceBytes, plainBytes, cipherBytes, tag, associatedBytes);

            var cipherWithTag = new byte[cipherBytes.Length + tag.Length];
            Buffer.BlockCopy(cipherBytes, 0, cipherWithTag, 0, cipherBytes.Length);
            Buffer.BlockCopy(tag, 0, cipherWithTag, cipherBytes.Length, tag.Length);

            switch (stringType)
            {
                case "base64":
                    {
                        return Convert.ToBase64String(cipherWithTag);
                    }
                case "hex":
                    {
                        return Convert.ToHexString(cipherWithTag);
                    }
                default:
                    {
                        throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                    }
            }
        }



        /// <summary>
        /// 获取字符串的 Base64 编码
        /// </summary>
        /// <param name="text">待编码的字符串</param>
        /// <returns></returns>
        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }



        /// <summary>
        /// 解码 Base64 字符串
        /// </summary>
        /// <param name="text">待解码的字符串</param>
        /// <returns></returns>
        public static string Base64Decode(string text)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }



        /// <summary>
        /// MD5 签名计算
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetMD5(string text)
        {
            return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));
        }



        /// <summary>
        /// SHA256 签名计算
        /// </summary>
        /// <param name="srcString">The string to be encrypted</param>
        /// <returns></returns>
        public static string GetSHA256(string text)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }



        /// <summary>
        /// SHA512 签名计算
        /// </summary>
        /// <param name="srcString">The string to be encrypted</param>
        /// <returns></returns>
        public static string GetSHA512(string text)
        {
            return Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(text)));
        }



        /// <summary>
        /// SHA256withRSA 签名计算
        /// </summary>
        /// <param name="content"></param>
        /// <param name="privateKey"></param>      
        /// <param name="stringType">返回字符串编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static string GetSHA256withRSASignData(string content, string privateKey, string stringType)
        {
            privateKey = privateKey.Replace("\n", "").Replace("\r", "");

            using var rsa = RSA.Create();

        ImportInfo:
            if (privateKey.StartsWith("-") && privateKey.EndsWith("-"))
            {
                try
                {
                    rsa.ImportFromPem(privateKey);
                }
                catch
                {
                    privateKey = privateKey.Split("-").ToList().OrderByDescending(t => t.Length).First();

                    goto ImportInfo;
                }
            }
            else
            {
                byte[] keyData = Convert.FromBase64String(privateKey);

                try
                {
                    rsa.ImportRSAPrivateKey(keyData, out _);
                }
                catch
                {
                    try
                    {
                        rsa.ImportPkcs8PrivateKey(keyData, out _);
                    }
                    catch
                    {
                        throw new Exception("私钥导入初始化RSA失败");
                    }
                }
            }

            byte[] data = Encoding.UTF8.GetBytes(content);

            var signData = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            switch (stringType)
            {
                case "base64":
                    {
                        return Convert.ToBase64String(signData);
                    }
                case "hex":
                    {
                        return Convert.ToHexString(signData);
                    }
                default:
                    {
                        throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                    }
            }
        }



        /// <summary>
        /// SHA256withRSA 签名验证
        /// </summary>
        /// <param name="certText">证书文本内容</param>
        /// <param name="content">内容</param>
        /// <param name="signature">签名</param>
        /// <param name="stringType">签名编码类型 base64 或 hex</param>
        /// <returns></returns>
        public static bool GetSHA256withRSAVerifyData(string certText, string content, string signature, string stringType)
        {

            using X509Certificate2 x509Certificate2 = new(Encoding.UTF8.GetBytes(certText));
            using var rsa = x509Certificate2.GetRSAPublicKey();

            var data = Encoding.UTF8.GetBytes(content);

            if (rsa != null)
            {

                byte[] signData;

                switch (stringType)
                {
                    case "base64":
                        {
                            signData = Convert.FromBase64String(signature);
                            break;
                        }
                    case "hex":
                        {
                            signData = Convert.FromHexString(signature);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("stringType 无效，只能是 base64 或 hex");
                        }
                }

                var isOk = rsa.VerifyData(data, signData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return isOk;
            }
            else
            {
                throw new Exception("RSA 初始化失败");
            }


        }


    }
}
