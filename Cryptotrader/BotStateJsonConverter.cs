using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Cryptotrader.State;

namespace Cryptotrader
{
    public class BotStateJsonConverter : JsonConverter<BotStateBehavior>
    {
        public override BotStateBehavior Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string stateString = reader.GetString();
            BotState state = BotStateFromString(stateString);
            return BotStateBehavior.BehaviorFromBotState(state);
        }

        public override void Write(Utf8JsonWriter writer, BotStateBehavior value, JsonSerializerOptions options)
        {
            string stateString = StringFromBotState(value.State);
            writer.WriteStringValue(stateString);
        }

        private BotState BotStateFromString(string stateString)
        {
            string[] parts = stateString.Split(".");
            if (parts.Length != 2) throw new ArgumentException($"Invalid state string: '{stateString}'");
            if (Enum.TryParse(typeof(BotState), parts[1], false, out object state))
            {
                return (BotState)state;
            }
            throw new ArgumentException($"Invalid state: '{parts[1]}'");
        }
        private string StringFromBotState(BotState state)
        {
            string stateName = Enum.GetName(typeof(BotState), state);
            return $"State.{stateName}";
        }
    }
}
