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

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using yWorks.Annotations;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.GraphML;

namespace yWorks.Demo
{
  /// <summary>
  ///   A utility class for importing and exporting graph data in GraphML format.
  /// </summary>
  /// <remarks>
  ///   Import and export can be performed on three levels:
  ///   <ul>
  ///     <li>
  ///       The methods <see cref="ImportFromGraphML(string)"/> and <see cref="ExportToGraphML(string)"/> directly
  ///       import/export the graph from/to a GraphML file specified as parameter.
  ///     </li>
  ///     <li>
  ///       The methods <see cref="Open"/>, <see cref="Save"/> and <see cref="SaveAs"/> import/export
  ///       the graph from/to a GraphML file selected in a file dialog.
  ///     </li>
  ///     <li>
  ///       <see cref="RegisterOpenSaveCommandBindings"/> adds command bindings for <see cref="ApplicationCommands.Open"/>,
  ///       <see cref="ApplicationCommands.Save"/>, and <see cref="ApplicationCommands.SaveAs"/> to the
  ///       <see cref="GraphControl"/> that call <see cref="Open"/>, <see cref="Save"/>, or <see cref="SaveAs"/> and
  ///       active the <see cref="GraphControl"/>s <see cref="WaitInputMode"/> during the file operation if one can
  ///       be found.
  ///     </li>
  ///   </ul>
  /// </remarks>
  public sealed class GraphMLSupport
  {
    /// <summary>
    ///   Gets the <see cref="GraphControl"/> that this <see cref="GraphMLSupport"/> instance is associated with.
    /// </summary>
    public GraphControl GraphControl { get; }

    /// <summary>
    ///   Gets or sets the <see cref="GraphMLIOHandler"/> that this <see cref="GraphMLSupport"/> uses for GraphML handling.
    /// </summary>
    public GraphMLIOHandler GraphMLIOHandler { get; set; }

    /// <summary>
    ///   Creates a new instance of the <see cref="GraphMLSupport"/> class for the given <see cref="GraphControl"/>.
    /// </summary>
    /// <param name="graphControl">
    ///   The <see cref="GraphControl"/> that this <see cref="GraphMLSupport"/> instance is associated
    ///   with.
    /// </param>
    public GraphMLSupport(GraphControl graphControl) {
      GraphControl = graphControl;
      GraphMLIOHandler = new GraphMLIOHandler();
    }

    #region FileOperations command bindings

    private CommandBinding? openBinding;
    private CommandBinding? saveBinding;
    private CommandBinding? saveAsBinding;

    /// <summary>
    ///   Registers command bindings for <see cref="ApplicationCommands.Open"/>, <see cref="ApplicationCommands.Save"/>,
    ///   and <see cref="ApplicationCommands.SaveAs"/> to the <see cref="GraphControl"/>.
    /// </summary>
    /// <remarks>
    ///   The command bindings call <see cref="Open"/>, <see cref="Save"/>, or <see cref="SaveAs"/> and activate the
    ///   <see cref="GraphControl"/>s  <see cref="WaitInputMode"/> during the file operation if one can be found.
    /// </remarks>
    public void RegisterOpenSaveCommandBindings() {
      if (openBinding != null) {
        // prevent double registration
        return;
      }
      openBinding = new CommandBinding(ApplicationCommands.Open, OpenFileCommandExecuted, CanExecuteFileCommand);
      RegisterCommandBinding(openBinding);
      saveBinding = new CommandBinding(ApplicationCommands.Save, SaveFileCommandExecuted, CanExecuteFileCommand);
      RegisterCommandBinding(saveBinding);
      saveAsBinding = new CommandBinding(ApplicationCommands.SaveAs, SaveFileCommandExecuted, CanExecuteFileCommand);
      RegisterCommandBinding(saveAsBinding);
    }

    /// <summary>
    ///   Deregisters the command bindings for <see cref="ApplicationCommands.Open"/>,
    ///   <see cref="ApplicationCommands.Save"/>, and <see cref="ApplicationCommands.SaveAs"/> that had been registered
    ///   via <see cref="RegisterOpenSaveCommandBindings"/> from the <see cref="GraphControl"/>.
    /// </summary>
    public void DeregisterOpenSaveCommandBindings() {
      DeregisterCommandBinding(openBinding);
      openBinding = null;
      DeregisterCommandBinding(saveBinding);
      saveBinding = null;
      DeregisterCommandBinding(saveAsBinding);
      saveAsBinding = null;
    }

    private void RegisterCommandBinding(CommandBinding commandBinding) {
      // This is indeed required in WPF - cannot simply insert at 0
      if (GraphControl.CommandBindings.Count == 0) {
        GraphControl.CommandBindings.Add(commandBinding);
      } else {
        GraphControl.CommandBindings.Insert(0, commandBinding);
      }
    }

