using Swifter.Formatters;
using Swifter.RW;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 文档格式化器。
    /// </summary>
    public unsafe sealed class JsonFormatter : ITextFormatter
    {
        /// <summary>
        /// 读取或设置默认最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 JsonFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public static int DefaultMaxDepth { get; set; } = 20;

        /// <summary>
        /// 读取或设置默认缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultIndentedChars { get; set; } = "  ";

        /// <summary>
        /// 读取或设置默认换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultLineBreak { get; set; } = "\n";

        /// <summary>
        /// 读取或设置默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultMiddleChars { get; set; } = " ";

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <param name="options">反序列化配置项</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(string text, JsonFormatterOptions options = JsonFormatterOptions.Default)
        {
            fixed (char* chars = text)
            {
                return ValueInterface<T>.Content.ReadValue(new JsonDeserializer(chars, 0, text.Length, options));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="options">反序列化配置项</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(TextReader textReader, JsonFormatterOptions options = JsonFormatterOptions.Default)
        {
            return DeserializeObject<T>(textReader.ReadToEnd(), options);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(string text, Type type, JsonFormatterOptions options = JsonFormatterOptions.Default)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.GetInterface(type).ReadValue(new JsonDeserializer(chars, 0, text.Length, options));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options = JsonFormatterOptions.Default)
        {
            return DeserializeObject(textReader.ReadToEnd(), type, options);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <returns>返回 JSON 字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T value)
        {
            var serializer = new JsonDefaultSerializer(DefaultMaxDepth);
            
            ValueInterface<T>.Content.WriteValue(serializer, value);

            return serializer.ToString();
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter)
        {
            var serializer = new JsonDefaultSerializer(DefaultMaxDepth);
            
            ValueInterface<T>.Content.WriteValue(serializer, value);

            serializer.WriteTo(textWriter);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="options">序列化配置</param>
        /// <returns>返回 JSON 字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T value, JsonFormatterOptions options)
        {
            var serializer = new JsonSerializer(options, DefaultMaxDepth);

            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                serializer.indentedChars = DefaultIndentedChars;
                serializer.lineBreak = DefaultLineBreak;
                serializer.middleChars = DefaultMiddleChars;
            }

            ValueInterface<T>.Content.WriteValue(serializer, value);

            return serializer.ToString();
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
        {
            var serializer = new JsonSerializer(options, DefaultMaxDepth);

            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                serializer.indentedChars = DefaultIndentedChars;
                serializer.lineBreak = DefaultLineBreak;
                serializer.middleChars = DefaultMiddleChars;
            }

            ValueInterface<T>.Content.WriteValue(serializer, value);

            serializer.WriteTo(textWriter);
        }



        /// <summary>
        /// 读取或设置最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 JsonFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public int MaxDepth { get; set; } = DefaultMaxDepth;

        /// <summary>
        /// 读取或设置缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string IndentedChars { get; set; } = DefaultIndentedChars;

        /// <summary>
        /// 读取或设置换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string LineBreak { get; set; } = DefaultLineBreak;

        /// <summary>
        /// 读取或设置默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string MiddleChars { get; set; } = DefaultMiddleChars;

        /// <summary>
        /// JSON 格式化器配置项。
        /// </summary>
        public JsonFormatterOptions Options { get; set; }

        /// <summary>
        /// 初始化具有默认配置的 JSON 格式化器。
        /// </summary>
        public JsonFormatter()
        {
            Options = JsonFormatterOptions.Default;
        }

        /// <summary>
        /// 初始化指定配置的 JSON 格式化器。
        /// </summary>
        /// <param name="options">指定配置</param>
        public JsonFormatter(JsonFormatterOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(string text)
        {
            fixed (char* chars = text)
            {
                return ValueInterface<T>.Content.ReadValue(new JsonDeserializer(chars, 0, text.Length, Options));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(TextReader textReader)
        {
            return Deserialize<T>(textReader.ReadToEnd());
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.GetInterface(type).ReadValue(new JsonDeserializer(chars, 0, text.Length, Options));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(TextReader textReader, Type type)
        {
            return Deserialize(textReader.ReadToEnd(), type);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <returns>返回 JSON 字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string Serialize<T>(T value)
        {
            if (Options == JsonFormatterOptions.Default)
            {
                var serializer = new JsonDefaultSerializer(MaxDepth);

                ValueInterface<T>.Content.WriteValue(serializer, value);

                return serializer.ToString();
            }
            else
            {
                var serializer = new JsonSerializer(Options, MaxDepth);

                if ((Options & JsonFormatterOptions.Indented) != 0)
                {
                    serializer.indentedChars = IndentedChars;
                    serializer.lineBreak = LineBreak;
                    serializer.middleChars = MiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(serializer, value);

                return serializer.ToString();
            }
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, TextWriter textWriter)
        {
            if (Options == JsonFormatterOptions.Default)
            {
                var serializer = new JsonDefaultSerializer(MaxDepth);

                ValueInterface<T>.Content.WriteValue(serializer, value);

                serializer.WriteTo(textWriter);
            }
            else
            {
                var serializer = new JsonSerializer(Options, MaxDepth);

                if ((Options & JsonFormatterOptions.Indented) != 0)
                {
                    serializer.indentedChars = IndentedChars;
                    serializer.lineBreak = LineBreak;
                    serializer.middleChars = MiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(serializer, value);

                serializer.WriteTo(textWriter);
            }
        }
    }
}