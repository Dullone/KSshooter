using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSshooter.Classes.BinTree
{
    class BinTreeHitDetection
    {
        const int LEFTHEAVY = -1;
        const int BALANCED = 0;
        const int RIGHTHEAVY = 1;

        private BinTreeNode<HitableObject> rootNode;
        int numnodes;

        public BinTreeHitDetection()
        {
            rootNode = null;
            numnodes = 0;
        }

        public void AddHitableObject(HitableObject hObj)
        {
            BinTreeNode<HitableObject> node = new BinTreeNode<HitableObject>(hObj);
            //traverse tree find where to insert
            bool reviseBalance = false;
            InsertNode(ref rootNode, node, ref reviseBalance);
            numnodes++;
        }

        public void RemoveHitableObject(HitableObject hObj)
        {
        }

        private void UpdateLeftTree(ref BinTreeNode<HitableObject> Parent, ref bool ReviseBalance)
        {
            if (Parent.leftItem.balance == LEFTHEAVY)
            {
                RotateRight(ref Parent);
                ReviseBalance = false;
            }
            else if (Parent.leftItem.balance == RIGHTHEAVY)
            {
                DoubleRotateRight(ref Parent);
                ReviseBalance = false;
            }
        }

        private void UpdateRightTree(ref BinTreeNode<HitableObject> Parent, ref bool ReviseBalance)
        {
            if (Parent.righItem.balance == RIGHTHEAVY)
            {
                RotateLeft(ref Parent);
                ReviseBalance = false;
            }
            else if (Parent.righItem.balance == LEFTHEAVY)
            {
                DoubleRotateLeft(ref Parent);
                ReviseBalance = false;
            }
        }

        /// <summary>
        /// recursive insert node
        /// </summary>
        /// <param name="tree">node we are in</param>
        /// <param name="inode">node to insert</param>
        /// <returns>amount tree grows by </returns>
        /// <param name="nodevalue">Value to compare</param>
        /// <param name="inodevalue">Value to compare</param>
        /// <returns>The size of the subtree</returns>
        private void InsertNode(ref BinTreeNode<HitableObject> tree, BinTreeNode<HitableObject> inode, ref bool ReviseBalance)
        {
            bool RebalanceCurrentNode = false;

            if(tree == null)
            {
                tree = inode;
                tree.balance = BALANCED;
                ReviseBalance = true;
            }
            else if (inode.dataItem.HitRectangle.X > tree.dataItem.HitRectangle.X)
            {
                InsertNode(ref tree.righItem, inode, ref RebalanceCurrentNode);
                if(RebalanceCurrentNode == true)
                {
                    if (tree.balance == LEFTHEAVY)
                    {
                        tree.balance = BALANCED;
                        ReviseBalance = false;
                    }
                    else if (tree.balance == BALANCED)
                    {
                        tree.balance = RIGHTHEAVY;
                        ReviseBalance = true;
                    }
                    else //was right heavy, now +2
                    {
                        UpdateRightTree(ref tree, ref ReviseBalance);
                    }
                }
            }
            else if (inode.dataItem.HitRectangle.X < tree.dataItem.HitRectangle.X)
            {
                InsertNode(ref tree.leftItem, inode, ref RebalanceCurrentNode);
                if (RebalanceCurrentNode == true)
                {
                    if (tree.balance == LEFTHEAVY)
                        UpdateLeftTree(ref tree, ref ReviseBalance);
                    else if (tree.balance == BALANCED)
                    {
                        tree.balance = LEFTHEAVY;
                        ReviseBalance = true;
                    }
                    else
                    {
                        tree.balance = BALANCED;
                        ReviseBalance = false;
                    }

                }
                else //they are equal, using y value now
                {
                    if (inode.dataItem.HitRectangle.Y > tree.dataItem.HitRectangle.Y)
                    {
                        InsertNode(ref tree.righItem, inode, ref RebalanceCurrentNode);
                        if (RebalanceCurrentNode == true)
                        {
                            if (tree.balance == LEFTHEAVY)
                            {
                                tree.balance = BALANCED;
                                ReviseBalance = false;
                            }
                            else if (tree.balance == BALANCED)
                            {
                                tree.balance = RIGHTHEAVY;
                                ReviseBalance = true;
                            }
                            else //was right heavy, now +2
                            {
                                UpdateRightTree(ref tree, ref ReviseBalance);
                            }
                        }
                    }
                    else if (inode.dataItem.HitRectangle.Y < tree.dataItem.HitRectangle.Y)
                    {
                        InsertNode(ref tree.leftItem, inode, ref RebalanceCurrentNode);
                        if (RebalanceCurrentNode == true)
                        {
                            if (tree.balance == LEFTHEAVY)
                                UpdateLeftTree(ref tree, ref ReviseBalance);
                            else if (tree.balance == BALANCED)
                            {
                                tree.balance = LEFTHEAVY;
                                ReviseBalance = true;
                            }
                            else
                            {
                                tree.balance = BALANCED;
                                ReviseBalance = false;
                            }
                        }
                    }
                    else  //they are overlapping, throw an error
                    {
                        throw new BinTreeOverlapException();
                    }
                }
            }
        }

        private void RemoveNode(BinTreeNode<HitableObject> node)
        {
        }

        private void RotateRight(ref BinTreeNode<HitableObject> aNode)
        {
            BinTreeNode<HitableObject> leftNode = aNode.leftItem;
            aNode.balance = BALANCED;
            aNode.leftItem.balance = BALANCED;

            aNode.leftItem = leftNode.righItem;
            leftNode.righItem = aNode;
            aNode = leftNode;
        }
        private void RotateLeft(ref BinTreeNode<HitableObject> aNode)
        {
            BinTreeNode<HitableObject> rightNode = aNode.righItem;
            aNode.balance = BALANCED;
            aNode.righItem.balance = BALANCED;

            aNode.righItem = rightNode.leftItem;
            rightNode.leftItem = aNode;
            aNode = rightNode;
        }
        
        private void DoubleRotateLeft(ref BinTreeNode<HitableObject> Parent)
        {
            //RightChildPtr = reinterpret_cast<AVLNodePtr>(ParentPtr->Right);
            BinTreeNode<HitableObject> RightChild = Parent.righItem;
            //NewParentPtr = reinterpret_cast<AVLNodePtr>(RightChildPtr->Left);
            BinTreeNode<HitableObject> NewParent = RightChild.leftItem;

            if (NewParent.balance == LEFTHEAVY)
            {
                Parent.balance = BALANCED;
                RightChild.balance = RIGHTHEAVY;
            }
            else if (NewParent.balance == BALANCED)
            {
                Parent.balance = BALANCED;
                RightChild.balance = BALANCED;
            }
            else
            {
                Parent.balance = LEFTHEAVY;
                RightChild.balance = BALANCED;
            }

            NewParent.balance = BALANCED;
            RightChild.leftItem = NewParent.righItem;
            NewParent.righItem = RightChild;
            Parent.righItem = NewParent.leftItem;
            NewParent.leftItem = Parent;
            Parent = NewParent;
        }

        private void DoubleRotateRight(ref BinTreeNode<HitableObject> Parent)
        {
            BinTreeNode<HitableObject> LeftChild = Parent.leftItem;
            BinTreeNode<HitableObject> NewParent = LeftChild.righItem;

            if (NewParent.balance == RIGHTHEAVY)
            {
                Parent.balance = BALANCED;
                LeftChild.balance = LEFTHEAVY;
            }
            else if (NewParent.balance == BALANCED)
            {
                Parent.balance = BALANCED;
                LeftChild.balance = BALANCED;
            }
            else
            {
                Parent.balance = RIGHTHEAVY;
                LeftChild.balance = BALANCED;
            }

            NewParent.balance = BALANCED;
            LeftChild.righItem = NewParent.leftItem;
            NewParent.leftItem = LeftChild;
            Parent.leftItem = NewParent.righItem;
            NewParent.righItem = Parent;
            Parent = NewParent;
        }

        public class BinTreeOverlapException : Exception
        {
            public BinTreeOverlapException()
                : base("Binary Tree overlap, check for hit before inserting.")
            {
            }

        }
    }
}


