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

    public static string ExceptionWithoutStackTrace(Exception e){
        string s = e.GetType() + " : " + e.Message;
        #nullable enable
        Exception? inner = e.InnerException;
        #nullable disable
        while (inner != null){
            s += "\n" + inner.GetType() + " : " + inner.Message;
            inner = inner.InnerException;
        }

        return s;
    }

    public static void Bytes_add_1(byte[] bytes, int first = 0)
    {
        int carry  = 1; 
        int idx = bytes.Length - 1;
        while (carry > 0 && idx >= first){
             carry += bytes[idx];
             bytes[idx] = (byte) carry;
             carry >>= 8;
             idx -= 1;
        } // will overflow if idx < first
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