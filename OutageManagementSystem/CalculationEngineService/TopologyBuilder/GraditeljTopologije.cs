//using CECommon;
//using CECommon.Interfaces;
//using CECommon.Model;
//using NetworkModelServiceFunctions;
//using Outage.Common;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TopologyBuilder
//{
//    public class GraditeljTopologije : ITopologyBuilder
//    {
//        ILogger logger = LoggerWrapper.Instance;
//        private List<Field> fields;
//        private HashSet<long> visited;
//        private Stack<long> stack;
//        private Stopwatch stopwatch = new Stopwatch();
//        private Dictionary<long, ITopologyElement> elements;
//        private Dictionary<long, Measurement> measurements;
//        private Dictionary<long, List<long>> connections;
//        public ITopology CreateGraphTopology(long firstElementGid)
//        {
//            logger.LogDebug("Web topology builder started.");
//            NMSManager.Instance.GetSvee(out elements, out measurements, out connections);
//            ITopology topology = new TopologyModel();
//            topology.FirstNode = firstElementGid;
//            Stack<long> stack = new Stack<long>();
//            stack.Push(firstElementGid);

//            while (stack.Count > 0)
//            {
//                ITopologyElement currentElement = elements[stack.Pop()];
                
//                    foreach (long child in GetReferencedElementsWithoutIgnorables(currentElement.Id))
//                    {
//                        long nextElement = child;
//                        stack.Push(nextElement);
//                    }
//                    uIModel.AddNode(newUINode);
               
//            }
//            logger.LogDebug("Web topology builder finished.");
//            return uIModel;
//        }

//        private List<long> GetReferencedElementsWithoutIgnorables(long gid)
//        {
//            var list = connections[gid].Where(e => !visited.Contains(e)).ToList();
//            List<long> refElements = new List<long>();
//            foreach (var element in list)
//            {
//                if (TopologyHelper.Instance.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
//                {
//                    visited.Add(element);
//                    refElements.AddRange(GetReferencedElementsWithoutIgnorables(element));
//                }
//                else
//                {
//                    refElements.Add(element);
//                }
//            }
//            return refElements;
//        }
//    }
//}
