/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Reflection;
using System.Windows.Media;
using Demo.yFiles.Graph.Bpmn.Util;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="INodeStyle"/> implementation representing a Data Object according to the BPMN.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class DataObjectNodeStyle : BpmnNodeStyle {
    private IIcon dataIcon;
    private IIcon collectionIcon;

    private bool collection;

    /// <summary>
    /// Gets or sets whether this is a Collection Data Object.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(false)]
    public bool Collection {
      get {
        return collection;
      }
      set {
        if (collection != value) {
          ModCount++;
          collection = value;
        }
      }
    }

    private DataObjectType type;

    /// <summary>
    /// Gets or sets the data object type for this style.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(DataObjectType.None)]
    public DataObjectType Type {
      get {
        return type;
      }
      set {
        if (type != value) {
          ModCount++;
          type = value;
          UpdateTypeIcon();
        }
      }
    }

    private Brush background = BpmnConstants.DataObjectDefaultBackground;

    /// <summary>
    /// Gets or sets the background color of the data object.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DataObjectDefaultBackground")]
    public Brush Background {
      get { return background; }
      set {
        if (background != value) {
          ModCount++;
          background = value;
          UpdateDataIcon();
        }
      }
    }

    private Brush outline = BpmnConstants.DataObjectDefaultOutline;

    /// <summary>
    /// Gets or sets the outline color of the data object.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DataObjectDefaultOutline")]
    public Brush Outline {
      get { return outline; }
      set {
        if (outline != value) {
          ModCount++;
          outline = value;
          UpdateDataIcon();
        }
      }
    }

    private Brush iconColor = BpmnConstants.DefaultIconColor;

    /// <summary>
    /// Gets or sets the color for the icon.
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(BpmnConstants), "DefaultIconColor")]
    public Brush IconColor {
      get { return iconColor; }
      set {
        if (iconColor != value) {
          ModCount++;
          iconColor = value;
          UpdateTypeIcon();
          UpdateCollectionIcon();
        }
      }
    }

    private IIcon typeIcon;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public DataObjectNodeStyle() {
      MinimumSize = new SizeD(25, 30);
      Type = DataObjectType.None;
    }

    private void UpdateCollectionIcon() {
      collectionIcon = IconFactory.CreatePlacedIcon(
          IconFactory.CreateLoopCharacteristic(LoopCharacteristic.Parallel, IconColor),
          BpmnConstants.DataObjectMarkerPlacement, BpmnConstants.MarkerSize);
    }

    private void UpdateTypeIcon() {
      typeIcon = IconFactory.CreateDataObjectType(Type, IconColor);
      if (typeIcon != null) {
        typeIcon = IconFactory.CreatePlacedIcon(typeIcon, BpmnConstants.DataObjectTypePlacement, BpmnConstants.DataObjectTypeSize);
      }
    }

    private void UpdateDataIcon() {
      dataIcon = IconFactory.CreateDataObject(Background, Outline);
    }

    /// <inheritdoc/>
    internal override void UpdateIcon(INode node) {
      if (dataIcon == null) {
        UpdateDataIcon();
      }
      if (collectionIcon == null) {
        UpdateCollectionIcon();
      }

      var icons = new List<IIcon> {dataIcon};

      if (Collection) {
        icons.Add(collectionIcon);
      }
      if (typeIcon != null) {
        icons.Add(typeIcon);
      }
      if (icons.Count > 1) {
        Icon = IconFactory.CreateCombinedIcon(icons);
      } else {
        Icon = dataIcon;
      }
    }

    /// <inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override GeneralPath GetOutline(INode node) {
      var cornerSize = Math.Min(node.Layout.Width, node.Layout.Height) * 0.4;

      var path = new GeneralPath();
      path.MoveTo(0, 0);
      path.LineTo(node.Layout.Width - cornerSize, 0);
      path.LineTo(node.Layout.Width, cornerSize);
      path.LineTo(node.Layout.Width, node.Layout.Height);
      path.LineTo(0, node.Layout.Height);
      path.Close();

      var transform = new Matrix2D();
      transform.Translate(node.Layout.GetTopLeft());
      path.Transform(transform);
      return path;
    }
  }
}
