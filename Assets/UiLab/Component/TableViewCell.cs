using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class TableViewCell : UiBase
{
    public VerticalTableView tableView;
    public int index;
    public int offset;
}
