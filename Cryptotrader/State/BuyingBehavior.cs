using System.Text.Json;
using static Cryptotrader.State.BotState;

namespace Cryptotrader.State
{
    public class BuyingBehavior : BotStateBehavior
    {
        public override BotState State => Buying;

        private decimal lowestPrice;

        public BuyingBehavior()
        {
            lowestPrice = decimal.MaxValue;
        }
        public BuyingBehavior(decimal low)
        {
            lowestPrice = low;
        }

        public override void Update()
        {
            if (Bot.CurrentBuyPrice < lowestPrice)
            {
                log.Debug($"New lowest price of {Bot.CurrentBuyPrice}, previous lowest was {lowestPrice}");
                lowestPrice = Bot.CurrentBuyPrice;
            }
            else if (Bot.CurrentBuyPrice > lowestPrice)
            {
                log.Debug($"Price of {Bot.CurrentBuyPrice} is higher than low of {lowestPrice}, checking rebound threshold");
                decimal reboundAmount = GetRebound();
                log.Debug($"Price has rebounded by {reboundAmount * 100}% Threshold is {Bot.ReboundBeforeBuy * 100}%");
                if (reboundAmount >= Bot.ReboundBeforeBuy)
                {
                    log.Info($"Rebound amount of {reboundAmount * 100}% is greater than threshold of {Bot.ReboundBeforeBuy * 100}%");
                    log.Info("Price has stopped falling, buying crypto");
                    Bot.BuyCrypto();
                }
            }
        }

        public override void ReadFromJson(ref Utf8JsonReader reader)
        {
            base.ReadFromJson(ref reader);
            reader.GetDecimal()
        }

        private decimal GetRebound()
        {
            decimal rebound = Bot.CurrentBuyPrice - lowestPrice;
            decimal reboundPercent = rebound / lowestPrice;
            return reboundPercent;
        }
    }
}
