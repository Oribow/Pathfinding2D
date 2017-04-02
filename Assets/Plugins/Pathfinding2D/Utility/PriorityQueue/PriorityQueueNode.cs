using System;

namespace Utility
{
    [System.Serializable]
    public class PriorityQueueNode : IComparable<PriorityQueueNode>
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        public double Priority { get;
            set; 
        }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }

        public virtual int CompareTo(PriorityQueueNode other)
        {
            if (Priority > other.Priority)
                return 1;
            else if (Priority < other.Priority)
                return -1;
            else if (InsertionIndex < other.InsertionIndex)
                return 1;
            else
                return -1;
        }
    }
}
