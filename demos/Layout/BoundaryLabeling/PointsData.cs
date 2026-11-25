/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 4.1.
 ** Copyright (c) 2000-2026 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using yWorks.Geometry;

namespace yWorks.Demo.BoundaryLabeling
{
  /// <summary>
  ///   Contains the point data used to create this demo's graph.
  /// </summary>
  internal static class PointsData
  {
    public static readonly PointData[] Points = [
        new(new PointD(160, 20), "Water, dissolved salts, magnesium, calcium, sodium, potassium, and other compounds."),
        new(new PointD(100, 37.375), "<span style=\"font-weight: bold\">Ocean</span>"),
        new(new PointD(245, 29), "<span style=\"font-weight: bold\">Continental Crust</span> (32 km)"),
        new(new PointD(276.75, 65), "Mainly of granitic rocks rich in silica, aluminum, potassium, and sodium."),
        new(new PointD(267.5, 95),
            "Basalt rich in iron and magnesium, and smaller amounts of feldspar, pyroxene, and olivine."),
        new(new PointD(130, 115.5), "<span style=\"font-weight: bold\">Oceanic Crust</span> (8 km)"),
        new(new PointD(258, 126),
            "Silicate minerals rich in magnesium and iron, such as olivine, pyroxene, and garnet."),
        new(new PointD(130, 146.5), "<span style=\"font-weight: bold\">Upper Mantle</span> (100 km)"),
        new(new PointD(250, 160), "Silicate minerals, including olivine, pyroxene, and feldspar."),
        new(new PointD(130, 181.5),
            "<span style=\"font-weight: bold\">Partially Molten Asthenosphere</span> <span style=\"text-align: center\">(100 km)</span>"),
        new(new PointD(236.875, 205.625), "Solid rock and partially molten rock."),
        new(new PointD(130, 225.5), "<span style=\"font-weight: bold\">Solid Asthenosphere</span> (200 km)"),
        new(new PointD(224.125, 248.5), "Silicate minerals, such as garnet, bridgmanite, and post-perovskite."),
        new(new PointD(130, 266.5), "<span style=\"font-weight: bold\">Mantle Transition</span> (150 km)"),
        new(new PointD(213, 295),
            "Silicate minerals rich in iron and magnesium, such as silicate perovskite and ferropericlase."),
        new(new PointD(130, 315.5), "<span style=\"font-weight: bold\">Lower Mantle</span> (2240 km)"),
        new(new PointD(205, 342), "Liquid iron and nickel, oxygen, silicon, and sulfur."),
        new(new PointD(130, 371.5), "<span style=\"font-weight: bold\">Outer Molten Core</span> (2200 km)"),
        new(new PointD(192.875, 399), "Iron, nickel, sulfur, oxygen, and silicon."),
        new(new PointD(130, 415.5), "<span style=\"font-weight: bold\">Inner Solid Core</span> (1220 km)")
    ];
  }
}
