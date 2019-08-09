/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Demo.yFiles.Graph.UMLClassStyle
{
  public sealed class ClassInfo : INotifyPropertyChanged, ICloneable
  {
    private string name;
    private string type;
    private bool showDetails;

    private FeatureSection fields = new FeatureSection("Fields");
    private FeatureSection properties = new FeatureSection("Properties");
    private FeatureSection methods = new FeatureSection("Methods");

    public ClassInfo() {}

    public ClassInfo(string type, string name) {
      this.name = name;
      this.type = type;
    }

    public bool ShowDetails {
      get { return showDetails; }
      set {
        if (showDetails != value) {
          showDetails = value;
          OnPropertyChanged(new PropertyChangedEventArgs("ShowDetails"));
        }
      }
    }

    public string Name {
      get { return name; }
      set {
        if (name != value) {
          name = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Name"));
        }
      }
    }

    public string Type {
      get { return type; }
      set {
        if (type != value) {
          type = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Type"));
        }
      }
    }

    public FeatureSection Fields {
      get { return fields; }
      set { fields = value; }
    }

    public FeatureSection Properties {
      get { return properties; }
      set { properties = value; }
    }

    public FeatureSection Methods {
      get { return methods; }
      set { methods = value; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IEnumerable<FeatureSection> Sections {
      get { return new FeatureSection[] {fields, properties, methods}; }
    }

    private void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

    public object Clone() {
      ClassInfo classInfo = new ClassInfo(type, name);
      classInfo.ShowDetails = ShowDetails;
      CopyProperties(properties, classInfo.Properties);
      CopyProperties(fields, classInfo.Fields);
      CopyProperties(methods, classInfo.methods);
      return classInfo;
    }

    private void CopyProperties(FeatureSection oldProperties, FeatureSection newProperties) {
      foreach (var property in oldProperties) {
        newProperties.Add((FeatureInfo)property.Clone());
      }
      newProperties.ShowDetails = oldProperties.ShowDetails;
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }


  public enum FeatureModifier
  {
    Public,
    Protected,
    Private
  } ;

  public sealed class FeatureSection : List<FeatureInfo>, INotifyPropertyChanged
  {
    private string name;

    public FeatureSection() {}

    public FeatureSection(string name) {
      this.name = name;
    }

    public string Name {
      get { return name; }
      set { name = value; }
    }

    public bool ShowDetails {
      get { return showDetails; }
      set {
        if (value != showDetails) {
          showDetails = value;
          OnPropertyChanged(new PropertyChangedEventArgs("ShowDetails"));
        }
      }
    }

    private bool showDetails;

    private void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  public sealed class FeatureInfo : INotifyPropertyChanged, ICloneable
  {
    private FeatureModifier modifier;
    private string signature;

    public FeatureInfo() {}

    public FeatureInfo(FeatureModifier modifier, string signature) {
      this.modifier = modifier;
      this.signature = signature;
    }

    public FeatureModifier Modifier {
      get { return modifier; }
      set {
        if (value != modifier) {
          modifier = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Modifier"));
        }
      }
    }

    public string Signature {
      get { return signature; }
      set {
        if (signature != value) {
          signature = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Signature"));
        }
      }
    }

    private void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public object Clone() {
      return new FeatureInfo(modifier, signature);
    }
  }
}
