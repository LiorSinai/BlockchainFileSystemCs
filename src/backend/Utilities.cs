/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: 
 */

namespace BlockchainFileSystem
{  
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
class Utilities
{
    public static int ConvertToUnixTimestamp(DateTime date)
    {
        return Convert.ToInt32((date - DateTime.UnixEpoch).TotalSeconds);
    }
}

public class UnixDateTimeConverter : JsonConverter<DateTime>
    {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.UnixEpoch.AddSeconds(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Utilities.ConvertToUnixTimestamp(value));
    }
    }

} // namespace BlockchainFileSystem