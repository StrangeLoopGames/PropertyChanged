﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

/// <summary>Test class for processors working with property attributes (i.e. NoOwnNotifyProcessing).</summary>
public class ClassWithAttributedProperties : INotifyPropertyChanged
{
    public List<string>                       Notifications   = new List<string>();
    public event PropertyChangedEventHandler? PropertyChanged;


    [DisplayName]                        public string?  DisplayName             { get; set; }
    [Description]                        public string?  Description             { get; set; }
    [AlsoNotifyFor(nameof(DisplayName))] public string?  NoOwnNotify             { get; set; }
    [DisplayName]                        public string?  DisplayNameAuto         { get; set; } = "Auto";
    public                                      string?  NoOwnNotifyAuto         { get; set; } = "Auto";
    [DisplayName] public                        DateTime DisplayNameAutoReadOnly { get; }      = DateTime.Now;
    public                                      DateTime NoOwnNotifyAutoReadOnly { get; }      = DateTime.Now;

    public void OnNoOwnNotifyChanged() => Notifications.Add("method:NoOwnNotify");

    void OnPropertyChanged(string name) => Notifications.Add($"event:{name}");
}
