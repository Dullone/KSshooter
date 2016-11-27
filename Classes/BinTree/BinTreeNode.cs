using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSshooter.Classes.BinTree
{
    class BinTreeNode<T>
    {
        public T dataItem;
        public BinTreeNode<T> leftItem; 
        public BinTreeNode<T> righItem;
        public int balance;

        public BinTreeNode(T data)
        {
            dataItem = data;
            leftItem = null;
            righItem = null;
            balance = 0;
        }
    }
}
