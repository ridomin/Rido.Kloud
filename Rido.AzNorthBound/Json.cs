﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rido.AzNorthBound;

internal class Json
{
    public static string Stringify(object o) => JsonSerializer.Serialize(o,
            new JsonSerializerOptions()
            {
                Converters =
               {
                    new JsonStringEnumConverter()
               }
            });

    public static T FromString<T>(string s) => JsonSerializer.Deserialize<T>(s,
          new JsonSerializerOptions()
          {
              Converters =
                  {
                            new JsonStringEnumConverter()
                  }
          })!;
}
