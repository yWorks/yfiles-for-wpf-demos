/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.6.
 ** Copyright (c) 2000-2024 by yWorks GmbH, Vor dem Kreuzberg 28,
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

using System.Collections.Generic;
using Demo.yFiles.Option.Handler;
using yWorks.Algorithms;
using yWorks.Layout;
using yWorks.Layout.Organic;

namespace Demo.yFiles.GraphEditor.Modules.Layout
{
  /// <summary>
  /// This module represents a wrapper for <see cref="ClassicOrganicLayout"/>. 
  /// 
  /// This demo shows not only how to write your own LayoutModule and 
  /// OptionHandler but also how to configure ClassicOrganicLayout by using its API.
  /// 
  /// The module provides an option handler that allows to configure
  /// the layout algorithm interactively.
  /// 
  /// A module will be started by calling its start method.
  ///
  /// When executed as a standalone demo this class will display the 
  /// option handler defined to this module.
  /// </summary>
  public class OrganicLayoutModule : LayoutModule
  {
    #region configuration constants
    private const string ACTIVATE_DETERMINISTIC_MODE = "ACTIVATE_DETERMINISTIC_MODE";
    private const string ACTIVATE_TREE_BEAUTIFIER = "ACTIVATE_TREE_BEAUTIFIER";
    private const string ACTIVATE_MULTI_THREADING = "ACTIVATE_MULTI_THREADING";
    private const string MAXIMAL_DURATION = "MAXIMAL_DURATION";
    private const string ITERATION_FACTOR = "ITERATION_FACTOR";
    private const string OBEY_NODE_SIZES = "OBEY_NODE_SIZES";
    private const string GRAVITY_FACTOR = "GRAVITY_FACTOR";
    private const string SCOPE = "SCOPE";
    private const string INITIAL_PLACEMENT = "INITIAL_PLACEMENT";
    private const string PREFERRED_EDGE_LENGTH = "PREFERRED_EDGE_LENGTH";
    private const string VISUAL = "VISUAL";
    private const string ALGORITHM = "ALGORITHM";
    private const string ORGANIC = "ORGANIC";
    private const string ONLY_SELECTION = "ONLY_SELECTION";
    private const string MAINLY_SELECTION = "MAINLY_SELECTION";
    private const string ALL = "ALL";
    private const string AS_IS = "AS_IS";
    private const string RANDOM = "RANDOM";
    private const string AT_ORIGIN = "AT_ORIGIN";
    private const string REPULSION = "REPULSION";
    private const string ATTRACTION = "ATTRACTION";

    private const string GROUPING = "GROUPING";
    private const string GROUP_LAYOUT_POLICY = "GROUP_LAYOUT_POLICY";
    private const string IGNORE_GROUPS = "IGNORE_GROUPS";
    private const string LAYOUT_GROUPS = "LAYOUT_GROUPS";
    private const string FIX_GROUPS = "FIX_GROUPS";
    private const string GROUP_NODE_COMPACTNESS = "GROUP_NODE_COMPACTNESS";

    private static readonly Dictionary<string, InitialPlacement> initialPlacementEnum = new Dictionary<string, InitialPlacement>();
    private static readonly Dictionary<string, Scope> scopeEnum = new Dictionary<string, Scope>();
    private static readonly Dictionary<string, GroupsPolicy> groupPolicyEnum = new Dictionary<string, GroupsPolicy>();

    static OrganicLayoutModule() {
      initialPlacementEnum.Add(AS_IS, InitialPlacement.AsIs);
      initialPlacementEnum.Add(AT_ORIGIN, InitialPlacement.Zero);
      initialPlacementEnum.Add(RANDOM, InitialPlacement.Random);
      
      scopeEnum.Add(ALL, Scope.All);
      scopeEnum.Add(MAINLY_SELECTION, Scope.MainlySubset);
      scopeEnum.Add(ONLY_SELECTION, Scope.Subset);

      groupPolicyEnum.Add(LAYOUT_GROUPS, GroupsPolicy.Layout);
      groupPolicyEnum.Add(FIX_GROUPS, GroupsPolicy.Fixed);
      groupPolicyEnum.Add(IGNORE_GROUPS, GroupsPolicy.Ignore);
    }
    #endregion

    #region private members
    private ClassicOrganicLayout organic;
    #endregion

    /// <summary>
    /// Create a new instance
    /// </summary>
    public OrganicLayoutModule() : base(ORGANIC) {
    }
    
