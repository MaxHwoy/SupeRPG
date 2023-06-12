using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace SupeRPG.Input
{
    public class AssetParser
    {
        public static T[] ParseFromCSV<T>(string file, bool useResources) where T : IDisposable
        {
            var type = typeof(T);

            var properties = AssetParser.GetProperties(type);

            TextReader stream;

            if (useResources)
            {
                stream = new StreamReader(new MemoryStream(Resources.Load<TextAsset>(file).bytes));
            }
            else
            {
                stream = new StreamReader(File.OpenRead(file));
            }

            try
            {
                var instances = new List<T>();

                var reader = new CsvReader(stream);

                int index = 1;

                while (reader.Read())
                {
                    if (reader.FieldsCount != properties.Length)
                    {
                        throw new Exception($"Unable to parse CSV file at line {index}, expected {properties.Length} number of arguments, got {reader.FieldsCount}");
                    }

                    var instance = (T)Activator.CreateInstance(type);

                    for (int k = 0; k < reader.FieldsCount; ++k)
                    {
                        AssetParser.PopulateProperty(properties[k], instance, reader[k]);
                    }

                    index++;

                    instances.Add(instance);
                }

                return instances.ToArray();
            }
            finally
            {
                stream.Dispose();
            }
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            var properties = new List<(PropertyInfo info, int order)>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = property.GetCustomAttribute<OrderAttribute>();

                if (attribute is not null && property.GetSetMethod(false) is not null)
                {
                    properties.Add((property, attribute.Order));
                }
            }

            properties.Sort((x, y) => x.order.CompareTo(y.order));

            if (properties.Count == 0)
            {
                return Array.Empty<PropertyInfo>();
            }
            else
            {
                var result = new PropertyInfo[properties.Count];

                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = properties[i].info;
                }

                return result;
            }
        }

        private static void PopulateProperty(PropertyInfo property, object target, string value)
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(target, value);
            }
            else if (property.PropertyType == typeof(bool))
            {
                if (!Boolean.TryParse(value, out bool result))
                {
                    result = Int32.Parse(value) != 0;
                }

                property.SetValue(target, result);
            }
            else if (property.PropertyType == typeof(sbyte))
            {
                property.SetValue(target, SByte.Parse(value));
            }
            else if (property.PropertyType == typeof(byte))
            {
                property.SetValue(target, Byte.Parse(value));
            }
            else if (property.PropertyType == typeof(short))
            {
                property.SetValue(target, Int16.Parse(value));
            }
            else if (property.PropertyType == typeof(ushort))
            {
                property.SetValue(target, UInt16.Parse(value));
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(target, Int32.Parse(value));
            }
            else if (property.PropertyType == typeof(uint))
            {
                property.SetValue(target, UInt32.Parse(value));
            }
            else if (property.PropertyType == typeof(long))
            {
                property.SetValue(target, Int64.Parse(value));
            }
            else if (property.PropertyType == typeof(ulong))
            {
                property.SetValue(target, UInt64.Parse(value));
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(target, Single.Parse(value));
            }
            else if (property.PropertyType == typeof(double))
            {
                property.SetValue(target, Double.Parse(value));
            }
            else if (property.PropertyType.IsEnum)
            {
                property.SetValue(target, Enum.Parse(property.PropertyType, value));
            }
            else if (property.PropertyType == typeof(Sprite))
            {
                property.SetValue(target, ResourceManager.LoadSprite(value));
            }
            else if (property.PropertyType == typeof(Texture2D))
            {
                property.SetValue(target, ResourceManager.LoadTexture2D(value));
            }
            else if (property.PropertyType == typeof(string[]))
            {
                var splits = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < splits.Length; ++i)
                {
                    splits[i] = splits[i].Trim();
                }

                property.SetValue(target, splits);
            }
            else if (property.PropertyType == typeof(int[]))
            {
                var splits = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var result = new int[splits.Length];

                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = Int32.Parse(splits[i].Trim());
                }

                property.SetValue(target, result);
            }
            else if (property.PropertyType == typeof(float[]))
            {
                var splits = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var result = new float[splits.Length];

                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = Single.Parse(splits[i].Trim());
                }

                property.SetValue(target, result);
            }
            else if (property.PropertyType.IsEnum)
            {
                property.SetValue(target, Enum.Parse(property.PropertyType, value));
            }
            else
            {
                throw new Exception($"Unable to parse the property {property.Name} since it has unsupported parsing type");
            }
        }
    }
}
