using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;
[GlobalClass]
public partial class TestNode : Node
{
    void hi(string who) {
        GD.Print($"Hi, {who}");
    }

   Array<String> get_list()
    {
        
        Array<String> list = new Array<String>();
        list.Add("hi");
        return list;
    }
}
