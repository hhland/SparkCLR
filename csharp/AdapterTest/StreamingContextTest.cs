﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdapterTest.Mocks;
using Microsoft.Spark.CSharp.Core;
using Microsoft.Spark.CSharp.Streaming;

namespace AdapterTest
{
    [TestClass]
    public class StreamingContextTest
    {
        [TestMethod]
        public void TestStreamingContext()
        {
            var ssc = new StreamingContext(new SparkContext(Env.SPARK_MASTER_URL, "xxxx"), 1000);
            Assert.IsNotNull((ssc.streamingContextProxy as MockStreamingContextProxy));

            ssc.Start();
            ssc.Remember(1000);
            ssc.Checkpoint(Path.GetTempPath());

            var textFile = ssc.TextFileStream(Path.GetTempPath());
            Assert.IsNotNull(textFile.DStreamProxy);

            var socketStream = ssc.SocketTextStream("127.0.0.1", 12345);
            Assert.IsNotNull(socketStream.DStreamProxy);

            var kafkaStream = ssc.KafkaStream("127.0.0.1:2181", "testGroupId", new Dictionary<string, int> { { "testTopic1", 1 } }, new Dictionary<string, string>());
            Assert.IsNotNull(kafkaStream.DStreamProxy);

            var directKafkaStream = ssc.DirectKafkaStream(new List<string> { "testTopic2" }, new Dictionary<string, string>(), new Dictionary<string, long>());
            Assert.IsNotNull(directKafkaStream.DStreamProxy);

            var union = ssc.Union(textFile, socketStream);
            Assert.IsNotNull(union.DStreamProxy);

            ssc.AwaitTermination();
            ssc.Stop();
        }
    }
}
