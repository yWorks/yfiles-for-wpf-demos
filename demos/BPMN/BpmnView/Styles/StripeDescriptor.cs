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

using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Demo.yFiles.Graph.Bpmn.Styles {
  /// <summary>
  /// Helper class that can be used as StyleTag to bundle common visualization parameters for stripes
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class StripeDescriptor {
    private Brush backgroundBrush = Brushes.Transparent;

    /// <summary>
    /// The background brush for a stripe
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(Brush), "Transparent")]
    public Brush BackgroundBrush {
      get { return backgroundBrush; }
      set { backgroundBrush = value; }
    }

    private Brush insetBrush = Brushes.Transparent;

    /// <summary>
    /// The inset brush for a stripe
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(Brush), "Transparent")]
    public Brush InsetBrush {
      get { return insetBrush; }
      set { insetBrush = value; }
    }

    private Brush borderBrush = Brushes.Black;

    /// <summary>
    /// The border brush for a stripe
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(Brush), "Black")]
    public Brush BorderBrush {
      get { return borderBrush; }
      set { borderBrush = value; }
    }

    private Thickness borderThickness = new Thickness(1);

    /// <summary>
    /// The border thickness for a stripe
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    [DefaultValue(typeof(Thickness), "1")]
    public Thickness BorderThickness {
      get { return borderThickness; }
      set { borderThickness = value; }
    }

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static bool operator ==(StripeDescriptor p1, StripeDescriptor p2) {
      if (ReferenceEquals(p1, p2)) {
        return true;
      }
      if (ReferenceEquals(null, p1) || ReferenceEquals(null, p2)) {
        return false;
      }
      return p1.InsetBrush == p2.InsetBrush && p1.BorderBrush == p2.BorderBrush && p1.BackgroundBrush == p2.BackgroundBrush && p1.BorderThickness == p2.BorderThickness;
    }

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public static bool operator !=(StripeDescriptor p1, StripeDescriptor p2) {
      return !(p1 == p2);
    }

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public bool Equals(StripeDescriptor other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return Equals(other.backgroundBrush, backgroundBrush) && Equals(other.insetBrush, insetBrush) && Equals(other.borderBrush, borderBrush) && other.borderThickness.Equals(borderThickness);
    }

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != typeof(StripeDescriptor)) {
        return false;
      }
      return Equals((StripeDescriptor)obj);
    }

    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public override int GetHashCode() {
      unchecked {
        int result = (backgroundBrush != null ? backgroundBrush.GetHashCode() : 0);
        result = (result * 397) ^ (insetBrush != null ? insetBrush.GetHashCode() : 0);
        result = (result * 397) ^ (borderBrush != null ? borderBrush.GetHashCode() : 0);
        result = (result * 397) ^ borderThickness.GetHashCode();
        return result;
      }
    }
  }
}
