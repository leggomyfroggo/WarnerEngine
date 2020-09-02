using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.Structure
{
    public class DepthGraphNode
    {
        public IDraw BackingDrawable { get; private set; }
        public HashSet<DepthGraphNode> OutgoingNodes { get; private set; }

        public DepthGraphNode()
        {
            OutgoingNodes = new HashSet<DepthGraphNode>();
        }

        /*public DepthGraphNode InsertNode(DepthGraphNode CandidateNode)
        {
            Box candidateBox = CandidateNode.BackingDrawable.GetBackingBox().B;
            Box thisBox = BackingDrawable.GetBackingBox().B;
            if (candidateBox.DoesProjectionOverlapOther(BackingDrawable.GetBackingBox().B))
            {
                if (thisBox.Compare(candidateBox) > 0)
                {
                    OutgoingNodes.Add(CandidateNode);
                }
                else
                {
                    CandidateNode.TryInsertNode(this);
                }
                foreach (DepthGraphNode outgoingNode in OutgoingNodes)
                {
                    if (outgoingNode == CandidateNode)
                    {
                        continue;
                    }
                    outgoingNode.TryInsertNode(CandidateNode);
                }
                OutgoingNodes.Add(Node);
                BoundingRectangle = Rectangle.Union(nodeRectangle, BoundingRectangle);
                return true;
            }
            return false;
        }

        public bool TryRemoveNode(IDraw BackingDrawable)
        {
            DepthGraphNode nodeToRemove = null;
            foreach (DepthGraphNode node in OutgoingNodes)
            {
                if (nodeToRemove != null)
                {
                    if (node.OutgoingNodes.Contains(nodeToRemove))
                    {
                        node.OutgoingNodes.Remove(nodeToRemove);
                    }
                }
                else if (node.BackingDrawable == BackingDrawable)
                {
                    nodeToRemove = node;
                }
            }
            if (nodeToRemove != null)
            {
                OutgoingNodes.Remove(nodeToRemove);
                return true;
            }
            return false;
        }*/
    }
}
