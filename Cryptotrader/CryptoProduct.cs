using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptotrader
{
    public struct CryptoProduct
    {
        public static CryptoProduct ETH_USDC = new CryptoProduct("ETH-USDC");

        public string Crypto { get; set; }
        public string Currency { get; set; }

        public CryptoProduct(string crypto, string currency)
        {
            Crypto = crypto;
            Currency = currency;
        }

        public CryptoProduct(string product)
        {
            string[] parts = product.Split('-');
            if (parts.Length != 2) throw new ArgumentException($"Invalid product string: '{product}'");
            Crypto = parts[0];
            Currency = parts[1];
        }
    }
}
