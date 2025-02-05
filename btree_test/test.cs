/// Copyright (c) 2020 Code Ex Machina, LLC. All rights reserved.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with this program.If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using CodeExMachina;
using BTree = CodeExMachina.BTree<CodeExMachina.Int>;

namespace btree_test
{
    internal static class Extensions
    {
        public static int[] Perm(this Random r, int n)
        {
            int[] m = new int[n];
            for (int i = 0; i < n; i++)
            {
                int j = r.Next(i + 1);
                m[i] = m[j];
                m[j] = i;
            }
            return m;
        }

        public static string Dump<T>(this IEnumerable<T> list)
        {
            return string.Join(" ", list);
        }
    }

    public class Test
    {
        private readonly ITestOutputHelper _output;
        private readonly Random _rand;

        public Test(ITestOutputHelper output)
        {
            _output = output;
            _rand = new Random();
        }

        /// <summary>
        /// perm returns a random permutation of n Int items in the range [0, n).
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private List<Int> Perm(int n)
        {
            List<Int> list = new List<Int>();
            foreach (int v in _rand.Perm(n))
            {
                list.Add(new Int(v));
            }
            return list;
        }

        /// <summary>
        /// rang returns an ordered list of Int items in the range [0, n).
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private List<Int> Rang(int n)
        {
            List<Int> list = new List<Int>();
            for (int i = 0; i < n; i++)
            {
                list.Add(new Int(i));
            }
            return list;
        }

        /// <summary>
        /// all extracts all items from a tree in order as a slice.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<Int> All(BTree t)
        {
            List<Int> list = new List<Int>();
            t.Ascend((Int a) =>
            {
                list.Add(a);
                return true;
            });
            return list;
        }

        private List<Int> RangRev(int n)
        {
            List<Int> list = new List<Int>();
            for (int i = n - 1; i >= 0; i--)
            {
                list.Add(new Int(i));
            }
            return list;
        }

        private List<Int> AllRev(BTree t)
        {
            List<Int> list = new List<Int>();
            t.Descend((Int a) =>
            {
                list.Add(a);
                return true;
            });
            return list;
        }

        private const int BTreeDegree = 32;

        private bool DeepEqual(IEnumerable<Int> a, IEnumerable<Int> b)
        {
            List<Int> al = new List<Int>(a);
            List<Int> bl = new List<Int>(b);

            if (al.Count != bl.Count)
                return false;

            bool success = true;

            for (int i = 0; i < al.Count; ++i)
            {
                Int obj1 = (Int)al[i];
                Int obj2 = (Int)bl[i];

                if (obj1 != obj2)
                {
                    success = false;
                    break;
                }
            }

            return success;
        }

        [Fact]
        public void TestBTree()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            const int treeSize = 10000;
            for (int i = 0; i < 10; i++)
            {
                Int min = tr.Min();
                if (min != null)
                {
                    Assert.False(min != null, $"empty min, got {min}");
                }
                Int max = tr.Max();
                if (max != null)
                {
                    Assert.False(max != null, $"empty max, got {max}");
                }
                foreach (Int item in Perm(treeSize))
                {
                    Int x = tr.ReplaceOrInsert(item);
                    if (x != null)
                    {
                        Assert.False(x != null, $"insert found item {item}");
                    }
                }
                foreach (Int item in Perm(treeSize))
                {
                    Int x = tr.ReplaceOrInsert(item);
                    if (x == null)
                    {
                        Assert.False(x == null, $"insert didn't find item {item}");
                    }
                }
                min = tr.Min();
                Int want = new Int(0);
                if ((Int)min != want)
                {
                    Assert.False(true, $"min: want {want}, got {min}");
                }
                max = tr.Max();
                want = new Int(treeSize - 1);
                if ((Int)max != want)
                {
                    Assert.False(true, $"max: want {want}, got {max}");
                }
                List<Int> got = All(tr);
                List<Int> want2 = Rang(treeSize);
                if (!DeepEqual(got, want2))
                {
                    Assert.False(true, $"mismatch:\n got: {got.Dump()}\nwant: {want2.Dump()}");
                }
                List<Int> gotrev = AllRev(tr);
                List<Int> wantrev = RangRev(treeSize);
                if (!DeepEqual(gotrev, wantrev))
                {
                    Assert.False(true, $"mismatch:\n got: {got.Dump()}\nwant: {wantrev.Dump()}");
                }
                foreach (Int item in Perm(treeSize))
                {
                    Int x = tr.Delete(item);
                    if (x == null)
                    {
                        Assert.False(x == null, $"Didn't find {item}");
                    }
                }
                got = All(tr);
                if (got.Count > 0)
                {
                    Assert.False(got.Count > 0, $"some left!: {got.Dump()}");
                }
            }
        }

