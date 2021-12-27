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
using System.Windows.Media;

namespace Demo.yFiles.Option.Editor
{
  public static class ColorHelper {

    /// <summary>
    /// Converts an RGB color value to HSB values
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>An array of length 4 that contains the following values (in that order): The hue, the saturation, the brightness and the alpha.
    /// </returns>
    public static double[] ToHSB(Color color) {
      double hue, saturation, brightness, alpha;
      double r = color.R/255.0d;
      double g = color.G/255.0d;
      double b = color.B/255.0d;
      double a = color.A/255.0d;
      double max = Math.Max(b, Math.Max(r, g));
      double min = Math.Min(b, Math.Min(r, g));

      if (max == min) {
        hue = 0;
      } else if (max == r && g >= b) {
        hue = 1/6.0d*(g - b)/(max - min);
      } else if (max == r && g < b) {
        hue = 1/6.0d*(g - b)/(max - min) + 1;
      } else if (max == g) {
        hue = 1/6.0d*(b - r)/(max - min) + 2/6.0d;
      } else {//if (max == b) {
        hue = 1/6.0d*(r - g)/(max - min) + 4/6.0d;
      }

      double l = (max + min)*0.5d;
      if (max == min) {
        saturation = 0;
      } else if (l <= 0.5) {
        saturation = (max - min)/(2*l);
      } else {
        saturation = (max - min)/(2 - 2*l);
      }
      saturation = Math.Min(1, Math.Max(0, saturation));
      hue = Math.Min(1, Math.Max(0, hue));
      brightness = Math.Min(1, Math.Max(0, max));
      alpha = Math.Min(1, Math.Max(0, a));
      return new[] {hue, saturation, brightness, alpha};
    }

    /// <summary>
    /// Create a color from HSB values.
    /// </summary>
    public static Color FromHSB(double hue, double saturation, double brightness, double alpha) {
      int r = 0, g = 0, b = 0;
      if (saturation == 0) {
        r = g = b = (int) (brightness*255.0f + 0.5f);
      } else {
        double h = (hue - Math.Floor(hue))*6.0f;
        double f = h - Math.Floor(h);
        double p = brightness*(1.0f - saturation);
        double q = brightness*(1.0f - saturation*f);
        double t = brightness*(1.0f - (saturation*(1.0f - f)));
        switch ((int) h) {
          case 0:
            r = (int) (brightness*255.0f + 0.5f);
            g = (int) (t*255.0f + 0.5f);
            b = (int) (p*255.0f + 0.5f);
            break;
          case 1:
            r = (int) (q*255.0f + 0.5f);
            g = (int) (brightness*255.0f + 0.5f);
            b = (int) (p*255.0f + 0.5f);
            break;
          case 2:
            r = (int) (p*255.0f + 0.5f);
            g = (int) (brightness*255.0f + 0.5f);
            b = (int) (t*255.0f + 0.5f);
            break;
          case 3:
            r = (int) (p*255.0f + 0.5f);
            g = (int) (q*255.0f + 0.5f);
            b = (int) (brightness*255.0f + 0.5f);
            break;
          case 4:
            r = (int) (t*255.0f + 0.5f);
            g = (int) (p*255.0f + 0.5f);
            b = (int) (brightness*255.0f + 0.5f);
            break;
          case 5:
            r = (int) (brightness*255.0f + 0.5f);
            g = (int) (p*255.0f + 0.5f);
            b = (int) (q*255.0f + 0.5f);
            break;
        }
      }
      return Color.FromArgb(Convert.ToByte(alpha*255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
    }
  }
}