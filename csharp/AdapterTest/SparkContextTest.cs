﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Collections.Generic;
using AdapterTest.Mocks;
using Microsoft.Spark.CSharp.Core;
using Microsoft.Spark.CSharp.Interop.Ipc;
using Microsoft.Spark.CSharp.Proxy;
using Microsoft.Spark.CSharp.Proxy.Ipc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdapterTest
{
    /// <summary>
    /// Validates interaction between SparkContext and its proxy
    /// </summary>
    [TestClass]
    public class SparkContextTest
    {
        //TODO - complete impl

        [TestMethod]
        public void TestSparkContextProxy()
        {
            var sparkContext = new SparkContext(Env.SPARK_MASTER_URL, "appName");
            sparkContext.AddFile(null);
            sparkContext.BinaryFiles(null, null);
            sparkContext.CancelAllJobs();
            sparkContext.CancelJobGroup(null);
            sparkContext.EmptyRDD<string>();
            sparkContext.GetLocalProperty(null);
            sparkContext.HadoopFile(null, null, null, null);
            sparkContext.HadoopRDD(null, null, null);
            sparkContext.NewAPIHadoopFile(null, null, null, null);
            sparkContext.NewAPIHadoopRDD(null, null, null);
            sparkContext.Parallelize<int>(new int[] { 1, 2, 3, 4, 5 });
            sparkContext.SequenceFile(null, null, null, null, null, null);
            sparkContext.SetCheckpointDir(null);
            sparkContext.SetJobGroup(null, null);
            sparkContext.SetLocalProperty(null, null);
            sparkContext.SetLogLevel(null);
            sparkContext.TextFile(null);
            sparkContext.WholeTextFiles(null);
            sparkContext.Stop();
            sparkContext.Union<string>(null);
        }

        [TestMethod]
        public void TestSparkContextConstructor()
        {
            var sparkContext = new SparkContext("masterUrl", "appName");
            Assert.IsNotNull((sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference);
            var paramValuesToConstructor = (sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference as object[];
            Assert.AreEqual("masterUrl", (paramValuesToConstructor[0] as MockSparkConfProxy).stringConfDictionary["mockmaster"]);
            Assert.AreEqual("appName", (paramValuesToConstructor[0] as MockSparkConfProxy).stringConfDictionary["mockappName"]);

            sparkContext = new SparkContext("masterUrl", "appName", "sparkhome");
            Assert.IsNotNull((sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference);
            paramValuesToConstructor = (sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference as object[];
            Assert.AreEqual("masterUrl", (paramValuesToConstructor[0] as MockSparkConfProxy).stringConfDictionary["mockmaster"]);
            Assert.AreEqual("appName", (paramValuesToConstructor[0] as MockSparkConfProxy).stringConfDictionary["mockappName"]);
            Assert.AreEqual("sparkhome", (paramValuesToConstructor[0] as MockSparkConfProxy).stringConfDictionary["mockhome"]);

            sparkContext = new SparkContext(null);
            Assert.IsNotNull((sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference);
            paramValuesToConstructor = (sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference as object[];
            Assert.IsNotNull(paramValuesToConstructor[0]); //because SparkContext constructor create default sparkConf
        }

        [TestMethod]
        public void TestSparkContextStop()
        {
            var sparkContext = new SparkContext(null);
            Assert.IsNotNull((sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference);
            sparkContext.Stop();
            Assert.IsNull((sparkContext.SparkContextProxy as MockSparkContextProxy).mockSparkContextReference);
        }

        [TestMethod]
        public void TestSparkContextTextFile()
        {
            var sparkContext = new SparkContext(null);
            var rdd = sparkContext.TextFile(@"c:\path\to\rddinput.txt", 8);
            var paramValuesToTextFileMethod = (rdd.RddProxy as MockRddProxy).mockRddReference as object[];
            Assert.AreEqual(@"c:\path\to\rddinput.txt", paramValuesToTextFileMethod[0]);
            Assert.AreEqual(8, paramValuesToTextFileMethod[1]);
        }
    }
}
