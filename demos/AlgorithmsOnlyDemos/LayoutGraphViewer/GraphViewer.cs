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
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using yWorks.Layout;

namespace Demo.yWorks.LayoutGraphViewer

{
  /// <summary>
  /// Summary description for GraphViewer.
  /// </summary>
  public class GraphViewer : Form

  {
    private TabControl tabControl;

    private MenuItem fileMenu;

    private MenuItem exitItem;

    private ToolBar toolBar1;

    private ToolBarButton zoomIn;

    private ToolBarButton zoomOut;

    private ToolBarButton zoomFit;

    private MainMenu mainMenu;

    private MenuItem menuItem1;

    private IContainer components;


    public GraphViewer() : this(null, null) {}


    public GraphViewer(LayoutGraph graph) : this(graph, "") {}


    public GraphViewer(LayoutGraph graph, String title) {
      this.Text = "yFiles LayoutGraph Viewer";

      this.AutoScroll = false;

      InitializeComponent();


      if (graph != null) {
        AddLayoutGraph(graph, title);
      }
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
      if (disposing) {
        if (components != null) {
          components.Dispose();
        }
      }

      base.Dispose(disposing);
    }


    private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
			this.fileMenu = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.exitItem = new System.Windows.Forms.MenuItem();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.zoomIn = new System.Windows.Forms.ToolBarButton();
			this.zoomOut = new System.Windows.Forms.ToolBarButton();
			this.zoomFit = new System.Windows.Forms.ToolBarButton();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 42);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(392, 276);
			this.tabControl.TabIndex = 4;
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenu});
			// 
			// fileMenu
			// 
			this.fileMenu.Index = 0;
			this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.exitItem});
			this.fileMenu.Text = "File";
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.menuItem1.Text = "Save as EMF...";
			this.menuItem1.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// exitItem
			// 
			this.exitItem.Index = 1;
			this.exitItem.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
			this.exitItem.Text = "Exit";
			this.exitItem.Click += new System.EventHandler(this.exit_Click);
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.zoomIn,
            this.zoomOut,
            this.zoomFit});
			this.toolBar1.ButtonSize = new System.Drawing.Size(75, 42);
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(392, 42);
			this.toolBar1.TabIndex = 5;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// zoomIn
			// 
			this.zoomIn.Name = "zoomIn";
			this.zoomIn.Text = "Zoom In";
			this.zoomIn.ToolTipText = "Zoom In";
			// 
			// zoomOut
			// 
			this.zoomOut.Name = "zoomOut";
			this.zoomOut.Text = "Zoom Out";
			this.zoomOut.ToolTipText = "Zoom Out";
			// 
			// zoomFit
			// 
			this.zoomFit.Name = "zoomFit";
			this.zoomFit.Text = "Zoom to 100%";
			this.zoomFit.ToolTipText = "Zoom to 100%";
			
			// 
			// GraphViewer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(392, 318);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.toolBar1);
			this.Menu = this.mainMenu;
			this.Name = "GraphViewer";
			this.ResumeLayout(false);
			this.PerformLayout();

    }


    public void AddLayoutGraph(LayoutGraph graph, String title) {
      LayoutGraphPanel panel = new LayoutGraphPanel(graph);

      panel.BorderStyle = BorderStyle.None;

      TabPage tp = new TabPage(title);

      tp.Controls.Add(panel);

      panel.Dock = DockStyle.Fill;

      this.tabControl.Controls.Add(tp);
    }


    private LayoutGraphPanel GetCurrentLayoutGraphPanel() {
      return (LayoutGraphPanel) this.tabControl.SelectedTab.Controls[0];
    }

    private void saveButton_Click(object sender, EventArgs e) {
      SaveFileDialog saveFileDialog = new SaveFileDialog();

      saveFileDialog.Filter = "EMF File|*.emf";

      saveFileDialog.Title = "Save an EMF File";

      saveFileDialog.ShowDialog();


      // If the file name is not an empty string open it for saving.

      if (saveFileDialog.FileName != "") {
        GetCurrentLayoutGraphPanel().ExportToEmf(saveFileDialog.FileName);
      }
    }


    private void exit_Click(object sender, EventArgs e) {
      Close();
    }


    private void toolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
      if (e.Button == this.zoomFit) {
        GetCurrentLayoutGraphPanel().Zoom = 1.0F;
      } else if (e.Button == this.zoomFit) {
        GetCurrentLayoutGraphPanel().Zoom = 1.0F;
      } else if (e.Button == this.zoomIn) {
        GetCurrentLayoutGraphPanel().Zoom *= 1.2F;
      } else if (e.Button == this.zoomOut) {
        GetCurrentLayoutGraphPanel().Zoom /= 1.2F;
      }
    }
  }
}
