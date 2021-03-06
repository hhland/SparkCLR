﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using AdapterTest.Mocks;
using Microsoft.Spark.CSharp.Core;
using Microsoft.Spark.CSharp.Interop.Ipc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdapterTest
{
    /// <summary>
    /// Validates interaction between RDD and its proxy
    /// </summary>
    [TestClass]
    public class RDDTest
    {
        private static RDD<string> words;

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            var sparkContext = new SparkContext(null);
            var lines = sparkContext.TextFile(Path.GetTempFileName());
            words = lines.FlatMap(l => l.Split(' '));
        }

        [TestMethod]
        public void TestRddProxy()
        {
            words.Cache();
            words.Persist(StorageLevelType.MEMORY_AND_DISK_SER_2);
            words.Unpersist();
            words.Checkpoint();
            words.GetNumPartitions();
            words.Sample(false, 0.0, 0L);
            words.RandomSplit(null, 0L);
            words.Union(words);
            words.Cartesian(words);
            words.Pipe(null);
            words.Repartition(1);
            words.Coalesce(1, false);
            words.SetName(null);
            words.RandomSampleWithRange(0.0, 0.0, 0L);
            words.Zip(words);
            words.ToDebugString();
            words.Count();
            words.SaveAsTextFile(null);
        }

        [TestMethod]
        public void TestRddCountByValue()
        {
            foreach (var record in words.CountByValue())
            {
                Assert.AreEqual(record.Key == "The" || record.Key == "dog" || record.Key == "lazy" ? 23 : 22, record.Value);
            }
        }

        [TestMethod]
        public void TestRddDistinct()
        {
            Assert.AreEqual(9, words.Distinct().Collect().Length);
        }

        [TestMethod]
        public void TestRddSubtract()
        {
            Assert.AreEqual(23, words.Subtract(words.Filter(w => w != "The")).Collect().Length);
        }

        [TestMethod]
        public void TestRddIntersection()
        {
            Assert.AreEqual(1, words.Intersection(words.Filter(w => w == "The")).Collect().Length);
        }

        [TestMethod]
        public void TestRddToLocalIterator()
        {
            Assert.AreEqual(201, words.ToLocalIterator().Count());
        }

        [TestMethod]
        public void TestRddTreeReduce()
        {
            Assert.AreEqual(201, words.Map(w => 1).TreeReduce((x, y) => x + y));
        }

        [TestMethod]
        public void TestRddTreeAggregate()
        {
            Assert.AreEqual(201, words.Map(w => 1).TreeAggregate(0, (x, y) => x + y, (x, y) => x + y));
        }

        [TestMethod]
        public void TestRddAggregate()
        {
            Assert.AreEqual(201, words.Map(w => 1).Aggregate(0, (x, y) => x + y, (x, y) => x + y));
        }

        [TestMethod]
        public void TestRddReduce()
        {
            Assert.AreEqual(201, words.Map(w => 1).Reduce((x, y) => x + y));
        }

        [TestMethod]
        public void TestRddFirst()
        {
            Assert.AreEqual("The", words.First());
        }

        [TestMethod]
        public void TestRddFold()
        {
            Assert.AreEqual(201, words.Map(w => 1).Fold(0, (x, y) => x + y));
        }

        [TestMethod]
        public void TestRddGlom()
        {
            Assert.AreEqual(201, words.Map(w => 1).Glom().Collect()[0].Length);
        }

        [TestMethod]
        public void TestRddGroupBy()
        {
            words.GroupBy(w => w).Foreach(record =>
            {
                Assert.AreEqual(record.Key == "The" || record.Key == "dog" || record.Key == "lazy" ? 23 : 22, record.Value.Count);
            });
            
            words.GroupBy(w => w).ForeachPartition(iter =>
            {
                foreach (var record in iter)
                {
                    Assert.AreEqual(record.Key == "The" || record.Key == "dog" || record.Key == "lazy" ? 23 : 22, record.Value.Count);
                }
            });
        }

        [TestMethod]
        public void TestRddIsEmpty()
        {
            Assert.IsFalse(words.IsEmpty());
            Assert.IsTrue(words.Filter(w => w == null).IsEmpty());
        }

        [TestMethod]
        public void TestRddZipWithIndex()
        {
            int index = 0;
            foreach(var record in words.ZipWithIndex().Collect())
            {
                Assert.AreEqual(index++, record.Value);
            }
        }

        [TestMethod]
        public void TestRddZipWithUniqueId()
        {
            int index = 0;
            foreach (var record in words.ZipWithUniqueId().Collect())
            {
                Assert.AreEqual(index++, record.Value);
            }
        }

        [TestMethod]
        public void TestRddTakeSample()
        {
            Assert.AreEqual(20, words.TakeSample(true, 20, 1).Length);
        }

        [TestMethod]
        public void TestRddMap()
        {
            var sparkContext = new SparkContext(null);
            var rdd = sparkContext.TextFile(@"c:\path\to\rddinput.txt");
            var rdd2 = rdd.Map(s => s.ToLower() + ".com");
            Assert.IsTrue(rdd2.GetType() == typeof(PipelinedRDD<string>));
            var pipelinedRdd = rdd2 as PipelinedRDD<string>;
            var func = pipelinedRdd.func;
            var result = func(1, new String[] { "ABC" });
            var output = result.First();
            Assert.AreEqual("ABC".ToLower() + ".com", output);

            var pipelinedRdd2 = rdd2.Map(s => "HTTP://" + s) as PipelinedRDD<string>;
            var func2 = pipelinedRdd2.func;
            var result2 = func2(1, new String[] { "ABC" });
            var output2 = result2.First();
            Assert.AreEqual("HTTP://" + ("ABC".ToLower() + ".com"), output2); //tolower and ".com" appended first before adding prefix due to the way func2 wraps func in implementation
        }

        [TestMethod]
        public void TestRddTextFile()
        {
            var sparkContext = new SparkContext(null);
            var rdd = sparkContext.TextFile(@"c:\path\to\rddinput.txt");
            var paramValuesToTextFileMethod = (rdd.RddProxy as MockRddProxy).mockRddReference as object[];
            Assert.AreEqual(@"c:\path\to\rddinput.txt", paramValuesToTextFileMethod[0]);
            Assert.AreEqual(0, int.Parse(paramValuesToTextFileMethod[1].ToString())); //checking default partitions
        }

        [TestMethod]
        public void TestRddUnion()
        {
            var sparkContext = new SparkContext(null);
            var rdd = sparkContext.TextFile(@"c:\path\to\rddinput.txt"); 
            var rdd2 = sparkContext.TextFile(@"c:\path\to\rddinput2.txt");
            var unionRdd = rdd.Union(rdd2);
            var paramValuesToUnionMethod = ((unionRdd.RddProxy as MockRddProxy).mockRddReference as object[]);
            var paramValuesToTextFileMethodInRdd1 = (paramValuesToUnionMethod[0] as MockRddProxy).mockRddReference as object[];
            Assert.AreEqual(@"c:\path\to\rddinput.txt", paramValuesToTextFileMethodInRdd1[0]);
            var paramValuesToTextFileMethodInRdd2 = (paramValuesToUnionMethod[1] as MockRddProxy).mockRddReference as object[];
            Assert.AreEqual(@"c:\path\to\rddinput2.txt", paramValuesToTextFileMethodInRdd2[0]);
        }
    }
}
