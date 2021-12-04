using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptotrader
{
    public class CoinbaseDataProvider
    {
        public CryptoProduct Product { get; private set; }

        public CoinbaseDataProvider(CryptoProduct product)
        {
            Product = product;
        }
        public CoinbaseDataProvider(string product)
        {
            Product = new CryptoProduct(product);
        }

        public decimal GetCryptoSellPrice()
        {
            throw new NotImplementedException();
        }
        public decimal GetCryptoBuyPrice()
        {
            throw new NotImplementedException();
        }

        public void PlaceSellOrder()
        {
            throw new NotImplementedException();
        }
        public void PlaceBuyOrder()
        {
            throw new NotImplementedException();
        }
    }
}