    private void DeregisterCommandBinding(CommandBinding? commandBinding) {
      if (commandBinding is not null) {
        GraphControl.CommandBindings.Remove(commandBinding);
      }
    }

    private void CanExecuteFileCommand(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = true;
      e.Handled = true;
    }

    private void OpenFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
      var waitMode = GraphControl.Lookup(typeof(WaitInputMode)) as WaitInputMode;
      if (waitMode != null && !waitMode.Waiting) {
        if (waitMode.CanStartWaiting()) {
          waitMode.Waiting = true;
        }
      }
      try {
        Open();
      } finally {
        if (waitMode != null) {
          waitMode.Waiting = false;
        }
      }
      e.Handled = true;
    }

    private void SaveFileCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
      var waitMode = GraphControl.Lookup(typeof(WaitInputMode)) as WaitInputMode;
      if (waitMode != null && !waitMode.Waiting) {
        if (waitMode.CanStartWaiting()) {
          waitMode.Waiting = true;
        }
      }
      try {
        if (e.Command == ApplicationCommands.Save) {
          Save();
        } else if (e.Command == ApplicationCommands.SaveAs) {
          SaveAs();
        }
      } finally {
        if (waitMode != null) {
          waitMode.Waiting = false;
        }
      }
      e.Handled = true;
    }

    #endregion

    #region Open/Save methods via FileDialogs

    private OpenFileDialog? openFileDialog;
    private SaveFileDialog? saveFileDialog;

    /// <summary>
    ///   Loads the graph from a GraphML file the user will be prompted to select in a file dialog.
    /// </summary>
    public void Open() {
      if (openFileDialog == null) {
        openFileDialog = CreateOpenFileDialog();
      }
      if (openFileDialog != null) {
        if (openFileDialog.ShowDialog() == true) {
          try {
            GraphMLIOHandler.Read(GraphControl.Graph, openFileDialog.FileName);
            GraphControl.FitGraphBounds();
            if (saveFileDialog != null) {
              saveFileDialog.FileName = openFileDialog.FileName;
            }
          } catch (Exception e) {
            Console.WriteLine(e);
            ShowErrorMessageBox("There was a problem during the loading of the file.", "Error Loading File");
          }
        }
      }
    }

    /// <summary>
    ///   Saves the graph to a GraphML file using the file name last used in a save or open file dialog.
    /// </summary>
    /// <remarks>
    ///   If no previous file dialogs are initialized, the user will be prompted to select a file.
    /// </remarks>
    public void Save() {
      try {
        //Shortcut if any of the previous file dialogs is already initialized
        //to provide a file name
        string? lastFile = null;
        if (saveFileDialog != null && !string.IsNullOrEmpty(saveFileDialog.FileName)) {
          lastFile = saveFileDialog.FileName;
        }
        if (lastFile == null && openFileDialog != null && !string.IsNullOrEmpty(openFileDialog.FileName)) {
          lastFile = openFileDialog.FileName;
        }
        if (!string.IsNullOrEmpty(lastFile)) {
          ExportToGraphML(lastFile);
          return;
        }
        //Otherwise, we fall through
      } catch (Exception ex) {
        Console.WriteLine(ex);
        ShowErrorMessageBox("There was a problem during the saving of the file: " + ex.Message, "Error Saving File");
        return;
      }
      SaveAs();
    }

    /// <summary>
    ///   Saves the graph to a GraphML file the user will be prompted to select in a file dialog.
    /// </summary>
    public void SaveAs() {
      try {
        if (saveFileDialog == null) {
          saveFileDialog = CreateSaveFileDialog();
        }
        if (saveFileDialog != null) {
          if (saveFileDialog.ShowDialog() == true) {
            ExportToGraphML(saveFileDialog.FileName);
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex);
        ShowErrorMessageBox("There was a problem during the saving of the file: " + ex.Message, "Error Saving File");
      }
    }

    private void ShowErrorMessageBox(string messageBoxText, string caption) {
      var messageBoxOwner = MessageBoxOwner;
      if (messageBoxOwner != null) {
        MessageBox.Show(messageBoxOwner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
      } else {
        MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    [CanBeNull]
    private Window? MessageBoxOwner =>
        Window.GetWindow(GraphControl) ?? (Application.Current != null ? Application.Current.MainWindow : null);

    [NotNull]
    private OpenFileDialog CreateOpenFileDialog() =>
        new() {
            DefaultExt = "graphml",
            Filter = "GraphML Files|*.graphml|XML Files|*.xml|All files|*.*",
            Title = "Open Diagram..."
        };

    [NotNull]
    private SaveFileDialog CreateSaveFileDialog() =>
        new() {
            DefaultExt = "graphml",
            Filter = "GraphML Files|*.graphml|XML Files|*.xml|All files|*.*",
            Title = "Save Diagram..."
        };

    #endregion

    #region Import/Export methods

    /// <summary>
    ///   Imports the <see cref="GraphControl.Graph"/> from a <see cref="GraphMLIOHandler">GraphML</see> file.
    /// </summary>
    /// <remarks>
    ///   For full control over the input, use the <see cref="yWorks.GraphML.GraphMLIOHandler"/> class directly.
    ///   To change the <see cref="yWorks.GraphML.GraphMLIOHandler"/> instance used implicitly,
    ///   set the <see cref="GraphMLIOHandler"/> property.
    ///   <para>
    ///     After the graph has been loaded, the <see cref="CanvasControl.Viewport"/> will be
    ///     <see cref="GraphControl.FitGraphBounds">updated</see> to display the entire graph. If
    ///     undo is enabled, the <see cref="GraphControl.Graph"/>'s undo engine will be
    ///     <see cref="UndoEngine.Clear">cleared</see>.
    ///   </para>
    /// </remarks>
    /// <param name="fileName">The name of the GraphML file to read the graph from.</param>
    public void ImportFromGraphML(string fileName) {
      GraphMLIOHandler.Read(GraphControl.Graph, fileName);
      GraphControl.FitGraphBounds();
      GraphControl.Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Imports the <see cref="GraphControl.Graph"/> from a <see cref="GraphMLIOHandler">GraphML</see> reader.
    /// </summary>
    /// <remarks>
    ///   For full control over the input, use the <see cref="yWorks.GraphML.GraphMLIOHandler"/> class directly.
    ///   To change the <see cref="yWorks.GraphML.GraphMLIOHandler"/> instance used implicitly,
    ///   set the <see cref="GraphMLIOHandler"/> property.
    ///   <para>
    ///     After the graph has been loaded, the <see cref="CanvasControl.Viewport"/> will be
    ///     <see cref="GraphControl.FitGraphBounds">updated</see> to display the entire graph. If
    ///     undo is enabled, the <see cref="GraphControl.Graph"/>'s undo engine will be
    ///     <see cref="UndoEngine.Clear">cleared</see>.
    ///   </para>
    /// </remarks>
    /// <param name="reader">The reader to read the graph from.</param>
    public void ImportFromGraphML(TextReader reader) {
      GraphMLIOHandler.Read(GraphControl.Graph, reader);
      GraphControl.FitGraphBounds();
      GraphControl.Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Imports the <see cref="GraphControl.Graph"/> from a <see cref="GraphMLIOHandler">GraphML</see> stream.
    /// </summary>
    /// <remarks>
    ///   For full control over the input, use the <see cref="yWorks.GraphML.GraphMLIOHandler"/> class directly.
    ///   To change the <see cref="yWorks.GraphML.GraphMLIOHandler"/> instance used implicitly,
    ///   set the <see cref="GraphMLIOHandler"/> property.
    ///   <para>
    ///     After the graph has been loaded, the <see cref="CanvasControl.Viewport"/> will be
    ///     <see cref="GraphControl.FitGraphBounds">updated</see> to display the entire graph. If
    ///     undo is enabled, the <see cref="GraphControl.Graph"/>'s undo engine will be
    ///     <see cref="UndoEngine.Clear">cleared</see>.
    ///   </para>
    /// </remarks>
    /// <param name="stream">The stream to read the graph from.</param>
    public void ImportFromGraphML(Stream stream) {
      GraphMLIOHandler.Read(GraphControl.Graph, stream);
      GraphControl.FitGraphBounds();
      GraphControl.Graph.GetUndoEngine()?.Clear();
    }

    /// <summary>
    ///   Exports the <see cref="GraphControl.Graph"/> to a <see cref="GraphMLIOHandler">GraphML</see> file.
    /// </summary>
    /// <remarks>
    ///   For full control over the output, use the <see cref="yWorks.GraphML.GraphMLIOHandler"/> class directly.
    ///   To change the <see cref="yWorks.GraphML.GraphMLIOHandler"/> instance used implicitly,
    ///   set the <see cref="GraphMLIOHandler"/> property.
    /// </remarks>
    /// <param name="filename">The file name to export the graph to.</param>
    public void ExportToGraphML(string filename) {
      GraphMLIOHandler.Write(GraphControl.Graph, filename);
    }

    /// <summary>
    ///   Exports the <see cref="GraphControl.Graph"/> to a <see cref="GraphMLIOHandler">GraphML</see> file.
    /// </summary>
    /// <remarks>
    ///   For full control over the output, use the <see cref="yWorks.GraphML.GraphMLIOHandler"/> class directly.
    ///   To change the <see cref="yWorks.GraphML.GraphMLIOHandler"/> instance used implicitly,
    ///   set the <see cref="GraphMLIOHandler"/> property.
    /// </remarks>
    /// <param name="writer">The writer to export the graph to.</param>
    public void ExportToGraphML(TextWriter writer) {
      GraphMLIOHandler.Write(GraphControl.Graph, writer);
    }

    #endregion Import/Export methods
  }
}