        private void ExampleBTree()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            for (int i = 0; i < 10; i++)
            {
                _ = tr.ReplaceOrInsert(new Int(i));
            }
            Console.WriteLine("len:       {0}", tr.Length);
            Console.WriteLine("get3:      {0}", tr.Get(new Int(3)));
            Console.WriteLine("get100:    {0}", tr.Get(new Int(100)));
            Console.WriteLine("del4:      {0}", tr.Delete(new Int(4)));
            Console.WriteLine("del100:    {0}", tr.Delete(new Int(100)));
            Console.WriteLine("replace5:  {0}", tr.ReplaceOrInsert(new Int(5)));
            Console.WriteLine("replace100:{0}", tr.ReplaceOrInsert(new Int(100)));
            Console.WriteLine("min:       {0}", tr.Min());
            Console.WriteLine("delmin:    {0}", tr.DeleteMin());
            Console.WriteLine("max:       {0}", tr.Max());
            Console.WriteLine("delmax:    {0}", tr.DeleteMax());
            Console.WriteLine("len:       {0}", tr.Length);
            // Output:
            // len:        10
            // get3:       3
            // get100:     <nil>
            // del4:       4
            // del100:     <nil>
            // replace5:   5
            // replace100: <nil>
            // min:        0
            // delmin:     0
            // max:        100
            // delmax:     100
            // len:        8
        }

        [Fact]
        public void TestDeleteMin()
        {
            BTree tr = new BTree(3, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            for (Int v = tr.DeleteMin(); v != null; v = tr.DeleteMin())
            {
                got.Add(v);
            }
            List<Int> want = Rang(100);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestDeleteMax()
        {
            BTree tr = new BTree(3, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            for (Int v = tr.DeleteMax(); v != null; v = tr.DeleteMax())
            {
                got.Add(v);
            }
            for (int i = 0; i < got.Count / 2; i++)
            {
                (got[i], got[got.Count - i - 1]) = (got[got.Count - i - 1], got[i]);
            }
            List<Int> want = Rang(100);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestAscendRange()
        {
            BTree tr = new BTree(2, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.AscendRange(new Int(40), new Int(60), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = Rang(100).GetRange(40, 60 - 40);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.AscendRange(new Int(40), new Int(60), (Int a) =>
            {
                if ((Int)a > new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = Rang(100).GetRange(40, 51 - 40);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestDescendRange()
        {
            BTree tr = new BTree(2, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.DescendRange(new Int(60), new Int(40), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = RangRev(100).GetRange(39, 59 - 39);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.DescendRange(new Int(60), new Int(40), (Int a) =>
            {
                if ((Int)a < new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = RangRev(100).GetRange(39, 50 - 39);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestAscendLessThan()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.AscendLessThan(new Int(60), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = Rang(100).GetRange(0, 60);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.AscendLessThan(new Int(60), (Int a) =>
            {
                if ((Int)a > new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = Rang(100).GetRange(0, 51);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestDescendLessOrEqual()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.DescendLessOrEqual(new Int(40), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = RangRev(100).GetRange(59, 100 - 59);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendlessorequal:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.DescendLessOrEqual(new Int(60), (Int a) =>
            {
                if ((Int)a < new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = RangRev(100).GetRange(39, 50 - 39);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendlessorequal:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestAscendGreaterOrEqual()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.AscendGreaterOrEqual(new Int(40), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = Rang(100).GetRange(40, 100 - 40);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.AscendGreaterOrEqual(new Int(40), (Int a) =>
            {
                if ((Int)a > new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = Rang(100).GetRange(40, 51 - 40);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"ascendrange:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        [Fact]
        public void TestDescendGreaterThan()
        {
            BTree tr = new BTree(BTreeDegree, new IntComparer());
            foreach (Int v in Perm(100))
            {
                _ = tr.ReplaceOrInsert(v);
            }
            List<Int> got = new List<Int>();
            tr.DescendGreaterThan(new Int(40), (Int a) =>
            {
                got.Add(a);
                return true;
            });
            IEnumerable<Int> want = RangRev(100).GetRange(0, 59);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendgreaterthan:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
            got.Clear();
            tr.DescendGreaterThan(new Int(40), (Int a) =>
            {
                if ((Int)a < new Int(50))
                {
                    return false;
                }

                got.Add(a);
                return true;
            });
            want = RangRev(100).GetRange(0, 50);
            if (!DeepEqual(got, want))
            {
                Assert.False(true, $"descendgreaterthan:\n got: {got.Dump()}\nwant: {want.Dump()}");
            }
        }

        private const int cloneTestSize = 10000;

        private void CloneTest(BTree b, int start, List<Int> p, CountdownEvent wg, List<BTree> trees, object mutex)
        {
            _output.WriteLine($"Starting new clone at {start}");
            lock (mutex)
            {
                trees.Add(b);
            }
            for (int i = start; i < cloneTestSize; i++)
            {
                lock (mutex)
                {
                    _ = b.ReplaceOrInsert(p[i]);
                }

                if (i % (cloneTestSize / 5) == 0)
                {
                    wg.AddCount(1);
                    BTree clone = b.Clone();
                    int index = i + 1;
                    _ = Task.Run(() => CloneTest(clone, index, p, wg, trees, mutex));
                }
            }
            _ = wg.Signal();
        }

        [Fact]
        public void TestCloneConcurrentOperations()
        {
            BTree b = new BTree(BTreeDegree, new IntComparer());
            List<BTree> trees = new List<BTree>();
            List<Int> p = Perm(cloneTestSize);
            CountdownEvent wg = new CountdownEvent(1);
            _ = Task.Run(() => CloneTest(b, 0, p, wg, trees, new object()));
            wg.Wait();
            List<Int> want = Rang(cloneTestSize);
            _output.WriteLine($"Starting equality checks on {trees.Count} trees");
            int i = 0;
            foreach (BTree tree in trees)
            {
                if (!DeepEqual(want, All(tree)))
                {
                    Assert.False(true, $"tree {i} mismatch");
                }
                i++;
            }
        }
    }
}
