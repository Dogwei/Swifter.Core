using Swifter.Json;
using Swifter.Tools;
using System;
using System.Diagnostics;
using System.Threading;

namespace Swifter.Test
{
    class JsonTester
    {

        public static void Serialize<T>(string testName, int testTimes, T obj)
        {
            var begin = DefineVar();





            try
            {
                begin = Begin();

                Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Newtonsoft",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Newtonsoft",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Newtonsoft", "Serialize");
            }

            Console.WriteLine();




            try
            {
                begin = Begin();

                LitJson.JsonMapper.ToJson(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "LitJson",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    LitJson.JsonMapper.ToJson(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "LitJson",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "LitJson", "Serialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                ServiceStack.JSON.stringify(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "ServiceStack",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    ServiceStack.JSON.stringify(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "ServiceStack",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "ServiceStack", "Serialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                fastJSON.JSON.ToJSON(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "fastJSON",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    fastJSON.JSON.ToJSON(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "fastJSON",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "fastJSON", "Serialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                Utf8Json.JsonSerializer.Serialize(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Utf8Json",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Utf8Json.JsonSerializer.Serialize(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Utf8Json",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Utf8Json", "Serialize");
            }

            Console.WriteLine();




            //try
            //{
            //    begin = Begin();

            //    Jil.JSON.Serialize(obj);

            //    Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
            //        "Jil",
            //        "First Serialize",
            //        testName,
            //        End(begin)));

            //    begin = Begin();

            //    for (int i = testTimes; i >= 0; --i)
            //    {
            //        Jil.JSON.Serialize(obj);
            //    }

            //    Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
            //        "Jil",
            //        "Serialize",
            //        testName,
            //        End(begin)));
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("Error {0} {1}.", "Jil", "Serialize");
            //}

            //Console.WriteLine();





            try
            {
                begin = Begin();

                Jil.JSON.SerializeDynamic(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Jil.Dynamic",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Jil.JSON.SerializeDynamic(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Jil.Dynamic",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Jil.Dynamic", "Serialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                NetJSON.NetJSON.Serialize(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "NetJSON",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    NetJSON.NetJSON.Serialize(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "NetJSON",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "NetJSON", "Serialize");
            }

            Console.WriteLine();




            try
            {
                begin = Begin();

                JsonFormatter.SerializeObject(obj);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Swifter",
                    "First Serialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    JsonFormatter.SerializeObject(obj);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Swifter",
                    "Serialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Swifter", "Serialize");
            }

            Console.WriteLine();
        }

        public static void Deserialize<T>(string testName, int testTimes, string json)
        {
            GC.Collect();

            var begin = DefineVar();





            try
            {
                begin = Begin();

                Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Newtonsoft",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Newtonsoft",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Newtonsoft", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                LitJson.JsonMapper.ToObject<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "LitJson",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    LitJson.JsonMapper.ToObject<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "LitJson",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "LitJson", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "ServiceStack",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "ServiceStack",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "ServiceStack", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                fastJSON.JSON.ToObject<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "fastJSON",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    fastJSON.JSON.ToObject<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "fastJSON",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "fastJSON", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                Utf8Json.JsonSerializer.Deserialize<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Utf8Json",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Utf8Json.JsonSerializer.Deserialize<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Utf8Json",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "Utf8Json", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                Jil.JSON.Deserialize<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Jil",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    Jil.JSON.Deserialize<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Jil",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception)
            {
                Console.WriteLine("Error {0} {1}.", "Jil", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                NetJSON.NetJSON.Deserialize<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "NetJSON",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    NetJSON.NetJSON.Deserialize<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "NetJSON",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "NetJSON", "Deserialize");
            }

            Console.WriteLine();





            try
            {
                begin = Begin();

                JsonFormatter.DeserializeObject<T>(json);

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Swifter",
                    "First Deserialize",
                    testName,
                    End(begin)));

                begin = Begin();

                for (int i = testTimes; i >= 0; --i)
                {
                    JsonFormatter.DeserializeObject<T>(json);
                }

                Console.WriteLine(StringHelper.Format("{0} {1} {2} used time: {3}",
                    "Swifter",
                    "Deserialize",
                    testName,
                    End(begin)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error {0} {1}.", "Swifter", "Deserialize");
            }

            Console.WriteLine();
        }





        public static DateTime DefineVar()
        {
            return default(DateTime);
        }

        public static DateTime Begin()
        {
            return DateTime.Now;
        }

        public static string End(DateTime begin)
        {
            return (DateTime.Now - begin).TotalMilliseconds.ToString();
        }

    }
}
