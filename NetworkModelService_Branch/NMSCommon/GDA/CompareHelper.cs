using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Common
{
	public class CompareHelper
	{
		public static bool CompareLists(List<long> xList, List<long> yList, bool compareReferences = true)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (compareReferences == true)
						{
							if (!yList.Contains(xList[i]))
							{
								return false;
							}
						}
						else
						{
							if (xList[i] != yList[i])
							{
								return false;
							}
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<int> xList, List<int> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (xList[i] != yList[i])
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<bool> xList, List<bool> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (xList[i] != yList[i])
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<short> xList, List<short> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (xList[i] != yList[i])
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<DMSType> xList, List<DMSType> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (xList[i] != yList[i])
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<float> xList, List<float> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (xList[i] != yList[i])
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareLists(List<string> xList, List<string> yList)
		{
			if (Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xList, null) && !Object.ReferenceEquals(yList, null)) || (!Object.ReferenceEquals(xList, null) && Object.ReferenceEquals(yList, null)))
			{
				return false;
			}
			else
			{
				if (xList.Count == yList.Count)
				{
					for (int i = 0; i < xList.Count; i++)
					{
						if (!xList[i].Equals(yList[i]))
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}		
		
		public static bool CompareArrays<T>(T[] a1, T[] a2)
		{
			if (Object.ReferenceEquals(a1, null) && Object.ReferenceEquals(a2, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(a1, null) && !Object.ReferenceEquals(a2, null)) || (!Object.ReferenceEquals(a1, null) && Object.ReferenceEquals(a2, null)))
			{
				return false;
			}
			else
			{
				if (a1.Length == a2.Length)
				{
					for (int i = 0; i < a1.Length; i++)
					{
						if (!a1[i].Equals(a2[i]))
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareHashSets(HashSet<int> xSet, HashSet<int> ySet)
		{
			if (Object.ReferenceEquals(xSet, null) && Object.ReferenceEquals(ySet, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xSet, null) && !Object.ReferenceEquals(ySet, null)) || (!Object.ReferenceEquals(xSet, null) && Object.ReferenceEquals(ySet, null)))
			{
				return false;
			}
			else
			{
				if (xSet.Count == ySet.Count)
				{
					foreach (int x in xSet)
					{
						if (!ySet.Contains(x))
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool CompareHashSets(HashSet<long> xSet, HashSet<long> ySet)
		{
			if (Object.ReferenceEquals(xSet, null) && Object.ReferenceEquals(ySet, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(xSet, null) && !Object.ReferenceEquals(ySet, null)) || (!Object.ReferenceEquals(xSet, null) && Object.ReferenceEquals(ySet, null)))
			{
				return false;
			}
			else
			{
				if (xSet.Count == ySet.Count)
				{
					foreach (long x in xSet)
					{
						if (!ySet.Contains(x))
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}
