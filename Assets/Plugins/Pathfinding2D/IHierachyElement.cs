using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorUI
{
    public interface IHierarchyElement
    {
        object Data { get; }
        IEnumerable<IHierarchyElement> Children { get; }
        int ChildrenCount { get; }
        IHierarchyElement GetChildrenAt(int index);
        float HeightOffsetRelativeToParent { get; set; }
    }
}