    #region LayoutModule interface
    ///<inheritdoc/>
    protected override void SetupHandler() {
      createOrganic();
      OptionGroup visualGroup = Handler.AddGroup(VISUAL);
      visualGroup.AddList( SCOPE, scopeEnum.Keys, ALL);
      visualGroup.AddList( INITIAL_PLACEMENT, initialPlacementEnum.Keys, AS_IS);
      visualGroup.AddDouble( PREFERRED_EDGE_LENGTH, organic.PreferredEdgeLength, 0.0, double.MaxValue);
      visualGroup.AddBool( OBEY_NODE_SIZES, organic.ConsiderNodeSizes);
      visualGroup.AddInt( ATTRACTION, organic.Attraction, 0, 2);
      visualGroup.AddInt( REPULSION, organic.Repulsion, 0, 2);
      visualGroup.AddDouble(GRAVITY_FACTOR, organic.GravityFactor, -0.2, 2);
      visualGroup.AddBool( ACTIVATE_TREE_BEAUTIFIER, organic.ActivateTreeBeautifier);

      OptionGroup algoGroup = Handler.AddGroup(ALGORITHM);
      algoGroup.AddDouble( ITERATION_FACTOR, organic.IterationFactor);
      algoGroup.AddInt( MAXIMAL_DURATION, (int)(organic.MaximumDuration / 1000));
      algoGroup.AddBool( ACTIVATE_DETERMINISTIC_MODE, organic.Deterministic);
      algoGroup.AddBool( ACTIVATE_MULTI_THREADING, organic.MultiThreadingAllowed);
      
      OptionGroup groupingGroup = Handler.AddGroup(GROUPING);
      groupingGroup.AddList(GROUP_LAYOUT_POLICY, groupPolicyEnum.Keys, LAYOUT_GROUPS);
      groupingGroup.AddDouble(GROUP_NODE_COMPACTNESS, organic.GroupNodeCompactness, 0, 1);
    }
    
    ///<inheritdoc/>
    protected override void ConfigureLayout() {
      createOrganic();
      OptionGroup visualGroup = (OptionGroup) Handler.GetGroupByName(VISUAL);
      
      organic.PreferredEdgeLength = (int) visualGroup[PREFERRED_EDGE_LENGTH].Value;
      string initialPlacementChoice = (string) visualGroup[INITIAL_PLACEMENT].Value;
      organic.InitialPlacement = initialPlacementEnum[initialPlacementChoice];
      string scopeChoice = (string) visualGroup[SCOPE].Value;
      organic.Scope = scopeEnum[scopeChoice];
      organic.GravityFactor = (double) visualGroup[GRAVITY_FACTOR].Value;
      organic.ConsiderNodeSizes = (bool) visualGroup[OBEY_NODE_SIZES].Value;
      organic.ActivateTreeBeautifier = (bool) visualGroup[ACTIVATE_TREE_BEAUTIFIER].Value;
      organic.Attraction = (int) visualGroup[ATTRACTION].Value;
      organic.Repulsion = 2 - (int)visualGroup[REPULSION].Value;

      OptionGroup algoGroup = (OptionGroup) Handler.GetGroupByName(ALGORITHM);
      organic.MaximumDuration = 1000 * (int)algoGroup[MAXIMAL_DURATION].Value;
      organic.IterationFactor = (double)algoGroup[ITERATION_FACTOR].Value;
      organic.Deterministic = (bool)algoGroup[ACTIVATE_DETERMINISTIC_MODE].Value;
           
      if (ContainsGroupNodes()) {
        string groupPolicyChoice = (string)Handler.GetValue(GROUPING, GROUP_LAYOUT_POLICY);
        organic.GroupNodePolicy = groupPolicyEnum[groupPolicyChoice];
        organic.GroupNodeCompactness = (double)Handler.GetValue(GROUPING, GROUP_NODE_COMPACTNESS);
      }

      organic.MultiThreadingAllowed = (bool)algoGroup[ACTIVATE_MULTI_THREADING].Value;
      
      LayoutAlgorithm = organic;
    }

    ///<inheritdoc/>
    protected override void PerformPreLayout() {
      IDataProvider affectedNodes = CurrentLayoutGraph.GetDataProvider(LayoutKeys.AffectedNodesDpKey);
      if(affectedNodes != null) {
        CurrentLayoutGraph.AddDataProvider(ClassicOrganicLayout.AffectedNodesDpKey, affectedNodes);        
      }
    }
    
    ///<inheritdoc/>
    protected  override void PerformPostLayout() {
      CurrentLayoutGraph.RemoveDataProvider(ClassicOrganicLayout.AffectedNodesDpKey);
    }
    #endregion

    #region private helpers
    private void createOrganic() {
      organic = new ClassicOrganicLayout();
      organic.MultiThreadingAllowed = true;
    }
    #endregion
  }
}
